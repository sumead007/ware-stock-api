using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Settings;

/// <summary>Lazily creates the 1:1 <see cref="UserSettings"/> row for a user on first access.</summary>
public static class UserSettingsHelper
{
    public static async Task<UserSettings> GetOrCreateAsync(IApplicationDbContext context, string userId, CancellationToken cancellationToken)
    {
        var settings = await context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (settings is not null) return settings;

        settings = new UserSettings { UserId = userId };
        context.UserSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken);

        return settings;
    }
}
