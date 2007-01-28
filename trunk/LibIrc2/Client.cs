using System;
using System.Collections;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// This class provides high-level irc functions
	/// </summary>
	public class Client
	{
		Protocol protocol;
		ChannelCollection channels;
		string server = "irc.droso.net";
		int port = 6667;
		string nickname = "Bimsebot";
		string username = "bimse";
		string realname = "B. Imse";
		string altNick = "";
		string fingerInfo = "fnord";
		string versionInfo = "LibIrc "+System.Reflection.Assembly.GetCallingAssembly().GetName().Version;

		public string Server 
		{
			get { return server; }
			set { server = value; }
		}

		public int Port 
		{
			get { return port; }
			set { port = value; }
		}

		public string Nickname 
		{
			get { return nickname; }
			set { nickname = value; }
		}

		public string Username 
		{
			get { return username; }
			set { username = value; }
		}

		public string Realname 
		{
			get { return realname; }
			set { realname = value; }
		}
		public string AlternativeNick 
		{
			get { return altNick; }
			set { altNick = value; }
		}

		public string FingerInfo 
		{
			get { return fingerInfo; }
			set { fingerInfo = value; }
		}

		public string VersionInfo 
		{
			get { return versionInfo; }
			set { versionInfo = value; }
		}

		public ChannelCollection Channels 
		{
			get { return channels; }
		}

		public Protocol Protocol 
		{
			get { return protocol; }
		}

		public event MessageHandler OnPublicMessage;
		public event MessageHandler OnPrivateMessage;
		public event MessageHandler OnPublicNotice;
		public event MessageHandler OnPrivateNotice;
		public event NickChangeHandler OnNickChange;
		//bot-actions
		public event BotMessageHandler OnSendToChannel;
		public event BotMessageHandler OnSendToUser;
		public event BotMessageHandler OnSendNotice;
		public event BotMessageHandler OnSetMode;

		public delegate void MessageHandler(Object bot, MessageEventArgs mea);
		public delegate void NickChangeHandler(string newname, string oldname, string hostmask);
		public delegate void BotMessageHandler( string botName, string target, string text ); // bottens eget navn kendes ikke her, det sættes på i client
		public delegate void LogMessageHandler(string message);
		public event LogMessageHandler OnLogMessage;

		private void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) OnLogMessage( message );
		}



		public Client()
		{
			protocol = new Protocol();
			protocol.AlternativeNick = altNick;
			channels = new ChannelCollection( protocol );
			protocol.OnChannelUserList += new Protocol.ChannelUserListHandler(OnChannelUserList);
			protocol.OnTopicChange += new Protocol.ChannelTopicHandler(OnTopicChange);
			protocol.OnPrivateMessage += new NielsRask.LibIrc.Protocol.MessageHandler(protocol_OnPrivateMessage);
			protocol.OnPublicMessage += new NielsRask.LibIrc.Protocol.MessageHandler(protocol_OnPublicMessage);
			protocol.OnPublicNotice += new NielsRask.LibIrc.Protocol.MessageHandler(protocol_OnPublicNotice);
			protocol.OnPrivateNotice += new NielsRask.LibIrc.Protocol.MessageHandler(protocol_OnPrivateNotice);

			protocol.OnSendToUser += new NielsRask.LibIrc.Protocol.BotMessageHandler(protocol_OnSendToUser);
			protocol.OnSendToChannel += new NielsRask.LibIrc.Protocol.BotMessageHandler(protocol_OnSendToChannel);
			protocol.OnSetMode += new NielsRask.LibIrc.Protocol.BotMessageHandler(protocol_OnSetMode);
			protocol.OnSendNotice += new NielsRask.LibIrc.Protocol.BotMessageHandler(protocol_OnSendNotice);
			protocol.OnNickChange += new NielsRask.LibIrc.Protocol.NickChangeHandler(protocol_OnNickChange);

			protocol.OnLogMessage += new Protocol.LogMessageHandler( WriteLogMessage );
		}

		#region control

		public void Connect() 
		{
			protocol.FingerInfo = fingerInfo;
			protocol.VersionInfo = versionInfo;
			protocol.Connect( server, port );
			protocol.Register( nickname, username, realname );
		}

		public void Join(string channel) 
		{
			protocol.Join( channel );
		}
		public void Part(string channel) 
		{
			protocol.Part( channel );
		}
		public void SendToUser( string user, string text ) 
		{
			protocol.SendToUser( user, text );
		}
		public void SendToChannel( string channel, string text ) 
		{
			protocol.SendToChannel( channel, text );
		}
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

		private void OnTopicChange(Object bot, MessageEventArgs mea)
		{
			GetChannel(mea.Channel).SetTopic( mea.Message );
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

		private void protocol_OnPrivateMessage(Object bot, MessageEventArgs mea)
		{
			if (OnPrivateMessage != null) OnPrivateMessage( bot, mea );
		}

		private void protocol_OnPublicMessage(Object bot, MessageEventArgs mea)
		{
			if (OnPublicMessage != null) OnPublicMessage( bot, mea );
		}

		private void protocol_OnPublicNotice(Object bot, MessageEventArgs mea)
		{
			if (OnPublicNotice != null) OnPublicNotice( bot, mea );
		}

		private void protocol_OnPrivateNotice(Object bot, MessageEventArgs mea)
		{
			if (OnPrivateNotice != null) OnPrivateNotice( bot, mea );
		}
		#endregion



		private void protocol_OnSendToUser(string target, string text)
		{
			if (OnSendToUser != null) OnSendToUser( nickname, target, text );
		}

		private void protocol_OnSendToChannel(string target, string text)
		{
			if (OnSendToChannel != null) OnSendToChannel( nickname, target, text );
		}

		private void protocol_OnSetMode(string target, string text)
		{
			if (OnSetMode != null) OnSetMode( nickname, target, text );
		}

		private void protocol_OnSendNotice(string target, string text)
		{
			if (OnSendNotice != null) OnSendNotice( nickname, target, text );
		}

		private void protocol_OnNickChange(string newname, string oldname, string hostmask)
		{
			if (OnNickChange != null) OnNickChange( newname, oldname, hostmask );
		}
	}

	public class Channel 
	{
		string name;
		string topic;
		Protocol protocol;
		UserCollection users;

		public Channel(Protocol protocol, string name) 
		{
			this.name = name;
			this.protocol = protocol;
			users = new UserCollection();
		}
		public string Name 
		{
			get { return name; }
		}

		public string Topic 
		{
			get { return topic; }
		}

		internal void SetTopic(string value) 
		{
			Console.WriteLine(""+name+" has new topic: "+value);
			topic = value;
		}

		public void SetUserList(string[] list) 
		{
			users.SetUserCollection( list );
		}

		public UserCollection Users 
		{
			get { return users; }
		}

		#region channel control
		public void SendToChannel( string channel, string text ) 
		{
			protocol.SendToChannel( channel, text );
		}

		public void SetTopic(string channel, string topic) 
		{
			protocol.SetTopic( channel, topic );
		}

		public void SetMode(string channel, string mode) 
		{
			protocol.SetMode( channel, mode );;
		}

		public void Kick(string channel, string user ) 
		{
			protocol.Kick( channel, user );
		}
		public void Kick(string channel, string user, string reason) 
		{
			protocol.Kick( channel, user, reason );
		}
		#endregion

	}

	public class ChannelCollection : CollectionBase 
	{
		Protocol protocol;

		public event ChannelEvent OnChannelListChange;
		public delegate void ChannelEvent();

		public ChannelCollection(Protocol protocol) 
		{
			this.protocol = protocol;
		}

		public void Add(Channel chn) 
		{
			if (OnChannelListChange != null) OnChannelListChange();
			List.Add( chn );
		}

		public void Add(string name) 
		{
			Add( new Channel( protocol, name ) );
		}

		public void Remove(Channel chn) 
		{
			if (OnChannelListChange != null) OnChannelListChange();
			List.Remove( chn );
		}

		public Channel this[int i] 
		{
			get { return (Channel)List[i]; }
		}
		
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

	public class User 
	{
		string nickName;
		string hostmask;
		bool isOperator;
		bool hasVoice;

		public string NickName 
		{
			get { return nickName; }
		}
		public string Hostmask 
		{
			get { return hostmask; }
		}
		public bool IsOperator 
		{
			get { return isOperator; }
		}
		public bool HasVoice 
		{
			get { return hasVoice; }
		}

		internal void SetHostmask(string value) 
		{
			hostmask = value;
		}

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

		public override string ToString()
		{
			if (IsOperator) return "@"+NickName;
			else if (HasVoice) return "+"+NickName;
			else return NickName;
		}

	}

	public class UserCollection : CollectionBase
	{
		public void Add(User user) 
		{
			List.Add( user );
		}

		public void Remove(User user) 
		{
			List.Remove( user );
		}

		public User this[int i] 
		{
			get { return (User)List[i]; }
			set { List[i] = value; }
		}

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

		public UserCollection GetUsersByHostmask( string hostmask ) 
		{
			UserCollection outcol = new UserCollection();
			for(int i=0; i<Count; i++)
			{
				if( wildcmp( hostmask, this[i].NickName, false) ) outcol.Add( this[i] );
			}
			return outcol;
		}

		public UserCollection GetOperators() 
		{
			UserCollection outcol = new UserCollection();
			for(int i=0; i<Count; i++)
			{
				if( this[i].IsOperator ) outcol.Add( this[i] );
			}
			return outcol;

		}

		public void SetUserCollection(string[] users) 
		{
			List.Clear();
			for (int i=0; i<users.Length; i++) Add( new User( users[i] ) );
		}

		private bool wildcmp(string wild, string str, bool case_sensitive)
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
		
			//			while (j
			while (j < wild.Length && wild[j] == '*')
			{
				j++;
			}
			return j>=wild.Length;
		}

	}
}
