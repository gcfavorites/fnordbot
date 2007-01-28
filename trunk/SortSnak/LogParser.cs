using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NielsRask.SortSnak
{

	public class FlatLogParser 
	{
		string path;
		Plugin plugin;

		public FlatLogParser( string path, Plugin plugin ) 
		{
			this.path = path;
			this.plugin = plugin;
		}

		public void StartParser() {
			StreamReader reader = new StreamReader(path,System.Text.Encoding.Default);
			while (reader.Peek() > -1) {
				plugin.ParseLine( reader.ReadLine() );
			}
			reader.Close();
		}
	}

	public class WordListParser 
	{
		string path;
		Vocabulary vocab;

		public WordListParser(string path, Vocabulary vocab) 
		{
			this.path = path;
			this.vocab = vocab;
		}

		public void StartParser() 
		{
			StreamReader reader = new StreamReader(path,System.Text.Encoding.Default);
			while (reader.Peek() > -1) 
			{
				try 
				{
					Parse( reader.ReadLine() );
				} 
				catch {}
			}
			reader.Close();
		}

		private void Parse(string line) 
		{
			line = line.Substring(1, line.Length-2);
			string[] part = line.Split('¤');
			vocab.AddFragment( part[0], part[1], part[2], bool.Parse(part[3]), bool.Parse(part[4]) );
		}
	}

	/// <summary>
	/// Summary description for MircLogParser.
	/// </summary>
	public class MircLogParser
	{
		string path;
		Plugin plugin;
		int count;
		public MircLogParser( string path, Plugin plugin, int count ) 
		{
			this.path = path;
			this.plugin = plugin;
			this.count = count;
		}

		public void StartParser() 
		{
			StreamReader reader = new StreamReader(path,System.Text.Encoding.Default);
			Console.WriteLine("loading log file");
			int lin_chk = 0;
			int lin_prs = 0;

			//DateTime t0 = DateTime.Now;
			Regex rrep1 = new Regex(@"^(\[.{5}\]\s+)",RegexOptions.Compiled);
			Regex rrep2 = new Regex(@"^(\s*<.+?>\s*)",RegexOptions.Compiled);
			DateTime t0 = DateTime.Now;
			DateTime _t0 = DateTime.Now;
			for (int i=0;i<count && reader.Peek() >-1 ; i++) {
//			while (reader.Peek() >-1) {
				lin_chk++;
				string line = reader.ReadLine();// CleanUpIrcLog( reader.ReadLine() );
				string lower = line.ToLower().Trim();
				if ( !line.StartsWith("Session") && 
					 lower.Length != 0 &&
					 lower.IndexOf("*nickserv*")==-1 &&
					 lower.IndexOf("identify")==-1 ) 
				{
					line = rrep1.Replace(line,"",1);
					if ( !line.StartsWith("*") ) {
						line = rrep2.Replace(line,"",1);
						plugin.ParseLine(line);
						lin_prs++;
					}
				}
				if (lin_chk % 2000 == 0) {
					TimeSpan _dt = DateTime.Now - _t0;
					Console.WriteLine("read "+lin_chk+" lines, brain now at NaN triplets, took "+_dt.TotalSeconds.ToString("0.000")+" sec"); 
					_t0 = DateTime.Now;
				}
			}
			TimeSpan dt = DateTime.Now-t0;
			Console.WriteLine("log file loaded. took "+dt.TotalSeconds.ToString("0.000")+" seconds");
			Console.WriteLine("examined "+lin_chk+" lines, parsed "+lin_prs);
//			Console.WriteLine("Brain now contains "+brain.Count()+" elements");

			plugin.SaveVocabulary();
		}
	}
}
