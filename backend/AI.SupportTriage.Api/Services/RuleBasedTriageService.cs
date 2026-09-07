using System.Text.RegularExpressions;
using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Domain;

namespace AI.SupportTriage.Api.Services;

public sealed partial class RuleBasedTriageService : ITriageService
{
    private const int MaximumSummaryLength = 180;

    private static readonly (SupportCategory Category, string[] Keywords)[] CategoryRules =
    [
        (SupportCategory.Payments,
            ["payment", "pay", "card", "visa", "mastercard", "checkout", "transaction", "pago", "tarjeta"]),
        (SupportCategory.Account,
            ["account", "login", "password", "sign in", "profile", "cuenta", "contraseña", "iniciar sesión"]),
        (SupportCategory.Technical,
            ["error", "bug", "crash", "loading", "broken", "app", "website", "technical", "falla", "error técnico"]),
        (SupportCategory.Billing,
            ["invoice", "charge", "charged", "subscription", "refund", "billing", "factura", "cobro", "suscripción", "reembolso"]),
        (SupportCategory.Shipping,
            ["shipping", "delivery", "package", "tracking", "order arrived", "envío", "entrega", "paquete", "seguimiento"])
    ];

    private static readonly string[] CriticalPriorityKeywords =
    [
        "critical", "emergency", "production down", "cannot operate", "security breach",
        "urgente crítico", "sistema caído"
    ];

    private static readonly string[] HighPriorityKeywords =
    [
        "urgent", "asap", "immediately", "today", "blocked", "cannot complete",
        "necesito resolverlo hoy", "urgente", "bloqueado"
    ];

    private static readonly string[] LowPriorityKeywords =
    [
        "question", "suggestion", "whenever", "not urgent", "consulta", "sugerencia", "no es urgente"
    ];

    private static readonly string[] AngrySentimentKeywords =
    [
        "furious", "terrible", "unacceptable", "ridiculous", "angry", "pésimo", "inaceptable", "indignado"
    ];

    private static readonly string[] FrustratedSentimentKeywords =
    [
        "frustrated", "annoying", "doesn't work", "not working", "cannot", "can't",
        "no funciona", "no puedo", "frustrado"
    ];

    private static readonly string[] PositiveSentimentKeywords =
    [
        "thanks", "thank you", "great", "excellent", "love", "gracias", "excelente", "genial"
    ];

    public Task<TriageResult> AnalyzeAsync(
        TriageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var message = request.Message;
        var category = ClassifyCategory(message);

        var result = new TriageResult
        {
            Category = category,
            Priority = ClassifyPriority(message),
            Sentiment = ClassifySentiment(message),
            Summary = CreateSummary(message),
            SuggestedTeam = GetSuggestedTeam(category)
        };

        return Task.FromResult(result);
    }

    private static SupportCategory ClassifyCategory(string message)
    {
        foreach (var (category, keywords) in CategoryRules)
        {
            if (ContainsAny(message, keywords))
            {
                return category;
            }
        }

        return SupportCategory.Other;
    }

    private static SupportPriority ClassifyPriority(string message)
    {
        if (ContainsAny(message, CriticalPriorityKeywords))
        {
            return SupportPriority.Critical;
        }

        // Check Low before High because "not urgent" also contains "urgent".
        if (ContainsAny(message, LowPriorityKeywords))
        {
            return SupportPriority.Low;
        }

        return ContainsAny(message, HighPriorityKeywords)
            ? SupportPriority.High
            : SupportPriority.Medium;
    }

    private static SupportSentiment ClassifySentiment(string message)
    {
        if (ContainsAny(message, AngrySentimentKeywords))
        {
            return SupportSentiment.Angry;
        }

        if (ContainsAny(message, FrustratedSentimentKeywords))
        {
            return SupportSentiment.Frustrated;
        }

        return ContainsAny(message, PositiveSentimentKeywords)
            ? SupportSentiment.Positive
            : SupportSentiment.Neutral;
    }

    private static string CreateSummary(string message)
    {
        var normalized = WhitespaceRegex().Replace(message, " ").Trim();

        return normalized.Length <= MaximumSummaryLength
            ? normalized
            : $"{normalized[..(MaximumSummaryLength - 3)].TrimEnd()}...";
    }

    private static string GetSuggestedTeam(SupportCategory category)
    {
        return category switch
        {
            SupportCategory.Payments => "Payments Support",
            SupportCategory.Account => "Account Support",
            SupportCategory.Technical => "Technical Support",
            SupportCategory.Billing => "Billing Support",
            SupportCategory.Shipping => "Logistics Support",
            _ => "General Support"
        };
    }

    private static bool ContainsAny(string message, IEnumerable<string> keywords)
    {
        return keywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
