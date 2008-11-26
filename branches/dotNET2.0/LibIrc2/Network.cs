using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using log4net;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// The network layer
	/// </summary>
	public class Network
	{
		private TcpClient server;
		private NetworkStream stream;
		private StreamWriter writer;
		private IrcListener listener;

		// Create a logger for use in this class
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Occurs on message received from server
		/// </summary>
		public event ServerMessageHandler OnServerMessage;

		/// <summary>
		/// Occurs on successfull connect to server
		/// </summary>
		public event ServerStateHandler OnConnect;

		/// <summary>
		/// Occurs on disconnect from server
		/// </summary>
		public event ServerStateHandler OnDisconnect;

		/// <summary>
		/// Delegate for raw server messages
		/// </summary>
		public delegate void ServerMessageHandler(string message);

		/// <summary>
		/// Delegate for server state changes
		/// </summary>
		public delegate void ServerStateHandler();

		/// <summary>
		/// Calls the OnServerMessage event if a subscriber exists
		/// </summary>
		internal void CallOnServerMessage(string message) 
		{
			if (OnServerMessage != null) 
				OnServerMessage(message); 
		} 

		/// <summary>
		/// Calls the OnDisconnect event if a subscriber exists
		/// </summary>
        internal void CallOnDisconnect() 
		{ 
			if (OnDisconnect != null) 
				OnDisconnect(); 
		}

		/// <summary>
		/// Connects to a server
		/// </summary>
		public void Connect(string host, int port) 
		{
            try
            {
                log.Debug("Network: Connecting to server " + host + ":" + port + " ...");
                server = new TcpClient(host, port);
                if (OnConnect != null) OnConnect();
                log.Debug("Network: Successfully connected, starting listener...");

                stream = server.GetStream();

                listener = new IrcListener(this);

                listener.Start(stream);
                writer = new StreamWriter(stream, System.Text.Encoding.Default);
                log.Debug("Network: Writer and listener threads started.");
            } 
            catch (Exception e)
		    {
                throw new ConnectionRefusedException("Unable to connect to server '"+host+"'", e);
		    }
		}

		/// <summary>
		/// Sends a message to the serverr
		/// </summary>
		/// <param name="text">The text to send</param>
		public void SendToServer(string text) 
		{
			//if (!text.StartsWith("PONG :")) 
			log.Debug("Sending to server: '"+text+"'");
			
            lock( writer ) 
			{
				writer.WriteLine(text);
				writer.Flush();
			}
		}

	}

	/// <summary>
	/// network listener thread
	/// </summary>
	public class IrcListener 
	{
		private readonly Thread listener;
		private StreamReader reader;
		private string inputLine;
		private readonly Network network;

		// Create a logger for use in this class
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Initializes a new instance of the <see cref="IrcListener"/> class.
		/// </summary>
		/// <param name="network">The network.</param>
		internal IrcListener(Network network) 
		{
			this.network = network;
			listener = new Thread( this.Run );
			listener.IsBackground = true;
			listener.Name = "ListenerThread";
		}

		/// <summary>
		/// Starts the network listener thread
		/// </summary>
		/// <param name="stream"></param>
		internal void Start(NetworkStream stream) 
		{
			reader = new StreamReader(stream,System.Text.Encoding.Default);
			listener.Start();
			log.Info("Listener thread started.");
		}

		/// <summary>
		/// The main listening loop
		/// </summary>
		internal void Run() 
		{
			try 
			{
				lock (reader) 
				{ 
					while ( (inputLine = reader.ReadLine() ) != null) 
					{
						network.CallOnServerMessage(inputLine);
						if (!inputLine.StartsWith("PING :"))
							log.Debug("Received line: "+inputLine);
					} 
				}
			} 
			catch (Exception e) 
			{
				// this message can propably be discarded as we're only interested in the fact that the connection was lost
				log.Error("Error in listener.run, connection probably lost.",e);
			} 
			finally 
			{
				log.Warn("Calling network.CallOnDisconnect()");
				network.CallOnDisconnect();
			}
		}
	}
}
