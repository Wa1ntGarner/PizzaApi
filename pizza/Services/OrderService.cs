using Microsoft.EntityFrameworkCore;
using PizzaApi.Data;
using PizzaApi.DTOs;
using PizzaApi.Models;

namespace PizzaApi.Services;

/// <summary>
/// Операции над заказами пиццерии.
/// </summary>
public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default);

    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Бизнес-правила оформления заказов: проверка позиций,
/// фиксация цены и расчёт стоимости доставки.
/// </summary>
public class OrderService : IOrderService
{
    /// <summary>Стоимость доставки, если сумма заказа меньше порога бесплатной доставки.</summary>
    public const decimal DeliveryFee = 149m;

    /// <summary>Сумма заказа, начиная с которой доставка бесплатна.</summary>
    public const decimal FreeDeliveryThreshold = 1000m;

    /// <summary>Максимальное количество одной пиццы в позиции заказа.</summary>
    public const int MaxQuantityPerItem = 20;

    private const int MaxCustomerNameLength = 100;
    private const int MaxPhoneLength = 20;
    private const int MaxAddressLength = 200;

    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <summary>
    /// Стоимость доставки зависит от суммы позиций заказа.
    /// </summary>
    public static decimal CalculateDeliveryFee(decimal subtotal)
        => subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await QueryWithItems()
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await QueryWithItems()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return order is null ? null : ToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("Заказ должен содержать хотя бы одну позицию.", nameof(request));
        }

        var customerName = request.CustomerName?.Trim() ?? string.Empty;
        var customerPhone = request.CustomerPhone?.Trim() ?? string.Empty;
        var address = request.Address?.Trim() ?? string.Empty;

        if (customerName.Length == 0 || customerName.Length > MaxCustomerNameLength)
        {
            throw new ArgumentException(
                $"Имя клиента обязательно и не должно превышать {MaxCustomerNameLength} символов.",
                nameof(request));
        }

        if (customerPhone.Length == 0 || customerPhone.Length > MaxPhoneLength)
        {
            throw new ArgumentException(
                $"Телефон клиента обязателен и не должен превышать {MaxPhoneLength} символов.",
                nameof(request));
        }

        if (address.Length == 0 || address.Length > MaxAddressLength)
        {
            throw new ArgumentException(
                $"Адрес доставки обязателен и не должен превышать {MaxAddressLength} символов.",
                nameof(request));
        }

        var order = new Order
        {
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            Address = address,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var requestedItem in request.Items)
        {
            if (requestedItem.Quantity < 1 || requestedItem.Quantity > MaxQuantityPerItem)
            {
                throw new ArgumentException(
                    $"Количество одной пиццы должно быть от 1 до {MaxQuantityPerItem}.",
                    nameof(request));
            }

            var pizza = await _db.Pizzas
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == requestedItem.PizzaId, cancellationToken);

            if (pizza is null)
            {
                throw new InvalidOperationException($"Пицца с идентификатором {requestedItem.PizzaId} не найдена.");
            }

            if (!pizza.IsAvailable)
            {
                throw new InvalidOperationException($"Пицца «{pizza.Name}» недоступна для заказа.");
            }

            order.Items.Add(new OrderItem
            {
                PizzaId = pizza.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = pizza.Price
            });
        }

        var subtotal = order.Items.Sum(item => item.UnitPrice * item.Quantity);
        order.TotalPrice = subtotal + CalculateDeliveryFee(subtotal);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetOrderAsync(order.Id, cancellationToken) ?? ToDto(order);
    }

    private IQueryable<Order> QueryWithItems() => _db.Orders
        .AsNoTracking()
        .Include(order => order.Items)
        .ThenInclude(item => item.Pizza);

    private static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerName = order.CustomerName,
        CustomerPhone = order.CustomerPhone,
        Address = order.Address,
        Status = order.Status.ToString(),
        TotalPrice = order.TotalPrice,
        CreatedAt = order.CreatedAt,
        Items = order.Items
            .Select(item => new OrderItemDto
            {
                PizzaId = item.PizzaId,
                PizzaName = item.Pizza?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.UnitPrice * item.Quantity
            })
            .ToList()
    };
}
