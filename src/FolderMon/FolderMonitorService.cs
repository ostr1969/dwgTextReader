using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;

public class FileMonitorService
{
	private FileSystemWatcher _watcher;

	public void StartMonitoring(string pathToWatch)
	{
		_watcher = new FileSystemWatcher
		{
			Path = pathToWatch,
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
			Filter = "*.dwg",
			EnableRaisingEvents = true
		};

		_watcher.Created += OnChanged;
		_watcher.Changed += OnChanged;
	}

	private void OnChanged(object sender, FileSystemEventArgs e)
	{
		// Example action
		File.AppendAllText(@"C:\Users\barako\Documents\file_events.log", $"{DateTime.Now}: {e.ChangeType} - {e.FullPath}\n");

	}

	public void StopMonitoring()
	{
		_watcher?.Dispose();
	}
	static async Task esAppend(string path)
	{
		// 1. Run the external executable and capture the JSON output
		var startInfo = new ProcessStartInfo
		{
			FileName = "C:\\Users\\barako\\source\\repos\\ACadSharp\\src\\ACadSharp.Examples\\bin\\Debug\\net6.0-windows\\DwgCrawler.exe", // Replace with the actual executable path
			Arguments = $"-j -f \"{path}\"", // Add arguments if needed
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		string jsonOutput;
		using (var process = Process.Start(startInfo))
		{
			jsonOutput = await process.StandardOutput.ReadToEndAsync();
			process.WaitForExit();
		}

		// 2. Send the JSON in a PUT request
		using (var client = new HttpClient())
		{
			var content = new StringContent(jsonOutput, Encoding.UTF8, "application/json");

			// Replace with your actual URL
			string url = "https://your-api-endpoint.com/resource";

			HttpResponseMessage response = await client.PutAsync(url, content);
			string responseContent = await response.Content.ReadAsStringAsync();

			Console.WriteLine($"Status: {response.StatusCode}");
			Console.WriteLine("Response:");
			Console.WriteLine(responseContent);
		}
	}
}