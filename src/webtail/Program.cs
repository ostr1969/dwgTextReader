using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using webtail.Data;
using webtail.Models;
using webtail.Services;
using static webtail.Pages.EsSearch;


//Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessDataPath, EnvironmentVariableTarget.User);
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();
builder.Services.AddRadzenComponents();
builder.Services.AddSingleton<FileService>();
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.Configure<CrawlerOptions>(builder.Configuration.GetSection("Crawler"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ResultState>();
builder.Services.AddSingleton<FileService>(); // handles random file creation
//builder.WebHost.UseUrls("http://localhost:5000");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapControllers(); // <-- This enables your FileController endpoints
app.MapFallbackToPage("/_Host");
Console.WriteLine("WEBTAILS indexer Server started:");
foreach (var address in app.Urls)
{
	Console.WriteLine($"🚀 Server started at: {address}");
}

app.Run();


//dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\published  .\webtail\webtail.csproj
