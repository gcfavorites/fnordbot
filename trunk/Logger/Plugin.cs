using System;
using NielsRask.FnordBot;
using System.IO;
using log4net;
using System.Web.Mail;
using System.Collections;
using System.Threading;

namespace NielsRask.Logger
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Plugin : IPlugin
	{
		NielsRask.FnordBot.FnordBot bot;
		StreamWriter writer;
		string logFolderPath = "c:\\";
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//		DateTime lastWriteTime;
//		System.Collections.Specialized.StringCollection daily;
		ChannelLogDictionary chanlog;
		AlarmClock alarmClock;
		public Plugin()
		{
//			daily = new System.Collections.Specialized.StringCollection();
			chanlog = new ChannelLogDictionary();
			alarmClock = new AlarmClock();
			alarmClock.Start();
		}

		#region IPlugin Members

		public void Attach(NielsRask.FnordBot.FnordBot bot)
		{
			log.Info("Attaching logger plugin");
			try 
			{
				this.bot = bot;

				bot.OnPublicMessage +=new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
				bot.OnPrivateMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPrivateMessage);
				bot.OnChannelJoin += new NielsRask.FnordBot.FnordBot.ChannelActionHandler(bot_OnChannelJoin);
				bot.OnChannelPart += new NielsRask.FnordBot.FnordBot.ChannelActionHandler(bot_OnChannelPart);
				bot.OnChannelMode += new NielsRask.FnordBot.FnordBot.ChannelActionHandler(bot_OnChannelMode);
				bot.OnChannelKick += new NielsRask.FnordBot.FnordBot.ChannelActionHandler(bot_OnChannelKick);
				bot.OnServerQuit += new NielsRask.FnordBot.FnordBot.ChannelActionHandler(bot_OnServerQuit);
				bot.OnPrivateNotice += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPrivateNotice);
				bot.OnPublicNotice += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicNotice);
				bot.OnTopicChange += new NielsRask.FnordBot.FnordBot.ChannelTopicHandler(bot_OnTopicChange);
				bot.OnPublicAction += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicAction);
				bot.OnPrivateAction += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPrivateAction);
				bot.OnSendToChannel += new NielsRask.FnordBot.FnordBot.BotMessageHandler(bot_OnSendToChannel);
				bot.OnSendToUser += new NielsRask.FnordBot.FnordBot.BotMessageHandler(bot_OnSendToUser);
				bot.OnSendNotice +=new NielsRask.FnordBot.FnordBot.BotMessageHandler(bot_OnSendNotice);
				bot.OnSetMode += new NielsRask.FnordBot.FnordBot.BotMessageHandler(bot_OnSetMode);
				alarmClock.AddAlarm( new DelegateCaller(DateTime.Today.AddDays(1), new AlarmMethod( OnMidnight ) ) );
			} 
			catch (Exception e) 
			{
				log.Error("Failed in Logger.Attach",e);
			}
		}

		public void Init(System.Xml.XmlNode pluginNode)
		{
			try 
			{
				logFolderPath = pluginNode.SelectSingleNode("settings/logfolderpath/text()").Value;
				if (!Path.IsPathRooted( logFolderPath )) 
				{
					logFolderPath = Path.Combine(bot.InstallationFolderPath, logFolderPath);
				}
				Directory.CreateDirectory( logFolderPath );
				if ( !logFolderPath.EndsWith("\\") ) logFolderPath += "\\";
			} 
			catch (Exception e) 
			{
				log.Error("Error in logger init",e);
			}
		}

		#endregion

		private void bot_OnPublicMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			WriteToFile( channel, "<"+user.Name+"> "+message );
		}

		private void bot_OnPrivateMessage(NielsRask.FnordBot.User user, string channel, string message)
		{
			WriteToFile( user.NickName, "<"+user.Name+"> "+message );
		}

		private void bot_OnChannelJoin(string text, string channel, string target, string senderNick, string senderHost)
		{
			WriteToFile( channel, "*** "+senderNick+" ("+senderHost+") has joined "+channel );
		}

		private void bot_OnChannelPart(string text, string channel, string target, string senderNick, string senderHost)
		{
			WriteToFile( channel, "*** "+senderNick+" ("+senderHost+") has left "+channel );
		}

		private void bot_OnChannelMode(string text, string channel, string target, string senderNick, string senderHost)
		{
			WriteToFile( channel, "***"+senderNick+" sets mode "+text );
		}

		private void bot_OnChannelKick(string text, string channel, string target, string senderNick, string senderHost)
		{
			WriteToFile( channel, "*** "+senderNick+" kicked "+target +(text.Length>0?" ("+text+")":"") );
		}

		private void bot_OnPrivateNotice(NielsRask.FnordBot.User user, string channel, string message)
		{
			WriteToFile( user.NickName, "[Notice] "+message );
		}

		private void bot_OnPublicNotice(NielsRask.FnordBot.User user, string channel, string message)
		{
			WriteToFile( channel, "[Notice] "+message );
		}

		private void bot_OnTopicChange(NielsRask.FnordBot.User user, string channel, string topic)
		{
			WriteToFile( channel, "*** "+user.Name+" sets new topic: "+topic );
		}

		private void bot_OnPublicAction(User user, string channel, string message)
		{
			// public action
			WriteToFile(channel, "* "+user.Name+" "+message);
		}

		private void bot_OnPrivateAction(User user, string channel, string message)
		{
			// private action
			WriteToFile(channel, "* "+user.Name+" "+message);
		}

		private void bot_OnSendToChannel(string botName, string target, string text)
		{
			// bot talks to channel
			WriteToFile( target, "<"+botName+"> "+text );
		}

		private void bot_OnSendToUser(string botName, string target, string text)
		{
			// bot talks to user
			WriteToFile( target, "<"+botName+"> "+text );
		}

		private void bot_OnSendNotice(string botName, string target, string text)
		{
			// bot sets mode
			WriteToFile( target, "[Notice] "+text );		
		}

		private void bot_OnSetMode(string botName, string target, string text)
		{
			WriteToFile( target, "***"+botName+" sets mode "+text );
		}

		private void bot_OnServerQuit(string text, string channel, string target, string senderNick, string senderHost)
		{
			foreach (NielsRask.LibIrc.Channel chan in bot.Channels)	// logs quit in all channels - needs fixing
				WriteToFile( chan.Name, "*** "+senderNick+" ("+senderHost+") has quit IRC: "+text );
		}

		private void OnMidnight() 
		{
			foreach (NielsRask.LibIrc.Channel chan in bot.Channels) 
			{
				WriteToFile(chan.Name, "*** It is now "+DateTime.Now.ToLongDateString() );
			}
			foreach (DictionaryEntry de in chanlog) 
			{
				SendMail((string)de.Key, (System.Collections.Specialized.StringCollection)de.Value);
				( (System.Collections.Specialized.StringCollection)de.Value ).Clear();
			}
		}

		private void WriteToFile(string file, string message) 
		{
			try 
			{
				using ( writer = new StreamWriter(logFolderPath+file+".log", true, System.Text.Encoding.Default) ) 
				{
					writer.WriteLine( "["+DateTime.Now.ToLongTimeString()+"] "+message );
					chanlog[file].Add( "["+DateTime.Now.ToLongTimeString()+"] "+message );
				}
			}
			catch (Exception e) 
			{
				log.Error("Error in WriteToFile()",e);
			}
		}

		//TODO: support for several channels
		private void SendMail(string channame, System.Collections.Specialized.StringCollection col) 
		{
			MailMessage mail = new MailMessage(); 
			mail.To = "niels@crayon.dk";
			mail.Cc = "niels@itide.dk";
			mail.From = "niels@itide.dk";
			mail.Subject = "IRC log for "+channame;
			mail.Body = "Log: "+Environment.NewLine;
			foreach (string str in col)
				mail.Body += str+Environment.NewLine;
			SmtpMail.SmtpServer = "mail.itide.dk";
			SmtpMail.Send( mail );
		}

	}

	public delegate void AlarmMethod();

	public class DelegateCaller :IAlarmLauncher
	{
		DateTime time;
		AlarmMethod method;

		public DelegateCaller(DateTime time, AlarmMethod method ) 
		{
			this.time = time;
			this.method = method;
		}

		public DateTime Time
		{
			get{ return time; }
		}

		public void Reschedule(AlarmEngine engine, DateTime currentAlarmTime)
		{
			engine.AddAlarm( new DelegateCaller(time.AddDays(1), method) );	// ny kørsel imorgen
		}

		public void Execute()
		{
			Thread t = new Thread( new ThreadStart( method ) );
			method();
		}
	}

	public class ChannelLogDictionary : DictionaryBase 
	{
		public void Add(string channelName, System.Collections.Specialized.StringCollection log) 
		{
			Dictionary.Add( channelName, log );

		}

		public System.Collections.Specialized.StringCollection this[string name] 
		{
			get 
			{ 
				if(Dictionary[name] == null)
					Add( name, new System.Collections.Specialized.StringCollection() );
				return (System.Collections.Specialized.StringCollection)Dictionary[name]; 
			}
		}
	}
}
