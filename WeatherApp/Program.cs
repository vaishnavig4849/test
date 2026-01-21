using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Register HttpClient for weather API calls
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WeatherSettings:OpenMeteoBaseUrl"] ?? "https://archive-api.open-meteo.com/v1/archive");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
builder.Services.AddSingleton<IDateParsingService, DateParsingService>();
builder.Services.AddSingleton<IWeatherService, WeatherService>();

// Add CORS for potential API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
