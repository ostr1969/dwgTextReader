using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ACadSharp;
using ACadSharp.Tables.Collections;

namespace DwgCrawler
{
	public class DwgEsStorage
	{
		public CadSummaryInfo summaryInfo;
		//public DimensionStylesTable dimensionStyles;
		public List<string> layers=new();
		//public LinetypesTable linetypes;
		public List<string> textStyles=new();
		//public ViewportsTable viewports;
		//public BlockRecordsTable blockRecords;
		public Dictionary<string, string> customProperties;
		public JsonArray Content;


	}
}
