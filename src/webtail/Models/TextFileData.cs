namespace webtail.Models
{
	public class TextFileData
	{
		public string file { get; set; }

		public TxtMetadata metadata { get; set; }
		public string content { get; set; }
		public string searchpath { get; set; }

		
	}
	public class TxtMetadata
	{
		public long Size { get; set; }
		public string Extension { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastModified { get; set; }
		public string FullPath { get; set; }
	}
}
