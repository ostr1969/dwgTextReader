
using DwgCrawler;
using System.Diagnostics;
using System.Text;
using esTest;


internal class Program
{

	static async Task Main(string[] args)
	{
		string rootPath = @"C:\Users\barako\source\repos\ACadSharp\samples";
		string extension = "*._dwg";
		string[] files = Directory.GetFiles(rootPath, extension, SearchOption.AllDirectories);
		var es = new ElasticsearchService(uri: "http://localhost:9200", indexName: "dwg");
		foreach (var file in files)
		{

			var dwg=fileToDwgdata(file);
			try
			{
				await es.IndexArticleAsync(dwg);
				Console.WriteLine($"Indexed: {file}");
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
		// 1. Run the external executable and capture the JSON output
		var args=new DwgCrawler.Utils.ArgStorage { File = path, json = true, summary = false, appid = false,
			blockrecords = false, dimstyles = false, layers = false, linetypes = false, textstyles = false, ucs = false, views = false, vports = false
		};

	
		var p = new DwgParser(args);
		var jsonOutput=p.esStorage.ToLowercaseKeys();
		var dwg=DwgData.FromJObject(jsonOutput);
		return dwg;
		// 2. Send the JSON in a PUT request
		//using (var client = new HttpClient())
		//{
		//	var content = new StringContent(jsonOutput, Encoding.UTF8, "application/json");

		//	// Replace with your actual URL
		//	string url = "http://localhost:9200/dwg/_doc";
		//	Console.WriteLine(jsonOutput);
		//	HttpResponseMessage response = await client.PostAsync(url, content);
		//	string responseContent = await response.Content.ReadAsStringAsync();

		//	Console.WriteLine($"Status: {response.StatusCode}");
		//	Console.WriteLine("Response:");
		//	Console.WriteLine(responseContent);
		//}
	}
	
}