using WareStockApi.Application.Products.Commands.CreateProduct;
using WareStockApi.Application.UnitTests.Common;
using WareStockApi.Domain.Entities;
using WareStockApi.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Products.Commands.CreateProduct;

public class CreateProductCommandValidatorTests
{
    private ApplicationDbContext _context = null!;
    private CreateProductCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
        _validator = new CreateProductCommandValidator(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private static CreateProductCommand ValidCommand() => new()
    {
        Sku = "SKU-001",
        Name = "Widget",
        Category = "General",
        Unit = "pcs",
        Quantity = 10,
        MinStock = 1,
        Location = "A1"
    };

    [Test]
    public async Task ShouldSucceedForAValidCommand()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectADuplicateSku()
    {
        _context.Products.Add(new Product
        {
            Sku = "SKU-001",
            Name = "Existing",
            Category = "General",
            Unit = "pcs",
            Location = "A1"
        });
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Sku) && e.ErrorCode == "Unique");
    }

    [Test]
    public async Task ShouldRejectNegativeQuantity()
    {
        var command = ValidCommand() with { Quantity = -1 };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Quantity));
    }

    [Test]
    public async Task ShouldRejectNegativeMinStock()
    {
        var command = ValidCommand() with { MinStock = -1 };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.MinStock));
    }

    [Test]
    public async Task ShouldRejectNegativeCostPrice()
    {
        var command = ValidCommand() with { CostPrice = -5 };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.CostPrice));
    }

    [Test]
    public async Task ShouldAllowANullCostPrice()
    {
        var command = ValidCommand() with { CostPrice = null };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }
}
