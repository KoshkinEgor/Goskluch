using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");


app.MapGet("/orders", (ApplicationContext context) =>
{
    // Todo: Запросить статус заказа у ЕПГУ и склеить с ответом
    return context.Orders
        .Include(o => o.User)
        .Select(o => new Dto.OrderGet(o));

});

app.MapGet("/orders/{id:int}", (int id, ApplicationContext context) =>
{
    // Todo: Запросить статус заказа у ЕПГУ и склеить с ответом
    var result = context.Orders
        .Include(o => o.User)
        .FirstOrDefault(o => o.Id == id);

    return result is not null ? Results.Ok(new Dto.OrderGet(result)) : Results.NotFound();

});

app.MapPost("/orders/", ([FromForm] Dto.OrderCreate orderData, ApplicationContext context) =>
{
    // Todo: Сформировать заказ ЕПГУ и дождаться orderid

    var newOrder = new Repo.Order
    {
        EpguOrderId = 123123123, // Todo: Вытащить из ответа ЕПГУ
        CreatedDate = DateTime.Now,
        ReceiverId = orderData.ReceiverId,
        ReceiverIdType = orderData.ReceiverIdType,
        UserId = 1, // Todo: Вытащить из токена авторизации
        Description = orderData.Description,
        DocumentsPack = orderData.DocumentsPack.Select(d => d.FileName).ToList()

    };

    context.Orders.Add(newOrder);

    context.SaveChanges();

    return Results.Created($"/orders/{newOrder.Id}", newOrder.Id);


}).DisableAntiforgery();

app.MapGet("/configsettings", (ApplicationContext context) =>
{
    var config = context.ConfigSettings
        .FirstOrDefault();

    return config is not null
    ? Results.Ok(new Dto.ConfigSettingsGet(config))
    : Results.NotFound();

});

app.MapPut("/configsettings", (Dto.ConfigSettingsPut newConfig, ApplicationContext context) =>
{
    var config = context.ConfigSettings
        .FirstOrDefault();

    if (config is not null)
    {
        config.Mnemonics = newConfig.Mnemonics;
        config.ServiceName = newConfig.ServiceName;
        config.OrgName = newConfig.OrgName;
        config.ServiceCode = newConfig.ServiceCode;
        config.TargetCode = newConfig.TargetCode;
        config.Region = newConfig.Region;

    }

    context.SaveChanges();

}).DisableAntiforgery();

app.MapGet("/initusers", (ApplicationContext context) =>
{
    context.Orders.ExecuteDelete();
    context.Users.ExecuteDelete();
    context.ConfigSettings.ExecuteDelete();

    var user1 = new User
    {
        Name = "Петров Петр Петрович",
        Login = "PP@mail.ru",
        Password = "12345",
        Role = "user"
    };

    var user2 = new User
    {
        Name = "Васильев Василий Васильевич",
        Login = "VV@mail.ru",
        Password = "12345",
        Role = "user"
    };

    var user3 = new User
    {
        Name = "Тихонов Тихон Тихонович",
        Login = "TT@mail.ru",
        Password = "12345",
        Role = "admin"
    };

    context.Users.AddRange(user1, user2, user3);

    context.Orders.AddRange(
        new Order
        {
            CreatedDate = DateTime.Now,
            Description = "Запрос на подписание доверенности",
            EpguOrderId = 123456789,
            User = user1,
            ReceiverIdType = "snils",
            ReceiverId = "123 123 123 00"
        },
        new Order
        {
            CreatedDate = DateTime.Now.AddDays(-3),
            Description = "Запрос на подписание доверенности",
            EpguOrderId = 987654321,
            User = user2,
            ReceiverIdType = "oid",
            ReceiverId = "1000001234567"
        }
    );

    context.ConfigSettings.Add(new ConfigSettings
    {
        Mnemonics = "MNSV03",
        ServiceName = "Отправка документов на подпись в «Госключ»",
        OrgName = "ООО \"СИМЭНЕРГО\"",
        ServiceCode = "10000000374",
        TargetCode = "-10000000374",
        Region = "45000000000"

    }
    );

    context.SaveChanges();

    return Results.Ok("Пользователи и заказы успешно инициализированы и сохранены в базе данных.");
});

app.MapGet("/users", (ApplicationContext context) =>
{
    return context.Users.Select(u => new { u.Id, u.Name, u.Login, u.Password, u.Role });
});

app.Run();