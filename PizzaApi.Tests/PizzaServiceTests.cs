using Microsoft.EntityFrameworkCore;
using PizzaApi.DTOs;
using PizzaApi.Services;

namespace PizzaApi.Tests;

public class PizzaServiceTests
{
    [Fact]
    public async Task CreatePizzaAsync_WithZeroPrice_ThrowsArgumentException()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var request = new CreatePizzaRequest
        {
            Name = "Нулевая цена",
            Description = "Проверка валидации",
            Price = 0m,
            DiameterCm = 30
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreatePizzaAsync(request));

        Assert.Contains("Цена", exception.Message);
    }

    [Fact]
    public async Task CreatePizzaAsync_WithDiameterOutOfRange_ThrowsArgumentException()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var request = new CreatePizzaRequest
        {
            Name = "Слишком большая",
            Price = 500m,
            DiameterCm = PizzaService.MaxDiameterCm + 1
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePizzaAsync(request));
    }

    [Fact]
    public async Task CreatePizzaAsync_WithValidRequest_AddsPizzaToMenu()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var request = new CreatePizzaRequest
        {
            Name = "  Фирменная  ",
            Description = "Секретный рецепт",
            Price = 890m,
            DiameterCm = 35,
            IsVegetarian = false
        };

        var created = await service.CreatePizzaAsync(request);

        Assert.True(created.Id > 0);
        Assert.Equal("Фирменная", created.Name);
        Assert.Equal(890m, created.Price);
        Assert.True(created.IsAvailable);
        Assert.True(await context.Pizzas.AnyAsync(pizza => pizza.Id == created.Id));
    }

    [Fact]
    public async Task CreatePizzaAsync_WithDuplicateName_ThrowsArgumentException()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var request = new CreatePizzaRequest
        {
            Name = "Дубль",
            Price = 500m,
            DiameterCm = 30
        };

        await service.CreatePizzaAsync(request);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePizzaAsync(request));
    }

    [Fact]
    public async Task GetPizzasAsync_OnlyAvailable_SkipsUnavailablePizzas()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var available = await service.CreatePizzaAsync(new CreatePizzaRequest
        {
            Name = "Доступная",
            Price = 400m,
            DiameterCm = 25
        });

        var hidden = await service.CreatePizzaAsync(new CreatePizzaRequest
        {
            Name = "Скрытая",
            Price = 700m,
            DiameterCm = 40
        });

        await service.SetAvailabilityAsync(hidden.Id, false);

        var menu = await service.GetPizzasAsync(onlyAvailable: true);
        var fullMenu = await service.GetPizzasAsync(onlyAvailable: false);

        Assert.Contains(menu, pizza => pizza.Id == available.Id);
        Assert.DoesNotContain(menu, pizza => pizza.Id == hidden.Id);
        Assert.All(menu, pizza => Assert.True(pizza.IsAvailable));
        Assert.Contains(fullMenu, pizza => pizza.Id == hidden.Id);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ForUnknownPizza_ReturnsFalse()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new PizzaService(context);

        var updated = await service.SetAvailabilityAsync(id: 999_999, isAvailable: false);

        Assert.False(updated);
    }
}
