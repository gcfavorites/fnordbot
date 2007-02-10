using System;
using System.IO;
using System.Text;
using NielsRask.FnordBot;
using System.Threading;
using System.Xml;

namespace NielsRask.SortSnak
{
	public class Plugin : IPlugin
	{
		private Vocabulary vocab;
		private FnordBot.FnordBot bot;
		private XmlNode configNode;


		public Plugin()
		{
			vocab = new Vocabulary();
		}

		#region logik
		/// <summary>
		/// Parses a line of text and stores it in the vocabulary
		/// </summary>
		/// <param name="line"></param>
		public void ParseLine(string line)
		{
			StringQueue queue = new StringQueue(1, vocab);
		
			string[] words = line.Split(' ');
			if (words.Length > 3) 
			{

				//foreach (string word in words) {
				bool canStart = true;
				queue.Enqueue("START");	// lav en startnode
				bool canTerminate = false;
				for (int i=0; i<words.Length; i++) 
				{
					string word = words[i];
					if (word.Trim().Length > 0) 
					{
						if (i == words.Length) canTerminate = true;
						//					if (i == words.Length-1) canTerminate = true;
						queue.Enqueue(word);
						if (queue.Count == 3) 
						{
							//						triplist.Add( queue.CreateTriplet(canStart,canTerminate) );
							queue.CreateFragment(canStart, canTerminate);

							canStart = false;
							queue.Dequeue();
						}
					}
				}
				queue.Enqueue("SLUT");
				queue.CreateFragment(canStart, true);
			}
			queue.Clear();
		}

		/// <summary>
		/// finds the most important word in a string, and constructs a reply from this word
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public string GenerateAnswer(string line) 
		{
			int minscore = 9999;
			int minindex = 0;
			string[] parts = line.Split(' ');
			for (int i=0; i<parts.Length; i++) 
			{
				Word wrd = vocab.Words[parts[i]];
				if (wrd != null && wrd.Score < minscore) 
				{
					minscore = vocab.Words[parts[i]].Score;
					minindex = i;
				}
			}
			Console.WriteLine("rarity: "+parts[minindex]+", score: "+minscore);
			if (minscore == 9999) return "";
			return GenerateReply( parts[minindex] );
		}

		/// <summary>
		/// Generates a random string
		/// </summary>
		/// <returns></returns>
		public string GenerateLine()
		{
			StringBuilder sb = new StringBuilder(null);

			Fragment frag = vocab.GetRandomStartFragment();
			while (!frag.CanEnd) 
			{
				sb.Append( frag.ThisWord.Value+" " );
				frag = vocab.GetNextFragment( frag );
			}
			sb.Append( frag.ThisWord.Value );
			string wrd = sb.ToString().Split(' ')[0];
			Console.WriteLine("ord: "+wrd);
			Console.WriteLine("word: "+vocab.Words[wrd].Value+" score:"+vocab.Words[wrd].Score+"");

			return sb.ToString();
		}

		public bool KnowsWord(string word) 
		{
			return vocab.KnowsWord(word);	// placeholder for old implementations
		}

		private string GenerateReply(string word)
		{
			StringBuilder sb = new StringBuilder(null);

			Fragment frag = vocab.GetFragmentByWord(word); // hent random fragment med start-ord som thisword
			if (frag != null) 
			{
				Fragment cur = frag;
				while (!cur.CanEnd) 
				{
					sb.Append( cur.ThisWord.Value+" " );
					cur = vocab.GetNextFragment( cur );
				}
				sb.Append( cur.ThisWord.Value );

				cur = frag;
				while (!cur.CanStart) 
				{
					cur = vocab.GetPreviousFragment( cur );
					sb.Insert(0, cur.ThisWord.Value+" " );
				}
			}
			else 
			{
				Console.WriteLine("kunne ikke generere svar til '"+word+"'");
				return "";
			}
			return sb.ToString();
		}

		public int Count()
		{
			return 0;
		}

		public void Dump() 
		{
			Console.WriteLine("-------------------------\ncontents of prev-col");
			foreach (Fragment frg in vocab.PrevSortedFragments) Console.WriteLine("-> "+frg.ToString());
			Console.WriteLine("-------------------------\ncontents of this-col");
			foreach (Fragment frg in vocab.CenterSortedFragments) Console.WriteLine("-> "+frg.ToString());
			Console.WriteLine("-------------------------\ncontents of next-col");
			foreach (Fragment frg in vocab.NextSortedFragments) Console.WriteLine("-> "+frg.ToString());
		}
		#endregion
		#region IPlugin Members

		public void Attach(FnordBot.FnordBot bot)
		{
			// TODO:  Add Plugin.Attach implementation
			this.bot = bot;
			bot.OnPublicMessage += new FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
			bot.OnPrivateMessage += new FnordBot.FnordBot.MessageHandler(bot_OnPrivateMessage);
		}

		public void Init( XmlNode pluginNode )
		{
			this.configNode = pluginNode;
			// TODO:  Add Plugin.Init implementation

			Console.Write("loading vocabulary ... ");
			LoadVocabulary();
			Console.WriteLine("done");

//			MircLogParser mlp = new MircLogParser(@"c:\data\#crayon.log", this, 15000);
//			Thread thrdLogLoader = new Thread( new ThreadStart(mlp.StartParser) );
//			thrdLogLoader.IsBackground = true;
//			thrdLogLoader.Name = "Log loader thread";
//			thrdLogLoader.Start();

		}

		#endregion

		private void bot_OnPublicMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			if (!message.StartsWith("!") && message.Split(' ').Length > 2) ParseLine(message);

			if (message == "!talk")	// tal på kommando ...
			{
				bot.SendToChannel( channel, GenerateLine() );
			} 
			else if (message == "!save")	// tal på kommando ...
			{
				//				bot.SendToChannel( channel, GenerateLine() );
				SaveVocabulary();
			} 
			else if (message == "!load")	// tal på kommando ...
			{
				//				bot.SendToChannel( channel, GenerateLine() );
				LoadVocabulary();
			} 
			else if ( message.StartsWith("!") ) {} // ignorer disse
			else if (message.IndexOf( bot.NickName ) >= 0)	// de snakker om mig
			{
				string baseline = message.Replace(bot.NickName,"");	// forsøg på at undgå at den tager sit eget navn med 
				baseline = message.Replace(bot.NickName+":","");		// derfor skal vi fjerne dens navn så det ikke trigger på popularitet
				string line = GenerateAnswer(baseline);
				while ( line.IndexOf( bot.NickName )>0 ) line = GenerateAnswer(message); // TODO nasty, den skal muligvis igennem ret mange
				bot.SendToChannel( channel, line );
			}
			else if ( bot.TakeChance(sortSnakAnswerChance) )
			{
				string line = GenerateAnswer( message );
				if (line.Length > 0) bot.SendToChannel( channel, line );
			}
		}

		private int sortSnakAnswerChance = 15; // TODO skal hentes fra config

		internal void SaveVocabulary() 
		{
			string path = configNode.SelectSingleNode("vocabularyfilepath/text()").Value;
			Console.WriteLine("saving vocabulary ...");
			StreamWriter writer = new StreamWriter( path, false, Encoding.Default );
			foreach (Fragment frag in vocab.CenterSortedFragments) 
			{
				writer.WriteLine( frag.ToString() );
			}
			writer.Close();
			Console.WriteLine("vocabulary saved.");
		}

		private void LoadVocabulary() 
		{
			string path = configNode.SelectSingleNode("vocabularyfilepath/text()").Value;
			StreamReader reader = null;
			reader = new StreamReader( path, Encoding.Default );
			string line;
			try 
			{
				while (reader.Peek() > -1) 
				{
					line = reader.ReadLine();
					line = line.Substring(1, line.Length-2);
					string[] part = line.Split('¤');
					vocab.AddFragment( part[0], part[1], part[2], bool.Parse(part[3]), bool.Parse(part[4]) );
				}
			} 
			finally 
			{
				if (reader!=null) reader.Close();
			}

		}

		private void bot_OnPrivateMessage(NielsRask.FnordBot.User user, string channel, string message)
		{ //try omkring
			if ( message.StartsWith("!set_sortsnak_answerchance") ) 
			{
				string[] parts = message.Split(' ');
				sortSnakAnswerChance = int.Parse( parts[1] );
			} 
			else if ( message.StartsWith("!get_sortsnak_answerchance") ) 
			{
				bot.SendToUser(user.Name, sortSnakAnswerChance+"");
			}
			else if ( message.StartsWith("!set_sortsnak_minimumoverlap") ) 
			{
				string[] parts = message.Split(' ');
				vocab.MinimumOverlap = int.Parse( parts[1] );
			}
			else if ( message.StartsWith("!get_sortsnak_minimumoverlap") ) 
			{
				bot.SendToUser(user.Name, vocab.MinimumOverlap+"");
			}
			else if ( message.StartsWith("!set_sortsnak_simplechance") ) 
			{
				string[] parts = message.Split(' ');
				vocab.SimpleMatchChance = int.Parse( parts[1] );
			}
			else if ( message.StartsWith("!get_sortsnak_simplechance") ) 
			{
				bot.SendToUser(user.Name, vocab.SimpleMatchChance+"");
			}
			else if ( message.StartsWith("!set_sortsnak_ambientsimplechance") ) 
			{
				string[] parts = message.Split(' ');
				vocab.MinimumOverlap = int.Parse( parts[1] );
			}
			else if ( message.StartsWith("!get_sortsnak_ambientsimplechance") ) 
			{
				bot.SendToUser(user.Name, vocab.AmbientSimpleMatchChance+"");
			}
			else if ( message.StartsWith("!get_sortsnak_stat") ) 
			{
				bot.SendToUser(user.Name, "Words known: "+vocab.Words.Count);
				bot.SendToUser(user.Name, "Fragments known: "+vocab.CenterSortedFragments.Count);
			}
			else if ( message.StartsWith("!help sortsnak") ) 
			{
				bot.SendToUser(user.Name, "get/set_sortsnak_answerchance value");
				bot.SendToUser(user.Name, "get/set_sortsnak_minimumoverlap value");
				bot.SendToUser(user.Name, "get/set_sortsnak_simplechance value");
				bot.SendToUser(user.Name, "get/set_sortsnak_ambientsimplechance value");
				bot.SendToUser(user.Name, "get_sortsnak_stat");
			}
			//simplechance
			//ambientsimplechance
		}
	}

	public enum SortWord 
	{
		PreviousWord,
		CurrentWord,
		NextWord
	}
}
