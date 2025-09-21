namespace webtail.Services
{
	using System.IO;

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

			// Generate random file name
			var randomName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
			var destPath = Path.Combine(filesDir, randomName);
			var sourcePath = Path.Combine(_env.WebRootPath, "files", sourceFile);
			// Copy the source file to random name
			//await File.CopyAsync(, destPath);
			await Task.Run(() => File.Copy(sourcePath, destPath, overwrite: true));
			return randomName;
		}
	}
}
