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
		// the client layer
		NielsRask.LibIrc.Client irc;
		// channels to join at startup
		StringCollection channelsToJoin;
//		NielsRask.FnordBot.Users.Module userModule;
		UserCollection users;
		Random rnd;
		StringQueueHash queues;
		string installationFolderPath;
		XmlDocument xdoc = new XmlDocument();

		/// <summary>
		/// Gets the installationfolder path.
		/// </summary>
		/// <value>The installation folder path.</value>
		public string InstallationFolderPath 
		{
			get 
			{
				return installationFolderPath;
			}
		}

		#region log logic
		/// <summary>
		/// Delegate for logging
		/// </summary>
		public delegate void LogMessageHandler(string message);

		/// <summary>
		/// Occurs when the bot wants to log a message
		/// </summary>
		public event LogMessageHandler OnLogMessage;

		/// <summary>
		/// Write a message to the log
		/// </summary>
		/// <param name="message"></param>
		public void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) 
				OnLogMessage( message );
		}
		#endregion


		public FnordBot() 
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FnordBot"/> class.
		/// </summary>
		/// <param name="installationFolderPath">The installation folder path.</param>
		public FnordBot( string installationFolderPath )
		{	
			this.queues = new StringQueueHash(); // hent fra config
			this.installationFolderPath = installationFolderPath;
			// fix the install-path
			if ( !installationFolderPath.EndsWith("\\") ) 
				installationFolderPath += "\\";	
			// init the random number god
			rnd = new Random();
			channelsToJoin = new StringCollection();
			// initialize the user logic
//			userModule = new NielsRask.FnordBot.Users.Module( installationFolderPath+"Users.xml" ); // devel-sti, den skal også kigge i ./users.xml

			// load the userlist
			usersFilePath = installationFolderPath+"Users.xml";
			users = LoadUsers();
			
			// initialize the client layer - maybe we should use the protocol layer directly?
			irc = new Client();
			// attach to the events that the client layer can throw
			AttachEvents();
		}
		string usersFilePath = "";

		private UserCollection LoadUsers() 
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load( usersFilePath );

			XmlNodeList usrlst = xdoc.DocumentElement.SelectNodes("user");
			Console.WriteLine("found "+usrlst.Count+" usernodes");
			
			return UserCollection.UnpackUsers( usrlst, new UserCollection.SaveUsersDelegate( SaveUsers ) );		
		}

		
		private void SaveUsers() 
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.LoadXml( users.ToXmlString() );
			xdoc.Save( usersFilePath );
		}

		/// <summary>
		/// Inits this instance.
		/// </summary>
		/// <remarks>Configures the client layer, loads config and plugins</remarks>
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

				// process the list of channels to load at startup
				foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("client/channels/channel[name/text()]")) 
				{
					string name = node.SelectSingleNode("name/text()").Value;
					channelsToJoin.Add( name );
					int dMsg = 5;	// defasult values
					int dMin = 60;	// max 5 msg/hr
					if (node.SelectSingleNode("messagerate") != null) 
					{
						dMsg = int.Parse( node.SelectSingleNode("messagerate/@messages").Value );
						dMin = int.Parse( node.SelectSingleNode("messagerate/@minutes").Value );
					}
					queues.Add(name, dMsg, dMin); // initiate the antiflood queue

				}

				// load the specified plugins
				foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("plugins/plugin")) 
				{
					string typename = node.SelectSingleNode("@typename").Value;
					string path = node.SelectSingleNode("@path").Value;
					if (!Path.IsPathRooted( path )) 
					{
						path = Path.Combine(installationFolderPath, path);
//						Console.WriteLine("Relative path combined to "+path);
//					} 
//					else 
//					{
//						Console.WriteLine("path is absolute: "+path);
					}
					LoadPlugin( typename, path, node );
				}

				WriteLogMessage("Fnordbot started");
			} 
			catch (Exception e) 
			{
				WriteLogMessage("error in fnordbot .ctor: "+e);
			}
		}

		/// <summary>
		/// Connects to the server, joins requested channels
		/// </summary>
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
				// TODO: need some reconnect logic
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
		/// Sends a message to the specified channel. overrides the queue if allowed
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		public void SendToChannel( string channel, string text ) 
		{
			SendToChannel( channel, text, true ); 
		}
		/// <summary>
		/// Sends a message to the specified channel. allows overriding of flood-queue
		/// </summary>
		/// <param name="channel">thannel to send to</param>
		/// <param name="text">text to send</param>
		/// <param name="overrideQueue">Override the anti-flood queue, for priority messages. only possible if the config allows it</param>
		public void SendToChannel( string channel, string text, bool overrideQueue ) 
		{
			try 
			{
				// channel or message is empty - call will be ignored
				if (channel.Length == 0 || text.Length == 0) 
				{
					// someone made an error, dont send. maybe we should throw something :)
				}
				// override requested and is allowed - send to channel
				else if (overrideQueue && IsAllowed( GetCallingAssembly(), "CanOverrideSendToChannel" )) 
				{
					irc.SendToChannel( channel, text );
				}
				// override not requested or not allowed - send if queues allow it
				else 
				{
					// if we're allowed to send
					if ( queues[channel].CanEnqueue() ) 
					{
						queues[channel].Enqueue( text ); // auto-dequeues if too long
						irc.SendToChannel( channel, text );
					} 
					// we're not alowed to send - maybe we should signal that somehow
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
			// maybe we should include some anti-flood logic here ..
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
				if (node != null) 
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

		#region event forwarding

		private void AttachEvents() 
		{
			irc.OnLogMessage += new Client.LogMessageHandler(WriteLogMessage);
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


		private NielsRask.FnordBot.User GetUser( string nickName, string hostMask ) 
		{
			User user = users.GetByHostMatch( hostMask );
			if (user == null) // no user was found, make a pseudouser
			{
				user = new User( nickName, new UserCollection.SaveUsersDelegate( SaveUsers ) ); // this ctor wont make citizens
				user.Hostmasks.Add( new Hostmask( hostMask ) );
				bool learningMode = false;
				if (learningMode)// registrer alle users vi ser
				{
					// TODO: save evt den nye user - evt tilføj learning mode
					user.MakeCitizen();
					users.Add( user );
					users.Save();
				}
			}
			return user;
		}

		/// <summary>
		/// Occurs when a public message is received
		/// </summary>
		public event MessageHandler OnPublicMessage;
		/// <summary>
		/// Occurs when a private message is received
		/// </summary>
		public event MessageHandler OnPrivateMessage;
		/// <summary>
		/// Occurs when a private notice is received
		/// </summary>
		public event MessageHandler OnPublicNotice;
		/// <summary>
		/// Occurs when a private notice is received
		/// </summary>
		public event MessageHandler OnPrivateNotice;
		/// <summary>
		/// Occurs when a channel changes topic
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
		/// Occurs when someone changes the mode of a channel
		/// </summary>
		public event ChannelActionHandler OnChannelMode;
		/// <summary>
		/// Occurs when someone is kicked from a channel
		/// </summary>
		public event ChannelActionHandler OnChannelKick;
		/// <summary>
		/// Occurs when someone changes their nickname
		/// </summary>
		public event NickChangeHandler OnNickChange;
		/// <summary>
		/// Occurs when the bot talks to a channel
		/// </summary>
		public event BotMessageHandler OnSendToChannel;
		/// <summary>
		/// Occurs when the bot talks to a user
		/// </summary>
		public event BotMessageHandler OnSendToUser;
		/// <summary>
		/// Occurs when the bot sends a notice
		/// </summary>
		public event BotMessageHandler OnSendNotice;
		/// <summary>
		/// Occurs when the bot changes channel mode
		/// </summary>
		public event BotMessageHandler OnSetMode;

		/// <summary>
		/// Delegate or received messages
		/// </summary>
		public delegate void MessageHandler(NielsRask.FnordBot.User user, string channel, string message);
		/// <summary>
		/// Delegate for topic changes
		/// </summary>
		public delegate void ChannelTopicHandler(NielsRask.FnordBot.User user, string channel, string topic);
		/// <summary>
		/// Delegate for userlist events
		/// </summary>
		public delegate void ChannelUserListHandler(string channel, string[] list);
		/// <summary>
		/// delegate for channel actions
		/// </summary>
		public delegate void ChannelActionHandler(string text, string channel, string target, string senderNick, string senderHost);
		/// <summary>
		/// Delegate for nickname changes
		/// </summary>
		public delegate void NickChangeHandler(  string newname, string oldname, NielsRask.FnordBot.User user );
		/// <summary>
		/// Delegate for Messages from the bot itself
		/// </summary>
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
				User user = users.GetByHostMatch( senderHost );
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

	/// <summary>
	/// The plugin interface that all plugins must implement
	/// </summary>
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
	/// <summary>
	/// This holds a collection of stringqueues
	/// </summary>
	public class StringQueueHash : Hashtable 
	{
		/// <summary>
		/// Adds the specified queue.
		/// </summary>
		/// <param name="queueName">Name of the queue.</param>
		/// <param name="value">The value.</param>
		public void Add(string queueName, StringQueue value)
		{
			base.Add (queueName, value);
		}

		/// <summary>
		/// Adds the specified queue.
		/// </summary>
		/// <param name="queueName">Name of the queue.</param>
		/// <param name="deltaMsg">The delta MSG.</param>
		/// <param name="deltaMin">The delta min.</param>
		public void Add(string queueName, int deltaMsg, int deltaMin) 
		{
			base.Add(queueName, new StringQueue(deltaMsg, deltaMin));
		}

		/// <summary>
		/// Determines whether the list contains the specifiec queue.
		/// </summary>
		/// <param name="queueName">Name of the queue.</param>
		/// <returns>
		/// 	<c>true</c> if the specified queue name contains key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey(string queueName)
		{
			return base.ContainsKey (queueName);
		}

		/// <summary>
		/// Gets or sets the <see cref="StringQueue"/> with the specified queuename.
		/// </summary>
		/// <value></value>
		public StringQueue this[string queueName]
		{
			get
			{
				return (StringQueue)base[queueName.ToLower()];
			}
		}

	}
	/// <summary>
	/// This queue is for enforcing a specified messagerate. 
	/// </summary>
	/// <remarks>
	/// For a requested rate of 5msg/hr, the queue will store 5 messages and only allow enqueueing of 
	/// new messages if the oldest is at least one hour old. the oldest will then be dequeued.
	/// </remarks>
	public class StringQueue : Queue
	{
		string name;
		int dmsg;
		int dmin;

		/// <summary>
		/// Initializes a new instance of the <see cref="StringQueue"/> class.
		/// </summary>
		/// <param name="dmsg">delta-msg. upper part of the messages/time rate</param>
		/// <param name="dmin">delta-min. bottom part of he messages/time rate. specifie in minutes</param>
		public StringQueue(int dmsg, int dmin ) : base( dmsg )
		{
			this.name = name;
			this.dmsg = dmsg;
			this.dmin = dmin;
		}

		/// <summary>
		/// Dequeues the item at the front of the queue
		/// </summary>
		/// <returns></returns>
		public new StringQueueItem Dequeue()
		{
			return (StringQueueItem)base.Dequeue ();
		}

		/// <summary>
		/// Enqueues the specified item. Dequeues if the requested length is exceeded
		/// </summary>
		/// <param name="item">The item.</param>
		public void Enqueue(StringQueueItem item)
		{
			base.Enqueue (item);
			if (Count > dmsg) Dequeue();
		}

		/// <summary>
		/// Enqueues the specified item. Dequeues if the requested length is exceeded
		/// </summary>
		/// <param name="message">The message to enqueue.</param>
		public void Enqueue(string message) 
		{
			Enqueue( new StringQueueItem(message, DateTime.Now) );
		}

		/// <summary>
		/// Returns the item at the front of the queue without removing it
		/// </summary>
		/// <returns></returns>
		public new StringQueueItem Peek()
		{
			return (StringQueueItem)base.Peek ();
		}

		/// <summary>
		/// Check if we're allowed to send to this channel
		/// </summary>
		/// <returns>True if there is room in the queue or the specified amount of time has passed since first message in queue. Otherwise false</returns>
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

	/// <summary>
	/// An item in a stringqueue. represents a messge and the time it was sent
	/// </summary>
	public class StringQueueItem 
	{
		string text;
		DateTime timeStamp;

		/// <summary>
		/// Initializes a new instance of the <see cref="StringQueueItem"/> class.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="timeStamp">The time stamp.</param>
		public StringQueueItem(string text, DateTime timeStamp) 
		{
			this.text = text;
			this.timeStamp = timeStamp;
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text 
		{
			get { return text; }
//			set { text = value; }
		}
		/// <summary>
		/// Gets the time stamp.
		/// </summary>
		/// <value>The time stamp.</value>
		public DateTime TimeStamp 
		{
			get { return timeStamp; }
//			set { timeStamp = value; }
		}
	}
}
