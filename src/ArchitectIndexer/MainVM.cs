using DwgCrawler;
using Microsoft.Web.WebView2.WinForms;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public DelegateCommand<string> _delIndex { get; set; }
		public DelegateCommand _startIndexing { get; set; }
		public DelegateCommand _search { get; set; }

		private string _folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
		private Task _ensureIndexExistsTask;

		public MainVM()
		{
			folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
			Query = "";
			_folderBrowser = new DelegateCommand(OpenFolderBrowser);
			_startIndexing = new DelegateCommand(async () => await StartIndexing());
			_delIndex = new DelegateCommand<string>(DelIndexItem);
			_search = new DelegateCommand(async () => await Search());
			_ensureIndexExistsTask = GetElasticsearchService(); // Ensure the service is initialized
			_ensureIndexExistsTask.Wait();
			DwgCount =es.countIndex("dwg");
			PdfCount=es.countIndex("pdf");
		}
		public string folder 
		{
			get { return _folder; }
			set { SetProperty(ref _folder, value); }
		}
		private long _pdfCount;
		public long PdfCount
		{
			get => _pdfCount;
			set
			{
				SetProperty(ref _pdfCount, value);

			}
		}
		private long _dwgCount;
		public long DwgCount
		{
			get => _dwgCount;
			set
			{
				SetProperty(ref _dwgCount, value);

			}
		}

		public void DelIndexItem(string index)
		{
			
				es.deleteIndex(index);
			DwgCount = es.countIndex("dwg");
			PdfCount = es.countIndex("pdf");


		}

		private ObservableCollection<Presenter> _searchresults = new ObservableCollection<Presenter>();
		public ObservableCollection<Presenter> SearchResults
		{
			get => _searchresults;
			set => SetProperty(ref _searchresults, value);
		}
		private Presenter _selectedResult;
		public Presenter SelectedResult
		{
			get => _selectedResult;
			set
			{
				SetProperty(ref _selectedResult, value);
				
			}
		}
		public async Task Search()
		{
			_ensureIndexExistsTask.Wait();
			var results = await es.SearchArticlesAsync(Query);
			SearchResults.Clear();
			string HtmlContent = "";
			foreach (var hit in results.Hits)
				{if (hit.Highlight.ContainsKey("file"))
					HtmlContent = "<h4>File: " + hit.Highlight["file"].FirstOrDefault() + "</h4>";
				if (hit.Highlight.ContainsKey("content.value"))				
					foreach (var highlight in hit.Highlight["content.value"])
					{
						HtmlContent += "\n" + highlight;
					}
				SearchResults.Add(new Presenter { DwgData=hit.Source, HtmlContent= HtmlContent });
				
				
			}
			    

		}
		private ElasticsearchService es;
		public async Task<ElasticsearchService> GetElasticsearchService()
		{
			if (es == null)
			{
				es = await ElasticsearchService.CreateAsync(uri: "http://localhost:9200",  dwg_indexname: "dwg",pdf_indexname:"pdf");
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
			var extensions = new[] { ".dwg" };
			//string[] files = Directory.GetFiles(rootPath, extensions, SearchOption.AllDirectories);
			var files = Directory
				.GetFiles(folder)
				.Where(file => extensions.Any(file.ToLower().EndsWith))
				.ToArray();
			int newfiles = files.Length;
			foreach (var file in files)
			{
				var fileObj=new FileInfo(file);
				if (await es.FileExists(file))
				{
					Console.WriteLine($"File already indexed: {file}");
					newfiles--;
					continue;
				}
				if (fileObj.Extension==".pdf")
				{
					var pdfParser = new PdfParser(file);
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
			DwgCount = es.countIndex("dwg");
			PdfCount = es.countIndex("pdf");
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
