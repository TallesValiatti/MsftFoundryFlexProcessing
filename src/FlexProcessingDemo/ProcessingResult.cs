/// <summary>
/// Metrics captured from a single Responses API call.
/// </summary>
/// <param name="RequestedTier">The <c>service_tier</c> value sent in the request ("flex" or "default").</param>
/// <param name="ActualTier">The <c>service_tier</c> value returned by the API, i.e. the tier that actually processed the request.</param>
/// <param name="Text">The generated text, concatenated from all <c>output_text</c> content items.</param>
/// <param name="InputTokens">Total input tokens reported by <c>usage.input_tokens</c> (includes cached tokens).</param>
/// <param name="OutputTokens">Output tokens reported by <c>usage.output_tokens</c> (includes reasoning tokens).</param>
/// <param name="CachedInputTokens">The cached subset of the input tokens (<c>usage.input_tokens_details.cached_tokens</c>).</param>
/// <param name="Duration">Wall-clock time measured with <see cref="System.Diagnostics.Stopwatch"/>.</param>
public sealed record ProcessingResult(
    string RequestedTier,
    string ActualTier,
    string Text,
    int InputTokens,
    int OutputTokens,
    int CachedInputTokens,
    TimeSpan Duration
);
