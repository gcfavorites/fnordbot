using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// A simple Ident-server. see http://tools.ietf.org/html/rfc1413
	/// </summary>
	public class Ident
	{
		private readonly TcpListener listener;
		private readonly string userId;
		private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
		/// <summary>
		/// Initializes a new instance of the <see cref="Ident"/> class.
		/// </summary>
		/// <param name="userId">The user id.</param>
		public Ident(string userId)
		{
			this.userId = userId;
			listener = new TcpListener( IPAddress.Any, 113 );
		}

		/// <summary>
		/// Starts this instance.
		/// </summary>
		public void Start()
		{
			try
			{
				listener.Start();
				TcpClient client = listener.AcceptTcpClient();
				listener.Stop();
				using (NetworkStream s = client.GetStream())
				{
					StreamReader reader = new StreamReader(s);
					string str = reader.ReadLine();
					//reader.Close();

					StreamWriter writer = new StreamWriter(s);
					Console.WriteLine("Ident got: " + str + ", sending reply");
					writer.WriteLine(str + " : USERID : UNIX : " + userId);
					writer.Flush();
					Console.WriteLine("Ident sent reply");
				}
				log.Debug("Ident server exiting");
				Console.WriteLine("Ident server exiting");
			} 
			catch (SocketException e)
			{
				log.Error("Failed to start ident-server - is it already running?", e);
			}
		}
	}
}
