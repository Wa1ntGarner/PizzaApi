using Microsoft.EntityFrameworkCore;
using PizzaApi.Models;

namespace PizzaApi.Data;

/// <summary>
/// Контекст данных предметной области пиццерии: пиццы, заказы и позиции заказов.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pizza> Pizzas => Set<Pizza>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pizza>(entity =>
        {
            entity.ToTable("Pizzas");
            entity.Property(pizza => pizza.Name).IsRequired().HasMaxLength(80);
            entity.Property(pizza => pizza.Description).HasMaxLength(400);
            entity.Property(pizza => pizza.Price).HasPrecision(9, 2);
            entity.HasIndex(pizza => pizza.Name).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(order => order.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(order => order.CustomerPhone).IsRequired().HasMaxLength(20);
            entity.Property(order => order.Address).IsRequired().HasMaxLength(200);
            entity.Property(order => order.TotalPrice).HasPrecision(9, 2);
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.Property(item => item.UnitPrice).HasPrecision(9, 2);
            entity.Ignore(item => item.LineTotal);

            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Pizza)
                .WithMany(pizza => pizza.OrderItems)
                .HasForeignKey(item => item.PizzaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Pizza>().HasData(
            new Pizza
            {
                Id = 1,
                Name = "Маргарита",
                Description = "Томатный соус, моцарелла, базилик",
                Price = 450m,
                DiameterCm = 30,
                IsVegetarian = true,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 2,
                Name = "Пепперони",
                Description = "Томатный соус, моцарелла, острая пепперони",
                Price = 590m,
                DiameterCm = 30,
                IsVegetarian = false,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 3,
                Name = "Четыре сыра",
                Description = "Моцарелла, дорблю, чеддер, пармезан",
                Price = 620m,
                DiameterCm = 30,
                IsVegetarian = true,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 4,
                Name = "Мясная",
                Description = "Ветчина, бекон, колбаса, говядина",
                Price = 720m,
                DiameterCm = 40,
                IsVegetarian = false,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 5,
                Name = "Гавайская",
                Description = "Курица, ананас, моцарелла, соус",
                Price = 640m,
                DiameterCm = 35,
                IsVegetarian = false,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 6,
                Name = "Овощная",
                Description = "Перец, шампиньоны, оливки, томаты черри",
                Price = 560m,
                DiameterCm = 30,
                IsVegetarian = true,
                IsAvailable = true,
                CreatedAt = createdAt
            },
            new Pizza
            {
                Id = 7,
                Name = "Дьябло",
                Description = "Острый соус, халапеньо, пепперони",
                Price = 680m,
                DiameterCm = 35,
                IsVegetarian = false,
                IsAvailable = false,
                CreatedAt = createdAt
            });

        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = 1,
                CustomerName = "Иван Петров",
                CustomerPhone = "+7-900-000-00-01",
                Address = "г. Москва, ул. Пушкина, д. 10, кв. 5",
                Status = OrderStatus.Completed,
                TotalPrice = 1040m,
                CreatedAt = createdAt.AddHours(3)
            });

        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem
            {
                Id = 1,
                OrderId = 1,
                PizzaId = 1,
                Quantity = 1,
                UnitPrice = 450m
            },
            new OrderItem
            {
                Id = 2,
                OrderId = 1,
                PizzaId = 2,
                Quantity = 1,
                UnitPrice = 590m
            });
    }
}
