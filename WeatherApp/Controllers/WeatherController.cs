using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

/// <summary>
/// API Controller for weather data endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/weather
    /// Returns weather data for all dates in the input file
    /// </summary>
    /// <returns>Aggregated weather results</returns>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherApiResponse>> GetWeatherData()
    {
        try
        {
            _logger.LogInformation("GET /api/weather - Fetching all weather data");
            
            var result = await _weatherService.GetAllWeatherDataAsync();
            
            _logger.LogInformation(
                "Returning {TotalCount} results ({SuccessCount} success, {ErrorCount} errors)",
                result.TotalProcessed, result.SuccessCount, result.ErrorCount);

            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Dates file not found");
            return StatusCode(500, new { error = "Configuration error: dates file not found", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data");
            return StatusCode(500, new { error = "An error occurred while retrieving weather data", details = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/weather/health
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
