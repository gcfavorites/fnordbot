using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// Summary description for IrcClient.
	/// </summary>
	public class Network
	{
		private TcpClient server;
		private NetworkStream stream;
		private StreamWriter writer;
		private IrcListener listener;

		public event ServerMessageHandler OnServerMessage;
		public event ServerStateHandler OnConnect;
		public event ServerStateHandler OnDisconnect;

		/// <summary>
		/// Delegate for raw server messages
		/// </summary>
		public delegate void ServerMessageHandler(string message);
		public delegate void ServerStateHandler();

		internal void CallOnServerMessage(string message) { if (OnServerMessage!=null) OnServerMessage(message); } 
		internal void CallOnDisconnect() { if (OnDisconnect!=null) OnDisconnect(); }

		public delegate void LogMessageHandler(string message);
		public event LogMessageHandler OnLogMessage;
		private void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) OnLogMessage( message );
		}


		/// <summary>
		/// Connects to a server
		/// </summary>
		public void Connect(string host, int port) 
		{
			server = new TcpClient(host,port);
			if (OnConnect!=null) OnConnect();
			
			stream = server.GetStream();

			listener = new IrcListener(this);

			listener.OnLogMessage += new IrcListener.LogMessageHandler(WriteLogMessage);
			
			listener.Start(stream);
			writer = new StreamWriter(stream,System.Text.Encoding.Default);
		}

		/// <summary>
		/// Sends a message to a user
		/// </summary>
		/// <param name="text"></param>
		/// <param name="user"></param>
		public void SendToServer(string text) 
		{
			writer.WriteLine(text);
			writer.Flush();
		}

	}

	public class IrcListener 
	{
		private Thread listener;
		private StreamReader reader;
		private StreamWriter writer;
		private string inputLine;
		private Network network;

		public delegate void LogMessageHandler(string message);
		public event LogMessageHandler OnLogMessage;
		private void WriteLogMessage(string message) 
		{
			if ( OnLogMessage != null ) OnLogMessage( message );
		}

		internal IrcListener(Network network) 
		{
			this.network = network;
			listener = new Thread (new ThreadStart (this.Run) );
			listener.IsBackground = true;
		}

		internal void Start(NetworkStream stream) 
		{
			this.reader = new StreamReader(stream,System.Text.Encoding.Default);
			this.writer = new StreamWriter(stream,System.Text.Encoding.Default);
			listener.Start();
		}

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
				WriteLogMessage("Error in listener.run: "+e);
			} 
			finally 
			{
				network.CallOnDisconnect();
			}
		}
	}
}
