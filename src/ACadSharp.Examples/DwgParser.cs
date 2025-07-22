using DwgCrawler.Common;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Utils = DwgCrawler.Utils;
using System.Windows.Forms;
using ACadSharp.Entities;
using ACadSharp;
using System.Text.Json;
using System.Text.Json.Nodes;



namespace DwgCrawler
{
	class DwgParser
	{
		// const string _file = "../../../../../samples/sample_AC1032.dwg";
		//const string _file = "../../../../../samples/7227.dwg";
		const string FontPath = @"C:\Program Files\Autodesk\DWG TrueView 2026 - English\Fonts";
		public List<string> frog_styles, rev_styles, encode_styles;
		public CadDocument doc;
		public DwgEsStorage esStorage = new DwgEsStorage();
		public  void WriteDwg(string file)
		{
			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}
		public  DwgParser(string[] args)
		{
			var arguments = ArgParser(args);
			 frog_styles = File.ReadLines("frog_list.csv").First().Split(",").Select(item => item.Trim()).ToList();
			 rev_styles = File.ReadLines("rev_list.csv").First().Split(",").Select(item => item.Trim()).ToList();
			 encode_styles = File.ReadLines("encode_list.csv").First().Split(",").Select(item => item.Trim()).ToList();



			if (arguments.DwgFiles is null)
				arguments.DwgFiles = new string[1] { arguments.File };
			
			DwgPreview preview;
			foreach (var _file in arguments.DwgFiles)
			{Console.WriteLine($"Opening file: {_file}");
				using (DwgReader reader = new DwgReader(_file))
				{
					doc = reader.Read();
					preview = reader.ReadPreview();
					ExploreDocument(doc,arguments);
				}
			}
			//preview?.Save(Path.Combine(Path.GetDirectoryName(_file), $"preview.{preview.Code}"));
			List<string> fontlist = new List<string>();
			
			List<string> uniqueFontList = fontlist.Select(o => o).Distinct().ToList();
			//Console.WriteLine("FONTS:");
			//foreach (var f in uniqueFontList)
			//	Console.WriteLine($"\t{f}");
		}
		Dictionary<string, bool> layersStatus = new Dictionary<string, bool>();
		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		public void ExploreDocument(CadDocument doc,ArgStorage arguments)
		{
			Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");
			Console.WriteLine();
			if (arguments.summary)
			{
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
				esStorage.summaryInfo = doc.SummaryInfo;
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
			List<string> fontlist = new List<string>();
			//var blocks= doc.BlockRecords.Select(o => o).ToList();
			foreach (var l in doc.Layers)
			{ layersStatus[l.Name] = l.IsOn; }
			ExploreTable(doc.AppIds, arguments.appid);
			ExploreTable(doc.BlockRecords, arguments.blockrecords);
			ExploreTable(doc.DimensionStyles, arguments.dimstyles);
			ExploreTable(doc.Layers, arguments.layers);esStorage.layers = doc.Layers;
			ExploreTable(doc.LineTypes, arguments.linetypes);
			ExploreTable(doc.TextStyles, arguments.textstyles);esStorage.textStyles = doc.TextStyles;
			ExploreTable(doc.UCSs, arguments.ucs);
			ExploreTable(doc.Views, arguments.views);
			ExploreTable(doc.VPorts, arguments.vports);
			if (arguments.content)
			{
				string lay, tval,style;
				
				esStorage.Content = new JsonArray();
				Console.WriteLine("Content:");				
					 ExploreEntities(doc.Entities,"");

			}
			
			
			
		}

		public void ExploreTable<T>(Table<T> table, bool active)
			where T : TableEntry
		{
			if (!active)
				return;
			Console.WriteLine($"{table.ObjectName}");
			string style,tval,lay;
			//var a = table.Select(o => o).ToList();
			foreach (var item in table)
			{
				if (item is  not BlockRecord )				
					Console.WriteLine($"\tName: {item.Name}");
				if (item is BlockRecord blk)
					{
					Console.WriteLine($"Block Name: {item.Name}");
					ExploreEntities(blk.Entities,""); }




			}
		}
		int id = 0;
		public void ExploreEntities(CadObjectCollection<Entity> collection,string pre)
		{
			pre=pre+"\t";
			string style, tval, lay;
			//Console.WriteLine($"\tBlock Name: {blk.Name}");
			//var a = blk.Entities.Where(o => o.ObjectName == "TEXT").ToList();

			foreach (var o in collection)
			{

				//if (layersStatus[o.Layer.Name] == false)
				//	continue; //skip if layer is off

				if (o is ACadSharp.Entities.MText om)
				{
					
						tval = txtdecode(om,om.Value);
					if (File.Exists(Path.Combine(FontPath, om.Style.Name + ".shx")))
						style = om.Style.Name;
					else
					{
						style = $"#{om.Style.Name}#";
					}
					//tval = txtdecode(om);
					lay = om.Layer.Name;
					Console.WriteLine($"{id++}{pre}mtext({style}): {tval}");
					esStorage.Content.Add(new JsonObject
					{
						["type"] = "mtext",
						["style"] = style,
						["value"] = tval,
						["layer"] = lay
					});
				}

				if (o is ACadSharp.Entities.AttributeDefinition oa)
				{
					
					
						tval = txtdecode(oa,oa.Value);

					if (File.Exists(Path.Combine(FontPath, oa.Style.Name + ".shx")))
						style = oa.Style.Name;
					else
					{
						style = $"#{oa.Style.Name}#";
					}
					//tval = txtdecode(oa);
					string prompt = txtdecode(oa, oa.Prompt);
					


					lay = oa.Layer.Name;
					Console.WriteLine($"{id++}{pre}att({style}): {prompt}={tval}");
					esStorage.Content.Add(new JsonObject
					{
						["type"] = "att",
						["style"] = style,
						["value"] = tval,
						["prompt"] = prompt,
						["layer"] = lay
					});
					continue;
				}
				if (o is ACadSharp.Entities.TextEntity ot)
				{

					
						tval = txtdecode(ot,ot.Value);


					if (File.Exists(Path.Combine(FontPath, ot.Style.Name + ".shx")))
						style = ot.Style.Name;
					else
						style = $"#{ot.Style.Name}#";
					lay = ot.Layer.Name;

					Console.WriteLine($"{id++}{pre}text({style}): {tval}");
					esStorage.Content.Add(new JsonObject
					{
						["type"] = "text",
						["style"] = style,
						["value"] = tval,
						["layer"] = lay
					});
				}
				if (o is ACadSharp.Entities.Insert oi)
				{
					Console.WriteLine($"{id++}{pre}Block : {oi.Block.Name}");
					ExploreEntities(oi.Block.Entities,pre);
				}
				
			}
		}
		
		public string txtdecode(ACadSharp.Entities.TextEntity ot,string txt)
		{
			string tval;
			

			//if (ot.Style.Name == "DCADTXTSTYLE9" && ot.Value!="")
			//{
				
			//	foreach (var e in enc)
			//	{
			//		byte[] bytes = Encoding.GetEncoding(e).GetBytes(ot.Value);
			//		string correctHebrew = Encoding.GetEncoding(enc[0]).GetString(bytes) +
			//			Encoding.GetEncoding(enc[1]).GetString(bytes) +
			//			Encoding.GetEncoding(enc[2]).GetString(bytes) +
			//			Encoding.GetEncoding(enc[3]).GetString(bytes) + Encoding.GetEncoding(enc[4]).GetString(bytes) + Encoding.GetEncoding(enc[5]).GetString(bytes);
			//	}
			//}
			

			List<string> style2decode = encode_styles.Select(o=>o.ToLower()).ToList();
			List<string> style2frog = frog_styles;
			List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains( ot.Style.Name.ToLower()) )
			{
				tval = Utils.GibrishIterator(txt);
				
			}
			
			else
				tval = txt;

			if (style2rev.Contains(ot.Style.Name.ToLower()))
				tval = Utils.Rev(tval);
			return tval;
		}
		public string txtdecode(ACadSharp.Entities.MText ot,string txt)
		{
			string tval;
			List<string> style2decode = encode_styles.Select(o => o.ToLower()).ToList();
			List<string> style2frog = frog_styles;
			List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains(ot.Style.Name.ToLower()))
			{
				tval = Utils.GibrishIterator(txt);
				
			}
			
			else
				tval = txt;

			if (style2rev.Contains(ot.Style.Name.ToLower()))
				tval = Utils.Rev(tval);
			return tval;
		}
	
		
		
		static ArgStorage ArgParser(string[] args)
		{
			ArgStorage argStorage = new ArgStorage();
			

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-f":
					case "--file":
						if (i + 1 < args.Length)
							argStorage.File = args[++i];
						break;
					case "-s":
					case "--summary":
						argStorage.summary = true;
						break;
					case "-a":
					case "--appid":
						argStorage.appid = true;
						break;
					case "-b":
					case "--blocks":
						argStorage.blockrecords = true;
						break;
					case "-di":
					case "--dimstyles":
						argStorage.dimstyles = true;
						break;
					case "-la":
					case "--layers":
						argStorage.layers = true;
						break;
					case "-li":
					case "--linetypes":
						argStorage.linetypes = true;
						break;
					case "-t":
					case "--textstyles":
						argStorage.textstyles = true;
						break;
					case "-u":
					case "--ucs":
						argStorage.ucs = true;
						break;
					case "-vi":
					case "--views":
						argStorage.views = true;
						break;
					case "-vp":
					case "--vports":
						argStorage.vports = true;
						break;
					case "-c":
					case "--content":
						argStorage.content = true;
						break;
					case "-D":
					case "--dir":
						if (i + 1 < args.Length)
							argStorage.folder = args[++i];
						break;

					case "-v":
					case "--verbose":
						argStorage.verbose = true;
						break;
					case "-h":
					case "--help":
						Console.WriteLine("Usage: DwgCrawler [-f|--file file] [-D|--dir folder] [-s|--summary] [-a|--appid] [-b|--blocks] [-di|--dimstyles]" +
							" [-v|--verbose] [-te|--texts] [-u|--ucs] [-vp|--vports] [-t|--textstyles] [-li|--linetypes] [-la|--layers]");
						Environment.Exit(1);
						break;

					default:
						Console.WriteLine($"Unknown argument: {args[i]}");
						break;
				}

				
			}
			if (!string.IsNullOrEmpty(argStorage.folder))
				{
				argStorage.DwgFiles = Directory.GetFiles(argStorage.folder, "*.dwg", SearchOption.TopDirectoryOnly);
				return argStorage;
			}



			if (string.IsNullOrEmpty(argStorage.File))
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Title = "Select a file";
				openFileDialog.Filter = "Text files (*.dwg)|*.dwg|All files (*.*)|*.*";
				openFileDialog.Multiselect = false;

				// Show the dialog
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					argStorage.File = openFileDialog.FileName;
					//Console.WriteLine("Selected file: " + argStorage.File);
				}
				else
				{
					Console.WriteLine("No file selected.");
				}
			}
			if (string.IsNullOrEmpty(argStorage.File))
			{ Console.WriteLine("No file selected.");
				Environment.Exit(1);
			}
			return argStorage;
		}
		public class ArgStorage { public string File ; public bool verbose; public string[] DwgFiles; public bool summary;
			public bool appid; public bool blockrecords; public bool dimstyles; public bool layers; public bool linetypes;
			public bool textstyles; public bool ucs; public bool views; public bool vports; public bool content; public string folder;
		};
	}
	
}

		
	
