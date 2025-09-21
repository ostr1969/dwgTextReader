using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using webtail.Data;
using webtail.Models;
using webtail.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRadzenComponents();
builder.Services.AddSingleton<FileService>();
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.Configure<CrawlerOptions>(builder.Configuration.GetSection("Crawler"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
app.MapFallbackToPage("/_Host");
Console.WriteLine("WEBTAILS indexer Server started:");
foreach (var address in app.Urls)
{
	Console.WriteLine($"🚀 Server started at: {address}");
}

app.Run();

//dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile = true - o.\published
