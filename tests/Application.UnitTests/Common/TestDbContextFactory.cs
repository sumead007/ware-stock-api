using Microsoft.EntityFrameworkCore;
using WareStockApi.Infrastructure.Data;

namespace WareStockApi.Application.UnitTests.Common;

/// <summary>
/// Backs <see cref="Interfaces.IApplicationDbContext"/> with EF Core's InMemory provider for unit
/// tests that need a real, queryable <see cref="ApplicationDbContext"/> (e.g. async validator
/// rules or handlers that read/write through it) without a real SQL Server.
/// </summary>
internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
