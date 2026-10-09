using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Minimal client for the Azure OpenAI Responses API (<c>POST /openai/v1/responses</c>).
/// The only thing that changes between Flex and Standard is the <c>service_tier</c> value:
/// the endpoint, deployment, API key and prompt stay the same.
/// </summary>
public sealed class FoundryClient
{
    private readonly HttpClient _http;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _deployment;

    public FoundryClient(
        HttpClient http,
        string endpoint,
        string apiKey,
        string deployment)
    {
        _http = http;
        _endpoint = endpoint.TrimEnd('/');
        _apiKey = apiKey;
        _deployment = deployment;
    }

    /// <summary>
    /// Sends <paramref name="input"/> to the deployment using the requested service tier
    /// ("flex" or "default") and returns the generated text with its measured metrics.
    /// </summary>
    /// <exception cref="FoundryRequestException">The API returned a non-success status code.</exception>
    /// <exception cref="TimeoutException">The request exceeded <see cref="HttpClient.Timeout"/>.</exception>
    public async Task<ProcessingResult> ExecuteAsync(
        string input,
        string serviceTier,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            model = _deployment,
            input,
            service_tier = serviceTier
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_endpoint}/openai/v1/responses")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("api-key", _apiKey);

        var stopwatch = Stopwatch.StartNew();
        string responseBody;
        HttpResponseMessage response;

        try
        {
            response = await _http.SendAsync(request, cancellationToken);
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient reports its own timeout as a TaskCanceledException.
            // Surface it as a TimeoutException so callers can tell it apart from Ctrl+C.
            throw new TimeoutException(
                $"The request did not complete within {_http.Timeout.TotalSeconds:F0} seconds.", ex);
        }

        stopwatch.Stop();

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw FoundryRequestException.FromResponse(response, responseBody);
            }
        }

        return ParseResponse(responseBody, serviceTier, stopwatch.Elapsed);
    }

    /// <summary>
    /// Tries Flex first. If Flex returns HTTP 429 and <paramref name="enableFallback"/> is true,
    /// the same prompt is sent once more with <c>service_tier = "default"</c>.
    /// The service never does this automatically: fallback is always an application decision,
    /// and it changes the price of the request to the Standard rate.
    /// </summary>
    /// <param name="onFallback">Optional callback invoked with the Flex error right before the Standard attempt.</param>
    public async Task<ProcessingResult> ExecuteFlexWithFallbackAsync(
        string input,
        bool enableFallback,
        Action<FoundryRequestException>? onFallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteAsync(input, "flex", cancellationToken);
        }
        catch (FoundryRequestException ex)
            when (enableFallback && ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // HTTP 429 can mean "Flex capacity unavailable" OR "deployment quota exceeded".
            // Flex and Standard share the deployment quota, so this second call can also fail.
            onFallback?.Invoke(ex);

            return await ExecuteAsync(input, "default", cancellationToken);
        }
    }

    /// <summary>
    /// Extracts the processed tier, token usage and generated text from a Responses API payload.
    /// </summary>
    public static ProcessingResult ParseResponse(
        string responseBody,
        string requestedTier,
        TimeSpan duration)
    {
        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;

        // The tier that actually processed the request. Never assume it equals the requested tier.
        var actualTier =
            root.TryGetProperty("service_tier", out var tier) &&
            tier.ValueKind == JsonValueKind.String
                ? tier.GetString()!
                : "unknown";

        var inputTokens = 0;
        var outputTokens = 0;
        var cachedTokens = 0;

        if (root.TryGetProperty("usage", out var usage) &&
            usage.ValueKind == JsonValueKind.Object)
        {
            inputTokens = ReadInt(usage, "input_tokens");
            outputTokens = ReadInt(usage, "output_tokens");

            if (usage.TryGetProperty("input_tokens_details", out var details) &&
                details.ValueKind == JsonValueKind.Object)
            {
                cachedTokens = ReadInt(details, "cached_tokens");
            }
        }

        var textParts = new List<string>();

        if (root.TryGetProperty("output", out var outputItems) &&
            outputItems.ValueKind == JsonValueKind.Array)
        {
            foreach (var output in outputItems.EnumerateArray())
            {
                // Reasoning items have no "content"; only message items carry output_text.
                if (!output.TryGetProperty("content", out var content) ||
                    content.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var item in content.EnumerateArray())
                {
                    if (item.TryGetProperty("type", out var type) &&
                        type.GetString() == "output_text" &&
                        item.TryGetProperty("text", out var text))
                    {
                        textParts.Add(text.GetString() ?? "");
                    }
                }
            }
        }

        return new ProcessingResult(
            requestedTier,
            actualTier,
            string.Join(Environment.NewLine, textParts),
            inputTokens,
            outputTokens,
            cachedTokens,
            duration
        );
    }

    private static int ReadInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : 0;
}

/// <summary>
/// Raised when the Responses API returns a non-success HTTP status code.
/// </summary>
public sealed class FoundryRequestException : HttpRequestException
{
    public FoundryRequestException(
        HttpStatusCode statusCode,
        string? errorCode,
        string? errorType,
        string errorMessage,
        TimeSpan? retryAfter)
        : base($"HTTP {(int)statusCode} ({statusCode}): {errorMessage}", null, statusCode)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        RetryAfter = retryAfter;
    }

    /// <summary>The <c>error.code</c> value from the response body, when present.</summary>
    public string? ErrorCode { get; }

    /// <summary>The <c>error.type</c> value, e.g. <c>invalid_request_error</c>.</summary>
    public string? ErrorType { get; }

    /// <summary>The <c>error.message</c> value, or the raw body when it isn't JSON.</summary>
    public string ErrorMessage { get; }

    /// <summary>The delay suggested by the service through <c>Retry-After</c> or <c>retry-after-ms</c>.</summary>
    public TimeSpan? RetryAfter { get; }

    public static FoundryRequestException FromResponse(
        HttpResponseMessage response,
        string responseBody)
    {
        string? code = null;
        string? type = null;
        var message = string.IsNullOrWhiteSpace(responseBody)
            ? response.ReasonPhrase ?? "No response body."
            : responseBody;

        // Expected shape: { "error": { "code": "...", "type": "...", "message": "..." } }
        try
        {
            using var json = JsonDocument.Parse(responseBody);

            if (json.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object)
            {
                code = ReadString(error, "code");
                type = ReadString(error, "type");
                message = ReadString(error, "message") ?? message;
            }
        }
        catch (JsonException)
        {
            // Not JSON (for example, a gateway error page). Keep the raw body as the message.
        }

        return new FoundryRequestException(
            response.StatusCode, code, type, message, ReadRetryAfter(response));
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("retry-after-ms", out var values) &&
            double.TryParse(values.FirstOrDefault(), NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
        {
            return TimeSpan.FromMilliseconds(ms);
        }

        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
            return delta;

        if (retryAfter?.Date is { } date)
            return date - DateTimeOffset.UtcNow;

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
