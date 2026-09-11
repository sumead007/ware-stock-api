using WareStockApi.Application.Settings;
using WareStockApi.Application.Settings.Commands.UpdateAppearanceSettings;
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
        groupBuilder.MapPatch(UpdateAppearance, "appearance");
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

    [EndpointSummary("Save appearance settings")]
    public static async Task<Ok<ApiResponse<UserSettingsDto>>> UpdateAppearance(ISender sender, UpdateAppearanceSettingsCommand command)
    {
        var settings = await sender.Send(command);

        return TypedResults.Ok(settings.ToApiResponse("Appearance updated."));
    }
}
