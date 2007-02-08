using System;
using NielsRask.LibIrc;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using NielsRask.FnordBot.Users;
using System.Diagnostics;

namespace NielsRask.FnordBot
{
	/// <summary>
	/// The FbordBot irc-bot. supports flood-limiting, plugins and permissions. someday maybe even users :)
	/// </summary>
	public class FnordBot 
	{
		NielsRask.LibIrc.Client irc;
		StringCollection channelsToJoin;
		NielsRask.FnordBot.Users.Module userModule;
		Random rnd;
		StringQueueHash queues;
		string installationFolderPath;
		XmlDocument xdoc = new XmlDocument();

		#region logn logic
		public delegate void LogMessageHandler(string message);
		public LogMessageHandler OnLogMessage;
		public void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) OnLogMessage( message );
		}
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="FnordBot"/> class.
		/// </summary>
		/// <param name="installationFolderPath">The installation folder path.</param>
		public FnordBot( string installationFolderPath )
		{	
			this.queues = new StringQueueHash( 10 ); // hent fra config
			this.installationFolderPath = installationFolderPath;
			if ( !installationFolderPath.EndsWith("\\") ) installationFolderPath += "\\";	
			rnd = new Random();
			channelsToJoin = new StringCollection();
			userModule = new NielsRask.FnordBot.Users.Module( installationFolderPath+"Users.xml" ); // devel-sti, den skal også kigge i ./users.xml
			irc = new Client();
			AttachEvents();
		}

		/// <summary>
		/// Inits this instance.
		/// </summary>
		public void Init() 
		{
			try 
			{
				WriteLogMessage("Fnordbot.init");
				// load the config xml
				LoadConfig();

				// read config values
				irc.Port = int.Parse( GetXPathValue(xdoc,"client/server/@port") );
				irc.Server = GetXPathValue(xdoc,"client/server/text()");
				irc.Username = GetXPathValue(xdoc,"client/username/text()");
				irc.Realname = GetXPathValue(xdoc,"client/realname/text()");
				irc.Nickname = GetXPathValue(xdoc,"client/nickname/text()");
				irc.AlternativeNick = GetXPathValue(xdoc,"client/altnick/text()");

				// read a list o channels to join at startup
				foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("client/channels/channel/name/text()")) 
				{
					channelsToJoin.Add( node.Value );
				}

				// load the specified plugins
				foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("plugins/plugin")) 
				{
					string typename = node.SelectSingleNode("@typename").Value;
					string path = node.SelectSingleNode("@path").Value;
					if (!Path.IsPathRooted( path )) 
					{
						path = Path.Combine(installationFolderPath, path);
						Console.WriteLine("Relative path combined to "+path);
					} 
					else 
					{
						Console.WriteLine("path is absolute: "+path);
					}
					LoadPlugin( typename, path, node );
				}

				// initiate a message-queue for each channel that specifies it
				foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("client/channels/channel[messagerate]") ) 
				{
					string name = node.SelectSingleNode( "name/text()" ).Value;
					int dmsg = int.Parse( node.SelectSingleNode( "messagerate/@messages" ).Value );
					int dmin = int.Parse( node.SelectSingleNode( "messagerate/@minutes" ).Value );
					queues.Add( name, new StringQueue(name, dmsg, dmin) );
				}
				WriteLogMessage("Fnordbot started");
			} 
			catch (Exception e) 
			{
				WriteLogMessage("error in fnordbot .ctor: "+e);
			}
		}

		public void Connect() 
		{
			try 
			{
				// connect to server
				irc.Connect();
				
				// join each channel specified in config
				foreach (string channel in channelsToJoin) 
				{
					Console.WriteLine("joining channel "+channel+"");
					irc.Join(channel);
				}

			} 
			catch ( Exception e ) 
			{
				WriteLogMessage("Exception in Fnordbot.Connect(): "+e);
			}
			
		}

		/// <summary>
		/// Gets the channels.
		/// </summary>
		/// <value>The channels.</value>
		public ChannelCollection Channels 
		{
			get { return irc.Channels; }
		}

		/// <summary>
		/// Joins the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void Join(string channel) 
		{
			irc.Join( channel );
		}
		/// <summary>
		/// Parts the specified channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void Part(string channel) 
		{
			irc.Part( channel );
		}
		/// <summary>
		/// Sends to user.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="text">The text.</param>
		public void SendToUser( string user, string text ) 
		{
			irc.SendToUser( user, text );
		}
		/// <summary>
		/// Sends a message to the specified channel. messages are only sent if flood-queue allows it
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		public void SendToChannel( string channel, string text ) 
		{
			SendToChannel( channel, text, false ); // HACK skal være false, evt skal metoden obsoletes?
		}
		/// <summary>
		/// Sends a message to the specified channel. allows overriding of flood-queue
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		/// <param name="overrideQueue"></param>
		public void SendToChannel( string channel, string text, bool overrideQueue ) 
		{
			try 
			{
				if (channel.Length == 0 || text.Length == 0) {}
				else if (overrideQueue && IsAllowed( GetCallingAssembly(), "CanOverrideSendToChannel" )) irc.SendToChannel( channel, text );
				else 
				{
					if ( queues[channel].CanEnqueue() ) 
					{
						queues[channel].Enqueue( new StringQueueItem(text, DateTime.Now) );
						//					queues[channel].Dequeue();
						irc.SendToChannel( channel, text );
					} 
					else 
					{
						Console.WriteLine("message to "+channel+" was blocked by floodqueue");
					}
				}
			} 
			catch (Exception e) 
			{
				string chan = channel==null?"NULL":channel;
				string txt = text==null?"NULL":text;
				WriteLogMessage("Error in SendToChannel( \""+chan+"\", \""+txt+"\", "+overrideQueue+" ): "+e);
			}
		}

		/// <summary>
		/// Sends a notice to a user or channel
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="message">The message.</param>
		public void SendNotice( string target, string message ) 
		{
			irc.SendNotice( target, message );
		}

		/// <summary>
		/// Gets the nickname of the bot
		/// </summary>
		public string NickName 
		{
			get { return irc.Nickname; }
		}

		#region permission logic
		/// <summary>
		/// Checks if a permission is set for a plugin
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="permission"></param>
		/// <returns></returns>
		private bool IsAllowed( Assembly asm, string permission ) 
		{
			string xpath = "";
			string typename = "(unset)";
			if (asm == null) 
			{
				WriteLogMessage("IsAllowed called on NULL assembly");
				return false;
			}
			try 
			{
				typename = GetPluginNamespace( asm ); 
//				xpath = "plugins/plugin[@path='"+asm.Location.ToLower()+"']/permissions/permission[@name='"+permission+"']/@value";
				xpath = "plugins/plugin[@typename='"+typename+"']/permissions/permission[@name='"+permission+"']/@value";
				XmlNode node = xdoc.DocumentElement.SelectSingleNode( xpath );
				return bool.Parse( node.Value );
			} 
			catch (Exception e)
			{
				WriteLogMessage("error in Fnordbot.IsAllowed (xpath '"+xpath+"'): "+e);
			}
			return false;
		}

		private Assembly GetCallingAssembly() 
		{
			try 
			{
				StackTrace st = new StackTrace( true );
				int i=0;
				bool found = false;
				while ( !found && i<st.FrameCount) 
				{
					if (st.GetFrame(i).GetMethod().DeclaringType.Assembly.Location != Assembly.GetExecutingAssembly().Location) 
					{
						found = true;
					}
					else i++;
				}

				if (found) 
				{
					StackFrame sf = st.GetFrame( i );
					Assembly asm = sf.GetMethod().DeclaringType.Assembly;
					return asm;
				} 
				else 
				{
					WriteLogMessage("Cannot locate calling assembly?");
					return null;
				}
			} 
			catch (Exception e) 
			{
				WriteLogMessage("Error in GetcallingAssembly(): "+e);
			}
			return null;
		}
		#endregion

		#region helper methods
		/// <summary>
		/// Returns true a given percentage of calls.
		/// </summary>
		/// <remarks>For use in plugins</remarks>
		/// <param name="percent"></param>
		/// <returns></returns>
		public bool TakeChance(int percent) 
		{
			return ( (rnd.Next(100)+1) <= percent );	
		}

		#endregion

		#region config loading
		private void LoadConfig() 
		{
			string cfgpath = "";
			if (File.Exists(installationFolderPath+"Config.xml") )
				cfgpath = installationFolderPath+"Config.xml";				
			else if (File.Exists("Config.xml")) 
				cfgpath = "Config.xml";
			else if (File.Exists("..\\Config.xml")) 
				cfgpath = "..\\Config.xml";
			else if (File.Exists("..\\..\\Config.xml")) 
				cfgpath = "..\\..\\Config.xml";
			else throw new FileNotFoundException("Config file not found");
			Console.WriteLine("config found at "+cfgpath);

			xdoc.Load( cfgpath );
		}

		private string GetXPathValue(XmlDocument xdoc, string xpath) 
		{
			XmlNode node = xdoc.DocumentElement.SelectSingleNode( xpath );
			if ( node != null) 
			{
				return node.Value;
			}
			else 
			{
				return "";
			}
		}

		private void LoadPlugin( string type, string path, XmlNode pluginNode ) 
		{
			Assembly pAsm = Assembly.LoadFrom( path );
			Console.WriteLine("Loading plugin "+pAsm.CodeBase);
			IPlugin plugin = (IPlugin)pAsm.CreateInstance( type );
			plugin.Init( pluginNode );
			plugin.Attach( this );
			Console.WriteLine("Attached plugin "+type);
		}

//		public void PluginTest()
//		{
//			Assembly asm = Assembly.LoadFrom( @"c:\program files\nielsrask\fnordbot\plugins\wordgame\wordgame.dll" );
//			Console.WriteLine("CodeBase: "+asm.CodeBase);
//			Console.WriteLine("EscapedCodeBase: "+asm.EscapedCodeBase);
//			Console.WriteLine("FullName: "+asm.FullName);
//			Console.WriteLine("Location: "+asm.Location);
//			Type[] types = asm.GetTypes();
//			foreach (Type t in types) 
//			{
//				Console.WriteLine("");
//				Console.WriteLine("assembly defines type: "+t.Name);
//				Console.WriteLine("namespace: "+t.Namespace);
//				Console.WriteLine("basetype: "+t.BaseType.Name );
//				foreach (Type i in t.GetInterfaces())
//					Console.WriteLine("-interface: "+i.Name );
//			}
//			Console.WriteLine("plugin-search yielded: "+GetPluginNamespace( asm ) );
//			
//		}

		/// <summary>
		/// Returns he Fullname of the first class in the assembly that implements IPlugin
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		private string GetPluginNamespace(Assembly asm) 
		{
			int i=0;
			bool found = false;
			if (asm == null) 
			{
				WriteLogMessage("GetPluginNamespace() called with NULL argument");
				return "ERROR";
			}
			try 
			{
				Type[] types = asm.GetTypes();

				while( !found && i<types.Length ) 
				{
					Type[] interfaces = types[i].GetInterfaces();
					int j=0;
					while (!found && j<interfaces.Length) 
					{
						if (interfaces[j].Name.EndsWith("IPlugin") ) 
							found = true;
						else
							j++;
					}
					if (!found) i++;
				}
				if (found) return types[i].FullName;
				else return "UNKNOWN";
			} 
			catch (Exception e) 
			{
				WriteLogMessage("Error in getPluginNamespace( "+asm.FullName+" ). found="+found+": "+e);
			}
			return "ERROR";
		}
		#endregion

		#region methods for test
//		private void DumpCallStack() 
//		{
//			StackTrace st = new StackTrace( true );
//			for(int i=0; i<st.FrameCount; i++) 
//			{
//				StackFrame sf = st.GetFrame(i);
//				Console.WriteLine("frame "+i+": method "+sf.GetMethod().Name+" in "+sf.GetMethod().DeclaringType.Assembly.Location);
//			}
//		}

		private void Channels_OnChannelListChange()
		{
			foreach (Channel chn in irc.Channels) 
			{
				Console.WriteLine(""+chn.Name+" ("+chn.Topic+")");
				foreach (NielsRask.FnordBot.Users.User usr in chn.Users) 
				{
					Console.WriteLine("  "+usr.ToString());
				}
			}
		}

		private void Client_OnServerMessage(string message)
		{
//			Console.WriteLine("-> "+message);
		}
		#endregion

		#region event forwarding

		private void AttachEvents() 
		{
			irc.OnLogMessage += new Client.LogMessageHandler(WriteLogMessage);
			irc.Channels.OnChannelListChange += new ChannelCollection.ChannelEvent(Channels_OnChannelListChange);
			irc.Protocol.Network.OnServerMessage += new NielsRask.LibIrc.Network.ServerMessageHandler(Client_OnServerMessage);
			irc.Protocol.Network.OnDisconnect += new NielsRask.LibIrc.Network.ServerStateHandler(Network_OnDisconnect);
			irc.OnPublicMessage += new NielsRask.LibIrc.Protocol.MessageHandler(irc_OnPublicMessage);
			irc.OnPrivateMessage += new NielsRask.LibIrc.Protocol.MessageHandler(irc_OnPrivateMessage);
			irc.OnPrivateNotice += new NielsRask.LibIrc.Protocol.MessageHandler(irc_OnPrivateNotice);
			irc.OnPublicNotice += new NielsRask.LibIrc.Protocol.MessageHandler(irc_OnPublicNotice);
			irc.OnSendNotice +=new NielsRask.LibIrc.Client.BotMessageHandler(irc_OnSendNotice);
			irc.OnSendToChannel += new NielsRask.LibIrc.Client.BotMessageHandler(irc_OnSendToChannel);
			irc.OnSendToUser += new NielsRask.LibIrc.Client.BotMessageHandler(irc_OnSendToUser);
			irc.OnSetMode += new NielsRask.LibIrc.Client.BotMessageHandler(irc_OnSetMode);
			irc.OnNickChange += new NielsRask.LibIrc.Client.NickChangeHandler(irc_OnNickChange);
			irc.Protocol.OnChannelJoin += new NielsRask.LibIrc.Protocol.ChannelActionHandler(Protocol_OnChannelJoin);
			irc.Protocol.OnChannelKick += new NielsRask.LibIrc.Protocol.ChannelActionHandler(Protocol_OnChannelKick);
			irc.Protocol.OnChannelMode += new NielsRask.LibIrc.Protocol.ChannelActionHandler(Protocol_OnChannelMode);
			irc.Protocol.OnChannelPart += new NielsRask.LibIrc.Protocol.ChannelActionHandler(Protocol_OnChannelPart);
			irc.Protocol.OnTopicChange += new NielsRask.LibIrc.Protocol.ChannelTopicHandler(Protocol_OnTopicChange);
			irc.Protocol.OnPrivateMessage += new NielsRask.LibIrc.Protocol.MessageHandler(Protocol_OnPrivateMessage);
		}


		private NielsRask.FnordBot.Users.User GetUser( string nickName, string hostMask ) 
		{
			NielsRask.FnordBot.Users.User user = userModule.Users.GetByHostMatch( hostMask );
			if (user == null) // no user was found, make a pseudouser
			{
				user = new NielsRask.FnordBot.Users.User( nickName ); // this ctor wont make citizens
				user.Hostmasks.Add( new Hostmask( hostMask ) );
				// TODO: save evt den nye user - evt tilføj learning mode
			}
			return user;
		}

		public event MessageHandler OnPublicMessage;
		public event MessageHandler OnPrivateMessage;
		public event MessageHandler OnPublicNotice;
		public event MessageHandler OnPrivateNotice;
		public event ChannelTopicHandler OnTopicChange;
		public event ChannelActionHandler OnChannelJoin;
		public event ChannelActionHandler OnChannelPart;
		public event ChannelActionHandler OnChannelMode;
		public event ChannelActionHandler OnChannelKick;
		public event NickChangeHandler OnNickChange;
		public event BotMessageHandler OnSendToChannel;
		public event BotMessageHandler OnSendToUser;
		public event BotMessageHandler OnSendNotice;
		public event BotMessageHandler OnSetMode;

		public delegate void MessageHandler(NielsRask.FnordBot.Users.User user, string channel, string message);
		public delegate void ChannelTopicHandler(NielsRask.FnordBot.Users.User user, string channel, string topic);
		public delegate void ChannelUserListHandler(string channel, string[] list);
		public delegate void ChannelActionHandler(string text, string channel, string target, string senderNick, string senderHost);
		public delegate void NickChangeHandler(  string newname, string oldname, NielsRask.FnordBot.Users.User user );
		public delegate void BotMessageHandler( string botName, string target, string text );

		private void irc_OnPublicMessage(string message, string target, string senderNick, string senderHost)
		{
			if( OnPublicMessage != null ) 
				OnPublicMessage( GetUser(senderNick, senderHost), target, message );
		}
		private void irc_OnPrivateMessage(string message, string target, string senderNick, string senderHost)
		{
			if( OnPrivateMessage != null ) 
				OnPrivateMessage( GetUser(senderNick, senderHost), target, message );
		}
		private void irc_OnPublicNotice(string message, string target, string senderNick, string senderHost)
		{
			if( OnPublicNotice != null ) 
				OnPublicNotice( GetUser(senderNick, senderHost), target, message );
		}
		private void irc_OnPrivateNotice(string message, string target, string senderNick, string senderHost)
		{
			if( OnPrivateNotice != null ) 
				OnPrivateNotice( GetUser(senderNick, senderHost), target, message );
		}
		private void Protocol_OnChannelJoin(string text, string channel, string target, string senderNick, string senderHost)
		{
			if( OnChannelJoin != null ) 
				OnChannelJoin( text, channel, target, senderNick, senderHost );
		}
		private void Protocol_OnChannelKick(string text, string channel, string target, string senderNick, string senderHost)
		{
			if( OnChannelKick != null ) 
				OnChannelKick( text, channel, target, senderNick, senderHost );
		}
		private void Protocol_OnChannelMode(string text, string channel, string target, string senderNick, string senderHost)
		{
			if( OnChannelMode != null ) 
				OnChannelMode( text, channel, target, senderNick, senderHost );
		}
		private void Protocol_OnChannelPart(string text, string channel, string target, string senderNick, string senderHost)
		{
			if( OnChannelPart != null ) 
				OnChannelPart( text, channel, target, senderNick, senderHost );
		}
		private void Protocol_OnTopicChange(string newTopic, string channel, string changerNick, string changerHost)
		{
			if( OnTopicChange != null ) 
				OnTopicChange( GetUser(changerNick, changerHost), channel, newTopic );
		}

		private void irc_OnSendNotice(string botName, string target, string text)
		{
			if( OnSendNotice != null ) 
				OnSendNotice( botName, target, text );
		}

		private void irc_OnSendToChannel(string botName, string target, string text)
		{
			if( OnSendToChannel != null ) 
				OnSendToChannel( botName, target, text );
		}

		private void irc_OnSendToUser(string botName, string target, string text)
		{
			if( OnSendToUser != null ) 
				OnSendToUser( botName, target, text );
		}

		private void irc_OnSetMode(string botName, string target, string text)
		{
			if( OnSetMode != null ) 
				OnSetMode( botName, target, text );
		}

		private void irc_OnNickChange(string newname, string oldname, string hostmask)
		{
			if( OnNickChange != null ) 
				OnNickChange( newname, oldname, GetUser(newname, hostmask) );
		}		
		private void Network_OnDisconnect()
		{
			WriteLogMessage("Ooops, seems we lost our connection!");
		}
		private void Protocol_OnPrivateMessage(string message, string target, string senderNick, string senderHost)
		{
			if (message == "!whoami") 
			{
				NielsRask.FnordBot.Users.User user = userModule.Users.GetByHostMatch( senderHost );
				if (user != null)
					SendToUser( senderNick, "you appear to be "+user.Name+" (citizen: "+user.IsCitizen+")");
				else SendToUser( senderNick, "i dont know you.." );
			} 
			else if (message == "!ping") 
			{
				SendToUser(senderNick, "pong");
			}
		}		
		#endregion

	}

	public interface IPlugin 
	{
		/// <summary>
		/// For initalizing the plugin
		/// </summary>
		void Init( XmlNode pluginNode );

		/// <summary>
		/// Attach subscribers to the forwarders events
		/// </summary>
		/// <param name="bot">the event forwarder object</param>
		void Attach( FnordBot bot );
	}

	// en hashtable med [channelname, stringqueue]
	public class StringQueueHash : Hashtable 
	{
		int queueLength; // kun en default
		public StringQueueHash(int queueLength): base() 
		{
			this.queueLength = queueLength;
		}
		public void Add(string key, StringQueue value)
		{
			base.Add (key, value);
		}
		public bool ContainsKey(string key)
		{
			return base.ContainsKey (key);
		}

		public StringQueue this[string key]
		{
			get
			{
				return (StringQueue)base[key.ToLower()];
			}
			set
			{
				base[key.ToLower()] = value;
			}
		}

	}
	// a queue for flood-limiting
	public class StringQueue : Queue
	{
		string name;
		int dmsg;
		int dmin;

		public StringQueue( string name, int dmsg, int dmin ) : base( dmsg )
		{
			this.name = name;
			this.dmsg = dmsg;
			this.dmin = dmin;
		}

		public new StringQueueItem Dequeue()
		{
			return (StringQueueItem)base.Dequeue ();
		}

		public void Enqueue(StringQueueItem item)
		{
			base.Enqueue (item);
			if (Count > dmsg) Dequeue();
		}

		public new StringQueueItem Peek()
		{
			return (StringQueueItem)base.Peek ();
		}

		public bool CanEnqueue() 
		{
			if (Count < dmsg ) return true; // vi har ikke nået max endnu
			else 
			{
				TimeSpan dt = DateTime.Now-Peek().TimeStamp; // dT siden første item
				if (dt.TotalMinutes >= dmin) return true; // 
				else return false;
			}

		}
	}

	// an item in  the stringqueue
	public class StringQueueItem 
	{
		string text;
		DateTime timeStamp;

		public StringQueueItem(string text, DateTime timeStamp) 
		{
			this.text = text;
			this.timeStamp = timeStamp;
		}

		public string Text 
		{
			get { return text; }
			set { text = value; }
		}
		public DateTime TimeStamp 
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}
	}
}
