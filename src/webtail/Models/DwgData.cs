using ACadSharp;
using System.Text;

namespace webtail.Models
{
	public class DwgData
	{
		public string id { get; set; }
		public string file { get; set; }
		public List<string> layers { get; set; }
		public List<string> textstyles { get; set; }
		public List<string> imagenames { get; set; }
		public List<DwgText> contentaslist { get; set; }
		public CadSummaryInfo metadata { get; set; }
		public string content { get; set; }
		public string contentrev { get; set; }
		public string searchpath { get; set; }

		public static DwgData FromParser(DwgParser parser)
		{
			var container = new DwgData();
			container.layers = parser.Layers;
			container.textstyles = parser.TextStyles;
			container.file = parser.Filename;
			container.contentaslist = parser.dwgTexts;
			container.metadata = parser.CadSummaryInfo;
			container.imagenames = parser.ImageNames;
			StringBuilder sb = new StringBuilder();
			StringBuilder sbrev = new StringBuilder();
			foreach (var obj in parser.dwgTexts)
			{
				if (obj.prompt?.Length>1)
				{sb.Append(obj.prompt); sb.Append(' '); }
				if (obj.value.Length > 1)
					{sb.Append(obj.value); sb.Append(' '); }
				if (obj.value?.Length > 1 && obj.value!=obj.valuerev)
					{sbrev.Append(obj.valuerev); sbrev.Append(' '); }
				if (obj.prompt?.Length > 1 && obj.value != obj.valuerev)
					{sbrev.Append(obj.prompt.Rev()); sbrev.Append(' '); }

			}
			container.content = sb.ToString();
			container.contentrev = sbrev.ToString();
			return container;
		}
		public static DwgData FromJObject(Newtonsoft.Json.Linq.JObject jObject)
		{
			var container = new DwgData();
			container.layers = new List<string>();
			container.textstyles = new List<string>();
			container.contentaslist = new List<DwgText>();
			StringBuilder sb = new StringBuilder();
			StringBuilder sbrev = new StringBuilder();

			container.id = jObject["id"]?.ToString();
			container.file = jObject["file"]?.ToString();
			//container.metadata = jObject["summaryinfo"]?.ToString();

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
					var obj = new DwgText
					{
						type = item["type"]?.ToString(),
						value = item["value"]?.ToString(),
						valuerev = item["valuerev"]?.ToString(),
						layer = item["layer"]?.ToString(),
						style = item["style"]?.ToString(),
						prompt = item["prompt"]?.ToString(),
						block = item["block"]?.ToString()

					};
					container.contentaslist.Add(obj);
					sb.Append(obj.prompt); sb.Append(' ');
					sb.Append(obj.value); sb.Append(' ');

					sbrev.Append(obj.prompt); sb.Append(' ');
					sbrev.Append(obj.value); sb.Append(' ');
				}
			}
			container.content = sb.ToString();
			return container;
		}
	}
}
