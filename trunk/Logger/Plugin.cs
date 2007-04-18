using System;
using NielsRask.FnordBot;
using System.IO;
using log4net;

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

		public Plugin()
		{}

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
				bot.OnPrivateNotice += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPrivateNotice);
				bot.OnPublicNotice += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicNotice);
				bot.OnTopicChange += new NielsRask.FnordBot.FnordBot.ChannelTopicHandler(bot_OnTopicChange);
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
			WriteToFile( channel, "<"+user.Name+"> "+message );
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
			WriteToFile( user.Name, "[Notice] "+message );
		}

		private void bot_OnPublicNotice(NielsRask.FnordBot.User user, string channel, string message)
		{
			WriteToFile( channel, "[Notice] "+message );
		}

		private void bot_OnTopicChange(NielsRask.FnordBot.User user, string channel, string topic)
		{
			WriteToFile( channel, "*** "+user.Name+" sets new topic: "+topic );
		}
		private void WriteToFile(string file, string message) 
		{
			using ( writer = new StreamWriter(logFolderPath+file+".log", true, System.Text.Encoding.Default) ) 
			{
				writer.WriteLine( "["+DateTime.Now.ToLongTimeString()+"] "+message );
			}
		}

	}
}
