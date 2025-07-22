using System;
using System.IO;
using System.ServiceProcess;

public class FileMonitorService
{
	private FileSystemWatcher _watcher;

	public void StartMonitoring(string pathToWatch)
	{
		_watcher = new FileSystemWatcher
		{
			Path = pathToWatch,
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
			Filter = "*.*",
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
}