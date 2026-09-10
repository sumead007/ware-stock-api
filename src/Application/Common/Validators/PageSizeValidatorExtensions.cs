namespace WareStockApi.Application.Common.Validators;

/// <summary>
/// FluentValidation extension for the shared <c>pageSize</c> query parameter, which the OpenAPI
/// spec restricts to one of a fixed set of values (10, 20, 30, 40, 50).
/// </summary>
public static class PageSizeValidatorExtensions
{
    public static readonly int[] AllowedPageSizes = [10, 20, 30, 40, 50];

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder.Must(pageSize => AllowedPageSizes.Contains(pageSize))
            .WithMessage($"'{{PropertyName}}' must be one of: {string.Join(", ", AllowedPageSizes)}.");
}
