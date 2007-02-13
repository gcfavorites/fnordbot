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
		private int sortSnakAnswerChance = 15; 
		string vocabularyFilePath;
		int saveInterval = 5;


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
				bool canStart = true;
				queue.Enqueue("START");	// lav en startnode
				bool canTerminate = false;
				for (int i=0; i<words.Length; i++) // for each word in message
				{
					string word = words[i];
					if (word.Trim().Length > 0) 
					{
						if (i == words.Length) // last word in message
							canTerminate = true;
						queue.Enqueue(word);
						if (queue.Count == 3) // 3 words are queued - lets make a fragment of them
						{
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
			string[] parts = line.Split(' '); // split the message
			for (int i=0; i<parts.Length; i++) 
			{
				Word wrd = vocab.Words[parts[i]];			// get the word from wordlist
				if (wrd != null && wrd.Score < minscore)	// if the word is known and is rare enough
				{
					minscore = vocab.Words[parts[i]].Score;
					minindex = i;
				}
			}
			if (minscore == 9999) 
			{
				bot.WriteLogMessage("GenerateAnswer(\""+line+"\"): No rare words found?");
				return "";
			}
			else 
			{
				bot.WriteLogMessage("GenerateAnswer(\""+line+"\"): rarest word is '"+parts[minindex]+"' with score "+minscore);
			}
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

			bot.WriteLogMessage("Generated message '"+sb.ToString()+"'");
			return sb.ToString();
		}

		public bool KnowsWord(string word) 
		{
			return vocab.KnowsWord(word);	// placeholder for old implementations
		}

		// generate a reply that contains the specified word
		private string GenerateReply(string word)
		{
			StringBuilder sb = new StringBuilder(null);

			// get a random fragment that contains the word
			Fragment frag = vocab.GetFragmentByWord(word); 
			if (frag != null) 
			{
				Fragment cur = frag;
				// this word cannot end a message. 
				// note: ends as soon as possible - maybe we shouldcontinue sometimes, if at all possible
				while (!cur.CanEnd) 
				{
					sb.Append( cur.ThisWord.Value+" " );	// append to our stringbuilder
					cur = vocab.GetNextFragment( cur );		// get the next fragment
				}
				sb.Append( cur.ThisWord.Value );			// add the message-ending word

				cur = frag;
				// loop until we find a word that can start a message
				while (!cur.CanStart) 
				{
					cur = vocab.GetPreviousFragment( cur ); // get an random prevoius fragment
					sb.Insert(0, cur.ThisWord.Value+" " );	// add to beginning of stringbuilder
				}
			}
			else 
			{
				bot.WriteLogMessage("Unable to genearte reply based on '"+word+"'");
				return "";
			}
			return sb.ToString();
		}

		#endregion

		#region IPlugin Members

		public void Attach(FnordBot.FnordBot bot)
		{
			this.bot = bot;
			bot.OnPublicMessage += new FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
			bot.OnPrivateMessage += new FnordBot.FnordBot.MessageHandler(bot_OnPrivateMessage);
		}

		public void Init( XmlNode pluginNode )
		{
			this.configNode = pluginNode;
			Console.Write("loading vocabulary ... ");
			LoadConfig();
			if (saveInterval > 0)
			{
				Thread thrdVocabSave = new Thread( new ThreadStart( AutoSaveVocabulary ) );
				thrdVocabSave.Start();
			}
			Console.WriteLine("done");
		}

		#endregion

		private void AutoSaveVocabulary() 
		{
			Thread.Sleep( TimeSpan.FromMinutes( saveInterval ) );

			SaveVocabulary();
		}

		private void LoadConfig() 
		{
			if (configNode == null) 
			{
				bot.WriteLogMessage("sortsnak got null confignode!");
			}
			if (configNode.SelectSingleNode("settings/vocabularyfilepath/text()") != null) 
			{
				vocabularyFilePath = configNode.SelectSingleNode("settings/vocabularyfilepath/text()").Value;
				if (!Path.IsPathRooted( vocabularyFilePath )) 
				{
					vocabularyFilePath = Path.Combine(bot.InstallationFolderPath, vocabularyFilePath);
				}
				bot.WriteLogMessage("vocabulary found at "+vocabularyFilePath);
			} 
			else 
			{
				bot.WriteLogMessage("Error in LoadConfig: cannot read settings/vocabularyfilepath/text()");
			}

			if ( File.Exists(vocabularyFilePath) ) 
			{
				// start a thread for vocabulary loading
				Thread threadVocabularyLoader = new Thread( new ThreadStart( LoadVocabulary ) );
				threadVocabularyLoader.IsBackground = true;
				threadVocabularyLoader.Name = "VocabularyLoader";
				threadVocabularyLoader.Priority = ThreadPriority.BelowNormal;
				threadVocabularyLoader.Start();
			}

			try 
			{
				sortSnakAnswerChance = int.Parse(configNode.SelectSingleNode("settings/answerchance/text()").Value);
				vocab.SimpleMatchChance = int.Parse(configNode.SelectSingleNode("settings/simplechance/text()").Value);
				vocab.AmbientSimpleMatchChance = int.Parse(configNode.SelectSingleNode("settings/ambientsimplechance/text()").Value);
				vocab.MinimumOverlap = int.Parse(configNode.SelectSingleNode("settings/minimumoverlap/text()").Value);
				saveInterval = int.Parse(configNode.SelectSingleNode("settings/autosaving/text()").Value);
			} 
			catch (Exception e) 
			{
				Console.WriteLine("error on sortsnak loadconfig: "+e);
			}
		}

		// hmm ... this should really be a switch-case
		private void bot_OnPublicMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			// message is not a command and is more than 2 words - add to vocabulary
			if (!message.StartsWith("!") && message.Split(' ').Length > 2) 
			{
				ParseLine(message);
			}

			// make the bot say something random
			if (message == "!talk")	
			{
				bot.SendToChannel( channel, GenerateLine() );
			} 
			// saves the vocabulary - need to trig it by a timer
			else if (message == "!save")	
			{
				SaveVocabulary();
			} 
			// loads the vocabulary - why? we load it at startup
			else if (message == "!load")
			{
				LoadVocabulary();
			} 
			// non-commands are ignored
			else if ( message.StartsWith("!") ) 
			{
			} 
			// someone mentioned the bot by name - cant let that go unanswered
			else if (message.ToLower().IndexOf( bot.NickName.ToLower() ) >= 0)	
			{
				string baseline;
				// remove the bots name from the message
				baseline = message.Replace( bot.NickName+":", "" );	
				baseline = message.Replace( bot.NickName, "" );

				// generate a reply based on the modified message
				string line = GenerateAnswer( baseline );
				// if reply contains the bot name, generate another one - potentially lots of tries here!
				while ( line.ToLower().IndexOf( bot.NickName.ToLower() )>0 ) 
					line = GenerateAnswer(message); 

				bot.SendToChannel( channel, line ); 
			}
			// roll the dice to see if we'll answer
			else if ( bot.TakeChance(sortSnakAnswerChance) )
			{
				string line = GenerateAnswer( message );
				if (line.Length > 0) bot.SendToChannel( channel, line );
			}
		}


		internal void SaveVocabulary() 
		{
			bot.WriteLogMessage("saving vocabulary ...");
			StreamWriter writer = new StreamWriter( vocabularyFilePath, false, Encoding.Default );
			foreach (Fragment frag in vocab.CenterSortedFragments) 
			{
				writer.WriteLine( frag.ToString() );
			}
			writer.Close();
			bot.WriteLogMessage("vocabulary saved.");
		}

		private void LoadVocabulary() 
		{
			bot.WriteLogMessage("loading vocabulary ...");
			StreamReader reader = null;
			reader = new StreamReader( vocabularyFilePath, Encoding.Default );
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
			bot.WriteLogMessage("vocabulary loaded.");

		}

		// parse private messages
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
		}
	}

	public enum SortWord 
	{
		PreviousWord,
		CurrentWord,
		NextWord
	}
}
