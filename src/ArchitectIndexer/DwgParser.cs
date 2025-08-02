using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
//using DwgCrawler.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
//using Utils = DwgCrawler.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ArchitectIndexer
{
	public class DwgParser
	{
		// const string _file = "../../../../../samples/sample_AC1032.dwg";
		//const string _file = "../../../../../samples/7227.dwg";
		const string FontPath = @"C:\Program Files\Autodesk\DWG TrueView 2026 - English\Fonts";
		public List<string> frog_styles, rev_styles, encode_styles;
		public CadDocument doc;
		public JObject esStorage = new();
		public JArray esStorageContent = new();
		public  void WriteDwg(string file)
		{
			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}
		public static DwgParser JsonFromDwg(string path) //entry for json only
		{
			var args = new ArgStorage
			{
				File = path,
				json = true,
				summary = false,
				appid = false,
				blockrecords = false,
				dimstyles = false,
				layers = false,
				linetypes = false,
				textstyles = false,
				ucs = false,
				views = false,
				vports = false
			};


			var p = new DwgParser(args);
			return p;
		}
		public  DwgParser(ArgStorage arguments)
		{
			//var arguments = ArgParser(args);
			 frog_styles = File.ReadLines("frog_list.csv").First().Split(",").Select(item => item.Trim()).ToList();
			 rev_styles = File.ReadLines("rev_list.csv").First().Split(",").Select(item => item.Trim()).ToList();
			 encode_styles = File.ReadLines("encode_list.csv").First().Split(",").Select(item => item.Trim()).ToList();



			if (arguments.DwgFiles is null)
				arguments.DwgFiles = new string[1] { arguments.File };
			
			DwgPreview preview;
			foreach (var _file in arguments.DwgFiles)
			{ if(! arguments.json)
					Console.WriteLine($"Opening file: {_file}");
			  esStorage["file"] = _file;
				using (DwgReader reader = new DwgReader(_file))
				{
					doc = reader.Read();
					//preview = reader.ReadPreview();
					ExploreDocument(doc,arguments);
				}
			}
			
			
			
			
		}
		Dictionary<string, bool> layersStatus = new Dictionary<string, bool>();
		public bool isJson { get; set; } = false;
		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		public void ExploreDocument(CadDocument doc,ArgStorage arguments)
		{
			
			if (arguments.summary )
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
				 
				foreach (var item in doc.BlockRecords)
				{

					if (item.Name.ToLower() == BlockRecord.ModelSpaceName.ToLower() && item is BlockRecord model)
					{
						Console.WriteLine($"ENTITIES in the model:");
						foreach (var e in model.Entities.GroupBy(i => i.GetType().FullName))
						{
							Console.WriteLine($"\t{e.Key}: {e.Count()}");
						}
					}

				}
			}
			esStorage["SummaryInfo"] = JObject.FromObject(doc.SummaryInfo);
			List<string> fontlist = new List<string>();
			//var blocks= doc.BlockRecords.Select(o => o).ToList();
			foreach (var l in doc.Layers)
			{ layersStatus[l.Name] = l.IsOn; }
			if (arguments.appid)
				ExploreTable(doc.AppIds,true);
			if(arguments.blockrecords)
				ExploreTable(doc.BlockRecords,true);
			if (arguments.dimstyles)
				ExploreTable(doc.DimensionStyles, true);
			if (arguments.layers || arguments.json)
				ExploreTable(doc.Layers,arguments.layers );
			if (arguments.linetypes)
				ExploreTable(doc.LineTypes, true);
			if (arguments.textstyles || arguments.json)
				ExploreTable(doc.TextStyles,arguments.textstyles);
			if (arguments.ucs)
				ExploreTable(doc.UCSs, true);
			if (arguments.views)
				ExploreTable(doc.Views, true);
			if (arguments.vports)
				ExploreTable(doc.VPorts, true);
			if (arguments.content )
			{								
				Console.WriteLine("Content:");				
					 ExploreEntities(doc.Entities,"", true);				
			}
			else
				ExploreEntities(doc.Entities, "", false);
			esStorage["content"] = esStorageContent;
			esStorage=esStorage.ToLowercaseKeys();


		}
		static void ConditionalWrite(string s,bool active) { 
			if (active)
				Console.WriteLine(s);
		}

		public void ExploreTable<T>(Table<T> table,bool isWrite)
			where T : TableEntry
		{

			ConditionalWrite($"{table.ObjectName}",isWrite);
			
			string style,tval,lay;
			//var a = table.Select(o => o).ToList();
			var ts = new JArray();
			var ls = new JArray();
			foreach (var item in table)
			{
				if (item is  not BlockRecord )
					ConditionalWrite($"\tName: {item.Name}", isWrite);

				if (table is TextStylesTable)
				{
					ts.Add(item.Name);
					 }
				if (table is LayersTable)
					ls.Add(item.Name);


				if (item is BlockRecord blk)
					{
					ConditionalWrite($"Block Name: {item.Name}", isWrite);
					ExploreEntities(blk.Entities,"",isWrite); }




			}
			esStorage["Layers"] = ls;
			esStorage["TextStyles"] = ts;
		}
		int id = 0;
		public void ExportJson()
		{
			
			Console.WriteLine(esStorage.ToString());
		}
		public void ExploreEntities(CadObjectCollection<Entity> collection,string pre,bool isWrite)
		{
			pre=pre+"\t";
			string style, tval,tvalr, lay;
			//Console.WriteLine($"\tBlock Name: {blk.Name}");
			//var a = blk.Entities.Where(o => o.ObjectName == "TEXT").ToList();
			
			foreach (var o in collection)
			{
				string ownerName = "";
				if (o.Owner is BlockRecord owner)
					ownerName = owner.Name;
				//if (layersStatus[o.Layer.Name] == false)
				//	continue; //skip if layer is off

				if (o is ACadSharp.Entities.MText om)
				{
					
						(tval,tvalr) = txtdecode(om,om.Value);
					if (File.Exists(Path.Combine(FontPath, om.Style.Name + ".shx")))
						style = om.Style.Name;
					else
					{
						style = $"#{om.Style.Name}#";
					}
					//tval = txtdecode(om);
					lay = om.Layer.Name;
					ConditionalWrite($"{id++}{pre}mtext({style}): {tvalr}",isWrite);
					esStorageContent.Add(new JObject
					{
						["type"] = "mtext",
						["style"] = style.Replace("#",""),
						["value"] = tval,
						["layer"] = lay,
						["block"] = ownerName
					});
				}

				if (o is ACadSharp.Entities.AttributeDefinition oa)
				{


					(tval, tvalr) = txtdecode(oa,oa.Value);

					if (File.Exists(Path.Combine(FontPath, oa.Style.Name + ".shx")))
						style = oa.Style.Name;
					else
					{
						style = $"#{oa.Style.Name}#";
					}
				
					var prompt = txtdecode(oa, oa.Prompt);
					


					lay = oa.Layer.Name;
					ConditionalWrite($"{id++}{pre}att({style}): {prompt.Item2}={tvalr}", isWrite);
					esStorageContent.Add(new JObject
					{
						["type"] = "att",
						["style"] = style.Replace("#", ""),
						["value"] = tval,
						["prompt"] = prompt.Item1,
						["layer"] = lay,
						["block"] = ownerName
					});
					continue;
				}
				if (o is ACadSharp.Entities.TextEntity ot)
				{


					(tval, tvalr) = txtdecode(ot,ot.Value);


					if (File.Exists(Path.Combine(FontPath, ot.Style.Name + ".shx")))
						style = ot.Style.Name;
					else
						style = $"#{ot.Style.Name}#";
					lay = ot.Layer.Name;
					
					ConditionalWrite($"{id++}{pre}text({style}): {tvalr}", isWrite);
					esStorageContent.Add(new JObject
					{
						["type"] = "text",
						["style"] = style.Replace("#", ""),
						["value"] = tval,
						["layer"] = lay,
						["block"] = ownerName
					});
				}
				if (o is ACadSharp.Entities.Insert oi)
				{
					ConditionalWrite($"{id++}{pre}Block : {oi.Block.Name}", isWrite);
					ExploreEntities(oi.Block.Entities,pre,isWrite);
				}
				
			}
		}
		
		public (string,string) txtdecode(ACadSharp.Entities.TextEntity ot,string txt)
		{
			string tval;
			

					

			List<string> style2decode = encode_styles.Select(o=>o.ToLower()).ToList();
			List<string> style2frog = frog_styles;
			List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains( ot.Style.Name.ToLower()) )			
				tval = txt.GibrishIterator();									
			else
				tval = txt;
			string tvalReversed = tval;
			if (style2rev.Contains(ot.Style.Name.ToLower()))
				tvalReversed = tval.Rev();
			return (tval,tvalReversed);
		}
		public (string, string) txtdecode(ACadSharp.Entities.MText ot,string txt)
		{
			string tval;
			List<string> style2decode = encode_styles.Select(o => o.ToLower()).ToList();
			List<string> style2frog = frog_styles;
			List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains(ot.Style.Name.ToLower()))			
				tval = txt.GibrishIterator();
			else
				tval = txt;


			string tvalReversed = tval;
			if (style2rev.Contains(ot.Style.Name.ToLower()))
				tvalReversed = tval.Rev();
			return (tval, tvalReversed);
		}
	
		
		
		
		
	}
	
}

		
	
