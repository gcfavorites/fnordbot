//using System;
//using NielsRask.LibIrc2;
//
//namespace NielsRask.FnordBot
//{
//	/// <summary>
//	/// Summary description for FnordBot.
//	/// </summary>
//	public class FnordBot
//	{
//		NielsRask.LibIrc2.Application irc;
//		public FnordBot()
//		{
//			irc = new Application();
//			irc.Channels.OnChannelListChange += new ChannelCollection.ChannelEvent(Channels_OnChannelListChange);
//			irc.Protocol.Network.OnServerMessage += new NielsRask.LibIrc2.Network.ServerMessageHandler(Client_OnServerMessage);
//			irc.OnPublicMessage += new NielsRask.LibIrc2.Application.MessageHandler(irc_OnPublicMessage);
//		}
//
//		public void Connect() 
//		{
//			irc.Connect();
////			irc.Join("#craYon");
//			irc.Join("#bottest");
//			
//		}
//
//		public void ShowInfo() 
//		{
//			Channels_OnChannelListChange();
//		}
//
//		private void Channels_OnChannelListChange()
//		{
//			foreach (Channel chn in irc.Channels) 
//			{
//				Console.WriteLine(""+chn.Name+" ("+chn.Topic+")");
//				foreach (User usr in chn.Users) 
//				{
//					Console.WriteLine("  "+usr.ToString());
//				}
//			}
//		}
//
//		private void Client_OnServerMessage(string message)
//		{
//			Console.WriteLine("-> "+message);
//		}
//
//		private void irc_OnPublicMessage(object bot, NielsRask.LibIrc2.MessageEventArgs mea)
//		{
//			if (mea.Message=="*marv*") irc.SendToUser( mea.Sender, "*marv*" );
//		}
//	}
//}
