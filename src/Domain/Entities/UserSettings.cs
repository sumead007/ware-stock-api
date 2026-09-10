namespace WareStockApi.Domain.Entities;

/// <summary>
/// 1:1 settings record keyed by <see cref="UserId"/> (the owning user's id). Does not extend
/// <see cref="BaseEntity"/> since its primary key is the user id, not a generated GUID.
/// `name`/`email`/`username` are not stored here — they live directly on the user (ApplicationUser),
/// reached via <c>IIdentityService</c>.
/// </summary>
public class UserSettings
{
    public string UserId { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public Language Language { get; set; } = Language.En;

    public Theme Theme { get; set; } = Theme.Light;

    public AppFont Font { get; set; } = AppFont.Inter;

    public NotificationType NotificationType { get; set; } = NotificationType.All;

    public bool Mobile { get; set; }

    public bool CommunicationEmails { get; set; }

    public bool SocialEmails { get; set; }

    public bool MarketingEmails { get; set; }

    /// <summary>Always <see langword="true"/> — the UI does not allow disabling security emails.</summary>
    public bool SecurityEmails { get; set; } = true;

    public IList<SidebarItem> DisplayItems { get; set; } = [SidebarItem.Recents, SidebarItem.Home];
}
