using System;
using NielsRask.FnordBot;
using System.IO;
using log4net;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NielsRask.Voter
{

	/**
	 * TODO:
	 * hvad nu hvis der er flere afstemninger igang - måske må der kun være een kørende af gangen
	 * 
	 */
	public class Voter : IPlugin
	{
		private FnordBot.FnordBot bot;
		private Dictionary<string, Referendum> reflist;

		private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		public Voter()
		{
			reflist = new Dictionary<string, Referendum>();
		}

		#region IPlugin Members

		public void Attach( NielsRask.FnordBot.FnordBot bot )
		{
			log.Info( "Attaching voter plugin" );
			try
			{
				this.bot = bot;
				bot.OnPrivateMessage += new NielsRask.FnordBot.FnordBot.MessageHandler( bot_OnPrivateMessage );

			}
			catch (Exception e)
			{
				log.Error( "Failed in voter.Attach", e );
			}
		}

		void bot_OnPrivateMessage( User user, string channel, string message )
		{
			if (message.StartsWith("!startvote"))
			{
				if (!reflist.ContainsKey(user.NickName))
				{
					reflist.Add(user.NickName, new Referendum(user.NickName, bot)); // opret i liste hvis den ikke er der
					reflist[user.NickName].StartWizard();
				}
				else
				{
					//der er allerede en igang
					bot.SendToUser(user.NickName, "another vote is running, please wait");
				}
				return;
			}
			if ( message.StartsWith( "!vote " ) )
			{
				reflist[user.NickName].AcceptVote(user.NickName, message);
			}
			else if ( reflist.ContainsKey( user.NickName ) )
			{
				reflist[user.NickName].ProcessMessage( message ); // hvis der er oprettet et ref på denne user, send besked videre}
			}
		}

		public void Init( System.Xml.XmlNode pluginNode )
		{
			try
			{
				//logFolderPath = pluginNode.SelectSingleNode( "settings/logfolderpath/text()" ).Value;
				//if (!Path.IsPathRooted( logFolderPath ))
				//{
				//    logFolderPath = Path.Combine( bot.InstallationFolderPath, logFolderPath );
				//}
				//Directory.CreateDirectory( logFolderPath );
				//if (!logFolderPath.EndsWith( "\\" )) logFolderPath += "\\";
			}
			catch (Exception e)
			{
				log.Error( "Error in voter init", e );
			}
		}


		#endregion

	}

	public class Referendum
	{
		private bool IsOpen;
		private string[] scope; // en kanal eller en række users der er med
		private string question; //hvad er spm
		private string[] options; // muligheder
		private TimeSpan runtime; // hvor lang tid der er til at stemme
		private ResultScope resultScope; // hvem skal have resultatet
		private FnordBot.FnordBot bot;
		private string creator;
		private string resultchannel;
		/// <summary>
		/// collection of user, vote
		/// </summary>
		private Dictionary<string, int> votes;
		private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		public Referendum( string creator, FnordBot.FnordBot bot )
		{
			this.creator = creator;
			this.bot = bot;
			this.votes = new Dictionary<string, int>();
		}

		public void Start()
		{
			IsOpen = true;
			foreach( string str in scope)
			{
				if (str.StartsWith( "#" ))
				{
					bot.SendToChannel(str, "A vote has been called on \"" + question + "\"");
					for (int i=0; i<options.Length; i++)
					{
						bot.SendToChannel(str, i + ": " + options[i]);
					}
					bot.SendToChannel(str, "Write /msg BimseBot !vote 1 to vote for option 1, etc.");
				}
			}
			Thread closerThread = new Thread(new ThreadStart(WaitUntilEnd));
			closerThread.Start();
		}


		public void WaitUntilEnd()
		{
			Thread.Sleep( runtime );
			EndReferendum();
		}

		public void EndReferendum()
		{
			if (IsOpen)
			{
				IsOpen = false; // den kan lukkes når alle har svaret..
				ShowResults();
			}
		}

		private void ShowResults()
		{
			if (resultScope == ResultScope.Creator)
			{
				SendResults( ""+creator );
			}
			else if (resultScope == ResultScope.Participans)
			{
				foreach (string user in scope)
					SendResults( user );
			}
			else
			{
				// channel
				foreach ( string user in scope )
					SendResults( user );
			}
		}

		private void SendResults( string user )
		{
			// formatter resultaterne og bot.SendTouser / channel
			bot.SendToUser( creator, "Results for '"+question+"':");
			//foreach (string option in options)
			int totalVotes = GetTotalVotes();
			//int votes;
			int[] theVotes = new int[options.Length];
			int maxVotes = 0;
			int maxVotesIndex = 0;
			for ( int i = 0; i < options.Length; i++ )
			{
				theVotes[i] = GetVotesByOption( i );
				if ( theVotes[i] >= maxVotes )
				{
					maxVotes = theVotes[i];
					maxVotesIndex = i;
				}
			}
			for ( int i = 0; i < theVotes.Length; i++ )
			{
				bot.SendToUser( creator, "Option '" + options[i] + "' got " + votes + " votes (" + ( theVotes[i] * 100f / totalVotes ) + ")" + ( maxVotesIndex == i ? " - WINS!" : "" ) );
			}
		}

		private int GetTotalVotes()
		{
			//int count = 0;
			//foreach ( int key in votes.Keys )
			//    count++;
			//return count;
			return votes.Keys.Count;
		}

		private int GetVotesByOption( int option)
		{
			int count = 0;
			foreach ( int key in votes.Values )
				if ( key == option )
					count++;
			return count;
		}

		public void StartWizard()
		{
			bot.SendToUser( creator, "VoterWizard 1.0 :)" );
			bot.SendToUser( creator, "Please enter the subject of the vote:" );
			wizStep++;
		}

		public void AcceptVote( string user, string message )
		{
			// tjek om de er med, hvis det ikke er en channel-vote
			if ( message.StartsWith( "!vote " ) )
				message = message.Substring(6);
			try
			{
				int option = int.Parse(message);
				if (!votes.ContainsKey(user))
				{
					votes.Add(user, option);
					// thanks for voting for ""
					bot.SendToUser(user, "Thanks for voting.");
					log.Info("A vote was cast for '"+options[option]+"' by "+user);
					if (!scope[0].StartsWith("#") && votes.Count == scope.Length) // kun hvis ikke en kanal
					{
						//showsummary();
					} 
				}
				else
				{
					// you have already voted!
				}
			} catch (Exception e)
			{
				log.Error("error in AcceptVote with message \""+message+"\"", e);
			}
		}

		private int wizStep = 0;
		public void ProcessMessage(string message)
		{
			switch (wizStep)
			{
				case 1:
					question = message;
					bot.SendToUser( creator, "Registered question \"" + question + "\"" );
					bot.SendToUser( creator, "Enter each option [in brackets] on the same line");
					wizStep++;
					break;
				case 2:
					//options = Regex.Split(message, "] [");
					//for (int i=0; i< options.Length; i++)
					//    bot.SendToUser(creator, i + ": " + options[i]);
					string pattern = @"\[(.+?)\]";
					MatchCollection mcol = Regex.Matches(message, pattern);
					options = new string[mcol.Count];
					for ( int i = 0; i < mcol.Count; i++ )
						options[i] = mcol[i].Groups[1].Value;
					for ( int i = 0; i < options.Length; i++ )
						bot.SendToUser( creator, i + ": " + options[i] );
					bot.SendToUser( creator, "Who should be polled? enter a #channel or nicknames separated by spaces" );
					wizStep++;
					break;
				case 3:
					scope = message.Split(' ');
					bot.SendToUser( creator, "Registered " + scope.Length+ " participants."+(scope.Length==1?" ("+scope+")":"") );
					bot.SendToUser( creator, "For how many minutes should the poll be active? max is 30" );
					wizStep++;
					break;
				case 4:
					int minutes = int.Parse(message);
					runtime = TimeSpan.FromMinutes( minutes );
					bot.SendToUser( creator, "Poll will be active for "+minutes+" minutes" );
					bot.SendToUser( creator, "Who should see the results?" );
					bot.SendToUser( creator, "1: Just yourself" );
					bot.SendToUser( creator, "2: Those who voted" );
					bot.SendToUser( creator, "or enter a #channel to publish to" );
					wizStep++;
					break;
				case 5:
					if (message == "1")
						resultScope = ResultScope.Creator;
					else if (message == "2")
						resultScope = ResultScope.Participans;
					else
					{
						resultScope = ResultScope.Channel;
						resultchannel = message;
					}
					bot.SendToUser(creator, "Ok, now type \"start\" to start the voting ");
					wizStep++;
					break;
				case 6:
					if (message == "start")
					{
						bot.SendToUser( creator, "Voting started, will end "+DateTime.Now.Add(runtime).ToLongTimeString() );
						Start();
					}
					wizStep++;
					break;
			}	// TODO: vi skal vel også lige have registreret om botten skal sige hvem der kalder til afstemning ...?
		}
	}

	public enum ResultScope
	{
		Creator,
		Participans,
		Channel
	}

}
