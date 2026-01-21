using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherApp.Services;
using Xunit;

namespace WeatherApp.Tests;

/// <summary>
/// Unit tests for DateParsingService
/// </summary>
public class DateParsingServiceTests
{
    private readonly DateParsingService _service;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<DateParsingService>> _mockLogger;

    public DateParsingServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<DateParsingService>>();
        
        _mockConfig.Setup(c => c["WeatherSettings:DatesFilePath"]).Returns("dates.txt");
        
        _service = new DateParsingService(_mockConfig.Object, _mockLogger.Object);
    }

    #region TryParseDate Tests

    [Theory]
    [InlineData("02/27/2021")]  // MM/dd/yyyy
    [InlineData("2/27/2021")]   // M/d/yyyy
    public void TryParseDate_MMddyyyy_ReturnsTrue(string dateString)
    {
        // Act
        var result = _service.TryParseDate(dateString, out var parsedDate);

        // Assert
        Assert.True(result);
        Assert.Equal(2021, parsedDate.Year);
        Assert.Equal(2, parsedDate.Month);
        Assert.Equal(27, parsedDate.Day);
    }

    [Theory]
    [InlineData("June 2, 2022")]
    [InlineData("June 02, 2022")]
    public void TryParseDate_FullMonthFormat_ReturnsTrue(string dateString)
    {
        // Act
        var result = _service.TryParseDate(dateString, out var parsedDate);

        // Assert
        Assert.True(result);
        Assert.Equal(2022, parsedDate.Year);
        Assert.Equal(6, parsedDate.Month);
        Assert.Equal(2, parsedDate.Day);
    }

    [Fact]
    public void TryParseDate_MMMddyyyy_ReturnsTrue()
    {
        // Arrange
        var dateString = "Jul-13-2020";

        // Act
        var result = _service.TryParseDate(dateString, out var parsedDate);

        // Assert
        Assert.True(result);
        Assert.Equal(2020, parsedDate.Year);
        Assert.Equal(7, parsedDate.Month);
        Assert.Equal(13, parsedDate.Day);
    }

    [Fact]
    public void TryParseDate_ISOFormat_ReturnsTrue()
    {
        // Arrange
        var dateString = "2021-02-27";

        // Act
        var result = _service.TryParseDate(dateString, out var parsedDate);

        // Assert
        Assert.True(result);
        Assert.Equal(2021, parsedDate.Year);
        Assert.Equal(2, parsedDate.Month);
        Assert.Equal(27, parsedDate.Day);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryParseDate_EmptyOrNull_ReturnsFalse(string? dateString)
    {
        // Act
        var result = _service.TryParseDate(dateString!, out _);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("invalid date")]
    [InlineData("not a date")]
    [InlineData("13/32/2021")]
    public void TryParseDate_InvalidFormat_ReturnsFalse(string dateString)
    {
        // Act
        var result = _service.TryParseDate(dateString, out _);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ToIsoFormat Tests

    [Fact]
    public void ToIsoFormat_ValidDate_ReturnsCorrectFormat()
    {
        // Arrange
        var date = new DateTime(2021, 2, 27);

        // Act
        var result = _service.ToIsoFormat(date);

        // Assert
        Assert.Equal("2021-02-27", result);
    }

    [Fact]
    public void ToIsoFormat_SingleDigitMonthAndDay_PadsWithZeros()
    {
        // Arrange
        var date = new DateTime(2022, 1, 5);

        // Act
        var result = _service.ToIsoFormat(date);

        // Assert
        Assert.Equal("2022-01-05", result);
    }

    #endregion

    #region IsValidCalendarDate Tests

    [Theory]
    [InlineData("April 31, 2022")]  // April only has 30 days
    [InlineData("February 30, 2022")]  // February never has 30 days
    [InlineData("February 29, 2021")]  // 2021 is not a leap year
    public void IsValidCalendarDate_InvalidCalendarDate_ReturnsFalse(string dateString)
    {
        // Act
        var result = _service.IsValidCalendarDate(dateString, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(errorMessage);
    }

    [Theory]
    [InlineData("02/27/2021")]
    [InlineData("June 2, 2022")]
    [InlineData("Jul-13-2020")]
    [InlineData("February 29, 2020")]  // 2020 is a leap year
    public void IsValidCalendarDate_ValidDate_ReturnsTrue(string dateString)
    {
        // Act
        var result = _service.IsValidCalendarDate(dateString, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void IsValidCalendarDate_FutureDate_ReturnsFalse()
    {
        // Arrange
        var futureDate = DateTime.Today.AddYears(1).ToString("MM/dd/yyyy");

        // Act
        var result = _service.IsValidCalendarDate(futureDate, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Contains("future", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
