using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaApi.DTOs;
using PizzaApi.Services;

namespace PizzaApi.Controllers;

/// <summary>
/// Меню пиццерии. Чтение доступно всем, изменение — только аутентифицированным пользователям.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PizzasController : ControllerBase
{
    private readonly IPizzaService _pizzaService;
    private readonly ILogger<PizzasController> _logger;

    public PizzasController(IPizzaService pizzaService, ILogger<PizzasController> logger)
    {
        _pizzaService = pizzaService ?? throw new ArgumentNullException(nameof(pizzaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Возвращает меню. По умолчанию только доступные для заказа пиццы.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PizzaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PizzaDto>>> GetMenu(
        [FromQuery] bool onlyAvailable = true,
        CancellationToken cancellationToken = default)
    {
        var pizzas = await _pizzaService.GetPizzasAsync(onlyAvailable, cancellationToken);
        return Ok(pizzas);
    }

    /// <summary>
    /// Возвращает одну пиццу по идентификатору.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PizzaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var pizza = await _pizzaService.GetPizzaAsync(id, cancellationToken);
        if (pizza is null)
        {
            return NotFound(new { message = $"Пицца с идентификатором {id} не найдена." });
        }

        return Ok(pizza);
    }

    /// <summary>
    /// Добавляет новую пиццу в меню. Требует аутентификации.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PizzaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PizzaDto>> Create(
        [FromBody] CreatePizzaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pizza = await _pizzaService.CreatePizzaAsync(request, cancellationToken);
            _logger.LogInformation("В меню добавлена пицца {PizzaName} (Id={PizzaId}).", pizza.Name, pizza.Id);

            return CreatedAtAction(nameof(GetById), new { id = pizza.Id }, pizza);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Включает или выключает пиццу в меню. Требует аутентификации.
    /// </summary>
    [HttpPatch("{id:int}/availability")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        int id,
        [FromQuery] bool isAvailable,
        CancellationToken cancellationToken)
    {
        var updated = await _pizzaService.SetAvailabilityAsync(id, isAvailable, cancellationToken);
        if (!updated)
        {
            return NotFound(new { message = $"Пицца с идентификатором {id} не найдена." });
        }

        return NoContent();
    }
}
