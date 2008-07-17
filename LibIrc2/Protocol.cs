using System;
using System.Threading;
using System.Text.RegularExpressions;
using log4net;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// Summary description for Protocol.
	/// </summary>
	public class Protocol
	{
	    readonly Network network;
		private string versionReply = "";
		private string nickName = "";
		private string fingerReply = "";
		private string altNick = "";
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly PingListener pingListener;

		#region events
		/// <summary>
		/// Occurs when a public message is received in a channel
		/// </summary>
		public event MessageHandler OnPublicMessage;

		/// <summary>
		/// Occurs when a private message is received
		/// </summary>
		public event MessageHandler OnPrivateMessage;
		/// <summary>
		/// Occurs when a public notice is received
		/// </summary>
		public event MessageHandler OnPublicNotice;

		/// <summary>
		/// Occurs when a private notice is received
		/// </summary>
		public event MessageHandler OnPrivateNotice;

		/// <summary>
		/// Occurs when the topic in a joined channel changes
		/// </summary>
		public event ChannelTopicHandler OnTopicChange;

		/// <summary>
		/// Occurs when someone joins a channel
		/// </summary>
		public event ChannelActionHandler OnChannelJoin;

		/// <summary>
		/// Occurs when someone leaves a channel
		/// </summary>
		public event ChannelActionHandler OnChannelPart;

		/// <summary>
		/// Occurs when someone quits IRC entirely
		/// </summary>
		public event ChannelActionHandler OnServerQuit;

		/// <summary>
		/// Occurs when the mode of a channel changes
		/// </summary>
		public event ChannelActionHandler OnChannelMode;

		/// <summary>
		/// Occurs when someone is kicked from a channel
		/// </summary>
		public event ChannelActionHandler OnChannelKick;

		/// <summary>
		/// Occurs when we receive a channel userlist, as reply to a query
		/// </summary>
		public event ChannelUserListHandler OnChannelUserList;

		/// <summary>
		/// Occurs when someone changes their nickname
		/// </summary>
		public event NickChangeHandler OnNickChange;

		/// <summary>
		/// Occurs when we receive the message-of-the-day from the server. it is then ok to join a channel
		/// </summary>
		public event ServerDataHandler OnMotd;

		/// <summary>
		/// Occurs when we send a message to a channel
		/// </summary>
		public event BotMessageHandler OnSendToChannel;

		/// <summary>
		/// Occurs when we send a message to a user
		/// </summary>
		public event BotMessageHandler OnSendToUser;

		/// <summary>
		/// Occurs when we send a notice to a user or a channel
		/// </summary>
		public event BotMessageHandler OnSendNotice;

		/// <summary>
		/// Occurs when we set the mode for a channel
		/// </summary>
		public event BotMessageHandler OnSetMode;

		/// <summary>
		/// Occurs when a private action is received
		/// </summary>
		public event MessageHandler OnPrivateAction;

		/// <summary>
		/// Occurs when a public action is received
		/// </summary>
		public event MessageHandler OnPublicAction;

		/// <summary>
		/// Delegate for messages from server
		/// </summary>
		/// <param name="senderNick">Nickname of the user sending the message</param>
		/// <param name="senderHost">Host of the user sending the message</param>
		/// <param name="message">The message</param>
		/// <param name="target">The recipient of the message</param>
		public delegate void MessageHandler(string message, string target, string senderNick, string senderHost);

		/// <summary>
		/// delegate for channel actions from server
		/// </summary>
		/// <param name="senderNick">Nickname of the user that initiated the action</param>
		/// <param name="senderHost">Host of the user that initiated the action</param>
		/// <param name="text">Text for the action, such as reason for a kick</param>
		/// <param name="channel">The channel the action occurred in</param>
		/// <param name="target">Target of the action, if applicable</param>
		public delegate void ChannelActionHandler(string text, string channel, string target, string senderNick, string senderHost);

		/// <summary>
		/// Delegate for topic changes from server
		/// </summary>
		/// <param name="changerNick">Nickname of the user changing topic</param>
		/// <param name="changerHost">Host of the user changing topic</param>
		/// <param name="channel">The channel whose topic was changed</param>
		/// <param name="newTopic">The new topic of the channel</param>
		public delegate void ChannelTopicHandler(string newTopic, string channel, string changerNick, string changerHost);
		/// <summary>
		/// Delegate for receiving the userlist for a channel
		/// </summary>
		public delegate void ChannelUserListHandler(string channel, string[] list);
	
		/// <summary>
		/// Delegate for receiving data from the server
		/// </summary>
		public delegate void ServerDataHandler(string data);
	
		/// <summary>
		/// Delegate for information about nickname changes
		/// </summary>
		/// <param name="newname">The new name of the user</param>
		/// <param name="oldname">The old name of the user</param>
		/// <param name="hostmask">The host of the user changing nickname</param>
		public delegate void NickChangeHandler(string newname, string oldname, string hostmask);
	
		/// <summary>
		/// Delegate for messages and action generated by ourselves
		/// </summary>
		/// <param name="target">The target of the action, user or channel</param>
		/// <param name="text">The text that we sent</param>
		public delegate void BotMessageHandler( string target, string text ); // bottens eget navn kendes ikke her, det sættes på i client

		#endregion

		/// <summary>
		/// The string to be sent as reply to VERSION requests
		/// </summary>
		public string VersionInfo 
		{
			get { return versionReply; }
			set { versionReply = value; }
		}

		/// <summary>
		/// Gets or sets the alternative nick.
		/// </summary>
		/// <value>The alternative nick.</value>
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

		/// <summary>
		/// Returns the underlying network layer
		/// </summary>
		public Network Network 
		{
			get { return network; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Protocol"/> class.
		/// </summary>
		public Protocol()
		{
			network = new Network();
			network.OnServerMessage += ProcessMessage;
//			network.OnLogMessage += new Network.LogMessageHandler( WriteLogMessage );

			// pinglistener skal opdage når vi ikke har fået pings længe
			pingListener = new PingListener(network);
			network.OnServerMessage += pingListener.ProcessMessage;
			Thread tPingListen = new Thread( pingListener.Start );
			tPingListen.Name = "PingListenerThread";
			tPingListen.IsBackground = true;
			tPingListen.Start();
		} 

		/// <summary>
		/// Registers on the server after logon
		/// </summary>
		/// <param name="nickname">The nickname.</param>
		/// <param name="username">The username.</param>
		/// <param name="realname">The realname.</param>
		public void Register(string nickname, string username, string realname) 
		{
			nickName = nickname;
			Thread.Sleep(1000);

			network.SendToServer("USER "+username+" a b :"+realname);//"USER foo bar baz :botting");

			log.Info("Registering on server with nickname '"+nickname+"'");
			network.SendToServer("NICK "+nickname);//wintermute");
		}

		#region Control methods
		/// <summary>
		/// Connects to the specified server.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		public void Connect(string server, int port) 
		{
			network.Connect(server, port);
		}

		/// <summary>
		/// Joins the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void Join(string channel) 
		{
			network.SendToServer("JOIN " + channel);
		}
		/// <summary>
		/// Parts the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void Part(string channel) 
		{
			network.SendToServer("PART " + channel);
		}
		/// <summary>
		/// Sets the nick.
		/// </summary>
		/// <param name="nickname">The nickname.</param>
		public void SetNick( string nickname ) 
		{
			if (OnNickChange != null)
				OnNickChange(nickname, "", "");

			network.SendToServer("NICK "+ nickname );
		}

		/// <summary>
		/// Sends a message to a channel
		/// </summary>
		/// <param name="text"></param>
		/// <param name="channel"></param>
		public void SendToChannel( string channel, string text ) 
		{
			if (OnSendToChannel != null) 
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
			if (OnSendToUser != null) 
				OnSendToUser( user, text );

			network.SendToServer("PRIVMSG "+user+" :"+text);
		}

		/// <summary>
		/// Set the topic for a channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="topic"></param>
		public void SetTopic(string channel, string topic) 
		{
			if (OnTopicChange != null) 
				OnTopicChange(topic, channel,"","");	//TODO: get nick and host of the bot

			network.SendToServer("TOPIC "+channel+" "+topic);
		}

		/// <summary>
		/// Sets the mode fior a channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="mode">The mode.</param>
		public void SetMode(string channel, string mode) 
		{
			if (OnSetMode != null) 
				OnSetMode( channel, mode );

			network.SendToServer("MODE "+channel+" "+mode);
		}

		/// <summary>
		/// Kick someone from a channel without a reason
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="user">The user.</param>
		public void Kick(string channel, string user ) 
		{
			if (OnChannelKick != null) 
				OnChannelKick("", channel, user, "","");
			
			network.SendToServer("KICK "+channel+" "+user);
		}

		/// <summary>
		/// kicks someone from a channel with the specified reason
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="user">The user.</param>
		/// <param name="reason">The reason.</param>
		public void Kick(string channel, string user, string reason) 
		{
			if (OnChannelKick != null)
				OnChannelKick(reason,channel,user,"","");

			network.SendToServer("KICK "+channel+" "+user+" :"+reason);
		}

		/// <summary>
		/// Sends a notice to a user or channel.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="message">The message.</param>
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
			else if ( parts[1] == "NICK" ) 
			{
				// :WiZ!jto@tolsun.oulu.fi NICK Kilroy
				ParseNickChange( line );
			}
			else if ( parts[1] == "QUIT" )
			{
				// :smcRanger!~smc@user103.77-105-195.netatonce.net QUIT :Remote host closed the connection
				ParseServerQuit( line );
			}
			else if ( IsNumericReply( parts[1] ) )
			{
				ParseNumericReply( line );
			}
			else 
			{
				log.Warn("Fell through ProcessMessage cases on string '"+line+"'");
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
			if (OnNickChange != null) 
				OnNickChange( newname, username, hostmask );

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

			if (OnChannelJoin != null) 
				OnChannelJoin("", target, "", user, hostmask);
		}

		private void ParseChannelPart(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PART #bottest
			string[] parts = line.Split(new Char[] {' '},3);
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

			if (OnChannelPart != null) 
				OnChannelPart("", target,"",user, hostmask);
		}

		private void ParseServerQuit(string line) 
		{
			// ':smcRanger!~smc@user103.77-105-195.netatonce.net QUIT :Remote host closed the connection'
			string[] parts = line.Split(new Char[] {' '},3);
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
			string msg = parts[2];
			if (msg.StartsWith(":"))
				msg = msg.Substring(1);

			if (OnServerQuit != null) 
				OnServerQuit(msg, "","",user, hostmask);

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

			if (OnChannelMode != null) 
				OnChannelMode(mode, target, "",user, hostmask);
		}

		private void ParseChannelKick(string line) 
		{
			//:WiZ!jto@tolsun.oulu.fi KICK #Finnish John
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );

			string channel = parts[2];
			string victim = parts[3];

			if (OnChannelKick != null) 
				OnChannelKick("",channel,victim, rd.Username, rd.Hostmask); // TODO: mangler en reason
		}

		private void ParseChannelTopic(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk TOPIC #bottest :fooo bar
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );

			string channel = parts[2];
			string topic = parts[3];
			if ( topic.Length > 0) topic = topic.Substring(1); // fjern kolon før topic

			if (OnTopicChange != null) 
				OnTopicChange(topic, channel, rd.Username, rd.Hostmask);
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
				ParseCTCP(line);
			else if (target.StartsWith("#")) 
				if (OnPublicMessage != null) 
					OnPublicMessage(message, target, rd.Username, rd.Hostmask);
			else 
				if (OnPrivateMessage != null) 
					OnPrivateMessage(message, target, rd.Username, rd.Hostmask);
		}

		private void ParseCTCP( string line ) 
		{
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );
			string target = parts[2];
			string message = parts[3].Substring(1);

			string decoded_message = CTCPDequote(message); // fjern \001-quotes
			if (decoded_message.StartsWith("PING")) 
				network.SendToServer("NOTICE "+rd.Username+" :\u0001PING "+decoded_message.Substring(5)+"\u0001");
			else if (decoded_message.StartsWith("VERSION")) 
				network.SendToServer("NOTICE "+rd.Username+" :\u0001VERSION "+VersionInfo+"\u0001");
			else if (decoded_message.StartsWith("TIME")) 
				network.SendToServer("NOTICE "+rd.Username+" :\u0001TIME "+DateTime.Now+"\u0001");
			else if (decoded_message.StartsWith("FINGER")) 
				network.SendToServer("NOTICE "+rd.Username+" :\u0001FINGER "+FingerInfo+"\u0001");
			else if (decoded_message.StartsWith("ACTION"))
			{
				// :smcRanger!~smc@user103.77-105-195.netatonce.net PRIVMSG #craYon :ACTION havde lige samme tanke
				decoded_message = decoded_message.Substring( 7 ); //fjern "ACTION "
				if (target.StartsWith("#")) 
					if (OnPublicAction != null) 
						OnPublicAction(decoded_message, target, rd.Username, rd.Hostmask);
					else 
						if (OnPrivateAction != null) 
						OnPrivateAction(decoded_message, target, rd.Username, rd.Hostmask);
			}
			else if (decoded_message.StartsWith("SOUND")) 
			{
				//SOUND ignorerer vi bare ...
			}
			else
				log.Warn("ParseCTCP got unknown message '"+message+"'");
		}

		private void ParseNotice(string line) 
		{
			//:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PRIVMSG #bottest :fooo
			string[] parts = line.Split(new Char[] {' '},4);

			ReplyData rd = ReplyData.GetFromArray( parts );
			string target = parts[2];
			string message = parts[3].Substring(1);

			if (message.ToCharArray()[0] == 1) 
				ParseCTCP( line );
			else if (target.StartsWith("#")) 
				if (OnPublicNotice != null) 
					OnPublicNotice(message, target, rd.Username, rd.Hostmask);
			else 
				if (OnPrivateNotice != null) 
					OnPrivateNotice(message, target, rd.Username, rd.Hostmask);
		}


		private string motd = "";

		private void ParseNumericReply(string line) 
		{
			string[] parts = line.Split(new Char[] {' '});

			ReplyCode reply = ReplyCode.None;
			try 
			{
				reply = (ReplyCode)int.Parse( parts[1] );
			} 
			catch (Exception)
			{}

			if (reply == ReplyCode.RPL_MOTD) 
			{
				// :koala.droso.net 372 BimseBot :- This is ircd-hybrid MOTD replace it with something better
				string motdline = line.Split(new Char[] {' '}, 4)[3];
				if (motdline.Length>1) 
					motd += motdline.Substring(1);
			} 
			else if (reply == ReplyCode.RPL_MOTDSTART) 
			{
				motd = "";// nulstil eksisterende motd
			}
			else if (reply == ReplyCode.RPL_ENDOFMOTD)
			{	// vi formodes allerede at have opsamlet motd - vi signalerer nu at den er færdig
				if (OnMotd != null) 
					OnMotd( motd );
			}
			else if (reply == ReplyCode.RPL_NAMREPLY) 
			{
				// :koala.droso.net 353 BimseBot = #bottest :BimseBot @NordCore
				string[] users = new string[parts.Length-5];
				for (int i=5; i<parts.Length; i++) 
					users[i-5] = parts[i];
				if (OnChannelUserList != null) 
					OnChannelUserList( parts[4], users );
			} 
			else if (reply == ReplyCode.RPL_TOPIC ) 
			{
				// :koala.droso.net 332 BimseBot #bottest :dingeling
				if (OnTopicChange != null) 
					OnTopicChange(parts[4].Substring(1),parts[3],"",""); // dem kan vi kun få opdateret i en 333(rpl_topicauthor)
			}
			else if (reply == ReplyCode.RPL_NOTOPIC ) 
			{
				if (OnTopicChange != null) 
					OnTopicChange("",parts[3],"","" );
			} 
			else if (reply == ReplyCode.ERR_NICKINUSE ) 
			{
				log.Warn("It seems nickname '"+nickName+"' is in use");
				if (altNick.Length > 0) 
				{
					log.Info("Trying alternative nick '"+altNick+"'");
					network.SendToServer( "NICK "+altNick );
					altNick = "";	// next time we get this message, we'll generate a new nick
				} 
				else 
				{
					string randnick = parts[3]+new Random().Next(999).ToString("0000");
					log.Info("Trying random nick '"+randnick+"'");
					network.SendToServer( "NICK "+randnick );
				}
			}
		}

		#endregion

		#region helper methods
		private static bool IsNumericReply(string txt) 
		{
			return Regex.IsMatch( txt, "\\d{3}" );
		}

		/// <summary>
		/// remove quotes from ctcp message
		/// </summary>
		/// <param name="quotedstring"></param>
		/// <returns></returns>
		private static string CTCPDequote(string quotedstring) 
		{
			quotedstring = quotedstring.Substring(1);
			if (quotedstring.ToCharArray()[quotedstring.Length-1] == 1) 
			{
				quotedstring = quotedstring.Substring(0,quotedstring.Length-1);
			} 
			else 
			{
				log.Warn("CTCPDequote: "+quotedstring+" ends with "+quotedstring.ToCharArray()[quotedstring.Length-1]);
			}
			return quotedstring;
		}
		#endregion
	}

	/// <summary>
	/// Struct for parsing replies
	/// </summary>
	public struct ReplyData 
	{
		/// <summary>
		/// The name of the user
		/// </summary>
		public string Username;

		/// <summary>
		/// The host of the user
		/// </summary>
		public string Hostmask;

		/// <summary>
		/// The command
		/// </summary>
		public string Command;

		/// <summary>
		/// parse a string
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static ReplyData GetFromString(string line) 
		{
			string[] parts = line.Split(new Char[] {' '},4);
			return GetFromArray( parts );
		}

		/// <summary>
		/// parse an array
		/// </summary>
		/// <param name="parts"></param>
		/// <returns></returns>
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

	/// <exclude>
	public class PingListener 
	{
		private DateTime lastPing = DateTime.Now;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Network network;

		/// <exclude>
		public PingListener(Network network)
		{
			this.network = network;
		}

		/// <exclude>
		public void Start() 
		{
			log.Info("PingListener was started ...");
			bool alive = true;
			while (alive) 
			{
				if ( lastPing < DateTime.Now.AddMinutes(-5) ) 
				{
					log.Warn("Last ping was recieved at "+lastPing+" - connection might be lost!");
					network.CallOnDisconnect();
					alive = false;
				}
				else
					Thread.Sleep( new TimeSpan(0, 5, 0) );
			}
		}
	
		/// <exclude>
		public void ProcessMessage(string line) 
		{
			if ( line.StartsWith("PING") ) 
				lastPing = DateTime.Now;
		}
	}
}
