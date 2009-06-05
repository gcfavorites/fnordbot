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
			log.Info( "Attaching logger plugin" );
			try
			{
				this.bot = bot;
				bot.OnPrivateMessage += new NielsRask.FnordBot.FnordBot.MessageHandler( bot_OnPrivateMessage );

			}
			catch (Exception e)
			{
				log.Error( "Failed in Logger.Attach", e );
			}
		}

		void bot_OnPrivateMessage( User user, string channel, string message )
		{
			if (message.StartsWith( "StartVote" ))
			{
				if (!reflist.ContainsKey( user.NickName ))
				{
					reflist.Add( user.NickName, new Referendum( user.NickName, bot ) );	// opret i liste hvis den ikke er der
					reflist[user.NickName].StartWizard();
				}
				else
				{
					//der er allerede en igang
				}
			}
			if (message.StartsWith( "vote" ))
				reflist[user.NickName].AcceptVote( user.NickName, message );

			if (reflist.ContainsKey( user.NickName ))
				reflist[user.NickName].ProcessMessage( message ); // hvis der er oprettet et ref på denne user, send besked videre
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
				log.Error( "Error in logger init", e );
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
		private Dictionary<string, int> votes;

		public Referendum( string creator, FnordBot.FnordBot bot )
		{
			this.creator = creator;
			this.bot = bot;
			this.votes = new Dictionary<string, int>();
		}

		public void Start()
		{
			foreach( string str in scope)
			{
				if (str.StartsWith( "#" ))
				{
					bot.SendToChannel(str, "A vote has been called on \"" + question + "\"");
					for (int i=0; i<options.Length; i++)
					{
						bot.SendToChannel(str, i + ": " + options[i]);
					}
					bot.SendToChannel(str, "Write /msg BimseBot vote 1 to vote for option 1, etc."
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
				IsOpen = false; // den kan lukkes når ale har svaret..
				ShowResults();
			}
		}

		private void ShowResults()
		{
			if (resultScope == ResultScope.Creator)
			{
				SendResults( "" );
			}
			else if (resultScope == ResultScope.Participans)
			{
				SendResults( "" );
			}
			else
			{
				SendResults( "" );
			}
		}

		private void SendResults( string user )
		{
			// formatter resultaterne og bot.SendTouser / channel
		}

		public void StartWizard()
		{
			bot.SendToUser( creator, "VoterWizard 0.1 :)" );
			bot.SendToUser( creator, "Please enter the question for the vote:" );
			wizStep++;
		}

		public void AcceptVote( string user, string message )
		{
			// tjek om de er med, hvis det ikke er en channel-vote
			int option = int.Parse(message);
			if (!votes.ContainsKey(user))
			{
				votes.Add(user, option);
				// thanks for voting for ""
				if (votes.Count == scope.Length) // kun hvis ikek en kanal
					{}//showsummary();

			}
			else
			{
				// you have already voted!
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
					bot.SendToUser( creator, "Enter each option [in brackets] [Separated by spaces]";);
					wizStep++;
					break;
				case 2:
					options = Regex.Split(message, "] [");
					for (int i=0; i< options.Length; i++)
						bot.SendToUser(creator, i + ": " + options[i]);
					bot.SendToUser( creator, "Who should be polled? enter a #channel or nicknames separated by spaces" );
					wizStep++;
					break;
				case 3:
					scope = message.Split(' ');
					bot.SendToUser( creator, "Registered " + scope.Length+ " participants." );
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
					bot.SendToUser(creator, "Ok, now type \"start\" to sart the voting ");
					wizStep++;
					break;
				case 6:
					if (message == "start")
					{
						Start();
					}
					wizStep++;
					break;
			}	// vi skal vel også lige have registreret om botten skal sige hvem der kalder til afstemning ...?
		}
	}

	public enum ResultScope
	{
		Creator,
		Participans,
		Channel
	}

}
