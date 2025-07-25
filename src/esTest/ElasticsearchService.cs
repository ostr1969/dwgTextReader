using ACadSharp.Tables.Collections;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Nest.Infer;


namespace esTest
{
	public class ElasticsearchService
	{
		private ElasticClient client;
		private string _indexName;

		public  ElasticsearchService(string uri, string indexName)
		{
			_indexName = indexName;

			var settings = new ConnectionSettings(new Uri(uri))
				.DefaultIndex(indexName);

			client = new ElasticClient(settings);

			ensureIndexExists().Wait();
		}

		private async Task ensureIndexExists()
		{
			var exists = await client.Indices.ExistsAsync(_indexName);
			if (!exists.Exists)
			{
				await client.Indices.CreateAsync(_indexName, c => c
					.Map<Text>(m => m.AutoMap())
				);
			}
		}

		public async Task IndexArticleAsync(DwgData article)
		{
			
			var response = await client.IndexDocumentAsync(article);
			if (!response.IsValid)
			{
				throw new Exception(response.OriginalException.Message);
			}
			Console.WriteLine($"Indexed: {article.file}");
		}

		public async Task<ISearchResponse<DwgData>> SearchArticlesAsync(string keyword)
		{
			var searchResponse = client.Search<DwgData>(s => s
	.Query(q => q
		.Match(m => m
			.Field("content.value")
			.Query(keyword) // Text to search for within the array elements
		)
	).Highlight(k=>k.Fields(l=>l.Field("content.value")))
		.Size(100) // Limit the number of results returned
		.Source(src => src.Includes(i => i.Field("content.value").Field(f => f.file)))
// Include only the 'content', 'file',  fields in the response
// Adjust the fields as necessary based on your requirements
// Note: The 'content' field is an array, so it will return all matching elements
);

			return searchResponse;
		}
		public  bool FileExists(string filename)
		{
			var searchResponse = client.Search<DwgData>(s => s
	.Query(q => q
		.Match(m => m
			.Field("file")
			.Query(filename) // Text to search for within the array elements
		)
	)
// Include only the 'content', 'file',  fields in the response
// Adjust the fields as necessary based on your requirements
// Note: The 'content' field is an array, so it will return all matching elements
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
