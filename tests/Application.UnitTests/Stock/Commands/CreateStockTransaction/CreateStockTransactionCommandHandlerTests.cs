using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Stock.Commands.CreateStockTransaction;
using WareStockApi.Application.UnitTests.Common;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;
using WareStockApi.Infrastructure.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;

namespace WareStockApi.Application.UnitTests.Stock.Commands.CreateStockTransaction;

/// <summary>
/// Covers the most important business rule in the system: a withdraw transaction may never take
/// <see cref="Product.Quantity"/> below zero, and every recorded transaction must adjust the
/// product's quantity as a side effect. This lives in the Application-layer handler (not the Web
/// endpoint) — see <c>src/Web/Endpoints/StockTransactions.cs</c>, which does nothing but dispatch.
/// </summary>
public class CreateStockTransactionCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private CreateStockTransactionCommandHandler _handler = null!;
    private Product _product = null!;

    [SetUp]
    public async Task Setup()
    {
        _context = TestDbContextFactory.Create();

        using var loggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var configuration = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(IApplicationDbContext).Assembly),
            loggerFactory: loggerFactory);
        var mapper = configuration.CreateMapper();

        _product = new Product
        {
            Sku = "SKU-001",
            Name = "Widget",
            Category = "General",
            Unit = "pcs",
            Quantity = 10,
            MinStock = 1,
            Location = "A1"
        };
        _context.Products.Add(_product);
        await _context.SaveChangesAsync(CancellationToken.None);

        _handler = new CreateStockTransactionCommandHandler(_context, mapper);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private CreateStockTransactionCommand Command(TransactionType type, decimal quantity, string? productId = null) => new()
    {
        Type = type,
        ProductId = productId ?? _product.Id,
        Quantity = quantity,
        Date = DateOnly.FromDateTime(DateTime.UtcNow),
        Counterparty = "Supplier Co",
        PerformedBy = "tester"
    };

    [Test]
    public async Task ShouldIncreaseProductQuantityOnReceive()
    {
        await _handler.Handle(Command(TransactionType.Receive, 5), CancellationToken.None);

        _product.Quantity.ShouldBe(15);
    }

    [Test]
    public async Task ShouldDecreaseProductQuantityOnWithdraw()
    {
        await _handler.Handle(Command(TransactionType.Withdraw, 4), CancellationToken.None);

        _product.Quantity.ShouldBe(6);
    }

    [Test]
    public async Task ShouldAllowAWithdrawThatExactlyExhaustsStock()
    {
        await _handler.Handle(Command(TransactionType.Withdraw, 10), CancellationToken.None);

        _product.Quantity.ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectAWithdrawThatExceedsCurrentStock()
    {
        await Should.ThrowAsync<ValidationException>(() => _handler.Handle(Command(TransactionType.Withdraw, 11), CancellationToken.None));

        _product.Quantity.ShouldBe(10, "a rejected withdraw must not mutate stock on hand");
    }

    [Test]
    public async Task ShouldRejectAnUnknownProduct()
    {
        await Should.ThrowAsync<ValidationException>(() =>
            _handler.Handle(Command(TransactionType.Receive, 1, productId: "does-not-exist"), CancellationToken.None));
    }
}
