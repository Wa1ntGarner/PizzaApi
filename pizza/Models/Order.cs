namespace PizzaApi.Models;

/// <summary>
/// Связанная сущность: заказ клиента пиццерии.
/// </summary>
public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.New;

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
