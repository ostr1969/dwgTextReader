using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Examples
{ 
	class Program
	{
		// const string _file = "../../../../../samples/sample_AC1032.dwg";
		const string _file = "../../../../../samples/b1.dwg";

		static void Main(string[] args)
		{
			CadDocument doc;
			DwgPreview preview;
			using (DwgReader reader = new DwgReader(_file))
			{
				doc = reader.Read();
				preview = reader.ReadPreview();
			}
			List<string> fontlist=new List<string>();
			ExploreDocument(doc,fontlist);
			List<string> uniqueFontList = fontlist.Select(o=>o).Distinct().ToList();
			Console.WriteLine("FONTS:");
			foreach (var f in uniqueFontList)
				Console.WriteLine($"\t{f}");
		}

		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		static void ExploreDocument(CadDocument doc,List<string> fontlist)
		{
			Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");
			Console.WriteLine();
			Console.WriteLine("SUMMARY INFO:");
			Console.WriteLine($"\tTitle: {doc.SummaryInfo.Title}");
			Console.WriteLine($"\tSubject: {doc.SummaryInfo.Subject}");
			Console.WriteLine($"\tAuthor: {doc.SummaryInfo.Author}");
			Console.WriteLine($"\tKeywords: {doc.SummaryInfo.Keywords}");
			Console.WriteLine($"\tComments: {doc.SummaryInfo.Comments}");
			Console.WriteLine($"\tLastSavedBy: {doc.SummaryInfo.LastSavedBy}");
			Console.WriteLine($"\tRevisionNumber: {doc.SummaryInfo.RevisionNumber}");
			Console.WriteLine($"\tHyperlinkBase: {doc.SummaryInfo.HyperlinkBase}");
			Console.WriteLine($"\tCreatedDate: {doc.SummaryInfo.CreatedDate}");
			Console.WriteLine($"\tModifiedDate: {doc.SummaryInfo.ModifiedDate}");
			
			ExploreTable(doc.AppIds,fontlist);
			ExploreTable(doc.BlockRecords,fontlist);
			ExploreTable(doc.DimensionStyles, fontlist);
			ExploreTable(doc.Layers, fontlist);
			ExploreTable(doc.LineTypes,fontlist);
			ExploreTable(doc.TextStyles, fontlist);
			ExploreTable(doc.UCSs, fontlist);
			ExploreTable(doc.Views, fontlist);
			ExploreTable(doc.VPorts, fontlist);
			Console.WriteLine("TEXTS:");
			foreach (var o in doc.Entities)
			{
				if (o is ACadSharp.Entities.TextEntity ot)
				{
					
					Console.WriteLine($"textentity({ot.Style.Name}): {ot.Value}");
					

				}
					
			};
			Console.WriteLine("MTEXTS:");
			foreach (var o in doc.Entities)
			{
				if (o is ACadSharp.Entities.MText om)
				{
					
					Console.WriteLine(om.Value);
				}

			};
		}
		
		static void ExploreTable<T>(Table<T> table,List<string>  fontlist)
			where T : TableEntry
		{
			Console.WriteLine($"{table.ObjectName}");
			foreach (var item in table)
			{
				Console.WriteLine($"\tName: {item.Name}");
				if(item is BlockRecord blk)
				{ foreach (var o in blk.Entities)
					{
						if (o is ACadSharp.Entities.MText om)
						{
							Console.WriteLine($"\t\tmtext: {om.Value}");
							fontlist.Add(om.Style.Name);
						}
						
						if (o is ACadSharp.Entities.AttributeDefinition oa)
						{
							Console.WriteLine($"\t\tatt({oa.Style.Name}): {oa.Prompt}={oa.Value}");
							fontlist.Add(oa.Style.Name);
							continue;
						}
						if (o is ACadSharp.Entities.TextEntity ot)
						{
							Console.WriteLine($"\t\ttext({ot.Style}): {ot.Value}");
							fontlist.Add(ot.Style.Name);
						}
					} 
				}
				if (item.Name == BlockRecord.ModelSpaceName && item is BlockRecord model)
				{
					Console.WriteLine($"\t\tEntities in the model:");
					foreach (var e in model.Entities.GroupBy(i => i.GetType().FullName))
					{
						Console.WriteLine($"\t\t{e.Key}: {e.Count()}");
					}
				}
			}
		}
	}
}