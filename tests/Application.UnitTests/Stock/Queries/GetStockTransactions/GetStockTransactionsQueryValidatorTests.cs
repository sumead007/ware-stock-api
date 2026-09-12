using WareStockApi.Application.Stock.Queries.GetStockTransactions;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Stock.Queries.GetStockTransactions;

public class GetStockTransactionsQueryValidatorTests
{
    private GetStockTransactionsQueryValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new GetStockTransactionsQueryValidator();

    [Test]
    public async Task ShouldAcceptWhenFromIsOnOrBeforeTo()
    {
        var query = new GetStockTransactionsQuery
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 1, 31)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptWhenFromEqualsTo()
    {
        var date = new DateOnly(2026, 1, 1);
        var query = new GetStockTransactionsQuery { From = date, To = date };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectWhenFromIsAfterTo()
    {
        var query = new GetStockTransactionsQuery
        {
            From = new DateOnly(2026, 2, 1),
            To = new DateOnly(2026, 1, 1)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetStockTransactionsQuery.From));
    }

    [Test]
    public async Task ShouldAcceptWhenOnlyOneBoundIsProvided()
    {
        var result = await _validator.ValidateAsync(new GetStockTransactionsQuery { From = new DateOnly(2026, 1, 1) });

        result.IsValid.ShouldBeTrue();
    }
}
