using ACadSharp;
using ACadSharp.Tables.Collections;
using Nest;
using Newtonsoft.Json.Linq;
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

		
		public ElasticsearchService(ElasticClient client)
		{
			this.client = client;
			
		}
		public static async Task<ElasticsearchService> CreateAsync(string uri, List<string> indexes)
		{
			var settings = new ConnectionSettings(new Uri(uri))
							   .DefaultIndex(indexes[0]).RequestTimeout(TimeSpan.FromSeconds(2));

			var client = new ElasticClient(settings);
			var pingResponse = client.Ping();

			if (pingResponse.IsValid)
			{
				Console.WriteLine("Elasticsearch is alive ✅");
			}
			else
			{
				return null;
			}
			foreach (var ind in indexes)
			
				{var existsResponse =  client.Indices.Exists(ind);

				if (!existsResponse.Exists)
				{
					var res = await client.Indices.CreateAsync(ind, c => c
						.Map(m => m
						.Dynamic(true)  // Allows dynamic fields
						.AutoMap()
						));
				}
			}
			
			
			

			return new ElasticsearchService(client);
		}

		public static async Task<ElasticsearchService> GetAsync(string uri, string dwg_indexname, string pdf_indexname)
		{
			var settings = new ConnectionSettings(new Uri(uri))
							   .DefaultIndex(pdf_indexname).RequestTimeout(TimeSpan.FromSeconds(2));

			var client = new ElasticClient(settings);
			var pingResponse = client.Ping();

			if (pingResponse.IsValid)
			{
				Console.WriteLine("Elasticsearch is alive ✅");
			}
			else
			{
				return null;
			}

			var existsResponse = client.Indices.Exists(pdf_indexname);

			if (!existsResponse.Exists)
			{
				return null;
			}

			//using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(2));
			existsResponse = client.Indices.Exists(dwg_indexname);

			if (!existsResponse.Exists)
			{
				return null;
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
		

		public async Task IndexDwgAsync(DwgData dwg)
		{

			var response = await client.IndexAsync(dwg, i => i.Index("dwg"));
			if (!response.IsValid)
			{
				throw new Exception(response.OriginalException.Message);
			}
			Console.WriteLine($"Indexed: {dwg.file}");
		}
		public async Task IndexDwgAsync(TextFileData txt)
		{

			var response = await client.IndexAsync(txt, i => i.Index("other"));
			if (!response.IsValid)
			{
				throw new Exception(response.OriginalException.Message);
			}
			Console.WriteLine($"Indexed: {txt.file}");
		}

		public async Task<ISearchResponse<object>> SearchArticlesAsync(string keyword,string index, Operator op)
		{
			var searchResponse = client.Search<object>(s => s.Index(index)
	.Query(q => q
		.MultiMatch(m => m
		.Query(keyword).Operator(op) // Text to search for within the array elements
			.Fields(f=>f
			.Field("file").Field("content").Field("contentrev")

		)))
	.Highlight(k=> k.PreTags("<mark>").PostTags("</mark>").MaxAnalyzedOffset(10000)
		.Fields(
			hf => hf.Field("file"),
			
			hf => hf.Field("content"),
			hf => hf.Field("contentrev")
		))
		.Size(100) // Limit the number of results returned
		.Source(src => src.Includes(i => i.Field("contentrev").Field("file").Field("id").Field("content").Field("metadata")))
// Include only the 'content', 'file',  fields in the response
// Adjust the fields as necessary based on your requirements
// Note: The 'content' field is an array, so it will return all matching elements
);

			return searchResponse;
		}
		public  async Task<bool> FileExists(string filename,string ext)
		{
			ext = ext.Replace(".", "");
			var searchResponse =await  client.SearchAsync<object>(s => s.Index(ext)
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
		public async Task<List<string>> GetAllPaths()
		{
			List<string> paths=new List<string>();
			var response = await client.SearchAsync<object>(s => s
		   .Size(0) // don't return documents, only aggregation
		   .Aggregations(a => a
			   .Terms("unique_orgpaths", t => t
				   .Field("searchpath.keyword") // use keyword to aggregate
				   .Size(10000) // adjust if you expect many unique values
			   )
		   )
	   );

			var buckets = response.Aggregations.Terms("unique_orgpaths").Buckets;
			foreach (var b in buckets)
				paths.Add(b.Key);
			return paths;
		}


	}
	
	
}
