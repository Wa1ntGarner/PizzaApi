using Microsoft.EntityFrameworkCore;
using PizzaApi.Data;

namespace PizzaApi.Tests;

/// <summary>
/// Создаёт AppDbContext на провайдере EF Core InMemory,
/// чтобы тесты не зависели от реального SQL Server.
/// </summary>
internal static class TestDbFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"pizza-tests-{Guid.NewGuid():N}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
