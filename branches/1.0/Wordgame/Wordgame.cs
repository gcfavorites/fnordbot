using System;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using NielsRask.FnordBot;
//using NielsRask.LibIrc2;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;

namespace NielsRask.Wordgame
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Plugin : IPlugin
	{
		FnordBot.FnordBot bot;
		WordgameCollection gameList;
		string wordListPath;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Plugin()
		{
			gameList = new WordgameCollection();
		}
		#region IPlugin Members
		public void Attach(FnordBot.FnordBot bot) 
		{
			this.bot = bot;
			bot.OnPublicMessage +=new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
			log.Info("Wordgame plugin attached");
		}

		public void Init( XmlNode pluginNode) 
		{
			wordListPath = pluginNode.SelectSingleNode("settings/wordlist/text()").Value;
			if (!Path.IsPathRooted( wordListPath )) 
			{
				wordListPath = Path.Combine(bot.InstallationFolderPath, wordListPath);
			}

			// TODO:  Add WordgameMain.Init implementation
		}
		#endregion

		private void bot_OnPublicMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			if (message == "!wordgame") 
			{
//				Console.WriteLine("someone requested a game");
				log.Info("A game was requested on "+channel);
				if (gameList.ChannelExists( channel ) ) 
				{
					// game is already running
					log.Warn("A game is already running.");
				} 
				else 
				{
					Wordgame wg = new Wordgame(bot, channel, gameList, wordListPath);
					gameList.Add( wg );
					Thread gameThread = new Thread( new ThreadStart( wg.Start ) );
					gameThread.Name = "wordgame_"+channel;
					gameThread.IsBackground = true;
					gameThread.Start();
				}
			} 
			else if (message == "!score") 
			{
				// TODO: list the top10 wordgamers
			}
		}
	}

	public class WordgameCollection : CollectionBase 
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void Add( Wordgame game) 
		{
			List.Add( game );
		}

		public void Remove( Wordgame game ) 
		{
			log.Debug("Removing game for channel "+game.Channel);
			
			try 
			{
				List.Remove( game );
			} 
			catch (Exception e) 
			{
				log.Error("Error removing from gamelist",e);
			}
		}

		public Wordgame this[int i] 
		{
			get { return (Wordgame)List[i]; }
			set { List[i] = value; }
		}

		public bool ChannelExists(string channel) 
		{
			bool found = false;
			int i=0;
			while (!found && i<Count) 
			{
				if ( string.Compare(this[i].Channel, channel, true) == 0 ) 
				{
					found = true;
				}
				else 
				{
					i++;
				}
			}
			return found;
		}
	}

	public class Wordgame : IDisposable	
	{
		// tråd der styrer wordgame for en kanal 
		// trigges af main-klassen. har begrænset levetid (spillets varighed..)
		// det er her al logikken for spillet ligger
		FnordBot.FnordBot bot;
		string channel;
		Timer tmrFirstHint;
		Timer tmrSecondHint;
		Timer tmrGameEnd;
		string secretWord;
		string wordHint;
		bool done;
		WordgameCollection gameList;
		Random rnd;
		string wordListPath;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Gets the channel that this game is running in.
		/// </summary>
		/// <value>The channel.</value>
		public string Channel 
		{
			get { return channel; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Wordgame"/> class.
		/// </summary>
		/// <param name="bot">The bot.</param>
		/// <param name="channel">The channel.</param>
		/// <param name="gameList">The game list.</param>
		public Wordgame(FnordBot.FnordBot bot, string channel, WordgameCollection gameList, string wordListPath) 
		{
			this.bot = bot;
			this.channel = channel;
			this.gameList = gameList;
			this.rnd = new Random();
			this.wordListPath = wordListPath;
		}

		/// <summary>
		/// Starts this game.
		/// </summary>
		public void Start() 
		{
			bot.OnPublicMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
			// vælg et ord og scramble det
			string[] word = SelectWord().Split(':');
			secretWord = word[0];
			wordHint = word[1];
			string scrambledWord = ScrambleWord( secretWord );
//			Console.WriteLine("gamethread started");
			log.Info("GameThread started");
			
			bot.SendToChannel( channel, "Unscramble ---> "+scrambledWord, true  );
			bot.SendToChannel( channel, "Clue ---> "+wordHint, true  );

			// start timer
			TimerCallback tmrCbFirstHint = new TimerCallback( OnFirstHint );
			tmrFirstHint = new Timer(tmrCbFirstHint, null, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1) );

			TimerCallback tmrCbSecondHint = new TimerCallback( OnSecondHint );
			tmrSecondHint = new Timer(tmrCbSecondHint, null, TimeSpan.FromSeconds(25), TimeSpan.FromMilliseconds(-1) );

			TimerCallback tmrCbGameEnd = new TimerCallback( OnGameEnd );
			tmrGameEnd = new Timer(tmrCbGameEnd, null, TimeSpan.FromSeconds(40), TimeSpan.FromMilliseconds(-1) );

			// ved korrekt ord, stop spil, tæl point op

			// ved halvvejs, giv hint

			// ved slut, afslør ord
			while (!done) // wait till the game is complete
			{
				Thread.Sleep(100);
			}
			Dispose();
		}

		private string ScrambleWord(string word) 
		{
			char[] inWrd = word.ToCharArray();
			char[] outWrd = new char[ inWrd.Length ];
			for (int i=0; i<outWrd.Length; i++) { outWrd[i] = ' ';}

			for (int j=0; j<inWrd.Length; j++) 
			{
				int pos = rnd.Next( inWrd.Length );
				while ( outWrd[pos] != ' ' ) 
				{
					pos = rnd.Next( inWrd.Length );
				}
				outWrd[ pos ] = inWrd[ j ];
			}
			return new string( outWrd );
		}

		private string SelectWord() 
		{
			string path = wordListPath;
//			FileInfo fi = new FileInfo( System.Reflection.Assembly.GetExecutingAssembly().Location );
//			path = fi.DirectoryName+"\\";
//			Console.WriteLine("basepath: "+path);
			if ( File.Exists(path+".\\wordlist.dat") ) path += ".\\wordlist.dat";
			else if ( File.Exists(path+"..\\..\\wordlist.dat") ) path += "..\\..\\wordlist.dat";
			else 
			{	
//				Console.WriteLine("cannot read wordlist");
				log.Error("Cannot read wordlist");
//				Assembly.GetCallingAssembly().
				return "error:Wordlist_not_loaded";
			}
			StreamReader sr = new StreamReader( path, Encoding.Default);
			StringCollection words = new StringCollection();
			string line = "";
			try 
			{
				while ( (line = sr.ReadLine()) != null ) 
				{
					words.Add( line );
				}
			} 
			catch (Exception e) 
			{
//				Console.WriteLine("eror reading wordlist: "+e);
				log.Error("Error reading wordlist", e);
			} 
			finally 
			{
				sr.Close();
			}
			int pos = rnd.Next( words.Count );
			return words[ pos ];
		}

		private void OnFirstHint(object obj) 
		{
			if (!done) 
			{
				log.Debug("Sending first hint for game on "+channel);
				bot.SendToChannel( channel, "First letter ---> "+secretWord.Substring(0,1), true  );
			}
		}

		private void OnSecondHint(object obj) 
		{
			if (!done) 
			{
				log.Debug("Sending second hint for game on "+channel);
				bot.SendToChannel( channel, "First two letters ---> "+secretWord.Substring(0,2), true  );
			}
		}

		private void OnGameEnd(object obj) 
		{
			if (!done) 
			{
				log.Info("Game on "+channel+" has ended with no winner.");
				bot.SendToChannel( channel, "Nobody got it... losers!", true );
				done = true;
			}
		}

		private void bot_OnPublicMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			if (!done) {
				if (string.Compare(this.channel, channel, true)==0)
				{
					if (string.Compare(message, secretWord, true) == 0) 
					{
						done = true;
//						Console.WriteLine("Word guessed, stopping game");
						log.Info("Word '"+secretWord+"' guessed by "+user.NickName+". Game on "+channel+" has ended.");
						bot.SendToChannel( channel, "Woohoo "+user.Name+"!! You got it... "+secretWord+"", true );
						string strOldScore = user.CustomSettings.GetCustomValue("wordgame", "score");
						int oldScore = 0;
						try 
						{
							if (strOldScore == null || strOldScore.Length == 0)
								strOldScore = "0";
							oldScore = int.Parse(strOldScore);
						} 
						catch {}
						user.CustomSettings.SetCustomValue("wordgame", "score", (oldScore+1)+"");
						user.Save();
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			gameList.Remove( this );
//			Console.WriteLine("disposing ...");
			tmrFirstHint.Dispose();
			tmrSecondHint.Dispose();
			tmrGameEnd.Dispose();
		}

		#endregion
	}
}
