using MongoDB.Driver;
using Search.WebApi.Domain.Products.Entities;

namespace Search.WebApi.Infrastructure.Services;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _products = database.GetCollection<Product>(config["MongoDB:CollectionName"]);
    }

    public async Task<List<Product>> GetAllAsync() => 
        await _products.Find(product => true).ToListAsync();

    public async Task<Product> GetByIdAsync(int id) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product product) =>
        await _products.InsertOneAsync(product);

    public async Task UpdateAsync(int id, Product updatedProduct) =>
        await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);

    public async Task DeleteAsync(int id) =>
        await _products.DeleteOneAsync(p => p.Id == id);
}