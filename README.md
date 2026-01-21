# Weather Data Application

A .NET 8 Blazor Server application that retrieves historical weather data from the Open-Meteo API for Dallas, TX based on dates from an input file.

## 🏗️ Project Structure

```
WeatherApp/
├── WeatherApp.sln              # Solution file
├── dates.txt                   # Input file with dates to process
├── weather-data/               # Cache directory for JSON files (created at runtime)
├── README.md                   # This file
├── AI_NOTES.md                 # AI usage documentation
├── WeatherApp/                 # Main Blazor Server project
│   ├── Controllers/            # API controllers
│   ├── Models/                 # Data models and DTOs
│   ├── Services/               # Business logic services
│   ├── Pages/                  # Blazor pages
│   ├── Shared/                 # Shared Blazor components
│   └── wwwroot/                # Static files
└── WeatherApp.Tests/           # Unit test project
```

## ✅ Features

- **Date Parsing**: Supports multiple date formats:
  - `MM/dd/yyyy` (e.g., 02/27/2021)
  - `MMMM d, yyyy` (e.g., June 2, 2022)
  - `MMM-dd-yyyy` (e.g., Jul-13-2020)
  - ISO format `yyyy-MM-dd`
  
- **Weather Data Retrieval**: Fetches historical weather from Open-Meteo API including:
  - Minimum temperature (°C)
  - Maximum temperature (°C)
  - Precipitation sum (mm)

- **Caching**: Stores API responses in `weather-data/` directory as JSON files to avoid repeat API calls

- **Error Handling**: Gracefully handles:
  - Invalid dates (e.g., April 31)
  - Network failures
  - API errors
  - Empty/missing data

- **Interactive UI**:
  - Loading states with spinner
  - Error messages with dismiss functionality
  - Sortable columns (date, temperature, precipitation)
  - Filterable by status (Success/Cached/Error)
  - Click row details modal

## 🚀 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A modern web browser

## 🔧 Running the Application

### Option 1: Using .NET CLI

1. **Navigate to the solution directory:**
   ```bash
   cd WeatherApp
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   cd WeatherApp
   dotnet run
   ```

5. **Open your browser and navigate to:**
   - UI: https://localhost:5001 or http://localhost:5000
   - API: https://localhost:5001/api/weather

### Option 2: Using Visual Studio

1. Open `WeatherApp.sln` in Visual Studio 2022
2. Set `WeatherApp` as the startup project
3. Press F5 to run

### Option 3: Using Visual Studio Code

1. Open the `WeatherApp` folder in VS Code
2. Press F5 or use the Run and Debug panel
3. Select `.NET Core Launch (web)` configuration

## 🌐 API Endpoints

### GET /api/weather

Returns weather data for all dates in the `dates.txt` file.

**Response Example:**
```json
{
  "results": [
    {
      "originalDate": "02/27/2021",
      "normalizedDate": "2021-02-27",
      "minTemperature": 2.1,
      "maxTemperature": 15.3,
      "precipitation": 0.0,
      "status": "Success",
      "errorMessage": null,
      "fromCache": false
    },
    {
      "originalDate": "April 31, 2022",
      "normalizedDate": null,
      "minTemperature": null,
      "maxTemperature": null,
      "precipitation": null,
      "status": "Error",
      "errorMessage": "Unable to parse date: 'April 31, 2022'",
      "fromCache": false
    }
  ],
  "totalProcessed": 4,
  "successCount": 3,
  "errorCount": 1,
  "cachedCount": 0,
  "timestamp": "2026-01-21T12:00:00Z"
}
```

### GET /api/weather/health

Health check endpoint.

## 🧪 Running Tests

```bash
cd WeatherApp.Tests
dotnet test
```

Or with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Input File Format

The `dates.txt` file should contain one date per line. Example:
```
02/27/2021
June 2, 2022
Jul-13-2020
April 31, 2022
```

## ⚙️ Configuration

Configuration is in `appsettings.json`:

```json
{
  "WeatherSettings": {
    "DatesFilePath": "../dates.txt",
    "WeatherDataDirectory": "../weather-data",
    "OpenMeteoBaseUrl": "https://archive-api.open-meteo.com/v1/archive",
    "DallasLatitude": 32.78,
    "DallasLongitude": -96.8
  }
}
```

## 🌡️ Weather API Details

- **API**: Open-Meteo Historical Weather API
- **Location**: Dallas, TX (lat: 32.78, lon: -96.8)
- **Data fields**: 
  - `temperature_2m_min` - Daily minimum temperature
  - `temperature_2m_max` - Daily maximum temperature
  - `precipitation_sum` - Daily precipitation sum

## 📝 Assumptions

1. **Date formats**: The application supports the specific formats mentioned in the requirements. Dates in other formats may not parse correctly.

2. **Historical data only**: The Open-Meteo historical API only provides data for past dates. Future dates will result in errors.

3. **Single location**: The application is configured for Dallas, TX only. Location coordinates are configurable.

4. **Caching**: Once weather data is cached, it won't be re-fetched unless the cache file is deleted.

5. **Network connectivity**: The application requires internet access to fetch weather data from the API.

6. **Temperature units**: All temperatures are in Celsius (as returned by Open-Meteo API).

7. **Precipitation units**: Precipitation is measured in millimeters.

## 🔐 No Secrets Required

This application uses the free Open-Meteo API which doesn't require authentication or API keys.

## 🐛 Troubleshooting

1. **Dates file not found**: Ensure `dates.txt` exists in the solution root directory.

2. **API errors**: Check internet connectivity and verify the Open-Meteo API is accessible.

3. **Port conflicts**: If ports 5000/5001 are in use, modify `launchSettings.json` or use:
   ```bash
   dotnet run --urls "http://localhost:5050"
   ```

## 📄 License

This project is created for educational/evaluation purposes.
