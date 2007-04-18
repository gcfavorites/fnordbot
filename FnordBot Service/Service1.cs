using System;
using System.Collections;
using System.ComponentModel;
//using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using NielsRask.FnordBot;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using log4net;
// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch=true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)
namespace NielsRask.FnordBotService
{
	[ComVisible(false)]
	public class Service1 : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components;
		private BotHandler handler;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Service1()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
			handler = new BotHandler();

			// TODO: Add any initialization after the InitComponent call
		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service1() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "Service1";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			try 
			{
				if( disposing )
				{
					if (components != null) 
					{
						components.Dispose();
					}
				}
			} 
			catch {}
			finally
			{
				base.Dispose( disposing );
			}
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			// TODO: Add code here to start your service.
			log.Info("Starting FnordBot service ...");
			handler.Start();
			log.Info("FnordBot service");

		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Add code here to perform any tear-down necessary to stop your service.
			log.Info("Stopping FnordBot service ...");
			handler.Stop();
			log.Info("FnordBot service stopped.");

		}
	}

	public class BotHandler 
	{
		Thread thread;
		NielsRask.FnordBot.FnordBot bot;
		string installationFolderPath;
		StreamWriter swlog;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public BotHandler() 
		{
			installationFolderPath = GetConfigFilePath();
			File.Delete( installationFolderPath+"err.txt" );
			File.Delete( installationFolderPath+"out.txt" );
			File.Delete( installationFolderPath+"log.txt" );
			StreamWriter swerr = new StreamWriter( installationFolderPath+"err.txt", true, System.Text.Encoding.Default );
			swerr.AutoFlush = true;
			StreamWriter swout = new StreamWriter( installationFolderPath+"out.txt", true, System.Text.Encoding.Default );
			swout.AutoFlush = true;
			Console.SetError( swerr );
			Console.SetOut( swout );
			log.Debug("Initiating fnordbot with path: "+installationFolderPath);
			bot = new NielsRask.FnordBot.FnordBot( installationFolderPath );
//			bot.OnLogMessage +=new NielsRask.FnordBot.FnordBot.LogMessageHandler(WriteLogMessage);
			bot.Init();
			log.Debug("initiating thread");
			thread = new Thread( new ThreadStart( bot.Connect ) );
			thread.IsBackground = false;
			thread.Name = "FnordBotThread";
		}

		private void WriteLogMessage(string message) 
		{
			try 
			{
				swlog = new StreamWriter(installationFolderPath+"log.txt",true, System.Text.Encoding.Default);
				swlog.WriteLine( DateTime.Now.ToLongTimeString()+": "+message );
			} 
			catch (Exception e) 
			{
				log.Error("Error writing to log ",e);
			} 
			finally 
			{
				swlog.Close();
			}
		}

		public void Start() 
		{
			log.Debug("starting thread");
			thread.Start();
		}

		public void Stop() 
		{
			log.Debug("stopping thread");
			thread.Abort();
		}

		private string GetConfigFilePath() 
		{
			RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\NielsRask\\FnordBot");
			return (string)rk.GetValue("InstallationFolderPath");
		}
	}
}
