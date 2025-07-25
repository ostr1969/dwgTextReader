using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DwgCrawler.DwgParser;


namespace DwgCrawler
{
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var Args= Utils.ArgParser(args);
			var p =new DwgParser( Args);
			p.ExportJson();
			
		}
		

	}
}
