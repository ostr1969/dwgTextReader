namespace webtail.Services
{
	using Microsoft.AspNetCore.Hosting;
	using Radzen;
	using System.IO;
	using System.Threading.Tasks;

	public class FileService
	{
		private readonly IWebHostEnvironment _env;

		public FileService(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<string> CreateRandomFileAsync(string sourceFile)
		{
			var filesDir = Path.Combine(_env.WebRootPath, "files");
			if (!Directory.Exists(filesDir))
				Directory.CreateDirectory(filesDir);

			var randomName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
			var destPath = Path.Combine(filesDir, randomName);
			var sourcePath = Path.Combine(_env.WebRootPath, "files", sourceFile);
			if (!File.Exists(sourcePath))
			{ 
				return "_";
			}
			await Task.Run(() => File.Copy(sourcePath, destPath, overwrite: true));

			return randomName;
		}
	}
}
