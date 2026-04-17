using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FlightAgent.Infrastructure.Health;

public class AviationStackHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AviationStackHealthCheck> _logger;

    public AviationStackHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AviationStackHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["AviationStack:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("AviationStack API key not configured");
                return HealthCheckResult.Unhealthy("AviationStack API key not configured. Run: dotnet user-secrets set AviationStack:ApiKey \"your-key\"");
            }

            var url = $"https://api.aviationstack.com/v1/flights?access_key={Uri.EscapeDataString(apiKey)}&flight_iata=AA100&limit=1";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("AviationStack API health check passed");
                return HealthCheckResult.Healthy("AviationStack API is reachable");
            }

            _logger.LogWarning("AviationStack API returned {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded(
                $"AviationStack API returned {response.StatusCode}",
                data: new Dictionary<string, object>
                {
                    ["statusCode"] = (int)response.StatusCode
                });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AviationStack API health check timed out");
            return HealthCheckResult.Degraded("AviationStack API request timed out after 5 seconds");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AviationStack API health check failed");
            return HealthCheckResult.Degraded(
                $"AviationStack API unreachable: {ex.Message}",
                exception: ex);
        }
    }
}
