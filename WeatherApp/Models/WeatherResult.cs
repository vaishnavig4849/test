namespace WeatherApp.Models;

/// <summary>
/// Represents the result for a single date's weather data processing
/// </summary>
public class WeatherResult
{
    /// <summary>
    /// The original date string from the input file
    /// </summary>
    public string OriginalDate { get; set; } = string.Empty;

    /// <summary>
    /// The normalized date in ISO format (yyyy-MM-dd), null if parsing failed
    /// </summary>
    public string? NormalizedDate { get; set; }

    /// <summary>
    /// Minimum temperature in Celsius
    /// </summary>
    public double? MinTemperature { get; set; }

    /// <summary>
    /// Maximum temperature in Celsius
    /// </summary>
    public double? MaxTemperature { get; set; }

    /// <summary>
    /// Precipitation sum in millimeters
    /// </summary>
    public double? Precipitation { get; set; }

    /// <summary>
    /// Status of the weather data retrieval: "Success", "Error", "Cached"
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Error message if the status is "Error"
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates if the data was loaded from cache
    /// </summary>
    public bool FromCache { get; set; }
}
