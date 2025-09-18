using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UglyToad.PdfPig;
namespace webtail.Models
{
	class PdfParser
	{
		public JObject PdfData { get; private set; }
		public PdfParser(string pdfPath)
		{
			// pdfPath = @"yourfile.pdf"; // Replace with your file path

			 PdfData = ReadPdfToJObject(pdfPath);

			//Console.WriteLine(pdfData.ToString());
		}

		static JObject ReadPdfToJObject(string filePath)
		{
			var result = new JObject();
			var metadata = new JObject();
			string content = "";

			using (PdfDocument document = PdfDocument.Open(filePath))
			{
				// Metadata
				var info = document.Information;
				
				metadata["title"] = info.Title ?? "";
				metadata["author"] = info.Author ?? "";
				metadata["subject"] = info.Subject ?? "";
				metadata["keywords"] = info.Keywords ?? "";
				metadata["creator"] = info.Creator ?? "";
				metadata["producer"] = info.Producer ?? "";
				metadata["creationDate"] = info.CreationDate ?? "";
				metadata["modificationDate"] = info.ModifiedDate ?? "";

				// Content
				foreach (var page in document.GetPages())
				{
					content += page.Text + "\n";
				}
			}

			result["metadata"] = metadata;
			result["content"] = content;
			result["file"] = filePath;

			return result;
		}
	}

}