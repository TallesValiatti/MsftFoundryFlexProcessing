using System.Globalization;
using System.Net;

// Keep number formatting identical on every machine (e.g. "8.41", not "8,41").
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// The same fixed prompt is used for every request, so Flex and Standard runs are comparable.
// It lives in prompt.txt, which the build copies next to the executable.
var promptPath = Path.Combine(AppContext.BaseDirectory, "prompt.txt");

if (!File.Exists(promptPath))
{
    WriteError($"Prompt file not found: {promptPath}");
    return 1;
}

var prompt = File.ReadAllText(promptPath);

// Convenience for local runs: load KEY=VALUE pairs from a .env file in the current directory.
// Variables already set in the shell take precedence over the file.
LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

DemoSettings settings;

try
{
    settings = DemoSettings.FromEnvironment();
}
catch (InvalidOperationException ex)
{
    WriteError("Configuration error");
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine("See the 'Configuration' section of README.md.");
    return 1;
}

using var http = new HttpClient
{
    // Flex requests can take much longer than Standard ones. The Flex docs suggest ~15 minutes.
    Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
};

var client = new FoundryClient(http, settings.Endpoint, settings.ApiKey, settings.Deployment);

// Ctrl+C cancels the request in progress and returns to the menu.
// When no request is running, Ctrl+C exits the app as usual.
CancellationTokenSource? currentOperation = null;

Console.CancelKeyPress += (_, e) =>
{
    if (currentOperation is { } operation)
    {
        e.Cancel = true;
        operation.Cancel();
    }
};

// Non-interactive mode: `dotnet run --project src/FlexProcessingDemo -- 3`
if (args.Length > 0)
{
    return await RunOptionAsync(args[0]) ? 0 : 1;
}

while (true)
{
    PrintMenu();

    var option = Console.ReadLine()?.Trim();

    if (option is null or "0")
        return 0;

    await RunOptionAsync(option);

    Console.WriteLine();
    Console.Write("Press Enter to return to the menu...");
    Console.ReadLine();
}

void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine(" Microsoft Foundry Flex Processing Demo");
    Console.WriteLine("========================================");
    Console.WriteLine();
    Console.WriteLine($"Model:    {settings.Deployment}");
    Console.WriteLine($"Endpoint: {settings.Endpoint}");
    Console.WriteLine($"Fallback: {(settings.EnableFallback ? "enabled" : "disabled")}");
    Console.WriteLine();
    Console.WriteLine("1. Run with Flex");
    Console.WriteLine("2. Run with Standard");
    Console.WriteLine("3. Compare Flex vs Standard");
    Console.WriteLine("4. Run Flex with Standard fallback");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Select an option: ");
}

async Task<bool> RunOptionAsync(string option)
{
    using var cts = new CancellationTokenSource();
    currentOperation = cts;

    try
    {
        switch (option)
        {
            case "1":
                await RunSingleAsync("flex", cts.Token);
                return true;
            case "2":
                await RunSingleAsync("default", cts.Token);
                return true;
            case "3":
                return await CompareAsync(cts.Token);
            case "4":
                return await RunWithFallbackAsync(cts.Token);
            default:
                WriteError($"Unknown option '{option}'. Choose 1, 2, 3, 4 or 0.");
                return false;
        }
    }
    catch (Exception ex) when (ex is FoundryRequestException or TimeoutException or HttpRequestException)
    {
        DescribeError(ex);
        return false;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        Console.WriteLine("Request cancelled.");
        return false;
    }
    finally
    {
        currentOperation = null;
    }
}

async Task RunSingleAsync(string serviceTier, CancellationToken cancellationToken)
{
    Console.WriteLine();
    Console.WriteLine($"Sending request with service_tier = \"{serviceTier}\"...");

    var result = await client.ExecuteAsync(prompt, serviceTier, cancellationToken);

    PrintResult(result);
}

async Task<bool> CompareAsync(CancellationToken cancellationToken)
{
    Console.WriteLine();
    Console.WriteLine("Running Flex request...");
    var flex = await TryExecuteAsync("flex", cancellationToken);

    Console.WriteLine("Running Standard request...");
    var standard = await TryExecuteAsync("default", cancellationToken);

    if (flex is null || standard is null)
    {
        Console.WriteLine();
        Console.WriteLine("The comparison needs both requests to succeed.");

        if ((flex ?? standard) is { } succeeded)
            PrintResult(succeeded);

        return false;
    }

    PrintSection("Flex response");
    Console.WriteLine(flex.Text);
    PrintSection("Standard response");
    Console.WriteLine(standard.Text);

    PrintSection("Flex vs Standard (single sample, not a benchmark)");
    Console.WriteLine($"{"Metric",-22} {"Flex",-16} {"Standard",-16}");
    Console.WriteLine(new string('-', 54));
    PrintRow("Requested tier", flex.RequestedTier, standard.RequestedTier);
    PrintRow("Actual tier", flex.ActualTier, standard.ActualTier);
    PrintRow("Latency (s)", $"{flex.Duration.TotalSeconds:F2}", $"{standard.Duration.TotalSeconds:F2}");
    PrintRow("Input tokens", $"{flex.InputTokens}", $"{standard.InputTokens}");
    PrintRow("Cached input tokens", $"{flex.CachedInputTokens}", $"{standard.CachedInputTokens}");
    PrintRow("Output tokens", $"{flex.OutputTokens}", $"{standard.OutputTokens}");
    PrintRow("Est. cost (USD)", FormatEstimatedCost(flex), FormatEstimatedCost(standard));

    // The two runs usually produce different token counts, so their costs aren't directly
    // comparable. Pricing the same usage at both tiers isolates the effect of service_tier.
    var standardCost = CostCalculator.Calculate(standard, settings.StandardPrices);
    var flexCost = CostCalculator.Calculate(standard, settings.FlexPrices);
    var savings = standardCost - flexCost;

    PrintSection("Same token usage priced at both tiers (Standard sample usage)");
    Console.WriteLine($"Standard estimate:  {FormatUsd(standardCost)}");
    Console.WriteLine($"Flex estimate:      {FormatUsd(flexCost)}");
    Console.WriteLine(standardCost > 0
        ? $"Potential savings:  {FormatUsd(savings)} ({savings / standardCost * 100:F0}%)"
        : $"Potential savings:  {FormatUsd(savings)}");

    Console.WriteLine();
    Console.WriteLine("Latency varies from run to run, and Flex has no latency SLA.");
    Console.WriteLine("Costs are estimates based on the configured prices, not billing data.");

    WarnIfTierDiffers(flex);
    WarnIfTierDiffers(standard);

    return true;
}

async Task<ProcessingResult?> TryExecuteAsync(string serviceTier, CancellationToken cancellationToken)
{
    try
    {
        return await client.ExecuteAsync(prompt, serviceTier, cancellationToken);
    }
    catch (Exception ex) when (ex is FoundryRequestException or TimeoutException or HttpRequestException)
    {
        Console.WriteLine($"The {serviceTier} request failed.");
        DescribeError(ex);
        return null;
    }
}

async Task<bool> RunWithFallbackAsync(CancellationToken cancellationToken)
{
    Console.WriteLine();
    Console.WriteLine($"Standard fallback is {(settings.EnableFallback ? "ENABLED" : "DISABLED")} " +
                      "(FLEX_FALLBACK_ENABLED).");
    Console.WriteLine("Sending request with service_tier = \"flex\"...");

    var fellBack = false;

    try
    {
        var result = await client.ExecuteFlexWithFallbackAsync(
            prompt,
            settings.EnableFallback,
            onFallback: ex =>
            {
                fellBack = true;
                Console.WriteLine();
                Console.WriteLine($"Flex returned HTTP 429: {ex.ErrorMessage}");
                if (ex.RetryAfter is { } retryAfter)
                    Console.WriteLine($"The service suggested retrying after {retryAfter.TotalSeconds:F0}s.");
                Console.WriteLine("Falling back to service_tier = \"default\" (billed at the Standard rate)...");
            },
            cancellationToken);

        PrintResult(result);

        Console.WriteLine();
        Console.WriteLine(fellBack
            ? "Completed with Standard fallback. This request was billed at the Standard rate."
            : "Completed without fallback: Flex accepted the request.");

        return true;
    }
    catch (FoundryRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        DescribeError(ex);
        Console.WriteLine();

        if (fellBack)
        {
            Console.WriteLine("The Standard fallback also returned HTTP 429.");
            Console.WriteLine("Flex and Standard share the deployment quota, so a quota limit blocks both tiers.");
            Console.WriteLine("Wait and retry later, or assign more quota to the deployment.");
        }
        else
        {
            Console.WriteLine("Fallback is disabled, so the app did not try Standard.");
            Console.WriteLine("Set FLEX_FALLBACK_ENABLED=true to allow fallback (it changes billing to Standard).");
        }

        return false;
    }
}

void PrintResult(ProcessingResult result)
{
    PrintSection("Response");
    Console.WriteLine(result.Text);

    PrintSection("Metrics");
    Console.WriteLine($"Requested tier:      {result.RequestedTier}");
    Console.WriteLine($"Actual tier:         {result.ActualTier}");
    Console.WriteLine($"Time:                {result.Duration.TotalSeconds:F2}s");
    Console.WriteLine($"Input tokens:        {result.InputTokens}");
    Console.WriteLine($"Cached input tokens: {result.CachedInputTokens}");
    Console.WriteLine($"Output tokens:       {result.OutputTokens}");
    Console.WriteLine($"Est. cost (USD):     {FormatEstimatedCost(result)}");

    WarnIfTierDiffers(result);
}

// Price a result by the tier that actually processed it, not the tier that was requested.
string FormatEstimatedCost(ProcessingResult result) =>
    result.ActualTier switch
    {
        "flex" => FormatUsd(CostCalculator.Calculate(result, settings.FlexPrices)),
        "default" => FormatUsd(CostCalculator.Calculate(result, settings.StandardPrices)),
        _ => $"n/a (tier '{result.ActualTier}')"
    };

void WarnIfTierDiffers(ProcessingResult result)
{
    if (result.ActualTier != result.RequestedTier)
    {
        Console.WriteLine();
        Console.WriteLine($"Note: requested '{result.RequestedTier}' but the API reported " +
                          $"'{result.ActualTier}'. Always trust the returned service_tier.");
    }
}

void DescribeError(Exception ex)
{
    Console.WriteLine();

    if (ex is TimeoutException)
    {
        WriteError($"Timeout: {ex.Message}");
        Console.WriteLine("Flex requests can take longer than Standard ones. Increase REQUEST_TIMEOUT_SECONDS,");
        Console.WriteLine("retry later, or use Standard if completion time matters more than price.");
        return;
    }

    if (ex is not FoundryRequestException api)
    {
        WriteError($"Network error: {ex.Message}");
        Console.WriteLine("Check AZURE_OPENAI_ENDPOINT and your network connection.");
        return;
    }

    WriteError(api.Message);

    if (api.ErrorType is not null || api.ErrorCode is not null)
        Console.WriteLine($"Error type: {api.ErrorType ?? "-"}, code: {api.ErrorCode ?? "-"}");

    var hint = (int?)api.StatusCode switch
    {
        400 => "The request was rejected and was NOT processed by Flex. The service does not fall back\n" +
               "to Standard. Check that the model and version support Flex (see the supported-model\n" +
               "table in the Flex docs). Don't resend the same request unchanged.",
        401 or 403 => "Authentication failed. Check AZURE_OPENAI_API_KEY and AZURE_OPENAI_ENDPOINT.\n" +
                      "Retrying won't help until the configuration changes.",
        404 => "Deployment not found. Check that AZURE_OPENAI_DEPLOYMENT matches the deployment name\n" +
               "and that AZURE_OPENAI_ENDPOINT points to the right resource.",
        408 => "The service timed out. Retry with backoff, or use Standard if completion time matters.",
        429 => "Flex capacity is unavailable OR the deployment quota/rate limit was exceeded.\n" +
               "Flex and Standard share the same deployment quota." +
               (api.RetryAfter is { } wait ? $"\nThe service suggested retrying after {wait.TotalSeconds:F0}s." : ""),
        >= 500 => "Transient service error. Retry later with bounded exponential backoff.",
        _ => null
    };

    if (hint is not null)
        Console.WriteLine(hint);
}

static void LoadDotEnv(string path)
{
    if (!File.Exists(path))
        return;

    foreach (var rawLine in File.ReadAllLines(path))
    {
        var line = rawLine.Trim();

        if (line.Length == 0 || line.StartsWith('#'))
            continue;

        if (line.StartsWith("export "))
            line = line["export ".Length..];

        var separator = line.IndexOf('=');
        if (separator <= 0)
            continue;

        var name = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim().Trim('"', '\'');

        if (Environment.GetEnvironmentVariable(name) is null)
            Environment.SetEnvironmentVariable(name, value);
    }
}

static void PrintSection(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

static void PrintRow(string metric, string flex, string standard) =>
    Console.WriteLine($"{metric,-22} {flex,-16} {standard,-16}");

static string FormatUsd(decimal value) => $"${value:F6}";

static void WriteError(string message)
{
    var previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(message);
    Console.ForegroundColor = previous;
}

/// <summary>
/// Demo configuration, read from environment variables.
/// </summary>
public sealed record DemoSettings(
    string Endpoint,
    string ApiKey,
    string Deployment,
    bool EnableFallback,
    int TimeoutSeconds,
    TokenPrices StandardPrices,
    TokenPrices FlexPrices)
{
    // GPT-5.6 Luna Global Standard prices (short context, up to 272K input tokens),
    // in USD per 1M tokens. Verify them on the official pricing page and override them
    // with environment variables when you use another model or agreement.
    public static readonly TokenPrices DefaultStandardPrices = new(
        InputPerMillion: 0.20m,
        CachedInputPerMillion: 0.02m,
        OutputPerMillion: 1.20m);

    // GPT-5.6 Luna Flex prices: 50% of the Standard rates.
    public static readonly TokenPrices DefaultFlexPrices = new(
        InputPerMillion: 0.10m,
        CachedInputPerMillion: 0.01m,
        OutputPerMillion: 0.60m);

    public static DemoSettings FromEnvironment() =>
        FromVariables(Environment.GetEnvironmentVariable);

    public static DemoSettings FromVariables(Func<string, string?> getVariable)
    {
        var errors = new List<string>();

        string? Get(string name)
        {
            var value = getVariable(name)?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        string Required(string name)
        {
            var value = Get(name);

            if (value is null)
                errors.Add($"- {name} is not set.");
            else if (value.Contains('<'))
                errors.Add($"- {name} still contains a placeholder ('{value}').");

            return value ?? "";
        }

        decimal Price(string name, decimal defaultValue)
        {
            var value = Get(name);

            if (value is null)
                return defaultValue;

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) && price >= 0)
                return price;

            errors.Add($"- {name} must be a non-negative number such as 0.10 (got '{value}').");
            return defaultValue;
        }

        var endpoint = Required("AZURE_OPENAI_ENDPOINT");
        var apiKey = Required("AZURE_OPENAI_API_KEY");
        var deployment = Required("AZURE_OPENAI_DEPLOYMENT");

        if (endpoint.Length > 0 && !endpoint.Contains('<') &&
            (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add($"- AZURE_OPENAI_ENDPOINT must be an https URL such as " +
                       $"https://<RESOURCE_NAME>.openai.azure.com (got '{endpoint}').");
        }

        // Accept endpoints copied with the API path, e.g. ".../openai/v1/".
        endpoint = endpoint.TrimEnd('/');
        foreach (var suffix in new[] { "/openai/v1", "/openai" })
        {
            if (endpoint.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint[..^suffix.Length];
        }

        var enableFallback = false;
        if (Get("FLEX_FALLBACK_ENABLED") is { } fallbackValue && !bool.TryParse(fallbackValue, out enableFallback))
            errors.Add($"- FLEX_FALLBACK_ENABLED must be 'true' or 'false' (got '{fallbackValue}').");

        var timeoutSeconds = 900;
        if (Get("REQUEST_TIMEOUT_SECONDS") is { } timeoutValue &&
            (!int.TryParse(timeoutValue, NumberStyles.None, CultureInfo.InvariantCulture, out timeoutSeconds) ||
             timeoutSeconds <= 0))
        {
            errors.Add($"- REQUEST_TIMEOUT_SECONDS must be a positive whole number (got '{timeoutValue}').");
        }

        var standardPrices = new TokenPrices(
            Price("STANDARD_INPUT_PRICE_PER_MILLION", DefaultStandardPrices.InputPerMillion),
            Price("STANDARD_CACHED_INPUT_PRICE_PER_MILLION", DefaultStandardPrices.CachedInputPerMillion),
            Price("STANDARD_OUTPUT_PRICE_PER_MILLION", DefaultStandardPrices.OutputPerMillion));

        var flexPrices = new TokenPrices(
            Price("FLEX_INPUT_PRICE_PER_MILLION", DefaultFlexPrices.InputPerMillion),
            Price("FLEX_CACHED_INPUT_PRICE_PER_MILLION", DefaultFlexPrices.CachedInputPerMillion),
            Price("FLEX_OUTPUT_PRICE_PER_MILLION", DefaultFlexPrices.OutputPerMillion));

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Fix the following environment variables and run the app again:" +
                Environment.NewLine + string.Join(Environment.NewLine, errors));
        }

        return new DemoSettings(
            endpoint, apiKey, deployment, enableFallback, timeoutSeconds, standardPrices, flexPrices);
    }
}
