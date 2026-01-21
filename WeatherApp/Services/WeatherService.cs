using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Service for retrieving and caching weather data from Open-Meteo API
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDateParsingService _dateParsingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;
    private readonly string _weatherDataDirectory;
    private readonly double _latitude;
    private readonly double _longitude;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public WeatherService(
        IHttpClientFactory httpClientFactory,
        IDateParsingService dateParsingService,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dateParsingService = dateParsingService;
        _configuration = configuration;
        _logger = logger;

        // Get configuration values
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var relativePath = _configuration["WeatherSettings:WeatherDataDirectory"] ?? "weather-data";
        _weatherDataDirectory = Path.GetFullPath(Path.Combine(basePath, relativePath));
        
        // If directory doesn't exist at the resolved path, try from current directory
        if (!Directory.Exists(_weatherDataDirectory))
        {
            var currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), "weather-data");
            if (Directory.Exists(Path.GetDirectoryName(currentDirPath)))
            {
                _weatherDataDirectory = currentDirPath;
            }
        }
        
        _latitude = double.Parse(_configuration["WeatherSettings:DallasLatitude"] ?? "32.78");
        _longitude = double.Parse(_configuration["WeatherSettings:DallasLongitude"] ?? "-96.8");

        // Ensure the weather data directory exists
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_weatherDataDirectory))
        {
            _logger.LogInformation("Creating weather data directory: {Directory}", _weatherDataDirectory);
            Directory.CreateDirectory(_weatherDataDirectory);
        }
    }

    public async Task<WeatherApiResponse> GetAllWeatherDataAsync()
    {
        var response = new WeatherApiResponse();

        try
        {
            // Read dates from file
            var dateStrings = await _dateParsingService.ReadDatesFromFileAsync();
            
            foreach (var dateString in dateStrings)
            {
                var result = await ProcessDateAsync(dateString);
                response.Results.Add(result);
            }

            // Calculate summary statistics
            response.TotalProcessed = response.Results.Count;
            response.SuccessCount = response.Results.Count(r => r.Status == "Success" || r.Status == "Cached");
            response.ErrorCount = response.Results.Count(r => r.Status == "Error");
            response.CachedCount = response.Results.Count(r => r.FromCache);
            response.Timestamp = DateTime.UtcNow;

            _logger.LogInformation(
                "Processed {Total} dates: {Success} successful, {Errors} errors, {Cached} from cache",
                response.TotalProcessed, response.SuccessCount, response.ErrorCount, response.CachedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing weather data");
            throw;
        }

        return response;
    }

    private async Task<WeatherResult> ProcessDateAsync(string dateString)
    {
        var result = new WeatherResult { OriginalDate = dateString };

        try
        {
            // Validate the date
            if (!_dateParsingService.IsValidCalendarDate(dateString, out var validationError))
            {
                result.Status = "Error";
                result.ErrorMessage = validationError;
                _logger.LogWarning("Invalid date: {Date} - {Error}", dateString, validationError);
                return result;
            }

            // Parse the date
            if (!_dateParsingService.TryParseDate(dateString, out var parsedDate))
            {
                result.Status = "Error";
                result.ErrorMessage = $"Failed to parse date: {dateString}";
                return result;
            }

            return await GetWeatherForDateAsync(dateString, parsedDate);
        }
        catch (Exception ex)
        {
            result.Status = "Error";
            result.ErrorMessage = $"Unexpected error processing date: {ex.Message}";
            _logger.LogError(ex, "Unexpected error processing date: {Date}", dateString);
            return result;
        }
    }

    public async Task<WeatherResult> GetWeatherForDateAsync(string originalDate, DateTime parsedDate)
    {
        var isoDate = _dateParsingService.ToIsoFormat(parsedDate);
        var result = new WeatherResult
        {
            OriginalDate = originalDate,
            NormalizedDate = isoDate
        };

        try
        {
            // Check cache first
            var cachedFilePath = GetCacheFilePath(isoDate);
            if (File.Exists(cachedFilePath))
            {
                _logger.LogInformation("Loading cached data for {Date}", isoDate);
                return await LoadFromCacheAsync(cachedFilePath, originalDate);
            }

            // Fetch from API
            var weatherData = await FetchWeatherFromApiAsync(isoDate);
            
            if (weatherData?.Daily != null && 
                weatherData.Daily.Time?.Count > 0 &&
                weatherData.Daily.TemperatureMin?.Count > 0 &&
                weatherData.Daily.TemperatureMax?.Count > 0 &&
                weatherData.Daily.PrecipitationSum?.Count > 0)
            {
                result.MinTemperature = weatherData.Daily.TemperatureMin[0];
                result.MaxTemperature = weatherData.Daily.TemperatureMax[0];
                result.Precipitation = weatherData.Daily.PrecipitationSum[0];
                result.Status = "Success";
                result.FromCache = false;

                // Save to cache
                await SaveToCacheAsync(cachedFilePath, weatherData);
            }
            else
            {
                result.Status = "Error";
                result.ErrorMessage = "No weather data returned from API";
                _logger.LogWarning("No data returned for {Date}", isoDate);
            }
        }
        catch (HttpRequestException ex)
        {
            result.Status = "Error";
            result.ErrorMessage = $"Network error: {ex.Message}";
            _logger.LogError(ex, "Network error fetching weather for {Date}", isoDate);
        }
        catch (TaskCanceledException ex)
        {
            result.Status = "Error";
            result.ErrorMessage = "Request timeout";
            _logger.LogError(ex, "Timeout fetching weather for {Date}", isoDate);
        }
        catch (Exception ex)
        {
            result.Status = "Error";
            result.ErrorMessage = $"Error: {ex.Message}";
            _logger.LogError(ex, "Error fetching weather for {Date}", isoDate);
        }

        return result;
    }

    private async Task<OpenMeteoResponse?> FetchWeatherFromApiAsync(string isoDate)
    {
        var client = _httpClientFactory.CreateClient("OpenMeteo");
        
        var queryString = $"?latitude={_latitude}&longitude={_longitude}" +
                         $"&start_date={isoDate}&end_date={isoDate}" +
                         "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
                         "&timezone=auto";

        var baseUrl = _configuration["WeatherSettings:OpenMeteoBaseUrl"] ?? "https://archive-api.open-meteo.com/v1/archive";
        var fullUrl = baseUrl + queryString;

        _logger.LogInformation("Fetching weather data from: {Url}", fullUrl);

        var response = await client.GetAsync(fullUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("API Response: {Content}", content);

        return JsonSerializer.Deserialize<OpenMeteoResponse>(content, JsonOptions);
    }

    private async Task<WeatherResult> LoadFromCacheAsync(string filePath, string originalDate)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var weatherData = JsonSerializer.Deserialize<OpenMeteoResponse>(json, JsonOptions);

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var result = new WeatherResult
        {
            OriginalDate = originalDate,
            NormalizedDate = fileName,
            FromCache = true,
            Status = "Cached"
        };

        if (weatherData?.Daily != null &&
            weatherData.Daily.TemperatureMin?.Count > 0 &&
            weatherData.Daily.TemperatureMax?.Count > 0 &&
            weatherData.Daily.PrecipitationSum?.Count > 0)
        {
            result.MinTemperature = weatherData.Daily.TemperatureMin[0];
            result.MaxTemperature = weatherData.Daily.TemperatureMax[0];
            result.Precipitation = weatherData.Daily.PrecipitationSum[0];
        }
        else
        {
            result.Status = "Error";
            result.ErrorMessage = "Cached data is incomplete";
        }

        return result;
    }

    private async Task SaveToCacheAsync(string filePath, OpenMeteoResponse weatherData)
    {
        var json = JsonSerializer.Serialize(weatherData, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("Saved weather data to cache: {FilePath}", filePath);
    }

    private string GetCacheFilePath(string isoDate)
    {
        return Path.Combine(_weatherDataDirectory, $"{isoDate}.json");
    }

    public bool IsCached(string isoDate)
    {
        return File.Exists(GetCacheFilePath(isoDate));
    }
}
