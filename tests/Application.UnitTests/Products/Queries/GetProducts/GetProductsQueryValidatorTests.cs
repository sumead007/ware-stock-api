using WareStockApi.Application.Products.Queries.GetProducts;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Products.Queries.GetProducts;

/// <summary>
/// Exercises the shared <see cref="Validators.PageSizeValidatorExtensions"/> rule through one of
/// its four real callers — Products/Tasks/Users/Stock all wire it the same way, so this one test
/// class covers the shared behaviour.
/// </summary>
public class GetProductsQueryValidatorTests
{
    private GetProductsQueryValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new GetProductsQueryValidator();

    [TestCase(10)]
    [TestCase(20)]
    [TestCase(30)]
    [TestCase(40)]
    [TestCase(50)]
    public async Task ShouldAcceptTheAllowedPageSizes(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetProductsQuery { PageSize = pageSize });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase(0)]
    [TestCase(15)]
    [TestCase(100)]
    public async Task ShouldRejectPageSizesOutsideTheAllowedSet(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetProductsQuery { PageSize = pageSize });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProductsQuery.PageSize));
    }

    [Test]
    public async Task ShouldRejectPageLessThanOne()
    {
        var result = await _validator.ValidateAsync(new GetProductsQuery { Page = 0 });

        result.IsValid.ShouldBeFalse();
    }
}
