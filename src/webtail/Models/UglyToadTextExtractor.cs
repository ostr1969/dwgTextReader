using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace webtail.Models
{
	public class UglyToadTextExtractor
	{
		public static string ExtractPageText(Page page, double spaceThresholdFactor = 0.5)
		{
			var sb = new StringBuilder();
			var letters = page.Letters;

			if (letters.Count == 0)
				return string.Empty;

			for (int i = 0; i < letters.Count - 1; i++)
			{
				var current = letters[i];
				var next = letters[i + 1];

				sb.Append(current.Value);

				// Horizontal gap
				double gap = next.StartBaseLine.X - current.EndBaseLine.X;

				// Average character width using GlyphRectangle
				double avgWidth = (current.GlyphRectangle.Width + next.GlyphRectangle.Width) / 2.0;

				// Insert space if gap is big enough
				if (gap > avgWidth * spaceThresholdFactor)
				{
					sb.Append(' ');
				}

				// Detect new line if Y drops significantly
				if (next.StartBaseLine.Y < current.StartBaseLine.Y - current.GlyphRectangle.Height * 0.5)
				{
					sb.Append(Environment.NewLine);
				}
			}

			// Add last letter
			sb.Append(letters[^1].Value);

			return sb.ToString();
		}
	}
}
