using System;
using System.Collections;
using System.Threading;
using log4net;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// This class provides high-level irc functions
	/// </summary>
	public class Client
	{
	    readonly Protocol protocol;
	    readonly ChannelCollection channels;
		string server = "irc.droso.net";
		int port = 6667;
		string nickname = "Bimsebot";
		string username = "bimse";
		string realname = "B. Imse";
		string altNick = "Bimmer";
		string fingerInfo = "fnord";
		string versionInfo = "LibIrc "+System.Reflection.Assembly.GetCallingAssembly().GetName().Version;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// majic numbaz
		// truncate any message longer than this
	    readonly int maxMessageLength = 250;

		/// <summary>
		/// Gets or sets the server.
		/// </summary>
		/// <value>The server.</value>
		public string Server 
		{
			get { return server; }
			set { server = value; }
		}

		/// <summary>
		/// Gets or sets the port.
		/// </summary>
		/// <value>The port.</value>
		public int Port 
		{
			get { return port; }
			set { port = value; }
		}

		/// <summary>
		/// Gets or sets the nickname.
		/// </summary>
		/// <value>The nickname.</value>
		public string Nickname 
		{
			get { return nickname; }
			set { nickname = value; }
		}

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The username.</value>
		public string Username 
		{
			get { return username; }
			set { username = value; }
		}

		/// <summary>
		/// Gets or sets the realname.
		/// </summary>
		/// <value>The realname.</value>
		public string Realname 
		{
			get { return realname; }
			set { realname = value; }
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
		/// Gets or sets the finger info.
		/// </summary>
		/// <value>The finger info.</value>
		public string FingerInfo 
		{
			get { return fingerInfo; }
			set { fingerInfo = value; }
		}

		/// <summary>
		/// Gets or sets the version info.
		/// </summary>
		/// <value>The version info.</value>
		public string VersionInfo 
		{
			get { return versionInfo; }
			set { versionInfo = value; }
		}

		/// <summary>
		/// Gets the channels.
		/// </summary>
		/// <value>The channels.</value>
		public ChannelCollection Channels 
		{
			get { return channels; }
		}

		/// <summary>
		/// Gets the protocol layer.
		/// </summary>
		/// <value>The protocol.</value>
		public Protocol Protocol 
		{
			get { return protocol; }
        }

        #region forwards from protocol
        /// <summary>
		/// Occurs when a public message is received
		/// </summary>
		public event Protocol.MessageHandler OnPublicMessage;

		/// <summary>
		/// Occurs when a private message is received
		/// </summary>
        public event Protocol.MessageHandler OnPrivateMessage;

		/// <summary>
		/// Occurs when a public notice is received
		/// </summary>
        public event Protocol.MessageHandler OnPublicNotice;

		/// <summary>
		/// Occurs when a private notice is received
		/// </summary>
        public event Protocol.MessageHandler OnPrivateNotice;

		/// <summary>
		/// Occurs when we receive MOTD from server. we are then ready to join channels
		/// </summary>
        public event Protocol.ServerDataHandler OnMotd;

        /// <summary>
        /// Occurs when a public action is received
        /// </summary>
        public event Protocol.MessageHandler OnPublicAction;

        /// <summary>
        /// Occurs when a private action is received
        /// </summary>
        public event Protocol.MessageHandler OnPrivateAction;

        /// <summary>
        /// Occurs when a topic change is received
        /// </summary>
        public event Protocol.ChannelTopicHandler OnTopicChange;
        #endregion

        #region forwards from network
		/// <summary>
		/// Occurs when the server sends us a message
		/// </summary>
        public event Network.ServerMessageHandler OnServerMessage;
        #endregion

        /// <summary>
		/// Occurs when a user changes nickname
		/// </summary>
		public event NickChangeHandler OnNickChange;

		//bot-actions
		/// <summary>
		/// Occurs when we send a public message
		/// </summary>
		public event BotMessageHandler OnSendToChannel;

		/// <summary>
		/// Occurs when we send a private message
		/// </summary>
		public event BotMessageHandler OnSendToUser;

		/// <summary>
		/// Occurs when we send a notice
		/// </summary>
		public event BotMessageHandler OnSendNotice;	//TODO: seperat private/public

		/// <summary>
		/// Occurs when we set the mode of a channel
		/// </summary>
		public event BotMessageHandler OnSetMode;
			
		/// <summary>
		/// Delegate for nickname changes
		/// </summary>
		public delegate void NickChangeHandler(string newname, string oldname, string hostmask);

		/// <summary>
		/// Delegate for messages from ourselves
		/// </summary>
		public delegate void BotMessageHandler( string botName, string target, string text ); // bottens eget navn kendes ikke her, det sættes på i client

		/// <summary>
		/// Initializes a new instance of the <see cref="Client"/> class.
		/// </summary>
		public Client()
		{
			protocol = new Protocol();
			protocol.AlternativeNick = altNick;
			channels = new ChannelCollection( protocol );
			protocol.OnChannelUserList += OnChannelUserList;
			protocol.OnPrivateMessage += protocol_OnPrivateMessage;
			protocol.OnPublicMessage += protocol_OnPublicMessage;
			protocol.OnPublicNotice += protocol_OnPublicNotice;
			protocol.OnPrivateNotice += protocol_OnPrivateNotice;

			protocol.OnSendToUser += protocol_OnSendToUser;
			protocol.OnSendToChannel += protocol_OnSendToChannel;
			protocol.OnSetMode += protocol_OnSetMode;
			protocol.OnSendNotice += protocol_OnSendNotice;
			protocol.OnNickChange += protocol_OnNickChange;
			protocol.Network.OnDisconnect += Network_OnDisconnect;
            protocol.Network.OnServerMessage += Network_OnServerMessage;
			protocol.OnMotd += protocol_OnMotd;

			protocol.OnTopicChange += protocol_OnTopicChange;
			protocol.OnPublicAction += protocol_OnPublicAction;
			protocol.OnPrivateAction += protocol_OnPrivateAction;
		}



		#region control

		/// <summary>
		/// Connects to the server.
		/// </summary>
		public void Connect() 
		{
			log.Info("Client: connecting to "+server+":"+port+"");
			Console.WriteLine( "Client.connect" );
			protocol.FingerInfo = fingerInfo;
			protocol.VersionInfo = versionInfo;
			protocol.Connect( server, port );
		
			// Det skal vi imkke gøre endnu, først når vi har fået MOTD tror jeg
			Thread.Sleep(1500);
			protocol.Register( nickname, username, realname );
		}

		/// <summary>
		/// Joins a channel
		/// </summary>
		/// <param name="channel"></param>
		public void Join(string channel) 
		{
			protocol.Join( channel );
		}

		/// <summary>
		/// leave a channel
		/// </summary>
		/// <param name="channel"></param>
		public void Part(string channel) 
		{
			protocol.Part( channel );
		}

		/// <summary>
		/// Send message to user
		/// </summary>
		/// <param name="user"></param>
		/// <param name="text"></param>
		public void SendToUser( string user, string text ) 
		{
			protocol.SendToUser( user, text );
		}

		/// <summary>
		/// send message to channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		public void SendToChannel( string channel, string text ) 
		{
			if (text.Length > maxMessageLength) 
			{
				text = text.Substring(0, maxMessageLength);
				log.Info("Flood-prevention: Truncated a line at "+maxMessageLength+" characters!");
			}
			protocol.SendToChannel( channel, text );
		}

		/// <summary>
		/// Send notice to user or channel
		/// </summary>
		/// <param name="target"></param>
		/// <param name="message"></param>
		public void SendNotice( string target, string message ) 
		{
			protocol.SendNotice( target, message );
		}
		#endregion

		#region stuff
		private void OnChannelUserList(string channel, string[] list)
		{
			GetChannel(channel).SetUserList( list );
		}

		private Channel GetChannel(string name) 
		{
			Channel chn = channels.GetChannelByName( name );
			if ( chn == null ) 
			{
				chn = new Channel( protocol, name);
				channels.Add( chn );
			} 
			return chn;
		}

        // this is called when the network layer detects a disconnect
        private void Network_OnDisconnect()
        {
            log.Warn("Got disconnected event - Starting reconnect loop");
            bool connected = false;
            while (!connected)
            {
                Thread.Sleep(30 * 1000);   // sleep for a bit before trying to reconnect
                try
                {
                    log.Info("Reconnecting ...");
					Console.WriteLine( "Network_OnDisconnect: reconnecting" );
					Connect();                       // try reconnecting
                    log.Warn("Reconnected to server");
                    connected = true;                // this will break the while-block
                }
                catch (ConnectionRefusedException)
                {}
            }
	    }

	    #endregion

        #region event forwards

        private void protocol_OnPrivateMessage(string message, string target, string senderNick, string senderHost)
		{
			if (OnPrivateMessage != null) 
				OnPrivateMessage( message, target, senderNick, senderHost );
		}

		private void protocol_OnPublicMessage(string message, string target, string senderNick, string senderHost)
		{
			if (OnPublicMessage != null) 
				OnPublicMessage( message, target, senderNick, senderHost );
		}

		private void protocol_OnPublicNotice(string message, string target, string senderNick, string senderHost)
		{
			if (OnPublicNotice != null) 
				OnPublicNotice( message, target, senderNick, senderHost );
		}

		private void protocol_OnPrivateNotice(string message, string target, string senderNick, string senderHost)
		{
			if (OnPrivateNotice != null) 
				OnPrivateNotice( message, target, senderNick, senderHost );
		}

		private void protocol_OnSendToUser(string target, string text)
		{
			if (OnSendToUser != null) 
				OnSendToUser( nickname, target, text );
		}

		private void protocol_OnSendToChannel(string target, string text)
		{
			if (OnSendToChannel != null) 
				OnSendToChannel( nickname, target, text );
		}

		private void protocol_OnSetMode(string target, string text)
		{
			if (OnSetMode != null) 
				OnSetMode( nickname, target, text );
		}

		private void protocol_OnSendNotice(string target, string text)
		{
			if (OnSendNotice != null) 
				OnSendNotice( nickname, target, text );
		}

		private void protocol_OnNickChange(string newname, string oldname, string hostmask)
		{
			if (OnNickChange != null) 
				OnNickChange( newname, oldname, hostmask );
		}


        private void Network_OnServerMessage(string message)
        {
            if (OnServerMessage != null)
                OnServerMessage(message);
        }


		private void protocol_OnMotd(string data)
		{
			if (OnMotd != null)
				OnMotd( data );
		}

		private void protocol_OnTopicChange(string newTopic, string channel, string changerNick, string changerHost)
		{
			GetChannel(channel).SetTopic( newTopic );
			if (OnTopicChange != null)
				OnTopicChange( newTopic, channel, changerNick, changerHost);
		}

		private void protocol_OnPublicAction(string message, string target, string senderNick, string senderHost)
		{
			if (OnPublicAction != null)
				OnPublicAction(message, target, senderNick, senderHost);
		}

		private void protocol_OnPrivateAction(string message, string target, string senderNick, string senderHost)
		{
			if (OnPrivateAction != null)
				OnPrivateAction( message, target, senderNick, senderHost );
		}
		#endregion
	}

	/// <summary>
	/// Represents a joined channnel
	/// </summary>
	public class Channel 
	{
	    readonly string name;
		string topic;
	    readonly Protocol protocol;
	    readonly UserCollection users;

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="protocol">The protocol.</param>
		/// <param name="name">The name.</param>
		public Channel(Protocol protocol, string name) 
		{
			this.name = name;
			this.protocol = protocol;
			users = new UserCollection();
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name 
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the topic.
		/// </summary>
		/// <value>The topic.</value>
		public string Topic 
		{
			get { return topic; }
		}

		/// <summary>
		/// Sets the topic.
		/// </summary>
		/// <param name="value">The value.</param>
		internal void SetTopic(string value) 
		{
			Console.WriteLine(""+name+" has new topic: "+value);
			topic = value;
		}

		/// <summary>
		/// Sets the user list.
		/// </summary>
		/// <param name="list">The list.</param>
		public void SetUserList(string[] list) 
		{
			users.SetUserCollection( list );
		}

		/// <summary>
		/// Gets the users.
		/// </summary>
		/// <value>The users.</value>
		public UserCollection Users 
		{
			get { return users; }
		}

		#region channel control
		/// <summary>
		/// Sends to channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="text">The text.</param>
		public void SendToChannel( string channel, string text ) 
		{
			protocol.SendToChannel( channel, text );
		}

		/// <summary>
		/// Sets the topic.
		/// </summary>
		/// <param name="channel">The channel.</param>
        /// <param name="newtopic">The topic.</param>
		public void SetTopic(string channel, string newtopic) 
		{
			protocol.SetTopic( channel, newtopic );
		}

		/// <summary>
		/// Sets the mode.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="mode">The mode.</param>
		public void SetMode(string channel, string mode) 
		{
			protocol.SetMode( channel, mode );;
		}

		/// <summary>
		/// Kicks the user from the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="user">The user.</param>
		public void Kick(string channel, string user ) 
		{
			protocol.Kick( channel, user );
		}
		/// <summary>
		/// Kicks the user from the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="user">The user.</param>
		/// <param name="reason">The reason.</param>
		public void Kick(string channel, string user, string reason) 
		{
			protocol.Kick( channel, user, reason );
		}
		#endregion

	}

	/// <summary>
	/// A collection of channels
	/// </summary>
	public class ChannelCollection : CollectionBase 
	{
	    readonly Protocol protocol;

		/// <summary>
		/// Occurs when the channel list changes
		/// </summary>
		public event ChannelEvent OnChannelListChange;

		/// <summary>
		/// Delegate for channel list events
		/// </summary>
		public delegate void ChannelEvent();

		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelCollection"/> class.
		/// </summary>
		/// <param name="protocol">The protocol.</param>
		public ChannelCollection(Protocol protocol) 
		{
			this.protocol = protocol;
		}

		/// <summary>
		/// Adds the specified CHN.
		/// </summary>
		/// <param name="chn">The CHN.</param>
		public void Add(Channel chn) 
		{
			if (OnChannelListChange != null) 
				OnChannelListChange();
			List.Add( chn );
		}

		/// <summary>
		/// Adds the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		public void Add(string name) 
		{
			Add( new Channel( protocol, name ) );
		}

		/// <summary>
		/// Removes the specified CHN.
		/// </summary>
		/// <param name="chn">The CHN.</param>
		public void Remove(Channel chn) 
		{
			if (OnChannelListChange != null) 
				OnChannelListChange();
			List.Remove( chn );
		}

		/// <summary>
		/// Gets the <see cref="Channel"/> with the specified i.
		/// </summary>
		/// <value></value>
		public Channel this[int i] 
		{
			get { return (Channel)List[i]; }
		}
		
		/// <summary>
		/// Determines whether the specified channel contains channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <returns>
		/// 	<c>true</c> if the specified channel contains channel; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsChannel(string channel) 
		{
			bool found = false;
			int i = 0;
			while (!found && i<Count) 
			{
				if ( string.Compare( channel, this[i].Name, true) == 0) 
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

		/// <summary>
		/// Gets the name of the channel by.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public Channel GetChannelByName(string name) 
		{
			bool found = false;
			int i = 0;
			while (!found && i<Count) 
			{
				if ( string.Compare( name, this[i].Name, true) == 0) 
				{
					found = true;
				} 
				else 
				{
					i++;
				}
			}
			if (found) return this[i];
			else return null;
		}
	}

	/// <summary>
	/// Represents a user in a joined channel
	/// </summary>
	public class User 
	{
        readonly string nickName;
        string hostmask;
	    readonly bool isOperator;
        readonly bool hasVoice;

		/// <summary>
		/// Gets the name of the nick.
		/// </summary>
		/// <value>The name of the nick.</value>
		public string NickName 
		{
			get { return nickName; }
		}

		/// <summary>
		/// Gets the hostmask.
		/// </summary>
		/// <value>The hostmask.</value>
		public string Hostmask 
		{
			get { return hostmask; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is operator.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is operator; otherwise, <c>false</c>.
		/// </value>
		public bool IsOperator 
		{
			get { return isOperator; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance has voice.
		/// </summary>
		/// <value><c>true</c> if this instance has voice; otherwise, <c>false</c>.</value>
		public bool HasVoice 
		{
			get { return hasVoice; }
		}

		/// <summary>
		/// Sets the hostmask.
		/// </summary>
		/// <param name="value">The value.</param>
		internal void SetHostmask(string value) 
		{
			hostmask = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		/// <param name="line">The line.</param>
		public User(string line)
		{
			if ( line.StartsWith("@") ) 
			{
				isOperator = true;
				line = line.Replace("@","");
			} 
			else if ( line.StartsWith("!") ) 
			{
				hasVoice = true;
				line = line.Replace("@","");
			} 
			else if (line.StartsWith(":"))
			{
				line = line.Substring(1);
			}
			nickName = line;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			if (IsOperator) return "@"+NickName;
			else if (HasVoice) return "+"+NickName;
			else return NickName;
		}

	}

	/// <summary>
	/// A collectio of users
	/// </summary>
	public class UserCollection : CollectionBase
	{
		/// <summary>
		/// Adds the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public void Add(User user) 
		{
			List.Add( user );
		}

		/// <summary>
		/// Removes the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public void Remove(User user) 
		{
			List.Remove( user );
		}

		/// <summary>
		/// Gets or sets the <see cref="User"/> with the specified i.
		/// </summary>
		/// <value></value>
		public User this[int i] 
		{
			get { return (User)List[i]; }
			set { List[i] = value; }
		}

		/// <summary>
		/// Gets the name of the user by nick.
		/// </summary>
		/// <param name="nickname">The nickname.</param>
		/// <returns></returns>
		public User GetUserByNickName( string nickname ) 
		{
			bool found = false;
			int i=0;
			while (!found && i<Count) 
			{
				if ( string.Compare( nickname, this[i].NickName, true) == 0) found = true;
				else i++;
			}
			if (found) return this[i];
			else return null;
		}

		/// <summary>
		/// Gets the users by hostmask.
		/// </summary>
		/// <param name="hostmask">The hostmask.</param>
		/// <returns></returns>
		public UserCollection GetUsersByHostmask( string hostmask ) 
		{
			UserCollection outcol = new UserCollection();
			for(int i=0; i<Count; i++)
			{
				if( wildcmp( hostmask, this[i].NickName, false) ) outcol.Add( this[i] );
			}
			return outcol;
		}

		/// <summary>
		/// Gets the operators.
		/// </summary>
		/// <returns></returns>
		public UserCollection GetOperators() 
		{
			UserCollection outcol = new UserCollection();
			for(int i=0; i<Count; i++)
			{
				if( this[i].IsOperator ) outcol.Add( this[i] );
			}
			return outcol;

		}

		/// <summary>
		/// Sets the user collection.
		/// </summary>
		/// <param name="users">The users.</param>
		public void SetUserCollection(string[] users) 
		{
			List.Clear();
			for (int i=0; i<users.Length; i++) Add( new User( users[i] ) );
		}

		//compare med wildcards, til brug i hostname matching
		private static bool wildcmp(string wild, string str, bool case_sensitive)
		{
			int cp=0, mp=0;
	
			int i=0;
			int j=0;

			if (! case_sensitive)
			{
				wild = wild.ToLower();
				str = str.ToLower();
			}
			
			while (i < str.Length && j < wild.Length && wild[j] != '*')
			{
				if ((wild[j] != str[i]) && (wild[j] != '?')) 
				{
					return false;
				}
				i++;
				j++;
			}
		
			while (i<str.Length) 
			{
				if (j<wild.Length && wild[j] == '*') 
				{
					if ((j++)>=wild.Length) 
					{
						return true;
					}
					mp = j;
					cp = i+1;
				} 
				else if (j<wild.Length && (wild[j] == str[i] || wild[j] == '?')) 
				{
					j++;
					i++;
				} 
				else 
				{
					j = mp;
					i = cp++;
				}
			}
		
			while (j < wild.Length && wild[j] == '*')
			{
				j++;
			}
			return j>=wild.Length;
		}

	}
}
