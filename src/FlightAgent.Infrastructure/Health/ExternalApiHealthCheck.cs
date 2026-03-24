using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FlightAgent.Infrastructure.Health;

/// <summary>
/// Health check that verifies connectivity to external APIs.
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiHealthCheck> _logger;

    public ExternalApiHealthCheck(IHttpClientFactory httpClientFactory, ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking external API connectivity...");

            // Use httpbin.org/get as a reliable external endpoint
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            var response = await _httpClient.GetAsync("https://httpbin.org/get", linkedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("External API connectivity check passed");
                return HealthCheckResult.Healthy("External API connectivity check");
            }

            _logger.LogWarning("External API returned non-success status code: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded(
                "External API connectivity check",
                data: new Dictionary<string, object>
                {
                    ["statusCode"] = (int)response.StatusCode
                });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("External API connectivity check timed out");
            return HealthCheckResult.Degraded(
                "External API connectivity check",
                data: new Dictionary<string, object>
                {
                    ["error"] = "Request timed out after 5 seconds"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API connectivity check failed");
            return HealthCheckResult.Unhealthy(
                "External API connectivity check",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                });
        }
    }
}
