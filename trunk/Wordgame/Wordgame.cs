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

namespace NielsRask.Wordgame
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Plugin : IPlugin
	{
		FnordBot.FnordBot bot;
		WordgameCollection gameList;
		public Plugin()
		{
			//
			// TODO: Add constructor logic here
			//
			gameList = new WordgameCollection();
		}
		#region IPlugin Members

		public void Attach(FnordBot.FnordBot bot) {
			// TODO:  Add WordgameMain.Attach implementation
			this.bot = bot;
			bot.WriteLogMessage("Wordgame say's hello");
//			bot.
//			fwd.OnPublicMessage +=new NielsRask.FnordBot.Forwarder.PublicMessageHandler(fwd_OnPublicMessage);
			bot.OnPublicMessage +=new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
		}

		public void Init( XmlNode pluginNode) {
			// TODO:  Add WordgameMain.Init implementation
		}

		#endregion

		private void bot_OnPublicMessage(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			if (message == "!wordgame") 
			{
				Console.WriteLine("someone requested a game");
				if (gameList.ChannelExists( channel ) ) 
				{
					Console.WriteLine("a game is already running, wait 'till it's finished");
					// game is already running
				} 
				else 
				{
					Console.WriteLine("Now starting a new game");
					Wordgame wg = new Wordgame(bot, channel, gameList);
					gameList.Add( wg );
					Thread gameThread = new Thread( new ThreadStart( wg.Start ) );
					gameThread.Name = "wordgame_"+channel;
					gameThread.IsBackground = true;
					gameThread.Start();
				}

			}
		}

	}



	public class WordgameCollection : CollectionBase 
	{
		public void Add( Wordgame game) 
		{
			List.Add( game );
		}

		public void Remove( Wordgame game ) 
		{
			Console.WriteLine("*** removing game for channel "+game.Channel);
			try 
			{
				List.Remove( game );
			} 
			catch (Exception e) 
			{
				Console.WriteLine("error removing from gamelist: "+e);
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

	public class Wordgame	: IDisposable	
	{
		// tråd der styrer wordgame for en kanal 
		// trigges af main-klassen. har begrænset levetid (spillets varighed..)
		// det er her al logikken for spillet ligger
		FnordBot.FnordBot bot;
		string channel;
		Timer tmrFirstHint;
		Timer tmrSecondHint;
		Timer tmrGameEnd;
		string secretWord = "hammer";
		string wordHint = "tool";
		bool done = false;
		WordgameCollection gameList;
		Random rnd;

		public string Channel 
		{
			get { return channel; }
		}

		public Wordgame(FnordBot.FnordBot bot, string channel, WordgameCollection gameList) 
		{
			this.bot = bot;
			this.channel = channel;
			this.gameList = gameList;
			this.rnd = new Random();
			// attach lytte-delegate 
		}

		public void Start() 
		{
			bot.OnPublicMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
//			bot.OnPublicMessage += new NielsRask.FnordBot.FnordBot.Irc.PublicMessageHandler(fwd_OnPublicMessage);
			// vælg et ord og scramble det
			string[] word = SelectWord().Split(':');
			secretWord = word[0];
			wordHint = word[1];
			string scrambledWord = ScrambleWord( secretWord );
			Console.WriteLine("gamethread started");
			
			bot.SendToChannel( channel, "Unscramble ---> "+scrambledWord );
			bot.SendToChannel( channel, "Clue ---> "+wordHint );

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
			Console.WriteLine("starting sleep");
			while (!done) 
			{
				Thread.Sleep(100);
			}
			Console.WriteLine("ending sleep");
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
			string path;
			FileInfo fi = new FileInfo( System.Reflection.Assembly.GetExecutingAssembly().Location );
			path = fi.DirectoryName+"\\";
			Console.WriteLine("basepath: "+path);
			if ( File.Exists(path+".\\wordlist.dat") ) path += ".\\wordlist.dat";
			else if ( File.Exists(path+"..\\..\\wordlist.dat") ) path += "..\\..\\wordlist.dat";
			else 
			{	
				Console.WriteLine("cannot read wordlist");
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
				Console.WriteLine("eror reading wordlist: "+e);
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
			Console.WriteLine("sending first hint");
			bot.SendToChannel( channel, "First letter ---> "+secretWord.Substring(0,1) );
		}

		private void OnSecondHint(object obj) 
		{
			Console.WriteLine("sending second hint");
			bot.SendToChannel( channel, "First two letters ---> "+secretWord.Substring(0,2) );
		}

		private void OnGameEnd(object obj) 
		{
			Console.WriteLine("game has ended");
			bot.SendToChannel( channel, "Nobody got it... losers!");
			done = true;
//			Dispose();
		}

		private void bot_OnPublicMessage(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			if (!done) {
				if (string.Compare(this.channel, channel, true)==0)
				{
					if (string.Compare(message, secretWord, true) == 0) 
					{
						bot.SendToChannel( channel, "Woohoo "+user.Name+"!! You got it... "+secretWord+"" );
//						user.CustomSettings.Add( new NielsRask.FnordBot.Users.CustomSetting("foo").
						done = true;
						//					Dispose();
					}
				}
			}
		}

		#region log
//		[15:30] <NetRanger> !word
//
//		[15:30] -Deep_Olga:#crayon- Unscramble ---> 6 cnaaleahv
//
//		[15:31] -Deep_Olga:#crayon- Clue ---> 12  pile
//
//		[15:31] -Deep_Olga:#crayon- First letter ---> 12  a
//
//		[15:31] <NordCore> avalanche
//
//		[15:31] -Deep_Olga:#crayon- Woohoo NordCore!! You got it... 4avalanche
//
//		[15:31] -Deep_Olga:#crayon- NordCore you've won 1 times today, baaaah!
//
//		[15:31] <NetRanger> hmm
//
//		[15:31] <NetRanger> !word
//
//		[15:31] -Deep_Olga:#crayon- Unscramble ---> 6 lbosasart
//
//		[15:31] -Deep_Olga:#crayon- Clue ---> 12  burden
//
//		[15:32] -Deep_Olga:#crayon- First letter ---> 12  a
//
//		[15:32] -Deep_Olga:#crayon- First two letters ---> 12  al
//
//		[15:32] -Deep_Olga:#crayon- Nobody got it... 4losers!
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			gameList.Remove( this );
			Console.WriteLine("disposing ...");
			tmrFirstHint.Dispose();
			tmrSecondHint.Dispose();
			tmrGameEnd.Dispose();
		}

		#endregion
	}
}
