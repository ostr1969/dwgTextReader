// See https://aka.ms/new-console-template for more information
using DwgTextract.Models;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace DwgTextract
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: DwgTextract <path_to_dwg_file>");
				return;
			}
			var filetoparse = args[0];


			Console.WriteLine(args[0]);
			var dwg = fileToDwgdata(args[0]);
		}
		static DwgData fileToDwgdata(string path)
		{
			// 1. Run the external executable and capture the JSON output



			//var p = DwgParser.JsonFromDwg(path,options);
			var pp = new DwgParser(path);
			//var jsonOutput = p.esStorage.ToLowercaseKeys();
			var dwg = DwgData.FromParser(pp);
			string json = JsonSerializer.Serialize(dwg);
			Console.WriteLine(json);
			return dwg;

		}
	}
}
//dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\dwgpublished .\dwgtextract\dwgtextract.csproj

