using WareStockApi.Application.Users.Commands.CreateUser;
using NUnit.Framework;
using Shouldly;

namespace WareStockApi.Application.UnitTests.Users.Commands.CreateUser;

public class CreateUserCommandValidatorTests
{
    private CreateUserCommandValidator _validator = null!;

    [SetUp]
    public void Setup() => _validator = new CreateUserCommandValidator();

    private static CreateUserCommand ValidCommand() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Username = "ada",
        Email = "ada@example.com",
        PhoneNumber = "0800000000",
        Password = "password1",
        ConfirmPassword = "password1"
    };

    [Test]
    public async Task ShouldSucceedForAValidCommand()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.ShouldBeTrue();
    }

    [TestCase("short1")] // fewer than 8 characters
    [TestCase("alllowercase")] // no digit
    [TestCase("ALLUPPER1")] // no lowercase letter
    public async Task ShouldRejectPasswordsThatFailThePolicy(string password)
    {
        var command = ValidCommand() with { Password = password, ConfirmPassword = password };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.Password));
    }

    [Test]
    public async Task ShouldRejectWhenConfirmPasswordDoesNotMatchPassword()
    {
        var command = ValidCommand() with { ConfirmPassword = "different1" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserCommand.ConfirmPassword));
    }
}
