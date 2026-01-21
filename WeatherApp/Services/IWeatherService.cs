using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Service interface for retrieving weather data
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather data for all dates in the input file
    /// </summary>
    /// <returns>Aggregated weather API response</returns>
    Task<WeatherApiResponse> GetAllWeatherDataAsync();

    /// <summary>
    /// Gets weather data for a specific date
    /// </summary>
    /// <param name="date">The date in ISO format (yyyy-MM-dd)</param>
    /// <returns>Weather result for the specified date</returns>
    Task<WeatherResult> GetWeatherForDateAsync(string originalDate, DateTime parsedDate);

    /// <summary>
    /// Checks if weather data is cached for a specific date
    /// </summary>
    /// <param name="isoDate">The date in ISO format</param>
    /// <returns>True if cached, false otherwise</returns>
    bool IsCached(string isoDate);
}
