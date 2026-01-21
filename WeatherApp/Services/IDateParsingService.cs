namespace WeatherApp.Services;

/// <summary>
/// Service interface for parsing date strings into normalized ISO format
/// </summary>
public interface IDateParsingService
{
    /// <summary>
    /// Parses a date string in various formats to a DateTime object
    /// </summary>
    /// <param name="dateString">The date string to parse</param>
    /// <param name="parsedDate">The parsed DateTime if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    bool TryParseDate(string dateString, out DateTime parsedDate);

    /// <summary>
    /// Converts a DateTime to ISO format string (yyyy-MM-dd)
    /// </summary>
    /// <param name="date">The date to format</param>
    /// <returns>ISO formatted date string</returns>
    string ToIsoFormat(DateTime date);

    /// <summary>
    /// Reads all date lines from the input file
    /// </summary>
    /// <returns>List of date strings from the file</returns>
    Task<List<string>> ReadDatesFromFileAsync();

    /// <summary>
    /// Validates if a date is a real calendar date (e.g., catches April 31)
    /// </summary>
    /// <param name="dateString">The date string to validate</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if date is valid, false otherwise</returns>
    bool IsValidCalendarDate(string dateString, out string errorMessage);
}
