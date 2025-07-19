using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Examples
{
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
}}
