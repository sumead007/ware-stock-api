using System.Reflection;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;
using WareStockApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WareStockApi.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();

    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    /// <summary>
    /// Not part of <see cref="IApplicationDbContext"/> — refresh tokens are an Identity/auth
    /// implementation detail (same category as <see cref="ApplicationUser"/> itself), accessed
    /// only from Infrastructure (<see cref="Identity.JwtTokenService"/>), never from Application.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
