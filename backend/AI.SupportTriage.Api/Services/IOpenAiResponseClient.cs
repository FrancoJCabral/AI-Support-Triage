namespace AI.SupportTriage.Api.Services;

internal interface IOpenAiResponseClient
{
    Task<string?> CreateResponseAsync(
        string model,
        string instructions,
        string input,
        string jsonSchema,
        CancellationToken cancellationToken);
}
