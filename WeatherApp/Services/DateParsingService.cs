using System.Globalization;

namespace WeatherApp.Services;

/// <summary>
/// Service for parsing date strings in multiple formats
/// </summary>
public class DateParsingService : IDateParsingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DateParsingService> _logger;

    // Supported date formats
    private static readonly string[] SupportedFormats = new[]
    {
        "MM/dd/yyyy",           // 02/27/2021
        "MMMM d, yyyy",         // June 2, 2022
        "MMMM dd, yyyy",        // June 02, 2022
        "MMM-dd-yyyy",          // Jul-13-2020
        "MMM-d-yyyy",           // Jul-1-2020
        "yyyy-MM-dd",           // 2021-02-27 (ISO format)
        "M/d/yyyy",             // 2/27/2021
        "d-MMM-yyyy",           // 13-Jul-2020
        "dd-MMM-yyyy",          // 13-Jul-2020
        "MMMM d yyyy",          // June 2 2022
        "MMM d, yyyy",          // Jun 2, 2022
        "MMM d yyyy",           // Jun 2 2022
    };

    public DateParsingService(IConfiguration configuration, ILogger<DateParsingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool TryParseDate(string dateString, out DateTime parsedDate)
    {
        parsedDate = default;

        if (string.IsNullOrWhiteSpace(dateString))
        {
            _logger.LogWarning("Empty or null date string provided");
            return false;
        }

        var trimmedDate = dateString.Trim();

        // Try parsing with each supported format
        foreach (var format in SupportedFormats)
        {
            if (DateTime.TryParseExact(
                trimmedDate, 
                format, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out parsedDate))
            {
                _logger.LogDebug("Successfully parsed '{DateString}' using format '{Format}'", trimmedDate, format);
                return true;
            }
        }

        // Try general parsing as a fallback
        if (DateTime.TryParse(trimmedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        {
            _logger.LogDebug("Successfully parsed '{DateString}' using general parsing", trimmedDate);
            return true;
        }

        _logger.LogWarning("Failed to parse date string: '{DateString}'", trimmedDate);
        return false;
    }

    public string ToIsoFormat(DateTime date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public async Task<List<string>> ReadDatesFromFileAsync()
    {
        var filePath = _configuration["WeatherSettings:DatesFilePath"] ?? "dates.txt";
        
        // Resolve relative path from the application base directory
        if (!Path.IsPathRooted(filePath))
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            filePath = Path.GetFullPath(Path.Combine(basePath, filePath));
        }

        _logger.LogInformation("Reading dates from file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("Dates file not found: {FilePath}", filePath);
            throw new FileNotFoundException($"Dates file not found: {filePath}", filePath);
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        var dates = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();

        _logger.LogInformation("Read {Count} date entries from file", dates.Count);
        return dates;
    }

    public bool IsValidCalendarDate(string dateString, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!TryParseDate(dateString, out var parsedDate))
        {
            errorMessage = $"Unable to parse date: '{dateString}'";
            return false;
        }

        // Additional validation for impossible dates like April 31
        // The parsing itself should catch these, but we can add explicit checks
        var year = parsedDate.Year;
        var month = parsedDate.Month;
        var day = parsedDate.Day;

        // Check if the day is valid for the month
        var daysInMonth = DateTime.DaysInMonth(year, month);
        if (day > daysInMonth)
        {
            errorMessage = $"Invalid date: {dateString}. Day {day} is invalid for month {month} (max: {daysInMonth})";
            return false;
        }

        // Check for reasonable date range (not too far in the future for historical API)
        if (parsedDate > DateTime.Today)
        {
            errorMessage = $"Date {dateString} is in the future. Historical weather API only supports past dates.";
            return false;
        }

        // Check for dates too far in the past (Open-Meteo typically has data from 1940)
        if (parsedDate < new DateTime(1940, 1, 1))
        {
            errorMessage = $"Date {dateString} is before 1940. Historical weather data may not be available.";
            return false;
        }

        return true;
    }
}
