using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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

        // Явное указание извлекать токен из Cookie
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

// Авторизация (доступен без авторизации)
app.MapPost("/auth", (Dto.UserAuth userAuth, ApplicationContext context, HttpContext httpContext) =>
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

    // Возвращаем токен также в теле ответа для обратной совместимости, 
    // если клиентское приложение ожидает его в JSON.
    return Results.Ok(new { token = tokenString, userRole = user.Role });
});

app.MapDelete("auth", (HttpContext httpContext) =>
{
    httpContext.Response.Cookies.Delete("UserRole");
    httpContext.Response.Cookies.Delete("AuthToken");

});

// Заказы
app.MapGet("/orders", (ApplicationContext context) =>
{
    return context.Orders
        .Include(o => o.User)
        .Include(o => o.Documents)
        .Select(o => new Dto.OrderGet(o));

}).RequireAuthorization();

app.MapGet("/orders/{id}", (int id, ApplicationContext context) =>
{
    var order = context.Orders
        .Include(o => o.User)          // Явная загрузка пользователя
        .Include(o => o.Documents)     // Явная загрузка документов (уже используется в вашем DTO)
        .FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new Dto.OrderGet(order));
}).RequireAuthorization();

app.MapPost("/orders/", ([FromForm] Dto.OrderCreate orderData, ApplicationContext context, ClaimsPrincipal user) =>
{

    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out int userId))
    {
        return Results.Unauthorized();
    }

    var newOrder = new Repo.Order
    {
        EpguOrderId = 123123123, // Todo: Вытащить из ответа ЕПГУ
        CreatedDate = DateTime.UtcNow,
        ReceiverId = orderData.ReceiverId,
        ReceiverIdType = orderData.ReceiverIdType,
        UserId = userId,
        Description = orderData.Description,
        Documents = new List<Repo.Document>()
    };

    context.Orders.Add(newOrder);

    // Рекомендуется выносить путь в конфигурацию (appsettings.json)
    string storagePath = "./DocumentsStorage"; 
    
    // Гарантируем существование директории перед записью
    Directory.CreateDirectory(storagePath);

    foreach (var doc in orderData.DocumentsPack)
    {
        if (doc == null || doc.Length == 0)
        {
            continue;
        }

        var fileExtension = Path.GetExtension(doc.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(storagePath, uniqueFileName);

        var newDocument = new Repo.Document
        {
            Name = doc.FileName,       // Оригинальное имя файла для отображения пользователю
            LocalName = uniqueFileName, // Уникальное имя для физического хранения
            Order = newOrder
        };

        context.Documents.Add(newDocument);

        // Сохраняем файл на диск
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            doc.CopyTo(stream);
        }
    }

    // Сохраняем все изменения в базе данных (и Order, и Document)
    context.SaveChanges();

    return Results.Created($"/orders/{newOrder.Id}", newOrder.Id);
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
app.MapGet("/users", (ApplicationContext context) =>
{
    return context.Users
        .Where(u => u.DeletedAt == null)
        .Select(u => new Dto.UserGet(u));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/users/{id:int}", (int id, ApplicationContext context) =>
{
    var user = context.Users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    user.DeletedAt = DateTime.UtcNow;
    context.SaveChanges();
    return Results.Ok(new { id });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPost("/users", (Dto.UserCreate userData, ApplicationContext context) =>
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

app.MapGet("/configsettings", (ApplicationContext context) =>
{
    var config = context.ConfigSettings.FirstOrDefault();
    return config is not null
        ? Results.Ok(new Dto.ConfigSettingsGet(config))
        : Results.NotFound();
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/configsettings", (Dto.ConfigSettingsPut newConfig, ApplicationContext context) =>
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

// Инициализация (доступно ТОЛЬКО для admin)
app.MapGet("/initusers", (ApplicationContext context) =>
{
    context.Orders.ExecuteDelete();
    context.Users.ExecuteDelete();
    context.ConfigSettings.ExecuteDelete();

    var user1 = new User { Name = "Петров Петр Петрович", Login = "PP@mail.ru", Password = "12345", Role = "user" };
    var user2 = new User { Name = "Васильев Василий Васильевич", Login = "VV@mail.ru", Password = "12345", Role = "user" };
    var user3 = new User { Name = "Тихонов Тихон Тихонович", Login = "TT@mail.ru", Password = "12345", Role = "admin" };

    context.Users.AddRange(user1, user2, user3);

    // context.Orders.AddRange(
    //     new Order { CreatedDate = DateTime.UtcNow, Description = "Запрос на подписание доверенности", EpguOrderId = 123456789, User = user1, ReceiverIdType = "snils", ReceiverId = "123 123 123 00" },
    //     new Order { CreatedDate = DateTime.UtcNow.AddDays(-3), Description = "Запрос на подписание доверенности", EpguOrderId = 987654321, User = user2, ReceiverIdType = "oid", ReceiverId = "1000001234567" }
    // );

    context.ConfigSettings.Add(new ConfigSettings
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