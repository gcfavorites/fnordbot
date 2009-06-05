using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using log4net;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// A simple Ident-server. see http://tools.ietf.org/html/rfc1413
	/// </summary>
	public class Ident
	{
		private TcpListener listener;
		private string userId;
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
			Console.WriteLine( "Ident started" );
			listener.Start();
			TcpClient client = listener.AcceptTcpClient();
			Console.WriteLine( "Ident got a connection" );
			using (NetworkStream s = client.GetStream() ) 
			{
				StreamReader reader = new StreamReader( s );
				string str = reader.ReadLine();
				//reader.Close();

				StreamWriter writer = new StreamWriter( s );
				Console.WriteLine("Ident got: "+str+", sending reply");
				writer.WriteLine( str + " : USERID : UNIX : NordCore" );
				writer.Flush();
				Console.WriteLine( "Ident sent reply" );
			}
		}
	}
}
