//using System;
//using NUnit.Framework;
//using System.Net;
//using System.Net.Sockets;
//using System.IO;
//using System.Threading;

//namespace NielsRask.LibIrc
//{
//    /// <summary>
//    /// Summary description for Test.
//    /// </summary>
//    [TestFixture]
//    public class Test
//    {
//        public Test()
//        {
//            //
//            // TODO: Add constructor logic here
//            //
//        }
//        Module mdl;

//        [TestFixtureSetUp]
//        public void Setup()
//        {
//        }


//        [Test]
//        public void TestUserCount()
//        {
//        }

//        [Test]
//        public void TestHostMatch()
//        {
//        }
//    }

//    public class TestServer
//    {
//        TcpListener listener;
//        ServerThread srvrthrd;
//        public void Start()
//        {
//            listener = new TcpListener( 6669 );
//            listener.Start();
//            TcpClient client = listener.AcceptTcpClient();
//            listener.Stop();
//            srvrthrd = new ServerThread( client );
//            Thread t = new Thread( new ThreadStart( srvrthrd.Start ) );
//            t.Start();
//        }

//        public void Send( string text )
//        {
//            srvrthrd.Send( txt );
//        }

//    }
//    public class ServerThread
//    {

//        StreamWriter sw;
//        public ServerThread( TcpClient client )
//        {
//            sw = new StreamWriter( client.GetStream() );
//        }

//        public void Start()
//        {

//        }

//        public void Send( string txt )
//        {
//            sw.WriteLine( txt );
//        }
//    }
//}
