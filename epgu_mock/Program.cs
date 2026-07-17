
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();


app.MapPost("/api/gusmev/order", ([FromBody] DTO.Meta meta) =>
{
    Console.WriteLine(meta.region);
    Console.WriteLine(meta.serviceCode);
    Console.WriteLine(meta.targetCode);

    // обработка

    return Results.Json(new
    {
        orderId = "2058854583"
    });
});

app.MapPost("/api/gusmev/push", ([FromForm] DTO.Meta meta, IFormFile file) =>
{
    if (file != null && file.Length > 0) Console.WriteLine("Файл получен");

    // обработка

    return Results.Json(new
    {
        orderId = "2058854583"
    });

}).DisableAntiforgery();

app.Run();