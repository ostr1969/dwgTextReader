using FolderMon;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
	.UseWindowsService() // This allows running as a Windows service
	.ConfigureServices(services =>
	{
		services.AddHostedService<Worker>();
	})
	.Build();

host.Run();