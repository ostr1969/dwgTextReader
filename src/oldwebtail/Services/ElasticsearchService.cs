using ACadSharp.Tables.Collections;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;



namespace webtail.Models
{
	public class ElasticsearchService
	{
		public ElasticClient client;
		private string _dwg_indexName;

		//public  ElasticsearchService(string uri, string indexName)
		//{
		//	_dwg_indexName = indexName;

		//	var settings = new ConnectionSettings(new Uri(uri))
		//		.DefaultIndex(indexName);

		//	client = new ElasticClient(settings);

		//	ensureIndexExists().Wait();
		//}
		public ElasticsearchService(ElasticClient client)
		{
			this.client = client;
			
		}
		public static async Task<ElasticsearchService> CreateAsync(string uri, string dwg_indexname,string pdf_indexname)
		{
			var settings = new ConnectionSettings(new Uri(uri))
							   .DefaultIndex(pdf_indexname);

			var client = new ElasticClient(settings);
			//using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
			{var existsResponse =  client.Indices.Exists(pdf_indexname);

				if (!existsResponse.Exists)
				{
					await client.Indices.CreateAsync(pdf_indexname, c => c
						.Map(m => m
						.Dynamic(true)  // Allows dynamic fields
						.AutoMap()
						));
				}
			}
			//using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(2));
			{ var existsResponse = client.Indices.Exists(dwg_indexname);

				if (!existsResponse.Exists)
				{
					await client.Indices.CreateAsync(dwg_indexname, c => c
					.Map(m => m
						.Dynamic(true)  // Allows dynamic fields
						.AutoMap()
					));
				}
			}

			return new ElasticsearchService(client);
		}
		public void deleteIndex(string index)
		{
			client.Indices.Delete(index);
		}
		public long countIndex(string index)
		{
			var response = client.Count<object>(c => c
				.Index(index));
			return response.Count;

		}
		//private async Task ensureIndexExists()
		//{
		//	//var a = client.Indices.ExistsAsync(_indexName);
		//	//await Task.Delay(1000);
		//	var exists =await client.Indices.ExistsAsync(_dwg_indexName);
			
		//	if (!exists.Exists)
		//	{
		//		await client.Indices.CreateAsync(_dwg_indexName, c => c
		//			.Map<Text>(m => m.AutoMap())
		//		);
		//	}
		//	exists = await client.Indices.ExistsAsync(_dwg_indexName);
		//}

		public async Task IndexArticleAsync(DwgData article)
		{

			var response = await client.IndexAsync(article, i => i.Index("dwg"));
			if (!response.IsValid)
			{
				throw new Exception(response.OriginalException.Message);
			}
			Console.WriteLine($"Indexed: {article.file}");
		}

		public async Task<ISearchResponse<object>> SearchArticlesAsync(string keyword,string index)
		{
			var searchResponse = client.Search<object>(s => s.Index(index)
	.Query(q => q
		.MultiMatch(m => m
		.Query(keyword) // Text to search for within the array elements
			.Fields(f=>f
			.Field("content.value").Field("file").Field("content")

		)))
	.Highlight(k=> k.PreTags("<mark>").PostTags("</mark>")
		.Fields(
			hf => hf.Field("file"),
			hf => hf.Field("content.value"),
			hf => hf.Field("content")
		))
		.Size(100) // Limit the number of results returned
		.Source(src => src.Includes(i => i.Field("content.value").Field("file").Field("id").Field("content")))
// Include only the 'content', 'file',  fields in the response
// Adjust the fields as necessary based on your requirements
// Note: The 'content' field is an array, so it will return all matching elements
);

			return searchResponse;
		}
		public  async Task<bool> FileExists(string filename)
		{
			var searchResponse =await  client.SearchAsync<object>(s => s.Index("dwg")
			.Query(q => q
				.Term(t => t
						.Field( "file.keyword")       // Use the exact field (usually keyword type)
						.Value(filename)      // Must match exactly
					)
				)			
			);
			if (searchResponse.IsValid && searchResponse.Documents.Any())
			{
				return true;
			}
			return false;
		}


	}
	public class DwgData
	{
		public string id { get; set; }
		public string file { get; set; }
		public List<string> layers { get; set; }
		public List<string> textstyles { get; set; }
		public List<Text> content { get; set; }

		public static DwgData FromJObject(Newtonsoft.Json.Linq.JObject jObject)
		{
			var container = new DwgData();
			container.layers = new List<string>();
			container.textstyles = new List<string>();
			container.content = new List<Text>();


			container.id = jObject["id"]?.ToString();
			container.file = jObject["file"]?.ToString();

			// Parse stylesList (array of strings)
			var stylesToken = jObject["textstyles"];
			if (stylesToken != null && stylesToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
			{
				foreach (var style in stylesToken)
					container.textstyles.Add(style.ToString());
			}
			var layersToken = jObject["layers"];
			if (layersToken != null && layersToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
			{
				foreach (var style in layersToken)
					container.layers.Add(style.ToString());
			}

			// Parse objectList (array of objects with "type" and "value")
			var objectListToken = jObject["content"];
			if (objectListToken != null && objectListToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
			{
				foreach (var item in objectListToken)
				{
					var obj = new Text
					{
						type = item["type"]?.ToString(),
						value = item["value"]?.ToString(),
						layer = item["layer"]?.ToString(),
						style = item["style"]?.ToString(),
						prompt = item["prompt"]?.ToString(),
						block = item["block"]?.ToString()

					};
					container.content.Add(obj);
				}
			}

			return container;
		}
	}
	public class Text
	{
		public string type { get; set; }
		public string value { get; set; }
		public string layer { get; set; }
		public string style { get; set; }
		public string prompt { get; set; }
		public string block { get; set; }

	}
}
