
using Epgu;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repo;
using Smev;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Repo.ApplicationContext>();
builder.Services.AddHttpClient();


var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
string storagePath = builder.Configuration["Storage:StoragePath"] ?? "./DocumentsStorage";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Извлекать токен из Cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<EpguIntegrationService>();
builder.Services.AddScoped<SmevIntegrationService>();
builder.Services.AddHostedService<SmevDeadRequestWorker>();
builder.Services.AddHostedService<EpguDeadRequestWorker>();


builder.Services.AddHttpClient("EpguApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Epgu:BaseUrl"] ?? "http://localhost:5010/api/gusmev/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("SmevApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Smev:BaseUrl"] ?? "http://localhost:5025/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowCredentials()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// --- Эндпоинты ---
app.MapPost("/auth", (Dto.UserAuth userAuth, Repo.ApplicationContext context, HttpContext httpContext) =>
{
    var user = context.Users.FirstOrDefault(u =>
        u.Login == userAuth.Login &&
        u.Password == userAuth.Password &&
        u.DeletedAt == null);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddDays(1),
        signingCredentials: creds
    );

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenString = tokenHandler.WriteToken(token);

    // Установка cookie-файла с токеном
    httpContext.Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax, // Разрешает отправку между разными портами
        Secure = false,               // false для локального HTTP, true для продакшена (HTTPS)
        Expires = DateTime.UtcNow.AddDays(1)
    });

    httpContext.Response.Cookies.Append("UserRole", user.Role, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = false,
        Expires = DateTime.UtcNow.AddDays(1)
    });

    return Results.Ok(new { token = tokenString, userRole = user.Role });
});

app.MapDelete("auth", (HttpContext httpContext) =>
{
    httpContext.Response.Cookies.Delete("UserRole");
    httpContext.Response.Cookies.Delete("AuthToken");

});

// Заказы
app.MapGet("/orders", (Repo.ApplicationContext context) =>
{
    return context.Orders
        .Include(o => o.User)
        .Include(o => o.Documents)
        .Include(o => o.EpguOrder)
        .Include(o => o.SmevOrder)
        .Where(o => o.IsDeadRequest == false)
        .Select(o => new Dto.OrderGet(o));

}).RequireAuthorization();

app.MapGet("/orders/{id}", async (
    int id,
    [FromServices] Repo.ApplicationContext context,
    [FromServices] IHttpClientFactory httpClientFactory,
    [FromServices] EpguIntegrationService epguIntegrationService,
    [FromServices] SmevIntegrationService smevIntegrationService
) =>
{
    var order = context.Orders
        .Include(o => o.User)
        .Include(o => o.Documents)
        .Include(o => o.EpguOrder)
        .Include(o => o.SmevOrder)
        .FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    if (order.EpguOrder != null)
    {
        var statusCode = await epguIntegrationService.GetOrderStatusAsync(order);

        if (statusCode != null)
        {
            order.EpguOrder.OrderStatusId = statusCode.Value.OrderStatusId;
            context.SaveChanges();
        }

    }

    else if (order.SmevOrder != null)
    {
        string? statusId = await smevIntegrationService.GetOrderStatusAsync(order);
        if (statusId != null)
        {
            order.SmevOrder.OrderStatusId = statusId;
            context.SaveChanges();
        }
        order.SmevOrder.OrderStatusId = statusId;

    }

    return Results.Ok(new Dto.OrderGet(order));

}).RequireAuthorization();

app.MapPost("/esiaorders/", async (
    [FromForm] Dto.EsiaOrderCreate orderData,
    [FromServices] Repo.ApplicationContext context,
    ClaimsPrincipal userClaims,
    [FromServices] EpguIntegrationService epguIntegrationService,
    [FromServices] SmevIntegrationService smevIntegrationService
    ) =>
{

    var orderCreatedDate = DateTime.UtcNow;
    var receiverOid = orderData.ReceiverOid;
    var receiverSnils = orderData.ReceiverSnils;
    var description = orderData.Description;
    var signType = orderData.SignatureType;
    var userIdStr = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int userId;
    var config = context.ConfigSettings.FirstOrDefault();

    if (!int.TryParse(userIdStr, out userId))
    {
        return Results.Unauthorized();
    }

    if (orderData.DocumentsPack.Any(d => !FileTypeHelper.IsValid(d)))
    {
        return Results.BadRequest();
    }

    var user = context.Users.FirstOrDefault(u => u.Id == userId);

    var newOrder = new Repo.Order
    {
        CreatedDate = orderCreatedDate,
        ReceiverOid = receiverOid,
        ReceiverSnils = receiverSnils,
        UserId = userId,
        User = user,
        Description = orderData.Description,
        EpguOrder = new Repo.EpguOrder() { SignatureType = signType }
    };

    context.Orders.Add(newOrder);
    context.SaveChanges();

    foreach (var doc in orderData.DocumentsPack)
    {
        if (doc == null || doc.Length == 0)
        {
            continue;
        }

        var fileExtension = Path.GetExtension(doc.FileName);
        var uniqueFileName = FileNameHelper.GetUniqueFileName(doc.FileName);
        var zipFileName = FileNameHelper.GetUniqueFileName(doc.FileName);
        var epguDocumentId = FileNameHelper.GetUniqueFileName(doc.FileName);

        var newDocument = new Repo.Document
        {
            Name = doc.FileName,
            LocalName = uniqueFileName,
            Order = newOrder,
            ZipFileName = zipFileName,
            DocumentEpguCode = epguDocumentId
        };

        context.Documents.Add(newDocument);

        // Сохранить файл на диск
        var filePath = Path.Combine(storagePath, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            doc.CopyTo(stream);
        }
    }
    context.SaveChanges();


    // Отправка запрос в ЕПГУ
    var epguOrderId = await epguIntegrationService.FetchOrderIdAsync();
    epguOrderId = await epguIntegrationService.SendOrderAsync(newOrder);

    if (epguOrderId is not null)
    {
        newOrder.EpguOrder.EpguOrderCode = epguOrderId;
        newOrder.IsDeadRequest = false;
    }

    context.SaveChanges();
    // ----

    return Results.Created($"/orders/{newOrder.Id}", new Dto.OrderGet(newOrder));

}).RequireAuthorization().DisableAntiforgery();



app.MapGet("/orders/download-signed/{id}", async (int id,
    [FromServices] Repo.ApplicationContext context,
    [FromServices] EpguIntegrationService epguIntegrationService,
    [FromServices] SmevIntegrationService smevIntegrationService
    ) =>
{
    var order = context.Orders
      .Include(o => o.User)
      .Include(o => o.Documents)
      .Include(o => o.EpguOrder)
      .Include(o => o.SmevOrder)
      .FirstOrDefault(o => o.Id == id);

    if (order is null)
    {
        return Results.NotFound("Заказ не найден");
    }

    // Проверка статуса подписания для СМЭВ
    if (order.SmevOrder is not null)
    {
        var orderResult = await smevIntegrationService.GetOrderResultAsync(order);
        if (orderResult is null)
        {
            return Results.NotFound("Запрос еще не подписан или подписи не найдены");
        }
    }

    // Проверка статуса для ЕПГУ 
    if (order.EpguOrder is not null)
    {
        var status = await epguIntegrationService.GetOrderStatusAsync(order);
        if (status == null || status.Value.OrderStatusId != "DONE")
        {
            return Results.NotFound("Запрос ЕПГУ еще не подписан");
        }
    }

    var filesToZip = new Dictionary<string, byte[]>();

    foreach (var doc in order.Documents)
    {
        var filePath = Path.Combine(storagePath, doc.LocalName);
        if (!File.Exists(filePath))
        {
            continue;
        }

        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

        var entryName = string.IsNullOrWhiteSpace(doc.ZipFileName) ? doc.Name : doc.ZipFileName;

        filesToZip[entryName] = fileBytes;

        var signatureBytes = FileSignatureHelper.CreateDetachedSignatureBytes(fileBytes);
        filesToZip[$"{entryName}.sig"] = signatureBytes;
    }

    if (filesToZip.Count == 0)
    {
        return Results.NotFound("Файлы документов отсутствуют или не найдены на сервере");
    }

    var archiveBytes = await ArchiveHelper.buildArchiveAsync(filesToZip);

    return Results.File(archiveBytes, "application/zip", $"order_{id}_signed.zip");

}).RequireAuthorization();



app.MapPost("/orders/retry/{id}", async (
    int id,
    [FromServices] Repo.ApplicationContext context,
    ClaimsPrincipal userClaims,
    [FromServices] EpguIntegrationService epguIntegrationService,
    [FromServices] SmevIntegrationService smevIntegrationService
) =>
{

    var orderData = context.Orders
        .Include(o => o.EpguOrder)
        .Include(o => o.SmevOrder)
        .Include(o => o.Documents)
        .Include(o => o.User)
        .FirstOrDefault(o => o.Id == id)
        ;
    if (orderData is null) return Results.NotFound("Запрос не найден");

    if (orderData.EpguOrder is not null)
    {
        // Отправка запрос в ЕПГУ
        orderData.IsDeadRequest = true;
        var epguOrderId = await epguIntegrationService.FetchOrderIdAsync();
        epguOrderId = await epguIntegrationService.SendOrderAsync(orderData);

        if (epguOrderId is not null)
        {
            orderData.EpguOrder.EpguOrderCode = epguOrderId;
            orderData.IsDeadRequest = false;
        }

        context.SaveChanges();
        // ----
    }

    else if (orderData.EpguOrder is not null)
    {
        // Отправка XML запроса в СМЭВ
        orderData.IsDeadRequest = true;
        orderData.SmevOrder = new Repo.SmevOrder();
        context.SaveChanges();

        var smevMessageId = await smevIntegrationService.SendOrderAsync(orderData);
        orderData.SmevOrder.SmevMessageId = smevMessageId ?? "";
        orderData.SmevOrder.OrderStatusId = "";
        context.SaveChanges();

        // Подтверждение отправки запроса
        if (smevMessageId is not null)
        {
            orderData.IsDeadRequest = false;
        }
        // ---
    }



    return Results.Created($"/orders/{orderData.Id}", new Dto.OrderGet(orderData));



});

app.MapPost("/smevorders/", async (
    [FromForm] Dto.SmevOrderCreate orderData,
    [FromServices] Repo.ApplicationContext context,
    ClaimsPrincipal userClaims,
    [FromServices] EpguIntegrationService epguIntegrationService,
    [FromServices] SmevIntegrationService smevIntegrationService
    ) =>
{

    var orderCreatedDate = DateTime.UtcNow;
    var receiverSnils = orderData.ReceiverSnils;
    var description = orderData.Description;
    var userIdStr = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int userId;
    var config = context.ConfigSettings.FirstOrDefault();

    if (!int.TryParse(userIdStr, out userId))
    {
        return Results.Unauthorized();
    }

    if (orderData.DocumentsPack.Any(d => !FileTypeHelper.IsValid(d)))
    {
        return Results.BadRequest();
    }

    var user = context.Users.FirstOrDefault(u => u.Id == userId);

    var newOrder = new Repo.Order
    {
        CreatedDate = orderCreatedDate,
        ReceiverSnils = receiverSnils,
        UserId = userId,
        User = user,
        Description = orderData.Description,
        SmevOrder = new Repo.SmevOrder()
    };

    context.Orders.Add(newOrder);
    context.SaveChanges();

    foreach (var doc in orderData.DocumentsPack)
    {
        if (doc == null || doc.Length == 0)
        {
            continue;
        }

        var fileExtension = Path.GetExtension(doc.FileName);
        var uniqueFileName = FileNameHelper.GetUniqueFileName(doc.FileName);
        var zipFileName = FileNameHelper.GetUniqueFileName(doc.FileName);
        var epguDocumentId = FileNameHelper.GetUniqueFileName(doc.FileName);

        var newDocument = new Repo.Document
        {
            Name = doc.FileName,
            LocalName = uniqueFileName,
            Order = newOrder,
            ZipFileName = zipFileName,
            DocumentEpguCode = epguDocumentId
        };

        context.Documents.Add(newDocument);

        // Сохранить файл на диск
        var filePath = Path.Combine(storagePath, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            doc.CopyTo(stream);
        }
    }

    // Отправка XML запроса в СМЭВ
    newOrder.SmevOrder = new Repo.SmevOrder();
    context.SaveChanges();

    var smevMessageId = await smevIntegrationService.SendOrderAsync(newOrder);
    newOrder.SmevOrder.SmevMessageId = smevMessageId ?? "";
    newOrder.SmevOrder.OrderStatusId = "";
    context.SaveChanges();

    // Подтверждение отправки запроса
    if (smevMessageId is not null)
    {
        newOrder.IsDeadRequest = false;
    }
    // ---

    return Results.Created($"/orders/{newOrder.Id}", new Dto.OrderGet(newOrder));

}).RequireAuthorization().DisableAntiforgery();





app.MapGet("/documents/{localName}", (string localName, IWebHostEnvironment env) =>
{

    var safeFileName = Path.GetFileName(localName);
    if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName != localName)
    {
        return Results.BadRequest("Некорректное имя файла.");
    }

    var storagePath = Path.Combine(env.ContentRootPath, "DocumentsStorage");
    var filePath = Path.Combine(storagePath, safeFileName);

    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    var contentType = "application/octet-stream";

    return Results.File(filePath, contentType, safeFileName);

}).RequireAuthorization();

// Пользователи и настройки
app.MapGet("/users", (Repo.ApplicationContext context) =>
{
    return context.Users
        .Where(u => u.DeletedAt == null)
        .Select(u => new Dto.UserGet(u));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/users/{id:int}", (int id, Repo.ApplicationContext context) =>
{
    var user = context.Users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    user.DeletedAt = DateTime.UtcNow;
    context.SaveChanges();
    return Results.Ok();

}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPost("/users", (Dto.UserCreate userData, Repo.ApplicationContext context) =>
{
    var newUser = new Repo.User
    {
        Name = userData.Name,
        Login = userData.Login,
        Password = userData.Password,
        Role = "user"
    };
    context.Users.Add(newUser);
    context.SaveChanges();

    return Results.Ok(new Dto.UserGet(newUser));

}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapGet("/configsettings", (Repo.ApplicationContext context) =>
{
    var config = context.ConfigSettings.FirstOrDefault();
    return config is not null
        ? Results.Ok(new Dto.ConfigSettingsGet(config))
        : Results.NotFound();

}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/configsettings", (Dto.ConfigSettingsPut newConfig, Repo.ApplicationContext context) =>
{
    var config = context.ConfigSettings.FirstOrDefault();
    if (config is not null)
    {
        config.Mnemonics = newConfig.Mnemonics;
        config.ServiceName = newConfig.ServiceName;
        config.OrgName = newConfig.OrgName;
        config.ServiceCode = newConfig.ServiceCode;
        config.TargetCode = newConfig.TargetCode;
        config.Region = newConfig.Region;
        context.SaveChanges();
    }
    return Results.Ok();
}).RequireAuthorization(policy => policy.RequireRole("admin")).DisableAntiforgery();


app.MapGet("/initusers", (Repo.ApplicationContext context) =>
{
    context.Orders.ExecuteDelete();
    context.Users.ExecuteDelete();
    context.ConfigSettings.ExecuteDelete();

    var user1 = new Repo.User { Name = "Петров Петр Петрович", Login = "PP@mail.ru", Password = "12345", Role = "user" };
    var user2 = new Repo.User { Name = "Васильев Василий Васильевич", Login = "VV@mail.ru", Password = "12345", Role = "user" };
    var user3 = new Repo.User { Name = "Тихонов Тихон Тихонович", Login = "TT@mail.ru", Password = "12345", Role = "admin" };

    context.Users.AddRange(user1, user2, user3);

    // context.Orders.AddRange(
    //     new Order { CreatedDate = DateTime.UtcNow, Description = "Запрос на подписание доверенности", EpguOrderId = 123456789, User = user1, ReceiverIdType = "snils", ReceiverId = "123 123 123 00" },
    //     new Order { CreatedDate = DateTime.UtcNow.AddDays(-3), Description = "Запрос на подписание доверенности", EpguOrderId = 987654321, User = user2, ReceiverIdType = "oid", ReceiverId = "1000001234567" }
    // );

    context.ConfigSettings.Add(new Repo.ConfigSettings
    {
        Mnemonics = "MNSV03",
        ServiceName = "Отправка документов на подпись в «Госключ»",
        OrgName = "ООО \"СИМЭНЕРГО\"",
        ServiceCode = "10000000374",
        TargetCode = "-10000000374",
        Region = "45000000000"
    });

    context.SaveChanges();
    return Results.Ok("Пользователи и заказы успешно инициализированы.");
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.Run();