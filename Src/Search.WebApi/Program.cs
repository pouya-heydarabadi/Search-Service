using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;
using Scalar.AspNetCore;
using Search.WebApi.Domain.Products.Entities;
using Search.WebApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ProductService>();
builder.WebHost.UseUrls("http://192.168.242.11:7000/");

var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
    .CertificateFingerprint(builder.Configuration["Elasticsearch:Fingerprint"])
    .Authentication(new BasicAuthentication( builder.Configuration["Elasticsearch:UserName"],builder.Configuration["Elasticsearch:Password"] ));
var client = new ElasticsearchClient(settings);

builder.Services.AddSingleton<ElasticsearchClient>(options =>
{
  return  client = new ElasticsearchClient(settings);
});

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

app.MapPost("/products", async (List<Product> products, ElasticsearchClient _elasticsearch ,ProductService service, CancellationToken CancellationToken) =>
{
    await Task.WhenAll(products.Select(product => service.CreateAsync(product)));

    var bulkRequest = new BulkRequest("products")
    {
        Operations = new BulkOperationsCollection(products.Select(p => new BulkIndexOperation<Product>(p)).ToList())
    };

    var response = await _elasticsearch.BulkAsync(bulkRequest, CancellationToken);
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

#region Search

app.MapGet("/Products/FuzzySearch/", async (ElasticsearchClient _elasticsearch, string query) =>
{
    var result = await _elasticsearch.SearchAsync<Product>(x =>
        x.Index("products")
            .Query(q => q.Fuzzy(f => f.Field(field => field.Name)
                .Value(query)
            )));

    if (result.IsValidResponse)
    {
        return Results.Ok(result.Documents.ToList());
    }
    return Results.BadRequest();
});




#endregion



app.Run();
