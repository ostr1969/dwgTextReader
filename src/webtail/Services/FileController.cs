using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace webtail.Services
{
	

	[Route("files")]
	public class FileController : Controller
	{
		[HttpGet("{filename}")]
		public IActionResult GetFile(string filename)
		{
			var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", filename);
			if (!System.IO.File.Exists(path))
				return NotFound();

			var ext = Path.GetExtension(filename).ToLower();
			Debug.Print(ext);
			var mimeType = ext switch
			{
				".pdf" => "application/pdf",				
				".png" => "image/png",
				".jpg" => "image/jpeg",
				".jpeg" => "image/jpeg",
				".gif" => "image/gif",
				".dwg" => "application/acad",
				".zip" => "application/zip",
				".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
				_ => "application/octet-stream"
			};

			var inlineTypes = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif" };
			var disposition = inlineTypes.Contains(ext) ? "inline" : "attachment";

			Response.Headers.Add("Content-Disposition", $"{disposition}; filename=\"{filename}\"");

			return PhysicalFile(path, mimeType, enableRangeProcessing: true);
		}
	}
}
