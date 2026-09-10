using System.Text.Json.Serialization;
using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Settings;

public class UserSettingsDto
{
    public required ProfileSettingsDto Profile { get; init; }

    public required AccountSettingsDto Account { get; init; }

    public required AppearanceSettingsDto Appearance { get; init; }

    public required NotificationSettingsDto Notifications { get; init; }

    public required DisplaySettingsDto Display { get; init; }

    public static UserSettingsDto From(UserDto user, UserSettings settings) => new()
    {
        Profile = new ProfileSettingsDto
        {
            Name = user.DisplayName,
            Username = user.Username,
            Email = user.Email
        },
        Account = new AccountSettingsDto
        {
            Name = user.DisplayName,
            Dob = settings.DateOfBirth,
            Language = settings.Language
        },
        Appearance = new AppearanceSettingsDto
        {
            Theme = settings.Theme,
            Font = settings.Font
        },
        Notifications = new NotificationSettingsDto
        {
            Type = settings.NotificationType,
            Mobile = settings.Mobile,
            CommunicationEmails = settings.CommunicationEmails,
            SocialEmails = settings.SocialEmails,
            MarketingEmails = settings.MarketingEmails,
            SecurityEmails = settings.SecurityEmails
        },
        Display = new DisplaySettingsDto
        {
            Items = settings.DisplayItems.ToList()
        }
    };
}

public class ProfileSettingsDto
{
    public string Name { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public class AccountSettingsDto
{
    public string Name { get; init; } = string.Empty;

    public DateOnly? Dob { get; init; }

    public Language Language { get; init; }
}

public class AppearanceSettingsDto
{
    public Theme Theme { get; init; }

    public AppFont Font { get; init; }
}

public class NotificationSettingsDto
{
    public NotificationType Type { get; init; }

    public bool Mobile { get; init; }

    [JsonPropertyName("communication_emails")]
    public bool CommunicationEmails { get; init; }

    [JsonPropertyName("social_emails")]
    public bool SocialEmails { get; init; }

    [JsonPropertyName("marketing_emails")]
    public bool MarketingEmails { get; init; }

    [JsonPropertyName("security_emails")]
    public bool SecurityEmails { get; init; }
}

public class DisplaySettingsDto
{
    public List<SidebarItem> Items { get; init; } = [];
}
