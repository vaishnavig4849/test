namespace WeatherApp.Models;

/// <summary>
/// The API response containing all weather results
/// </summary>
public class WeatherApiResponse
{
    /// <summary>
    /// List of all weather results (successful and failed)
    /// </summary>
    public List<WeatherResult> Results { get; set; } = new();

    /// <summary>
    /// Total number of dates processed
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Number of successful results
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed results
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of results loaded from cache
    /// </summary>
    public int CachedCount { get; set; }

    /// <summary>
    /// Timestamp when the data was retrieved
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
