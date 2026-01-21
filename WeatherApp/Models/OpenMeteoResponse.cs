using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/// <summary>
/// Represents the response from the Open-Meteo Historical Weather API
/// </summary>
public class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("timezone_abbreviation")]
    public string? TimezoneAbbreviation { get; set; }

    [JsonPropertyName("elevation")]
    public double Elevation { get; set; }

    [JsonPropertyName("daily_units")]
    public DailyUnits? DailyUnits { get; set; }

    [JsonPropertyName("daily")]
    public DailyData? Daily { get; set; }
}

public class DailyUnits
{
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public string? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public string? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public string? PrecipitationSum { get; set; }
}

public class DailyData
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double?>? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double?>? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double?>? PrecipitationSum { get; set; }
}
