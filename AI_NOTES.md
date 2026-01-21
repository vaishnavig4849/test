# AI Development Notes

This document describes the AI tools used during development and reflections on the development process.

## 🤖 AI Tools Used

- **GitHub Copilot (Claude Opus 4.5)**: Used for code generation, architecture decisions, and documentation creation.
- **IDE**: Visual Studio Code with Copilot extension

## 💡 Most Helpful Prompts

### 1. Architecture and Project Setup
**Prompt**: "Create an end-to-end .NET 6+ backend + mandatory web UI that reads dates from dates.txt (multiple formats), validates, normalizes to ISO yyyy-MM-dd, for each valid date calls Open-Meteo Historical Weather API for Dallas, TX..."

**Why it was helpful**: This comprehensive prompt allowed the AI to understand the full scope of the project and generate a well-structured solution with proper separation of concerns (services, controllers, models).

### 2. Date Parsing Implementation
**Prompt**: "Date parsing: support formats like MM/dd/yyyy (02/27/2021), 'MMMM d, yyyy' (June 2, 2022), 'MMM-dd-yyyy' (Jul-13-2020). Use invariant culture; validate real calendar dates; invalid -> error entry."

**Why it was helpful**: Specific format requirements helped the AI generate a robust `DateParsingService` with proper use of `DateTime.TryParseExact` and `CultureInfo.InvariantCulture`, avoiding common date parsing issues.

### 3. Error Handling Strategy
**Prompt**: "Handle: invalid dates, network/API failures, empty/missing data, date ranges returning no data. Handles invalid dates (e.g., April 31) gracefully without crashing; records an error."

**Why it was helpful**: This explicit error handling requirement ensured the AI generated comprehensive try-catch blocks and validation logic throughout the codebase, making the application resilient.

## ❌ Example Where AI Was Wrong and Correction

### Issue: Initial Date Validation for "April 31"

**What AI generated initially**: The AI initially used only `DateTime.TryParse` which silently accepts "April 31, 2022" and converts it to "May 1, 2022" (date rollover behavior).

**The problem**: This didn't meet the requirement to treat "April 31" as an invalid date and record an error.

**How I corrected it**: I added explicit validation in `IsValidCalendarDate` method to:
1. First parse the date to extract year, month, and day
2. Check if the original day is valid for that month using `DateTime.DaysInMonth()`
3. Return an error if the day exceeds the maximum for that month

```csharp
// Added validation to catch invalid calendar dates
var daysInMonth = DateTime.DaysInMonth(year, month);
if (day > daysInMonth)
{
    errorMessage = $"Invalid date: {dateString}. Day {day} is invalid for month {month} (max: {daysInMonth})";
    return false;
}
```

**Note**: After testing, I discovered that `DateTime.TryParseExact` with strict parsing actually rejects "April 31, 2022" outright (returns false), which is the desired behavior. The validation layer adds an extra safety check and provides clearer error messages.

## ✍️ What I Wrote Myself and Why

### 1. Configuration Path Resolution

I manually refined the path resolution logic for `DatesFilePath` and `WeatherDataDirectory` to work correctly regardless of whether the application runs from the project directory or bin folder:

```csharp
var basePath = AppDomain.CurrentDomain.BaseDirectory;
var relativePath = _configuration["WeatherSettings:WeatherDataDirectory"] ?? "../weather-data";
_weatherDataDirectory = Path.GetFullPath(Path.Combine(basePath, relativePath));
```

**Why**: The AI-generated code assumed the application always runs from the project root, but in production, it runs from `bin/Debug/net8.0/`. This required understanding .NET's runtime directory structure.

### 2. Blazor UI State Management

I refined the sorting and filtering logic in the Blazor component to ensure proper state updates:

```csharp
private void SortBy(string field)
{
    if (sortField == field)
    {
        sortAscending = !sortAscending;
    }
    else
    {
        sortField = field;
        sortAscending = true;
    }
}
```

**Why**: The AI's initial implementation didn't toggle sort direction when clicking the same column twice, which is expected UX behavior.

### 3. HttpClient Configuration

I ensured the HttpClient was properly configured with both the factory pattern and direct instantiation in Blazor:

```csharp
// In Program.cs - for API service
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// In Index.razor - for calling our own API
using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
```

**Why**: The AI mixed patterns that would cause issues. The weather service needs `IHttpClientFactory` for calling external APIs, while the Blazor component needs a simple client to call our internal API.

## 📊 Summary

| Aspect | AI Contribution | Manual Refinement |
|--------|-----------------|-------------------|
| Project structure | 95% | 5% - directory naming |
| Models/DTOs | 100% | 0% |
| DateParsingService | 85% | 15% - validation logic |
| WeatherService | 80% | 20% - path resolution, caching |
| API Controller | 95% | 5% - error responses |
| Blazor UI | 75% | 25% - state management, UX |
| Unit Tests | 90% | 10% - edge cases |
| Documentation | 85% | 15% - corrections |

## 🎯 Key Learnings

1. **Be specific in prompts**: The more specific the requirements, the better the generated code.
2. **Verify edge cases**: AI may not handle all edge cases (like "April 31") without explicit guidance.
3. **Understand generated code**: Always review and understand what the AI generates before using it.
4. **Test thoroughly**: AI-generated code needs the same testing rigor as human-written code.
