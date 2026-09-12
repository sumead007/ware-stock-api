using WareStockApi.Application.Reports.Queries.GetStockLevelsReport;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Reports.Queries.GetStockLevelsReport;

public class GetStockLevelsReportQueryValidatorTests
{
    private GetStockLevelsReportQueryValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new GetStockLevelsReportQueryValidator();

    [TestCase(10)]
    [TestCase(20)]
    [TestCase(30)]
    [TestCase(40)]
    [TestCase(50)]
    public async Task ShouldAcceptTheAllowedPageSizes(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetStockLevelsReportQuery { PageSize = pageSize });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase(0)]
    [TestCase(15)]
    [TestCase(100)]
    public async Task ShouldRejectPageSizesOutsideTheAllowedSet(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetStockLevelsReportQuery { PageSize = pageSize });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetStockLevelsReportQuery.PageSize));
    }

    [Test]
    public async Task ShouldRejectPageLessThanOne()
    {
        var result = await _validator.ValidateAsync(new GetStockLevelsReportQuery { Page = 0 });

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldAcceptWhenUpdatedFromIsOnOrBeforeUpdatedTo()
    {
        var query = new GetStockLevelsReportQuery
        {
            UpdatedFrom = new DateOnly(2026, 1, 1),
            UpdatedTo = new DateOnly(2026, 1, 31)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectWhenUpdatedFromIsAfterUpdatedTo()
    {
        var query = new GetStockLevelsReportQuery
        {
            UpdatedFrom = new DateOnly(2026, 2, 1),
            UpdatedTo = new DateOnly(2026, 1, 1)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetStockLevelsReportQuery.UpdatedFrom));
    }
}
