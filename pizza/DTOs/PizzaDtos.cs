using System.ComponentModel.DataAnnotations;

namespace PizzaApi.DTOs;

/// <summary>
/// Пицца, возвращаемая клиенту API.
/// </summary>
public class PizzaDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DiameterCm { get; set; }

    public bool IsVegetarian { get; set; }

    public bool IsAvailable { get; set; }
}

/// <summary>
/// Данные для добавления новой пиццы в меню.
/// </summary>
public class CreatePizzaRequest
{
    [Required(ErrorMessage = "Название пиццы обязательно.")]
    [MaxLength(80, ErrorMessage = "Название не должно превышать 80 символов.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400, ErrorMessage = "Описание не должно превышать 400 символов.")]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DiameterCm { get; set; }

    public bool IsVegetarian { get; set; }
}
