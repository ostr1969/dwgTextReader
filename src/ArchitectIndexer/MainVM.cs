using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectIndexer
{
	public class MainVM: BindableBase
	{
		private string _folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
		public string folder
		{
			get { return _folder; }
			set { SetProperty(ref _folder, value); }
		}
		
		private string _query = "";
		public string Query
		{
			get { return _query; }
			set { SetProperty(ref _query, value); }
		}
		public MainVM()
		{
			// Initialize properties or commands if needed
			// For example, you can set default values for folder and query
			folder = @"C:\Users\barako\source\repos\ACadSharp\samples";
			Query = "Enter your search query here...";
		}
	}
}
