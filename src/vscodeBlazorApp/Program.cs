using Microsoft.Extensions.DependencyInjection;
using Nest;
using Tewr.Blazor.FileReader;
using vscodeBlazorApp.Components;
using vscodeBlazorApp.Components.Models;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();     // Needed for _Host.cshtml
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<ElasticClient>(sp =>
{
    var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
    return new ElasticClient(settings);
});

//builder.Services.AddFileReaderService(options => options.UseWasmSharedBuffer = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
