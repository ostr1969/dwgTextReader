using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwgCrawler
{
	
	public static class Utils
	{
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
}
