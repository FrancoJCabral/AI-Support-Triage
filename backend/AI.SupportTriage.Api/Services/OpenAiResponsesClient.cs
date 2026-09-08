using OpenAI.Responses;

namespace AI.SupportTriage.Api.Services;

internal sealed class OpenAiResponsesClient(ResponsesClient client) : IOpenAiResponseClient
{
    public async Task<string?> CreateResponseAsync(
        string model,
        string instructions,
        string input,
        string jsonSchema,
        CancellationToken cancellationToken)
    {
        var options = new CreateResponseOptions
        {
            Model = model,
            Instructions = instructions,
            TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "support_triage",
                    jsonSchema: BinaryData.FromString(jsonSchema),
                    jsonSchemaFormatDescription: "A classified support ticket.",
                    jsonSchemaIsStrict: true)
            }
        };

        options.InputItems.Add(ResponseItem.CreateUserMessageItem(input));

        var response = await client.CreateResponseAsync(options, cancellationToken);
        return response.Value.GetOutputText();
    }
}
