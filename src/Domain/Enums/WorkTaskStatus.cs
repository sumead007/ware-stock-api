using System.Text.Json.Serialization;

namespace WareStockApi.Domain.Enums;

public enum WorkTaskStatus
{
    Backlog,
    Todo,

    [JsonStringEnumMemberName("in progress")]
    InProgress,

    Done,
    Canceled
}
