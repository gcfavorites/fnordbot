using System;
using NielsRask.FnordBot;
using System.IO;
//using System.

namespace Logger
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Logger : IPlugin
	{
		FnordBot bot;
		StreamWriter writer;
		string logFolderPath = "c:\\";

		public Logger()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region IPlugin Members

		public void Attach(FnordBot bot)
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

		public void Init(System.Xml.XmlNode pluginNode)
		{
			// TODO:  Add Logger.Init implementation
			try 
			{
				logFolderPath = pluginNode.SelectSingleNode("logfolderpath/text()").Value;
			} 
			catch {}
			Directory.CreateDirectory( logFolderPath );
			if ( !logFolderPath.EndsWith("\\") ) logFolderPath += "\\";
		}

		#endregion

		private void bot_OnPublicMessage(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			WriteToFile( channel, "<"+user.Name+"> "+message );
		}

		private void bot_OnPrivateMessage(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			WriteToFile( channel, "<"+user.Name+"> "+message );
		}

		private void bot_OnChannelJoin(object bot, NielsRask.LibIrc.ChannelActionEventArgs cea)
		{
			// *** Pornoting (tingeling@0x50c4070d.hrnxx2.adsl-dhcp.tele.dk) has joined #craYon
			WriteToFile( cea.Channel, "*** "+cea.Nickname+" ("+cea.Hostmask+") has joined "+cea.Channel );
		}

		private void bot_OnChannelPart(object bot, NielsRask.LibIrc.ChannelActionEventArgs cea)
		{
			WriteToFile( cea.Channel, "*** "+cea.Nickname+" ("+cea.Hostmask+") has left "+cea.Channel );
		}

		private void bot_OnChannelMode(object bot, NielsRask.LibIrc.ChannelActionEventArgs cea)
		{
			WriteToFile( cea.Channel, "***"+cea.Nickname+" sets mode "+cea.Text );
		}

		private void bot_OnChannelKick(object bot, NielsRask.LibIrc.ChannelActionEventArgs cea)
		{
			WriteToFile( cea.Channel, "*** "+cea.Nickname+" kicked "+cea.Text );
		}

		private void bot_OnPrivateNotice(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			WriteToFile( user.Name, "[Notice] "+message );
		}

		private void bot_OnPublicNotice(NielsRask.FnordBot.Users.User user, string channel, string message)
		{
			WriteToFile( channel, "[Notice] "+message );
		}

		private void bot_OnTopicChange(NielsRask.FnordBot.Users.User user, string channel, string topic)
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