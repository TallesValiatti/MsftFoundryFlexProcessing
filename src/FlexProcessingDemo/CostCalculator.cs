/// <summary>
/// Token prices in USD per 1 million tokens.
/// </summary>
public sealed record TokenPrices(
    decimal InputPerMillion,
    decimal CachedInputPerMillion,
    decimal OutputPerMillion);

/// <summary>
/// Estimates request cost from token usage. These are estimates, not billing data:
/// always confirm rates on the official Azure OpenAI pricing page.
/// </summary>
public static class CostCalculator
{
    /// <summary>
    /// Simplified formula that ignores prompt caching:
    /// cost = input × inputPrice / 1M + output × outputPrice / 1M.
    /// </summary>
    public static decimal Calculate(
        int inputTokens,
        int outputTokens,
        decimal inputPricePerMillion,
        decimal outputPricePerMillion)
    {
        return
            (inputTokens / 1_000_000m * inputPricePerMillion) +
            (outputTokens / 1_000_000m * outputPricePerMillion);
    }

    /// <summary>
    /// Cache-aware formula. <c>usage.input_tokens</c> already includes the cached tokens,
    /// so only the uncached part is charged at the full input rate:
    /// cost = (input − cached) × inputPrice / 1M + cached × cachedPrice / 1M + output × outputPrice / 1M.
    /// </summary>
    public static decimal Calculate(
        int inputTokens,
        int cachedInputTokens,
        int outputTokens,
        TokenPrices prices)
    {
        if (cachedInputTokens < 0 || cachedInputTokens > inputTokens)
            throw new ArgumentOutOfRangeException(
                nameof(cachedInputTokens),
                "Cached input tokens must be between 0 and the total input tokens.");

        var uncachedInputTokens = inputTokens - cachedInputTokens;

        return
            (uncachedInputTokens / 1_000_000m * prices.InputPerMillion) +
            (cachedInputTokens / 1_000_000m * prices.CachedInputPerMillion) +
            (outputTokens / 1_000_000m * prices.OutputPerMillion);
    }

    /// <summary>
    /// Estimates the cost of a completed request using the token usage reported by the API.
    /// </summary>
    public static decimal Calculate(ProcessingResult result, TokenPrices prices) =>
        Calculate(result.InputTokens, result.CachedInputTokens, result.OutputTokens, prices);
}
