using Microsoft.EntityFrameworkCore;
using PizzaApi.Data;
using PizzaApi.DTOs;
using PizzaApi.Models;

namespace PizzaApi.Services;

/// <summary>
/// Операции над меню пиццерии.
/// </summary>
public interface IPizzaService
{
    Task<IReadOnlyList<PizzaDto>> GetPizzasAsync(bool onlyAvailable, CancellationToken cancellationToken = default);

    Task<PizzaDto?> GetPizzaAsync(int id, CancellationToken cancellationToken = default);

    Task<PizzaDto> CreatePizzaAsync(CreatePizzaRequest request, CancellationToken cancellationToken = default);

    Task<bool> SetAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken = default);
}

/// <summary>
/// Логика работы с меню: чтение, добавление и управление доступностью пиццы.
/// </summary>
public class PizzaService : IPizzaService
{
    /// <summary>Минимальная допустимая цена пиццы.</summary>
    public const decimal MinPrice = 1m;

    /// <summary>Минимальный допустимый диаметр пиццы в сантиметрах.</summary>
    public const int MinDiameterCm = 20;

    /// <summary>Максимальный допустимый диаметр пиццы в сантиметрах.</summary>
    public const int MaxDiameterCm = 45;

    private const int MaxNameLength = 80;

    private readonly AppDbContext _db;

    public PizzaService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<PizzaDto>> GetPizzasAsync(bool onlyAvailable, CancellationToken cancellationToken = default)
    {
        var query = _db.Pizzas.AsNoTracking();

        if (onlyAvailable)
        {
            query = query.Where(pizza => pizza.IsAvailable);
        }

        var pizzas = await query
            .OrderBy(pizza => pizza.Name)
            .ToListAsync(cancellationToken);

        return pizzas.Select(ToDto).ToList();
    }

    public async Task<PizzaDto?> GetPizzaAsync(int id, CancellationToken cancellationToken = default)
    {
        var pizza = await _db.Pizzas
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return pizza is null ? null : ToDto(pizza);
    }

    public async Task<PizzaDto> CreatePizzaAsync(CreatePizzaRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            throw new ArgumentException("Название пиццы обязательно.", nameof(request));
        }

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Название пиццы не должно превышать {MaxNameLength} символов.", nameof(request));
        }

        if (request.Price < MinPrice)
        {
            throw new ArgumentException("Цена пиццы должна быть больше нуля.", nameof(request));
        }

        if (request.DiameterCm < MinDiameterCm || request.DiameterCm > MaxDiameterCm)
        {
            throw new ArgumentException(
                $"Диаметр пиццы должен быть от {MinDiameterCm} до {MaxDiameterCm} см.",
                nameof(request));
        }

        var nameTaken = await _db.Pizzas.AnyAsync(pizza => pizza.Name == name, cancellationToken);
        if (nameTaken)
        {
            throw new ArgumentException($"Пицца с названием «{name}» уже есть в меню.", nameof(request));
        }

        var newPizza = new Pizza
        {
            Name = name,
            Description = request.Description?.Trim() ?? string.Empty,
            Price = request.Price,
            DiameterCm = request.DiameterCm,
            IsVegetarian = request.IsVegetarian,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Pizzas.Add(newPizza);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(newPizza);
    }

    public async Task<bool> SetAvailabilityAsync(int id, bool isAvailable, CancellationToken cancellationToken = default)
    {
        var pizza = await _db.Pizzas.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (pizza is null)
        {
            return false;
        }

        pizza.IsAvailable = isAvailable;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static PizzaDto ToDto(Pizza pizza) => new()
    {
        Id = pizza.Id,
        Name = pizza.Name,
        Description = pizza.Description,
        Price = pizza.Price,
        DiameterCm = pizza.DiameterCm,
        IsVegetarian = pizza.IsVegetarian,
        IsAvailable = pizza.IsAvailable
    };
}
