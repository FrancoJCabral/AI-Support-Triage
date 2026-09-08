namespace AI.SupportTriage.Api.Services;

public sealed class TriageProviderException : Exception
{
    public TriageProviderException(string message)
        : base(message)
    {
    }

    public TriageProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
