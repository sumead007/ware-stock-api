using WareStockApi.Application.Users.Commands.BulkSetUserStatus;
using WareStockApi.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Users.Commands.BulkSetUserStatus;

public class BulkSetUserStatusCommandValidatorTests
{
    private BulkSetUserStatusCommandValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new BulkSetUserStatusCommandValidator();

    [TestCase(UserStatus.Active)]
    [TestCase(UserStatus.Inactive)]
    public async Task ShouldAcceptActiveOrInactive(UserStatus status)
    {
        var result = await _validator.ValidateAsync(new BulkSetUserStatusCommand(["id-1"], status));

        result.IsValid.ShouldBeTrue();
    }

    // The spec narrows bulk-status to active/inactive only, even though UserStatus has 3 values —
    // Suspended must stay rejected here even if someone widens the enum's Must() later.
    [TestCase(UserStatus.Suspended)]
    public async Task ShouldRejectStatusesOutsideTheBulkOperationScope(UserStatus status)
    {
        var result = await _validator.ValidateAsync(new BulkSetUserStatusCommand(["id-1"], status));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(BulkSetUserStatusCommand.Status));
    }

    [Test]
    public async Task ShouldRejectEmptyIds()
    {
        var result = await _validator.ValidateAsync(new BulkSetUserStatusCommand([], UserStatus.Active));

        result.IsValid.ShouldBeFalse();
    }
}
