using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DwgCrawler.DwgParser;

namespace DwgCrawler
{
	
	public static class Utils
	{
		public class ArgStorage
		{
			public string File; public bool verbose; public string[] DwgFiles; public bool summary;
			public bool appid; public bool blockrecords; public bool dimstyles; public bool layers; public bool linetypes;
			public bool textstyles; public bool ucs; public bool views; public bool vports; public bool content; public string folder;
			public bool json;
		};
		public static ArgStorage ArgParser(string[] args)
		{
			ArgStorage argStorage = new();


			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-f":
					case "--file":
						if (i + 1 < args.Length)
							argStorage.File = args[++i];
						break;
					case "-j":
					case "--json":
						argStorage.json = true;
						break;
					case "-s":
					case "--summary":
						argStorage.summary = true;
						break;
					case "-a":
					case "--appid":
						argStorage.appid = true;
						break;
					case "-b":
					case "--blocks":
						argStorage.blockrecords = true;
						break;
					case "-di":
					case "--dimstyles":
						argStorage.dimstyles = true;
						break;
					case "-la":
					case "--layers":
						argStorage.layers = true;
						break;
					case "-li":
					case "--linetypes":
						argStorage.linetypes = true;
						break;
					case "-t":
					case "--textstyles":
						argStorage.textstyles = true;
						break;
					case "-u":
					case "--ucs":
						argStorage.ucs = true;
						break;
					case "-vi":
					case "--views":
						argStorage.views = true;
						break;
					case "-vp":
					case "--vports":
						argStorage.vports = true;
						break;
					case "-c":
					case "--content":
						argStorage.content = true;
						break;
					case "-D":
					case "--dir":
						if (i + 1 < args.Length)
							argStorage.folder = args[++i];
						break;

					case "-v":
					case "--verbose":
						argStorage.verbose = true;
						break;
					case "-h":
					case "--help":
						Console.WriteLine("Usage: DwgCrawler [-f|--file file] [-D|--dir folder] [-s|--summary] [-a|--appid] [-b|--blocks] [-di|--dimstyles]" +
							" [-v|--verbose] [-te|--texts] [-u|--ucs] [-vp|--vports] [-t|--textstyles] [-li|--linetypes] [-la|--layers] [-j|--json]");
						Environment.Exit(1);
						break;

					default:
						Console.WriteLine($"Unknown argument: {args[i]}");
						break;
				}


			}
			if (!string.IsNullOrEmpty(argStorage.folder))
			{
				argStorage.DwgFiles = Directory.GetFiles(argStorage.folder, "*.dwg", SearchOption.TopDirectoryOnly);
				return argStorage;
			}



			if (string.IsNullOrEmpty(argStorage.File))
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Title = "Select a file";
				openFileDialog.Filter = "Text files (*.dwg)|*.dwg|All files (*.*)|*.*";
				openFileDialog.Multiselect = false;

				// Show the dialog
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					argStorage.File = openFileDialog.FileName;
					//Console.WriteLine("Selected file: " + argStorage.File);
				}
				else
				{
					Console.WriteLine("No file selected.");
				}
			}
			if (string.IsNullOrEmpty(argStorage.File))
			{
				Console.WriteLine("No file selected.");
				Environment.Exit(1);
			}
			return argStorage;
		}

		public static bool IsIntOrFloat(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return false;

			// Trim whitespace
			input = input.Trim();
			if (input.All(c => char.IsDigit(c) || c == '.' || c == '=' || c=='+' || c=='-' || c==' ' || c=='x' || c=='X'))
				return true;
			if ( input.EndsWith("%"))
			{
				input = input.Substring(0, input.Length - 1).Trim();

			}

				// Check if input is wrapped in parentheses
				if (input.StartsWith("(") && input.EndsWith(")"))
			{
				input = input.Substring(1, input.Length - 2).Trim(); // Remove the parentheses
			}
			// Try to parse as integer
			if (int.TryParse(input, out _))
				return true;

			// Try to parse as floating-point (using invariant culture for '.' decimal separator)
			if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
				return true;

			return false;
		}
		static int CountHebrewLetters(string text)
		{
			return text.Count(c => c >= 'א' && c <= 'ת');
		}
		public static string GibrishIterator(string gibrish)
		{
			// Example input string in gibberish (use your actual text or bytes)
			string originalGibberish = gibrish;
			//byte[] bytes = Encoding.Default.GetBytes(originalGibberish);
			if (IsIntOrFloat(originalGibberish))
				return originalGibberish; // Return as is if it's a number
			string[] encodings = { "ibm862", "windows-1255", "iso-8859-8", "iso-8859-8-i", "windows-1252", "utf-8","utf-16" };

			string bestDecoded = null;
			string bestFrom = null;
			string bestTo = null;
			double maxHebrewLetters = 0;

			foreach (string enc in encodings)
			{
				try
				{
					foreach (string enc2 in encodings)
					{
						byte[] bytes = Encoding.GetEncoding(enc2).GetBytes(gibrish);

						string decoded = Encoding.GetEncoding(enc).GetString(bytes);
						if (decoded.Length == 0)
							continue;
						double hebrewCount = CountHebrewLetters(decoded)/(double)decoded.Length;

						//Console.WriteLine($"Encoding: from {enc2} to {enc}, Hebrew Letters: {hebrewCount}, Decoded: {decoded}");

						if (hebrewCount > maxHebrewLetters)
						{
							maxHebrewLetters = hebrewCount;
							bestDecoded = decoded;
							bestTo = enc;
							bestFrom = enc2;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error with encoding {enc}: {ex.Message}");
				}
			}

			var frog= HebrewKeyboardConverter.ConvertToHebrewKeyboard(originalGibberish);
			if (CountHebrewLetters(frog)/(double)frog.Length > maxHebrewLetters)
				return frog;
			if (bestDecoded is null)
				return gibrish;
			//Console.WriteLine($"\nBest Encoding:from {bestFrom} to {bestTo}, Decoded Text: {Rev(bestDecoded)}");
			return Rev(bestDecoded);
		}
		public static string Rev(string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}
	}
	public static class JObjectExtensions
	{
		public static JObject ToLowercaseKeys(this JObject original)
		{
			JObject result = new JObject();

			foreach (var property in original.Properties())
			{
				string lowerKey = property.Name.ToLowerInvariant();

				if (property.Value.Type == JTokenType.Object)
				{
					result[lowerKey] = ((JObject)property.Value).ToLowercaseKeys();
				}
				else if (property.Value.Type == JTokenType.Array)
				{
					result[lowerKey] = ProcessArray((JArray)property.Value);
				}
				else
				{
					result[lowerKey] = property.Value;
				}
			}

			return result;
		}

		private static JArray ProcessArray(JArray array)
		{
			JArray newArray = new JArray();
			foreach (var item in array)
			{
				if (item.Type == JTokenType.Object)
					newArray.Add(((JObject)item).ToLowercaseKeys());
				else if (item.Type == JTokenType.Array)
					newArray.Add(ProcessArray((JArray)item));
				else
					newArray.Add(item);
			}
			return newArray;
		}
	}
}
