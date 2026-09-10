using WareStockApi.Application.Settings;
using WareStockApi.Application.Settings.Commands.UpdateAccountSettings;
using WareStockApi.Application.Settings.Commands.UpdateAppearanceSettings;
using WareStockApi.Application.Settings.Commands.UpdateDisplaySettings;
using WareStockApi.Application.Settings.Commands.UpdateNotificationSettings;
using WareStockApi.Application.Settings.Commands.UpdateProfileSettings;
using WareStockApi.Application.Settings.Queries.GetMySettings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Settings : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/me";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMySettings, "settings");
        groupBuilder.MapPatch(UpdateProfile, "profile");
        groupBuilder.MapPatch(UpdateAccount, "account");
        groupBuilder.MapPatch(UpdateAppearance, "appearance");
        groupBuilder.MapPatch(UpdateNotifications, "notifications");
        groupBuilder.MapPatch(UpdateDisplay, "display");
    }

    [EndpointSummary("Get current user's settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> GetMySettings(ISender sender)
    {
        var settings = await sender.Send(new GetMySettingsQuery());

        return TypedResults.Ok(settings.ToApiResponse());
    }

    [EndpointSummary("Save profile settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateProfile(ISender sender, UpdateProfileSettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Profile updated."));
    }

    [EndpointSummary("Save account settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateAccount(ISender sender, UpdateAccountSettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Account updated."));
    }

    [EndpointSummary("Save appearance settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateAppearance(ISender sender, UpdateAppearanceSettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Appearance updated."));
    }

    [EndpointSummary("Save notification settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateNotifications(ISender sender, UpdateNotificationSettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Notification settings updated."));
    }

    [EndpointSummary("Save display settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateDisplay(ISender sender, UpdateDisplaySettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Display settings updated."));
    }
}
