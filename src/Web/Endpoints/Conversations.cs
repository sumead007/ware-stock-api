using WareStockApi.Application.Chats;
using WareStockApi.Application.Chats.Commands.CreateConversation;
using WareStockApi.Application.Chats.Queries.GetConversations;
using WareStockApi.Application.Chats.Queries.GetMessagesByConversationId;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Conversations : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/conversations";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetConversations);
        groupBuilder.MapPost(CreateConversation);
        groupBuilder.MapGet(GetMessages, "{id}/messages");
    }

    [EndpointSummary("List conversations")]
    public static async Task<Ok<ApiResponse<IReadOnlyCollection<ConversationDto>>>> GetConversations(ISender sender, string? search = null)
    {
        var conversations = await sender.Send(new GetConversationsQuery(search));

        return TypedResults.Ok(((IReadOnlyCollection<ConversationDto>)conversations).ToApiResponse());
    }

    [EndpointSummary("Create a conversation")]
    public static async Task<Created<ApiResponse<ConversationDto>>> CreateConversation(ISender sender, CreateConversationCommand command)
    {
        var conversation = await sender.Send(command);

        return TypedResults.Created(
            $"/v1/conversations/{conversation.Id}", conversation.ToApiResponse("Conversation created.", StatusCodes.Status201Created));
    }

    [EndpointSummary("List messages in a conversation")]
    public static async Task<Ok<ApiResponse<IReadOnlyCollection<MessageDto>>>> GetMessages(ISender sender, string id)
    {
        var messages = await sender.Send(new GetMessagesByConversationIdQuery(id));

        return TypedResults.Ok(((IReadOnlyCollection<MessageDto>)messages).ToApiResponse());
    }
}
