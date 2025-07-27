using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UglyToad.PdfPig;
namespace ArchitectIndexer
{
	class PdfParser
	{
		public PdfParser(string pdfPath)
		{
			// pdfPath = @"yourfile.pdf"; // Replace with your file path

			JObject pdfData = ReadPdfToJObject(pdfPath);

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
				metadata["Title"] = info.Title ?? "";
				metadata["Author"] = info.Author ?? "";
				metadata["Subject"] = info.Subject ?? "";
				metadata["Keywords"] = info.Keywords ?? "";
				metadata["Creator"] = info.Creator ?? "";
				metadata["Producer"] = info.Producer ?? "";
				metadata["CreationDate"] = info.CreationDate ?? "";
				metadata["ModificationDate"] = info.ModifiedDate ?? "";

				// Content
				foreach (var page in document.GetPages())
				{
					content += page.Text + "\n";
				}
			}

			result["Metadata"] = metadata;
			result["Content"] = content;

			return result;
		}
	}

}