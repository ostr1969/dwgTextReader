using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectIndexer
{
	public class Presenter
	{
		public string file { get; set; }
		public string HtmlContent { get; set; }
		public string plain { get; set; }
	}
	public class ArgStorage
	{
		public string File; public bool verbose; public string[] DwgFiles; public bool summary;
		public bool appid; public bool blockrecords; public bool dimstyles; public bool layers; public bool linetypes;
		public bool textstyles; public bool ucs; public bool views; public bool vports; public bool content; public string folder;
		public bool json;
	};
}
