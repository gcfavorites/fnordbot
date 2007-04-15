using System;
using System.Net;
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
//		/// <summary>
//		/// Delegate for logging
//		/// </summary>
//		public delegate void LogMessageHandler(string message);
//
//		/// <summary>
//		/// Occurs when the network layer wishes to log a message
//		/// </summary>
//		public event LogMessageHandler OnLogMessage;
//
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

//		/// <summary>
//		/// Calls the OnLogMessage event if a subscriber exists
//		/// </summary>
//		private void WriteLogMessage(string message) 
//		{
//			if ( OnLogMessage != null ) 
//				OnLogMessage( message );
//		}


		/// <summary>
		/// Connects to a server
		/// </summary>
		public void Connect(string host, int port) 
		{
			server = new TcpClient(host,port);
			if (OnConnect!=null) OnConnect();
			
			stream = server.GetStream();

			listener = new IrcListener(this);

//			listener.OnLogMessage += new IrcListener.LogMessageHandler(WriteLogMessage);
			
			listener.Start(stream);
			writer = new StreamWriter(stream,System.Text.Encoding.Default);
		}

		/// <summary>
		/// Sends a message to the serverr
		/// </summary>
		/// <param name="text">The text to send</param>
		public void SendToServer(string text) 
		{
			log.Debug("Sending to server: '"+text+"'");
			writer.WriteLine(text);
			writer.Flush();
		}

	}

	/// <summary>
	/// network listener thread
	/// </summary>
	public class IrcListener 
	{
		private Thread listener;
		private StreamReader reader;
		private string inputLine;
		private Network network;
		// Create a logger for use in this class
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//		/// <summary>
//		/// Delegate for logging
//		/// </summary>
//		public delegate void LogMessageHandler(string message);
//		/// <summary>
//		/// Occurs when the network listener wants to log a message
//		/// </summary>
//		public event LogMessageHandler OnLogMessage;
//
//		/// <summary>
//		/// Writes a message to the log, if a subscriber exists
//		/// </summary>
//		/// <param name="message"></param>
//		private void WriteLogMessage(string message) 
//		{
//			if ( OnLogMessage != null ) 
//				OnLogMessage( message );
//		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IrcListener"/> class.
		/// </summary>
		/// <param name="network">The network.</param>
		internal IrcListener(Network network) 
		{
			this.network = network;
			listener = new Thread (new ThreadStart (this.Run) );
			listener.IsBackground = true;
		}

		/// <summary>
		/// Starts the network listener thread
		/// </summary>
		/// <param name="stream"></param>
		internal void Start(NetworkStream stream) 
		{
			this.reader = new StreamReader(stream,System.Text.Encoding.Default);
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
				while ( (inputLine = reader.ReadLine() ) != null) 
				{
					network.CallOnServerMessage(inputLine);
				} 
			} 
			catch (Exception e) 
			{
				// this message can propably be discarded as we're only interested in the fact that the connection was lost
//				WriteLogMessage("Error in listener.run, connection probably lost: "+e);
				log.Error("Error in listener.run, connection probably lost.",e);
			} 
			finally 
			{
				network.CallOnDisconnect();
			}
		}
	}
}
