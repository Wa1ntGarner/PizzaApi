namespace PizzaApi.Models;

/// <summary>
/// Основная сущность предметной области: пицца из меню пиццерии.
/// </summary>
public class Pizza
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DiameterCm { get; set; }

    public bool IsVegetarian { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
