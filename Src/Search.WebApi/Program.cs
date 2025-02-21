using System.Text.Json;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Search.WebApi.Domain.Products.Entities;
using Search.WebApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.MaxDepth = 5;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.IgnoreNullValues = true;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSingleton<ProductService>();
builder.WebHost.UseUrls("http://192.168.242.11:7000/");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();




#region Endpoints

app.MapGet("/products", async (ProductService service) =>
{
    return Results.Ok(await service.GetAllAsync());
});

app.MapGet("/products/{id}", async (int id, ProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (List<Product> products, ProductService service) =>
{
    products.ForEach(x=> _ = service.CreateAsync(x));
    return Results.Created();
});

app.MapPut("/products/{id}", async (int id, Product updatedProduct, ProductService service) =>
{
    var existingProduct = await service.GetByIdAsync(id);
    if (existingProduct is null)
        return Results.NotFound();

    await service.UpdateAsync(id, updatedProduct);
    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, ProductService service) =>
{
    var existingProduct = await service.GetByIdAsync(id);
    if (existingProduct is null)
        return Results.NotFound();

    await service.DeleteAsync(id);
    return Results.NoContent();
});

#endregion



app.Run();
