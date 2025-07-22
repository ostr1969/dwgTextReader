namespace FolderMon
{
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly FileMonitorService _fileMonitorService;

		public Worker(ILogger<Worker> logger)
		{
			_logger = logger;
			_fileMonitorService = new FileMonitorService();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			string pathToWatch = Environment.GetEnvironmentVariable("DWDCRAWLER_FOLDER")
							 ?? @"C:\install\Pdfs";

			_fileMonitorService.StartMonitoring(@"C:\install\Pdfs");

			// Keep running until cancellation requested
			return Task.CompletedTask;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_fileMonitorService.StopMonitoring();
			return base.StopAsync(cancellationToken);
		}
	}
}
