using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Examples
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var p =new DwgParser(args);
			p.WriteDwg("output.dwg");
		}
	}
}
