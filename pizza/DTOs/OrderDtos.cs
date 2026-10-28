using System.ComponentModel.DataAnnotations;

namespace PizzaApi.DTOs;

/// <summary>
/// Заказ, возвращаемый клиенту API.
/// </summary>
public class OrderDto
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Позиция заказа в ответе API.
/// </summary>
public class OrderItemDto
{
    public int PizzaId { get; set; }

    public string PizzaName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}

/// <summary>
/// Данные для оформления заказа.
/// </summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Имя клиента обязательно.")]
    [MaxLength(100, ErrorMessage = "Имя не должно превышать 100 символов.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон клиента обязателен.")]
    [MaxLength(20, ErrorMessage = "Телефон не должен превышать 20 символов.")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Адрес доставки обязателен.")]
    [MaxLength(200, ErrorMessage = "Адрес не должен превышать 200 символов.")]
    public string Address { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы одну позицию.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Позиция создаваемого заказа.
/// </summary>
public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Идентификатор пиццы должен быть положительным.")]
    public int PizzaId { get; set; }

    [Range(1, 20, ErrorMessage = "Количество одной пиццы должно быть от 1 до 20.")]
    public int Quantity { get; set; }
}
