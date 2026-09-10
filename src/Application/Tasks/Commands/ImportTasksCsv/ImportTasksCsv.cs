using System.Globalization;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;
using CsvHelper;
using CsvHelper.Configuration;

namespace WareStockApi.Application.Tasks.Commands.ImportTasksCsv;

public record ImportTasksCsvCommand(string FileContent) : IRequest<ImportTasksCsvResult>;

public record ImportTasksCsvResult(int Imported, int Failed);

public class ImportTasksCsvCommandValidator : AbstractValidator<ImportTasksCsvCommand>
{
    public ImportTasksCsvCommandValidator()
    {
        RuleFor(v => v.FileContent).NotEmpty();
    }
}

public class ImportTasksCsvCommandHandler : IRequestHandler<ImportTasksCsvCommand, ImportTasksCsvResult>
{
    private readonly IApplicationDbContext _context;

    public ImportTasksCsvCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportTasksCsvResult> Handle(ImportTasksCsvCommand request, CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StringReader(request.FileContent);
        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        var imported = 0;
        var failed = 0;
        var entities = new List<WorkTask>();

        while (await csv.ReadAsync())
        {
            var title = csv.GetField("title");
            var status = csv.GetField("status");
            var label = csv.GetField("label");
            var priority = csv.GetField("priority");

            if (string.IsNullOrWhiteSpace(title)
                || !TryParseStatus(status, out var parsedStatus)
                || !TryParseEnum<TaskLabel>(label, out var parsedLabel)
                || !TryParseEnum<TaskPriority>(priority, out var parsedPriority))
            {
                failed++;
                continue;
            }

            entities.Add(new WorkTask
            {
                Title = title,
                Status = parsedStatus,
                Label = parsedLabel,
                Priority = parsedPriority
            });
            imported++;
        }

        if (entities.Count > 0)
        {
            _context.WorkTasks.AddRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ImportTasksCsvResult(imported, failed);
    }

    private static bool TryParseStatus(string? value, out WorkTaskStatus status)
    {
        if (string.Equals(value?.Trim(), "in progress", StringComparison.OrdinalIgnoreCase))
        {
            status = WorkTaskStatus.InProgress;
            return true;
        }

        return TryParseEnum(value, out status);
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out result);
}
