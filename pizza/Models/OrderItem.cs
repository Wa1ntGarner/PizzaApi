namespace PizzaApi.Models;

/// <summary>
/// Позиция заказа: конкретная пицца и её количество.
/// Цена фиксируется на момент оформления заказа.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int PizzaId { get; set; }

    public Pizza? Pizza { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
