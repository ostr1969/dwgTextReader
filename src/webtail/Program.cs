using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using webtail.Data;
using webtail.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRadzenComponents();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.Configure<CrawlerOptions>(builder.Configuration.GetSection("Crawler"));

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

app.Run();
