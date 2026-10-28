using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PizzaApi.Data;

/// <summary>
/// Отдельный контекст для данных ASP.NET Core Identity:
/// пользователи, роли, токены и утверждения.
/// </summary>
public class AuthDbContext : IdentityDbContext<IdentityUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }
}
