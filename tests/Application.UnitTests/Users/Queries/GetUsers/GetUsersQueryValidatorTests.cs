using WareStockApi.Application.Users.Queries.GetUsers;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Users.Queries.GetUsers;

public class GetUsersQueryValidatorTests
{
    private GetUsersQueryValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new GetUsersQueryValidator();

    [Test]
    public async Task ShouldAcceptWhenFromIsOnOrBeforeTo()
    {
        var query = new GetUsersQuery
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 1, 31)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectWhenFromIsAfterTo()
    {
        var query = new GetUsersQuery
        {
            From = new DateOnly(2026, 2, 1),
            To = new DateOnly(2026, 1, 1)
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetUsersQuery.From));
    }
}
