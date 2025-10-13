namespace DwgTextract.Models
{
	public class CrawlerOptions
	{
		public string? DefaultFolder { get; set; }
		public string DwgFontFolder {  get; set; }
		public string Words {  get; set; }
		public string ElasticsearchUri {  get; set; }
	}
}
