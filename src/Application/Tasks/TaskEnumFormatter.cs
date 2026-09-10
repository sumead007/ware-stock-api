using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks;

/// <summary>
/// Converts task enums to/from the exact lowercase string values used by the OpenAPI spec
/// (including the <c>'in progress'</c> value, which contains a space and therefore can't rely on
/// the default case-insensitive <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/> query
/// string binding minimal APIs use for the other single-word enum values).
/// </summary>
public static class TaskEnumFormatter
{
    public static string ToCsvValue(WorkTaskStatus status) =>
        status == WorkTaskStatus.InProgress ? "in progress" : status.ToString().ToLowerInvariant();

    public static string ToCsvValue(TaskLabel label) => label.ToString().ToLowerInvariant();

    public static string ToCsvValue(TaskPriority priority) => priority.ToString().ToLowerInvariant();

    public static bool TryParseStatus(string? value, out WorkTaskStatus status)
    {
        if (string.Equals(value?.Trim(), "in progress", StringComparison.OrdinalIgnoreCase))
        {
            status = WorkTaskStatus.InProgress;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out status);
    }
}
