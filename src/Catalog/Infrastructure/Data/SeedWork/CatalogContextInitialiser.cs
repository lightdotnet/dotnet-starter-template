using Microsoft.Extensions.Logging;
using Monolith.Domain.Categories;
using Monolith.Domain.Products;

namespace Monolith.Infrastructure.Data.SeedWork;

public class CatalogContextInitialiser(CatalogContext context, ILogger<CatalogContextInitialiser> logger)
{
    public virtual async Task InitialiseAsync()
    {
        logger.LogInformation("Seeding...");

        try
        {
            if (context.Database.GetMigrations().Any())
            {
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();

                    var dbName = context.Database.GetDbConnection().Database;

                    logger.LogInformation("Database {name} initialized", dbName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        try
        {
            if (await context.Database.CanConnectAsync())
            {
                await SeedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        var drinkCategory = Category.Create("Drinks");
        var foodCategory = Category.Create("Foods");

        var categories = new Category[]
        {
            drinkCategory,
            foodCategory,
            Category.Create("Gas", drinkCategory.Id),
            Category.Create("Alcohol", drinkCategory.Id),
            Category.Create("Bread", foodCategory.Id),
            Category.Create("Rice", foodCategory.Id),
        };

        await context.Categories.AddRangeAsync(categories);

        var productImages = new List<string>()
        {
            "https://images.pexels.com/photos/90946/pexels-photo-90946.jpeg",
            "https://images.pexels.com/photos/335257/pexels-photo-335257.jpeg",
            "https://images.pexels.com/photos/3907507/pexels-photo-3907507.jpeg"
        };

        var products = new List<Product>();

        for (var i = 1; i <= 100; i++)
        {
            var random = new Random();

            var product = Product.Create("1", categories[random.Next(2, 5)].Id, $"Product Name {i}", $"Product Description {i}");
            product.ImageUrl = productImages.First();
            product.UpdateCode($"{100000 + i}");
            product.UpdateImages(productImages);

            products.Add(product);
        }

        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}
