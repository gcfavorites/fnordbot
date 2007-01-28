using System;
using System.Threading;
using System.Text.RegularExpressions;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// Summary description for Protocol.
	/// </summary>
	public class Protocol
	{
		Network network;

		#region event-forwarding
		public event MessageHandler OnPublicMessage;
		public event MessageHandler OnPrivateMessage;
		public event MessageHandler OnPublicNotice;
		public event MessageHandler OnPrivateNotice;
		public event ChannelTopicHandler OnTopicChange;
		public event ChannelActionHandler OnChannelJoin;
		public event ChannelActionHandler OnChannelPart;
		public event ChannelActionHandler OnChannelMode;
		public event ChannelActionHandler OnChannelKick;
		public event ChannelUserListHandler OnChannelUserList;
		public event NickChangeHandler OnNickChange;
		public event ServerDataHandler OnMotd;
		// events der trigges af botten selv
		public event BotMessageHandler OnSendToChannel;
		public event BotMessageHandler OnSendToUser;
		public event BotMessageHandler OnSendNotice;
		public event BotMessageHandler OnSetMode;

		/// <summary>
		/// Delegate for public messages
		/// </summary>
		public delegate void MessageHandler(Object bot, MessageEventArgs mea);
		public delegate void ChannelTopicHandler(Object bot, MessageEventArgs mea);
		public delegate void ChannelUserListHandler(string channel, string[] list);
		public delegate void ChannelActionHandler(Object bot, ChannelActionEventArgs cea);
		public delegate void ServerDataHandler(string data);
		public delegate void NickChangeHandler(string newname, string oldname, string hostmask);
		public delegate void BotMessageHandler( string target, string text ); // bottens eget navn kendes ikke her, det sættes på i client

		#endregion

		public delegate void LogMessageHandler(string message);
		public event LogMessageHandler OnLogMessage;
		private void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) OnLogMessage( message );
		}


		private string versionReply = "";
		private string fingerReply = "";
		private string altNick = "";
		/// <summary>
		/// The string to be sent as reply to VERSION requests
		/// </summary>
		public string VersionInfo 
		{
			get { return versionReply; }
			set { versionReply = value; }
		}

		public string AlternativeNick  
		{
			get { return altNick; }
			set { altNick = value; }
		}

		/// <summary>
		/// The string to be sent as reply to FINGER requests
		/// </summary>
		public string FingerInfo 
		{
			get { return fingerReply; }
			set { fingerReply = value; }
		}

		public Network Network 
		{
			get { return network; }
		}

		public Protocol()
		{
			network = new Network();
			network.OnServerMessage += new Network.ServerMessageHandler( ProcessMessage );
			network.OnLogMessage += new Network.LogMessageHandler( WriteLogMessage );
		} 

		public void Register(string nickname, string username, string realname) 
		{
			Thread.Sleep(1000);

			network.SendToServer("USER "+username+" a b :"+realname);//"USER foo bar baz :botting");

			network.SendToServer("NICK "+nickname);//wintermute");
		}


		#region control
		public void Connect(string server, int port) 
		{
			network.Connect(server, port);
		}

		public void Join(string channel) 
		{
			network.SendToServer("JOIN " + channel);
		}
		public void Part(string channel) 
		{
			network.SendToServer("PART " + channel);
		}
		public void SetNick( string nickname ) 
		{
			//TODO: trig event her
			network.SendToServer("NICK "+ nickname );
		}

		/// <summary>
		/// Sends a message to a channel
		/// </summary>
		/// <param name="text"></param>
		/// <param name="channel"></param>
		public void SendToChannel( string channel, string text ) 
		{
			OnSendToChannel(  channel, text );
			network.SendToServer("PRIVMSG "+channel+" :"+text);
		}

		/// <summary>
		/// Sends a message to a user
		/// </summary>
		/// <param name="text"></param>
		/// <param name="user"></param>
		public void SendToUser( string user, string text ) 
		{
			OnSendToUser( user, text );
			network.SendToServer("PRIVMSG "+user+" :"+text);
		}

		public void SetTopic(string channel, string topic) 
		{
			//TODO: trig event her
			network.SendToServer("TOPIC "+channel+" "+topic);
		}

		public void SetMode(string channel, string mode) 
		{
			OnSetMode( channel, mode );
			network.SendToServer("MODE "+channel+" "+mode);
		}

		public void Kick(string channel, string user ) 
		{
			//TODO: trig event her
			network.SendToServer("KICK "+channel+" "+user);
		}
		public void Kick(string channel, string user, string reason) 
		{
			//TODO: trig event her
			network.SendToServer("KICK "+channel+" "+user+" :"+reason);
		}

		public void SendNotice( string target, string message ) 
		{
			OnSendNotice( target, message );
			network.SendToServer("NOTICE "+target+" "+message);
		}
		#endregion

		#region processing
		private void ProcessMessage(string line) 
		{
			string[] parts = line.Split(new Char[] {' '},4);

			if ( line.StartsWith("PING :") ) 
			{
				network.SendToServer( "PONG :"+line.Substring(6) );
			} 
			else if ( parts[1] == "PRIVMSG" ) 
			{
				ParseMessage(line);
			} 
			else if ( parts[1] == "NOTICE" ) 
			{
				ParseNotice(line);
			} 
			else if ( parts[1] == "PART" ) 
			{
				ParseChannelPart( line );
			} 
			else if ( parts[1] == "KICK" ) 
			{
				ParseChannelKick( line );
			} 
			else if ( parts[1] == "JOIN" ) 
			{
				//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk JOIN :#bottest
				ParseChannelJoin( line );
			} 
			else if ( parts[1] == "MODE" ) 
			{
				ParseChannelMode( line );
			} 
			else if ( parts[1] == "TOPIC" ) 
			{
				ParseChannelTopic( line );
			} 
			else if ( parts[1] == "NICK") 
			{
				// :WiZ!jto@tolsun.oulu.fi NICK Kilroy
				ParseNickChange( line );
			}
			else if ( IsNumericReply( parts[1] ) )
			{
				ParseNumericReply( line );
			}
		}

		private void ParseNickChange( string line ) 
		{
			// :WiZ!jto@tolsun.oulu.fi NICK Kilroy
			string[] parts = line.Split(new Char[] {' '},4);
			int pos = parts[0].IndexOf("!");
			string username;
			string hostmask;
			if (pos>1) 
			{
				username = parts[0].Substring(1,pos-1);
				hostmask = parts[0].Substring(pos);
			} 
			else 
			{
				username = parts[0].Replace(":","");
				hostmask = "";
			}
			string newname = parts[2].Substring(1);

//			if (OnChannelJoin != null) OnChannelJoin(this, new ChannelActionEventArgs(target, user, hostmask, "") );
			if (OnNickChange != null) OnNickChange( newname, username, hostmask );

		}

		private void ParseChannelJoin(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk JOIN :#bottest
			string[] parts = line.Split(new Char[] {' '},4);
			int pos = parts[0].IndexOf("!");
			string user;
			string hostmask;
			if (pos>1) 
			{
				user = parts[0].Substring(1,pos-1);
				hostmask = parts[0].Substring(pos);
			} 
			else 
			{
				user = parts[0].Replace(":","");
				hostmask = "";
			}
			string target = parts[2].Substring(1);

			if (OnChannelJoin != null) OnChannelJoin(this, new ChannelActionEventArgs(target, user, hostmask, "") );
		}

		private void ParseChannelPart(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PART #bottest
			string[] parts = line.Split(new Char[] {' '},4);
			int pos = parts[0].IndexOf("!");
			string user;
			string hostmask;
			if (pos>1) 
			{
				user = parts[0].Substring(1,pos-1);
				hostmask = parts[0].Substring(pos);
			} 
			else 
			{
				user = parts[0].Replace(":","");
				hostmask = "";
			}
			string target = parts[2];

			if (OnChannelPart != null) OnChannelPart(this, new ChannelActionEventArgs(target, user, hostmask, "") );
		}

		private void ParseChannelMode(string line) 
		{
			// :WiZ!jto@tolsun.oulu.fi MODE #eu-opers -l
			string[] parts = line.Split(new Char[] {' '},4);
			int pos = parts[0].IndexOf("!");
			string user;
			string hostmask;
			if (pos>1) 
			{
				user = parts[0].Substring(1,pos-1);
				hostmask = parts[0].Substring(pos);
			} 
			else 
			{
				user = parts[0].Replace(":","");
				hostmask = "";
			}
			string target = parts[2];
			string mode = parts[3];

			if (OnChannelMode != null) OnChannelMode(this, new ChannelActionEventArgs(target, user, hostmask, mode) );
		}

		private void ParseChannelKick(string line) 
		{
			//:WiZ!jto@tolsun.oulu.fi KICK #Finnish John
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );

			string target = parts[2];

			if (OnChannelKick != null) OnChannelKick(this, new ChannelActionEventArgs(target, rd.Username, rd.Hostmask, parts[3]+"was kicked") ); // mangler en reason
		}

		private void ParseChannelTopic(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk TOPIC #bottest :fooo bar
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );

			string channel = parts[2];
			string topic = parts[3];
			if ( topic.Length > 0) topic = topic.Substring(1); // fjern kolon før topic

			if (OnTopicChange != null) OnTopicChange(this, new MessageEventArgs(topic, rd.Username, rd.Hostmask, channel ) ); // mangler en reason
		}

		// håndterer tekstsvar
		private void ParseMessage(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PRIVMSG #bottest :fooo
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );
			string target = parts[2];
			string message = parts[3].Substring(1);

			if (message.ToCharArray()[0] == 1) 
			{
				HandleCTCP(message,rd.Username);
			} 
			else if (target.StartsWith("#")) 
			{
				if (OnPublicMessage != null) OnPublicMessage(this, new MessageEventArgs(message,rd.Username,rd.Hostmask,target));
			} 
			else 
			{
				if (OnPrivateMessage != null) OnPrivateMessage(this, new MessageEventArgs(message,rd.Username,rd.Hostmask,target));
			}
		}

		private void ParseNotice(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PRIVMSG #bottest :fooo
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );
			string target = parts[2];
			string message = parts[3].Substring(1);

			if (message.ToCharArray()[0] == 1) 
			{
				HandleCTCP(message,rd.Username);
			} 
			else if (target.StartsWith("#")) 
			{
				if (OnPublicNotice != null) OnPublicNotice(this, new MessageEventArgs(message,rd.Username,rd.Hostmask,target));
			} 
			else 
			{
				if (OnPrivateNotice != null) OnPrivateNotice(this, new MessageEventArgs(message,rd.Username,rd.Hostmask,target));
			}
		}


		// håndterer numeriske svar
		private void ParseNumericReply(string line) 
		{
			string[] parts = line.Split(new Char[] {' '});

//			string server = parts[0].Trim();
//			string command = parts[1].Trim();
//			string args = parts[2].Trim();
			ReplyCode reply = ReplyCode.None;
			try 
			{
				reply = (ReplyCode)int.Parse( parts[1] );
			} 

			catch (Exception) {}

			if (reply == ReplyCode.RPL_MOTD) 
			{
				// :koala.droso.net 372 BimseBot :- This is ircd-hybrid MOTD replace it with something better
				string motd = line.Split(new Char[] {' '}, 4)[3];
				if (motd.Length>1) motd = motd.Substring(1);
				if (OnMotd != null) OnMotd( motd );
			} 
			else if (reply == ReplyCode.RPL_NAMREPLY) 
			{
				// :koala.droso.net 353 BimseBot = #bottest :BimseBot @NordCore
				string[] users = new string[parts.Length-5];
				for (int i=5; i<parts.Length; i++) users[i-5] = parts[i];
				if (OnChannelUserList != null) OnChannelUserList( parts[4], users );
			} 
			else if (reply == ReplyCode.RPL_TOPIC ) 
			{
				// :koala.droso.net 332 BimseBot #bottest :dingeling
				if (OnTopicChange != null) OnTopicChange( this, new MessageEventArgs( parts[4].Substring(1), "", "", parts[3] ) ); // dem kan vi kun få opdateret i en 333(rpl_topicauthor)
			}
			else if (reply == ReplyCode.RPL_NOTOPIC ) 
			{
				if (OnTopicChange != null) OnTopicChange( this, new MessageEventArgs( "", "", "", parts[3] ) );
			} 
			else if (reply == ReplyCode.ERR_NICKINUSE ) 
			{
				if (altNick.Length > 0) 
				{
					network.SendToServer( "NICK "+altNick );
					altNick = "";
				} 
				else 
				{
					network.SendToServer( "NICK "+parts[3]+new Random().Next(999).ToString("000") );
				}
			}
		}

		// håndterer ctcp privmsg's
		private void HandleCTCP(string message, string sender) 
		{
			string decoded_message = CTCPDequote(message); // fjern \001-quotes
			if (decoded_message.StartsWith("PING")) 
			{
				network.SendToServer("NOTICE "+sender+" :\u0001PING "+decoded_message.Substring(5)+"\u0001");
			} 
			else if (decoded_message.StartsWith("VERSION")) 
			{
				network.SendToServer("NOTICE "+sender+" :\u0001VERSION "+VersionInfo+"\u0001");
			} 
			else if (decoded_message.StartsWith("TIME")) 
			{
				network.SendToServer("NOTICE "+sender+" :\u0001TIME "+DateTime.Now.ToString()+"\u0001");
			} 
			else if (decoded_message.StartsWith("FINGER")) 
			{
				network.SendToServer("NOTICE "+sender+" :\u0001FINGER "+FingerInfo+"\u0001");
			} 
			else 
			{
				// HACK vi skal kunne sende fejl opad opå en pæn måde
				WriteLogMessage("got unknown "+decoded_message);
			}
		}
		#endregion

		#region helper methods
		private bool IsNumericReply(string txt) 
		{
			return Regex.IsMatch( txt, "\\d{3}" );
		}

	
		/// <summary>
		/// remove quotes from ctcp message
		/// </summary>
		/// <param name="quotedstring"></param>
		/// <returns></returns>
		private string CTCPDequote(string quotedstring) 
		{
			quotedstring = quotedstring.Substring(1);
			if (quotedstring.ToCharArray()[quotedstring.Length-1] == 1) 
			{
				quotedstring = quotedstring.Substring(0,quotedstring.Length-1);
			} 
			else 
			{
				Console.WriteLine("ends with "+quotedstring.ToCharArray()[quotedstring.Length-1]);
			}
			return quotedstring;
		}
		#endregion
	}

	public struct ReplyData 
	{
		public string Username;
		public string Hostmask;
		public string Command;

		public static ReplyData GetFromString(string line) 
		{
			string[] parts = line.Split(new Char[] {' '},4);
			return GetFromArray( parts );
		}
		public static ReplyData GetFromArray( string[] parts ) 
		{

			int pos = parts[0].IndexOf("!");
			ReplyData rd = new ReplyData();
			
			if (pos>1) 
			{
				rd.Username = parts[0].Substring(1,pos-1);
				rd.Hostmask = parts[0].Substring(pos);
			} 
			else 
			{
				rd.Username = parts[0].Replace(":","");
				rd.Hostmask = "";
			}			
			rd.Command = parts[1];

			return rd;
		}
	}

	public class MessageEventArgs : EventArgs 
	{
		internal MessageEventArgs(string message, string sender, string hostmask, string channel) 
		{
			this.message = message;
			this.sender = sender;
			this.hostmask = hostmask;
			this.channel = channel;
		}
		private string message;
		private string sender;
		private string hostmask;
		private string channel;

		/// <summary>
		/// the message recieved
		/// </summary>
		public string Message 
		{
			get { return message; }
		}
		/// <summary>
		/// the sender of the message
		/// </summary>
		public string Sender 
		{
			get { return sender; }
		}
		/// <summary>
		/// hostmask of the sender
		/// </summary>
		public string Hostmask 
		{
			get { return hostmask; }
		}
		/// <summary>
		/// channel the message was sent to
		/// </summary>
		public string Channel 
		{
			get { return channel; }
		}
	}
	public class ChannelActionEventArgs : EventArgs 
	{
		internal ChannelActionEventArgs(string channel, string nickname,  string hostmask, string text) 
		{
			this.channel = channel;
			this.nickname = nickname;
			this.hostmask = hostmask;
			this.text = text;
		}
		private string channel;
		private string nickname;
		private string hostmask;
		private string text;

		/// <summary>
		/// Channel the action occurred on
		/// </summary>
		public string Channel 
		{
			get { return channel; }
		}

		/// <summary>
		/// who initiated the action
		/// </summary>
		public string Nickname 
		{
			get { return nickname; }
		}
		/// <summary>
		/// Hostmask of the user that initiated the action
		/// </summary>
		public string Hostmask 
		{
			get { return hostmask; }
		}
		/// <summary>
		/// Text of the action
		/// </summary>
		public string Text 
		{
			get { return text; }
		}
	}



}
