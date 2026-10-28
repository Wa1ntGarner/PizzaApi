using Microsoft.EntityFrameworkCore;
using PizzaApi.Data;
using PizzaApi.DTOs;
using PizzaApi.Models;
using PizzaApi.Services;

namespace PizzaApi.Tests;

public class OrderServiceTests
{
    private const string CustomerName = "Мария Иванова";
    private const string CustomerPhone = "+7-900-111-22-33";
    private const string Address = "г. Москва, ул. Тверская, д. 1";

    [Fact]
    public async Task CreateOrderAsync_BelowFreeDeliveryThreshold_AddsDeliveryFee()
    {
        await using var context = TestDbFactory.CreateContext();
        var pizza = await AddPizzaAsync(context, price: 450m);
        var service = new OrderService(context);

        var order = await service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address,
            Items = { new CreateOrderItemRequest { PizzaId = pizza.Id, Quantity = 1 } }
        });

        Assert.Equal(450m, order.TotalPrice - OrderService.DeliveryFee);
        Assert.Equal(450m + OrderService.DeliveryFee, order.TotalPrice);
        Assert.Equal(OrderStatus.New.ToString(), order.Status);
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task CreateOrderAsync_AtFreeDeliveryThreshold_DoesNotAddDeliveryFee()
    {
        await using var context = TestDbFactory.CreateContext();
        var pizza = await AddPizzaAsync(context, price: 500m);
        var service = new OrderService(context);

        var order = await service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address,
            Items = { new CreateOrderItemRequest { PizzaId = pizza.Id, Quantity = 2 } }
        });

        Assert.Equal(OrderService.FreeDeliveryThreshold, order.TotalPrice);
        Assert.Equal(1000m, order.Items.Single().LineTotal);
    }

    [Fact]
    public async Task CreateOrderAsync_FixesPizzaPriceAtPurchase()
    {
        await using var context = TestDbFactory.CreateContext();
        var pizza = await AddPizzaAsync(context, price: 450m);
        var service = new OrderService(context);

        var order = await service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address,
            Items = { new CreateOrderItemRequest { PizzaId = pizza.Id, Quantity = 2 } }
        });

        pizza.Price = 900m;
        await context.SaveChangesAsync();

        var saved = await service.GetOrderAsync(order.Id);

        Assert.NotNull(saved);
        Assert.Equal(450m, saved!.Items.Single().UnitPrice);
        Assert.Equal(450m * 2 + OrderService.DeliveryFee, saved.TotalPrice);
    }

    [Fact]
    public async Task CreateOrderAsync_WithUnavailablePizza_ThrowsInvalidOperationException()
    {
        await using var context = TestDbFactory.CreateContext();
        var pizza = await AddPizzaAsync(context, price: 600m, isAvailable: false);
        var service = new OrderService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address,
            Items = { new CreateOrderItemRequest { PizzaId = pizza.Id, Quantity = 1 } }
        }));

        Assert.Contains("недоступна", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_WithUnknownPizza_ThrowsInvalidOperationException()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new OrderService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address,
            Items = { new CreateOrderItemRequest { PizzaId = 12_345, Quantity = 1 } }
        }));
    }

    [Fact]
    public async Task CreateOrderAsync_WithoutItems_ThrowsArgumentException()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new OrderService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone,
            Address = Address
        }));

        Assert.Contains("хотя бы одну позицию", exception.Message);
    }

    private static async Task<Pizza> AddPizzaAsync(AppDbContext context, decimal price, bool isAvailable = true)
    {
        var pizza = new Pizza
        {
            Name = $"Тестовая {Guid.NewGuid():N}",
            Description = "Пицца для тестов",
            Price = price,
            DiameterCm = 30,
            IsVegetarian = false,
            IsAvailable = isAvailable,
            CreatedAt = DateTime.UtcNow
        };

        context.Pizzas.Add(pizza);
        await context.SaveChangesAsync();

        return pizza;
    }
}
