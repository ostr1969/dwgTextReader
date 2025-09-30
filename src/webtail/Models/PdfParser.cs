using Newtonsoft.Json.Linq;
using PdfiumViewer;
using System;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Graphics;
namespace webtail.Models

{
	class PdfParser
	{
		public JObject PdfData { get; private set; }
		private bool includeOcr;
		public PdfParser(string pdfPath, bool includeocr)
		{
			// pdfPath = @"yourfile.pdf"; // Replace with your file path

			PdfData = ReadPdfToJObject(pdfPath,includeocr);
			includeOcr = includeocr; 


			//Console.WriteLine(pdfData.ToString());
		}

		static JObject ReadPdfToJObject(string filePath,bool includeOcr)
		{
			var result = new JObject();
			var metadata = new JObject();
			string content = "";
			

			using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(filePath))
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
					content += UglyToadTextExtractor.ExtractPageText(page);
				}
			}

			result["metadata"] = metadata;
			
			if (content == "" && includeOcr)
			{
				Console.WriteLine($"Ocr document {filePath}");
				string language = "eng";
				try
				{

					string baseDir = AppContext.BaseDirectory;
					string tessDataPath = Path.Combine(baseDir, "Resources","tessdata");
					
					using var document = PdfiumViewer.PdfDocument.Load(filePath);
					
					for (int i = 0; i < document.PageCount; i++)
					{
						var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
						var image = RenderPdfPageToBytes(document, i);
						var pix = Pix.LoadFromMemory(image);
						var page = engine.Process(pix);
						content+= page.GetText();
					}
				}
				catch (Exception ex)
				{
					return null;
				}
				 
				
			}
			result["content"] = content;
			result["file"] = filePath;

			return result;
		}
		static byte[] RenderPdfPageToBytes(PdfiumViewer.PdfDocument document, int pageNumber, int dpi = 300)
		{
			//using var document = PdfiumViewer.PdfDocument.Load(pdfPath);
			using var bmp = document.Render(pageNumber, dpi, dpi, true);

			using var ms = new MemoryStream();
			bmp.Save(ms,System.Drawing.Imaging.ImageFormat.Png); // Or ImageFormat.Jpeg, Bmp, etc.
			return ms.ToArray();
		}
	}

}