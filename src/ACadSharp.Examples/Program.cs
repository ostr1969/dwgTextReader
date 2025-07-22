using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwgCrawler
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var p =new DwgParser(args);
			//p.WriteDwg("output.dwg");
		}
	}
}
