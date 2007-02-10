using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
//using NielsRask.LibIrc;
using NielsRask.FnordBot;
using System.Reflection;

namespace NielsRask.FnordBot
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button btnConnect2;
		private System.Windows.Forms.Button btnShow;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			InitializeComponent();
//
//			client = new Client();
//			client.VersionInfo = "FnordBot 0.8";
//			client.FingerInfo = "LA LA LA";
//
////			usermod = new Users.Module( "UserData.xml" );
//			
//			forwarder = new Forwarder( client );
//			channelsToJoin = new StringCollection();
//
//			AttachForwarderHooks();
//
//			client.OnRegister += new NielsRask.LibIrc.Client.ClientRegisterHandler(client_OnRegister);
//			client.OnServerMessage += new NielsRask.LibIrc.Client.ServerMessageHandler(client_OnServerMessage);
//
//			LoadConfig();

//			LoadPlugin("TestPlugin.Class1",@"C:\Documents and Settings\Niels\My Documents\Visual Studio Projects\FnordBot\TestPlugin\bin\Debug\TestPlugin.dll");
		}

//		private Users.Module LoadUserModule() 
//		{
//			string cfgpath = "";
//			if (File.Exists("Config.xml")) 
//				cfgpath = "Config.xml";
//			else if (File.Exists("..\\Config.xml")) 
//				cfgpath = "..\\Config.xml";
//			else if (File.Exists("..\\..\\Config.xml")) 
//				cfgpath = "..\\..\\Config.xml";
//			else throw new FileNotFoundException("User data not found");
//		}

//		private void LoadConfig() 
//		{
//			string cfgpath = "";
//			if (File.Exists("Config.xml")) 
//				cfgpath = "Config.xml";
//			else if (File.Exists("..\\Config.xml")) 
//				cfgpath = "..\\Config.xml";
//			else if (File.Exists("..\\..\\Config.xml")) 
//				cfgpath = "..\\..\\Config.xml";
//			else throw new FileNotFoundException("Config file not found");
//			Console.WriteLine("config found at "+cfgpath);
//
//			XmlDocument xdoc = new XmlDocument();
//			xdoc.Load( cfgpath );
//
//			client.Port = int.Parse( GetXPathValue(xdoc,"client/server/@port") );
//			client.Server = GetXPathValue(xdoc,"client/server/text()");
//			client.Username = GetXPathValue(xdoc,"client/username/text()");
//			client.RealName = GetXPathValue(xdoc,"client/realname/text()");
//			client.Nick = GetXPathValue(xdoc,"client/nickname/text()");
//			forwarder.Nick = client.Nick;
//
//			foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("client/channels/channel/text()")) 
//			{
//				channelsToJoin.Add( node.Value );
//			}
//
//			foreach (XmlNode node in xdoc.DocumentElement.SelectNodes("plugins/plugin")) 
//			{
//				string typename = node.SelectSingleNode("@typename").Value;
//				string path = node.SelectSingleNode("@path").Value;
//				LoadPlugin( typename, path );
//			}
//
//
//		}
//
//		private string GetXPathValue(XmlDocument xdoc, string xpath) 
//		{
//			XmlNode node = xdoc.DocumentElement.SelectSingleNode( xpath );
//			if ( node != null) 
//			{
//				return node.Value;
//			}
//			else 
//			{
//				return "";
//			}
//		}

//		private void AttachForwarderHooks() 
//		{
//			client.OnPublicMessage +=new NielsRask.LibIrc.Client.PublicMessageHandler( forwarder.ForwardPublicMessage );
//			client.OnPrivateMessage +=new NielsRask.LibIrc.Client.PrivateMessageHandler( forwarder.ForwardPrivateMessage );
//			client.OnJoined +=new NielsRask.LibIrc.Client.ClientJoinedHandler( forwarder.ForwardJoined );
//			client.OnRegister +=new NielsRask.LibIrc.Client.ClientRegisterHandler( forwarder.ForwardRegister );
//			client.OnChannelJoin +=new NielsRask.LibIrc.Client.ChannelActionEventHandler( forwarder.ForwardChannelJoin );
//			client.OnChannelPart +=new NielsRask.LibIrc.Client.ChannelActionEventHandler( forwarder.ForwardChannelPart );
//			client.OnServerMessage +=new NielsRask.LibIrc.Client.ServerMessageHandler( forwarder.ForwardServerMessage );
//		}
//
//		private void LoadPlugin(string type, string path) 
//		{
//			Assembly pAsm = Assembly.LoadFrom( path );
//			IPlugin plugin = (IPlugin)pAsm.CreateInstance( type );
//			plugin.Init();
//			plugin.Attach( forwarder );
//			Console.WriteLine("Attached plugin "+type);
//		}
//
//		private void ConnectClient() 
//		{
//			client.Connect();
//		}

//		Forwarder forwarder;
//		Client client;
//		private StringCollection channelsToJoin;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnConnect = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.btnConnect2 = new System.Windows.Forms.Button();
			this.btnShow = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(192, 48);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.TabIndex = 0;
			this.btnConnect.Text = "plugin test";
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(192, 88);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "button1";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// btnConnect2
			// 
			this.btnConnect2.Location = new System.Drawing.Point(16, 24);
			this.btnConnect2.Name = "btnConnect2";
			this.btnConnect2.TabIndex = 2;
			this.btnConnect2.Text = "Connect";
			this.btnConnect2.Click += new System.EventHandler(this.btnConnect2_Click);
			// 
			// btnShow
			// 
			this.btnShow.Location = new System.Drawing.Point(16, 80);
			this.btnShow.Name = "btnShow";
			this.btnShow.TabIndex = 3;
			this.btnShow.Text = "Show";
			this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.btnShow);
			this.Controls.Add(this.btnConnect2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.btnConnect);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

//		private void btnConnect_Click(object sender, System.EventArgs e)
//		{
//			ConnectClient();
//		}
//
//		private void client_OnRegister(object bot)
//		{
//			Console.WriteLine("We're now registered!");
//			foreach (string channel in channelsToJoin) client.Join( channel );
//		}
//
//		private void client_OnServerMessage(string message)
//		{
//			Console.WriteLine( message );
//		}

		private void button1_Click(object sender, System.EventArgs e)
		{
//			client.GetChannelUsers("#craYon");
		}

		NielsRask.FnordBot.FnordBot bot;
		private void btnConnect2_Click(object sender, System.EventArgs e)
		{
			// HACK: There is an unresolved issue concerning finding the config files for the fnordbot assembly,
			// when running from within visual studio. The problem is that we're unable to find the directory
			// of the FnordBot2 project, as the assembly is copied to the launcher programs /bin directory
			// the solution is to edit the following relative path (starts in the launcher directory)
			// this should be resolved in the final releases
			bot = new FnordBot("..\\..\\..\\FnordBot2\\");
			bot.Init();
			bot.Connect();
		}

		private void btnShow_Click(object sender, System.EventArgs e)
		{
			foreach (NielsRask.LibIrc.Channel chn in bot.Channels) 
			{
				Console.WriteLine(" "+chn.Name+" ("+chn.Topic+")");
				foreach (NielsRask.LibIrc.User usr in chn.Users) 
				{
					Console.WriteLine("  "+usr.ToString());

				}
			}
		}

		private void btnConnect_Click(object sender, System.EventArgs e)
		{
			bot = new FnordBot("..\\..\\..\\FnordBot2\\");
//			bot.PluginTest();
			Close();
		}
	}
}
