using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwgTextract.Models
{
	public static class Extensions
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
		public static string Rev(this string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}
		public static bool IsPlainTextFile(string path, int sampleSize = 8000)
		{
			// quick extension check first
			string ext = Path.GetExtension(path).ToLowerInvariant();
			

			// verify content
			try
			{
				using var stream = File.OpenRead(path);
				byte[] buffer = new byte[Math.Min(sampleSize, (int)stream.Length)];
				int read = stream.Read(buffer, 0, buffer.Length);

				for (int i = 0; i < read; i++)
				{
					byte b = buffer[i];

					// skip common control chars: tab, newline, carriage return
					if (b == 0x09 || b == 0x0A || b == 0x0D)
						continue;

					// if control characters show up → binary
					if (b < 0x20 || b == 0x7F)
						return false;
				}

				return true;
			}
			catch
			{
				return false; // unreadable file, treat as non-text
			}
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
		public static string GibrishIterator(this string gibrish)
		{
			// Example input string in gibberish (use your actual text or bytes)
			string originalGibberish = gibrish;
			//byte[] bytes = Encoding.Default.GetBytes(originalGibberish);
			if (IsIntOrFloat(originalGibberish))
				return originalGibberish; // Return as is if it's a number
			string[] encodings = { "ibm862", "windows-1255", "iso-8859-8", "iso-8859-8-i", "windows-1252", "utf-8", "utf-16" };

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
						double hebrewCount = CountHebrewLetters(decoded) / (double)decoded.Length;

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


			var frog = HebrewKeyboardConverter.ConvertToHebrewKeyboard(originalGibberish);
			if (CountHebrewLetters(frog) / (double)frog.Length > maxHebrewLetters)
				return frog;
			if (bestDecoded is null)
				return gibrish;
			//Console.WriteLine($"\nBest Encoding:from {bestFrom} to {bestTo}, Decoded Text: {Rev(bestDecoded)}");
			return Rev(bestDecoded);
		}
		static int CountHebrewLetters(this string text)
		{
			return text.Count(c => c >= 'א' && c <= 'ת');
		}
		public static bool IsIntOrFloat(this string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return false;

			// Trim whitespace
			input = input.Trim();
			if (input.All(c => char.IsDigit(c) || c == '.' || c == '=' || c == '+' || c == '-' || c == ' ' || c == 'x' || c == 'X'))
				return true;
			if (input.EndsWith("%"))
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
	}
	public class HebrewKeyboardConverter
	{
		private static readonly Dictionary<char, char> EngToHeb = new()
		{
			['q'] = '/',
			['w'] = '\'',
			['e'] = 'ק',
			['r'] = 'ר',
			['t'] = 'א',
			['y'] = 'ט',
			['u'] = 'ו',
			['i'] = 'ן',
			['o'] = 'ם',
			['p'] = 'פ',
			['a'] = 'ש',
			['s'] = 'ד',
			['d'] = 'ג',
			['f'] = 'כ',
			['g'] = 'ע',
			['h'] = 'י',
			['j'] = 'ח',
			['k'] = 'ל',
			['l'] = 'ך',
			[';'] = 'ף',
			['z'] = 'ז',
			['x'] = 'ס',
			['c'] = 'ב',
			['v'] = 'ה',
			['b'] = 'נ',
			['n'] = 'מ',
			['m'] = 'צ',
			[','] = 'ת',
			['.'] = 'ץ',
			['/'] = '.',
			['Q'] = '/',
			['W'] = '\'',
			['E'] = 'ק',
			['R'] = 'ר',
			['T'] = 'א',
			['Y'] = 'ט',
			['U'] = 'ו',
			['I'] = 'ן',
			['O'] = 'ם',
			['P'] = 'פ',
			['A'] = 'ש',
			['S'] = 'ד',
			['D'] = 'ג',
			['F'] = 'כ',
			['G'] = 'ע',
			['H'] = 'י',
			['J'] = 'ח',
			['K'] = 'ל',
			['L'] = 'ך',
			['Z'] = 'ז',
			['X'] = 'ס',
			['C'] = 'ב',
			['V'] = 'ה',
			['B'] = 'נ',
			['N'] = 'מ',
			['M'] = 'צ'
		};

		public static string ConvertToHebrewKeyboard(string input)
		{
			var sb = new StringBuilder();

			foreach (char c in input)
			{
				if (EngToHeb.TryGetValue(c, out char heb))
					sb.Append(heb);
				else
					sb.Append(c); // keep non-mapped characters (e.g., digits, space)
			}

			return sb.ToString();
		}
	}
}

