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

		ChannelLogDictionary chanlog;
		AlarmClock alarmClock;
		public Plugin()
		{
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
			if (channel != "")
				WriteToFile( channel, "*** "+senderNick+" sets mode "+text );
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
            WriteDelayed( target, "<"+botName+"> "+text );
		}

		public void bot_OnSendToUser(string botName, string target, string text)
		{
			// bot talks to user
			WriteDelayed( target, "<"+botName+"> "+text );

		}

		private void WriteDelayed(string file, string message) 
		{
			WriterDelegate wrtdel = new WriterDelegate( this.WriteToFile );
			DelayWriter dw = new DelayWriter( file, message,1000, wrtdel );
			Thread t = new Thread( new ThreadStart( dw.Start ) );
			t.Name = "DelayedLogWriterThread";
			t.IsBackground = false;
			t.Start();
		}

		private void bot_OnSendNotice(string botName, string target, string text)
		{
			// bot sets mode
			WriteToFile( target, "[Notice] "+text );		
		}

		private void bot_OnSetMode(string botName, string target, string text)
		{
			WriteToFile( target, "*** "+botName+" sets mode "+text );
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
				string key = (string)de.Key; 
				System.Collections.Specialized.StringCollection value = (System.Collections.Specialized.StringCollection)de.Value;
				if (value.Count > 0)	//ovenfor har vi skrevet datoskift
					SendMail(key, value);
				( (System.Collections.Specialized.StringCollection)de.Value ).Clear();
			}
		}


		public void WriteToFile(string file, string message) 
		{
			try 
			{
				if (file != "")
				{
					using ( writer = new StreamWriter(logFolderPath+file+".log", true, System.Text.Encoding.Default) ) 
					{
						writer.WriteLine( "["+DateTime.Now.ToLongTimeString()+"] "+message );
						chanlog[file].Add( "["+DateTime.Now.ToLongTimeString()+"] "+message );
					}
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
	public delegate void WriterDelegate(string file, string message);

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

	public class DelayWriter 
	{
		string file;
		string message;
		int delay;
		WriterDelegate method;

		public DelayWriter( string file, string message, int delay,  WriterDelegate writeDelegate ) 
		{
			this.file = file;
			this.delay = delay;
			this.message = message;
			this.method = writeDelegate;
		}

		public void Start() 
		{
			int err=0;
			try 
			{
				err=1;
				Thread.Sleep( delay );
				err=2;
				method(file, message);
				err=3;
			} 
			catch (Exception e) 
			{
				throw new Exception("DelayWriter.Start died with "+e.GetType().Name+": "+e.Message+", at step "+err+"");
			}
		}
	}
}
