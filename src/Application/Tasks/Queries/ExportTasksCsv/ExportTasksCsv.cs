using System.Globalization;
using WareStockApi.Application.Common.Interfaces;
using CsvHelper;

namespace WareStockApi.Application.Tasks.Queries.ExportTasksCsv;

public record ExportTasksCsvQuery(IReadOnlyCollection<string> Ids) : IRequest<string>;

public class ExportTasksCsvQueryHandler : IRequestHandler<ExportTasksCsvQuery, string>
{
    private readonly IApplicationDbContext _context;

    public ExportTasksCsvQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(ExportTasksCsvQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _context.WorkTasks
            .AsNoTracking()
            .Where(t => request.Ids.Contains(t.Id))
            .OrderBy(t => t.Created)
            .ToListAsync(cancellationToken);

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("id");
        csv.WriteField("title");
        csv.WriteField("status");
        csv.WriteField("label");
        csv.WriteField("priority");
        await csv.NextRecordAsync();

        foreach (var task in tasks)
        {
            csv.WriteField(task.Id);
            csv.WriteField(task.Title);
            csv.WriteField(TaskEnumFormatter.ToCsvValue(task.Status));
            csv.WriteField(TaskEnumFormatter.ToCsvValue(task.Label));
            csv.WriteField(TaskEnumFormatter.ToCsvValue(task.Priority));
            await csv.NextRecordAsync();
        }

        await csv.FlushAsync();

        return writer.ToString();
    }
}
