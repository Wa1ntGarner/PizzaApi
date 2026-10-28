namespace PizzaApi.Models;

/// <summary>
/// Статус заказа пиццерии.
/// </summary>
public enum OrderStatus
{
    New = 0,
    Cooking = 1,
    Delivering = 2,
    Completed = 3,
    Cancelled = 4
}
