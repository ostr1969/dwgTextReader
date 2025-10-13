using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;


using Newtonsoft.Json.Linq;



using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;



namespace DwgTextract.Models
{
	public class DwgParser
	{
		
		//const string _FontPath = @"C:\Program Files\Autodesk\DWG TrueView 2026 - English\Fonts";
		public List<string> frog_styles, rev_styles, encode_styles;
		public CadDocument? doc;
		public JObject esStorage = new();
		public JArray esStorageContent = new();
		public CrawlerOptions crawlerOptions;
		public CadSummaryInfo? CadSummaryInfo;
		public List<string> TextStyles = new();
		public List<string> ImageNames = new();
		public List<string> Layers = new();
		public List<DwgText> dwgTexts = new();
		public string? Filename;
		
		
		public DwgParser(string filename)
		{
			string baseDir = AppContext.BaseDirectory;
			//string frogfile = Path.Combine(baseDir, "Resources", "frog_list.csv");
			//string revfile = Path.Combine(baseDir, "Resources", "rev_list.csv");
			string encodefile = Path.Combine(baseDir, "Resources", "encode_list.csv");
			if (!System.IO.File.Exists(encodefile))
			{ Console.WriteLine($"file not found: {encodefile}");
				return;
			}
			//frog_styles = System.IO.File.ReadLines(frogfile).First().Split(",").Select(item => item.Trim()).ToList();
			//rev_styles = System.IO.File.ReadLines(revfile).First().Split(",").Select(item => item.Trim()).ToList();
			encode_styles = System.IO.File.ReadLines(encodefile).First().Split(",").Select(item => item.Trim()).ToList();
			//crawlerOptions = options;
			Filename = filename;
			using (DwgReader reader = new DwgReader(filename))
			{
				reader.Configuration.KeepUnknownNonGraphicalObjects = true;
				reader.Configuration.KeepUnknownEntities = true;
				doc = reader.Read();
				
				try

				{ ExploreDocumentBin(doc); }
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
		
		Dictionary<string, bool> layersStatus = new Dictionary<string, bool>();
		public bool isJson { get; set; } = false;
		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		
		public void ExploreDocumentBin(CadDocument doc)
		{
			CadSummaryInfo = doc.SummaryInfo;
			
			
			List<string> fontlist = new List<string>();
			
			foreach (var l in doc.Layers)
			{ layersStatus[l.Name] = l.IsOn; }

				ExploreTable(doc.Layers, false);
				ExploreTable(doc.TextStyles, false);
			ExploreEntities(doc.Entities, "", false);

			try { ExploreEntities(doc.Entities, "", false); }
			catch (Exception ex) { Console.WriteLine("error on parsing dwg file"); }
				

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
					if (!TextStyles.Contains(item.Name))
						TextStyles.Add(item.Name);
					 }
				if (table is LayersTable)
					{ ls.Add(item.Name);
					if (!Layers.Contains(item.Name))
						Layers.Add(item.Name);
				}


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
			string baseDir = AppContext.BaseDirectory;
			string fontfolder = Path.Combine(baseDir, "Resources", "Fonts");
			if (!System.IO.Directory.Exists(fontfolder))
			{ Console.WriteLine($"font folder not found: {fontfolder}");
				return;
			}


			foreach (var o in collection)
			{
				string ownerName = "";
				if (o.Owner is BlockRecord owner)
					ownerName = owner.Name;
				//if (layersStatus[o.Layer.Name] == false)
				//	continue; //skip if layer is off
				if (o is ACadSharp.Entities.RasterImage or)
				{
					ImageNames.Add($"{or.Definition.Name} {or.Definition.FileName}");
				}

					if (o is ACadSharp.Entities.MText om)
				{
					
						(tval,tvalr) = txtdecode(om,om.Value);
					if (System.IO.File.Exists(Path.Combine(fontfolder, om.Style.Name + ".shx")))
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
							["style"] = style.Replace("#", ""),
							["value"] = tval,
							["valuerev"] = tvalr,
							["layer"] = lay,
							["block"] = ownerName
						});
					
					dwgTexts.Add(new DwgText { block=ownerName,type="mtext",style= style.Replace("#", ""), value=tval,layer=lay, valuerev = tvalr });
				}

				if (o is ACadSharp.Entities.AttributeDefinition oa)
				{


					(tval, tvalr) = txtdecode(oa,oa.Value);

					if (System.IO.File.Exists(Path.Combine(fontfolder, oa.Style.Name + ".shx")))
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
							["valuerev"] = tvalr,
							["prompt"]=prompt.Item1,
							["layer"] = lay,
							["block"] = ownerName
						});
					
					dwgTexts.Add(new DwgText { block = ownerName, type = "att", style = style.Replace("#", ""), value = tval, layer = lay,prompt=prompt.Item1,valuerev= tvalr });
					continue;
				}
				if (o is ACadSharp.Entities.TextEntity ot)
				{


					(tval, tvalr) = txtdecode(ot,ot.Value);


					if (System.IO.File.Exists(Path.Combine(fontfolder, ot.Style.Name + ".shx")))
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
						["valuerev"] = tvalr,
						["layer"] = lay,
						["block"] = ownerName
					});
					
					dwgTexts.Add(new DwgText { block = ownerName, type = "text", style = style.Replace("#", ""), value = tval, layer = lay,valuerev=tvalr });
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
			string tval, tvalReversed;
			

					

			List<string> style2decode = encode_styles.Select(o=>o.ToLower()).ToList();
			//List<string> style2frog = frog_styles;
			//List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains( ot.Style.Name.ToLower()) )			
				{tval = txt.GibrishIterator();
				tvalReversed = tval.Rev();
			}
			else
				{tval = txt;
				tvalReversed = tval;
			}
			//string tvalReversed = tval;
			//if (style2rev.Contains(ot.Style.Name.ToLower()))
			
			return (tval,tvalReversed);
		}
		public (string, string) txtdecode(ACadSharp.Entities.MText ot,string txt)
		{
			string tval, tvalReversed;
			List<string> style2decode = encode_styles.Select(o => o.ToLower()).ToList();
			//List<string> style2frog = frog_styles;
			//List<string> style2rev = rev_styles.Select(o => o.ToLower()).ToList();
			if (style2decode.Contains(ot.Style.Name.ToLower()))
			{
				tval = txt.GibrishIterator();
				tvalReversed = tval.Rev();
			}
			else
			{
				tval = txt;
				tvalReversed = tval;
			}
			//string tvalReversed = tval;
			//if (style2rev.Contains(ot.Style.Name.ToLower()))

			return (tval, tvalReversed);
		}
	
		
		
		
		
	}
	
}

		
	
