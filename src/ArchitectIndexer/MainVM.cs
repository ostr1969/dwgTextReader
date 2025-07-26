using DwgCrawler;
using Microsoft.Web.WebView2.WinForms;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArchitectIndexer
{
	public class MainVM: BindableBase
	{
		public DelegateCommand _folderBrowser { get; set; }
		public DelegateCommand _startIndexing { get; set; }
		public DelegateCommand _search { get; set; }

		private string _folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
		private Task _ensureIndexExistsTask;

		public MainVM()
		{
			folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
			Query = "Enter your search query here...";
			_folderBrowser = new DelegateCommand(OpenFolderBrowser);
			_startIndexing = new DelegateCommand(async () => await StartIndexing());
			_search = new DelegateCommand(async () => await Search());
			_ensureIndexExistsTask = GetElasticsearchService(); // Ensure the service is initialized
		}
		public string folder 
		{
			get { return _folder; }
			set { SetProperty(ref _folder, value); }
		}
		public WebView2 webview;
		
		public async Task Search()
		{
			_ensureIndexExistsTask.Wait();
			var results = await es.SearchArticlesAsync(Query);

			foreach (var hit in results.Hits)
				Debug.Print(hit.Highlight["content.value"].ToString());

		}
		private ElasticsearchService es;
		public async Task<ElasticsearchService> GetElasticsearchService()
		{
			if (es == null)
			{
				es = await ElasticsearchService.CreateAsync(uri: "http://localhost:9200", indexName: "dwg");
			}
			return es;
		}
		public async Task StartIndexing()
		{
			_ensureIndexExistsTask.Wait();
			if (string.IsNullOrEmpty(folder))
			{
				MessageBox.Show("Please select a folder to index.");
				return;
			}
			string rootPath = folder;
			string extension = "*.dwg";
			string[] files = Directory.GetFiles(rootPath, extension, SearchOption.AllDirectories);
			
			int newfiles = files.Length;
			foreach (var file in files)
			{
				if (es.FileExists(file))
				{
					Console.WriteLine($"File already indexed: {file}");
					newfiles--;
					continue;
				}
				var dwg = fileToDwgdata(file);
				try
				{
					await es.IndexArticleAsync(dwg);

				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error indexing {file}: {ex.Message}");
				}

			}
			MessageBox.Show($"Indexing completed with {newfiles} new and total {files.Length} dwg files.");
		}
		static DwgData fileToDwgdata(string path)
		{
			// 1. Run the external executable and capture the JSON output
			


			var p = DwgParser.JsonFromDwg(path);
			var jsonOutput = p.esStorage.ToLowercaseKeys();
			var dwg = DwgData.FromJObject(jsonOutput);
			return dwg;
			
		}
		public void  OpenFolderBrowser()
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				var result = dialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
					folder = dialog.SelectedPath;
				else
					folder = null;
			}
		}

		private string _query = "";
		public string Query
		{
			get { return _query; }
			set { SetProperty(ref _query, value); }
		}
		
	}
}
