
using DwgCrawler;
using System.Diagnostics;
using System.Text;
using esTest;


internal class Program
{

	static async Task Main(string[] args)
	{
		string rootPath = @"C:\Users\barako\source\repos\ACadSharp\samples";
		string extension = "*.dwg";
		string[] files = Directory.GetFiles(rootPath, extension, SearchOption.AllDirectories);
		var es = new ElasticsearchService(uri: "http://localhost:9200", indexName: "dwg");
		foreach (var file in files)
		{
			if (es.FileExists(file))
			{
				Console.WriteLine($"File already indexed: {file}");
				continue;
			}
			var dwg=fileToDwgdata(file);
			try
			{
				await es.IndexArticleAsync(dwg);
				
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error indexing {file}: {ex.Message}");
			}

		}
		
		
		
		
		var results = await es.SearchArticlesAsync("ביוב");

	}
	static DwgData fileToDwgdata(string path)
	{
		


		var p = DwgParser.JsonFromDwg(path);
		var jsonOutput=p.esStorage;
		var dwg=DwgData.FromJObject(jsonOutput);
		return dwg;
		
	}
	
}