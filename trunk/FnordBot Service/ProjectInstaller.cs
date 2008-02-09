using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace NielsRask.FnordBotService
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	[ComVisible(false)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{

		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
		private System.ServiceProcess.ServiceInstaller serviceInstaller1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components;

		public ProjectInstaller()
		{
			InitializeComponent();
			serviceInstaller1.DisplayName = "Fnordbot service";		// navnet der kommer i service-listen
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
					serviceInstaller1.Dispose();
					serviceProcessInstaller1.Dispose();
					if(components != null)
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

		//---
		public override void Install(System.Collections.IDictionary stateSaver) {
			base.Install(stateSaver);
			WriteInitialConfig();
		}

		//---


		private void WriteInitialConfig() 
		{
			try  
			{
				string installpath = Context.Parameters["path"];
				if ( !installpath.EndsWith("\\") )
					installpath += "\\";

				string filepath;
				if (!File.Exists(installpath+"Config.xml")) 
				{
					filepath = installpath+"Config.xml";
				}
				else 
				{
					filepath = installpath+"Config.foo";
				}

				StreamWriter sw = new StreamWriter(filepath,true, System.Text.Encoding.Default);
				sw.WriteLine("<?xml version=\"1.0\" encoding=\"iso-8859-1\" ?> ");
				sw.WriteLine("<config>");
				sw.WriteLine("	<client>");
				sw.WriteLine("		<server port=\"6667\">irc.droso.net</server>");
				sw.WriteLine("		<nickname>BimseBot</nickname>");
				sw.WriteLine("		<altnick>BimmerBot</altnick>");
				sw.WriteLine("		<realname>B. Imse</realname>");
				sw.WriteLine("		<username>bimmerfooo</username>");
				sw.WriteLine("		<channels>");
				sw.WriteLine("			<channel>");
				sw.WriteLine("				<name>#bottest</name>");
				sw.WriteLine("				<messagerate messages=\"5\" minutes=\"15\"/>");
				sw.WriteLine("			</channel>");
				sw.WriteLine("		</channels>");
				sw.WriteLine("	</client>");
				sw.WriteLine("	<plugins>");
				sw.WriteLine("		<plugin typename=\"NielsRask.SortSnak.Plugin\" path=\"plugins\\sortsnak\\sortsnak.dll\" >");
				sw.WriteLine("			<settings>");
				sw.WriteLine("				<vocabularyfilepath>plugins\\sortsnak\\vocabulary.dat</vocabularyfilepath>");
				sw.WriteLine("				<answerchance>15</answerchance>");
				sw.WriteLine("				<minimumoverlap>3</minimumoverlap>");
				sw.WriteLine("				<simplechance>35</simplechance>");
				sw.WriteLine("				<ambientsimplechance>10</ambientsimplechance>");
				sw.WriteLine("				<autosaving>5</autosaving>");
				sw.WriteLine("			</settings>");
				sw.WriteLine("			<permissions>");
				sw.WriteLine("				<permission name=\"CanOverrideSendToChannel\" value=\"False\" />");
				sw.WriteLine("			</permissions>");
				sw.WriteLine("		</plugin> ");
				sw.WriteLine("		<plugin typename=\"NielsRask.Wordgame.Plugin\" path=\"plugins\\wordgame\\wordgame.dll\" >");
				sw.WriteLine("			<settings>");
				sw.WriteLine("				<wordlist>plugins\\wordgame\\wordlist.dat</wordlist>");
				sw.WriteLine("			</settings>");
				sw.WriteLine("			<permissions>");
				sw.WriteLine("				<permission name=\"CanOverrideSendToChannel\" value=\"True\" />");
				sw.WriteLine("			</permissions>");
				sw.WriteLine("		</plugin> ");
				sw.WriteLine("		<plugin typename=\"NielsRask.Logger.Plugin\" path=\"plugins\\logger\\logger.dll\" >");
				sw.WriteLine("			<settings>");
				sw.WriteLine("				<logfolderpath>plugins\\logger\\logs</logfolderpath>");
				sw.WriteLine("			</settings>");
				sw.WriteLine("			<permissions>");
				sw.WriteLine("				<permission name=\"CanOverrideSendToChannel\" value=\"False\" />");
				sw.WriteLine("			</permissions>");
				sw.WriteLine("		</plugin> ");
				sw.WriteLine("		<plugin typename=\"NielsRask.Stat.StatPlugin\" path=\"plugins\\stat\\stat.dll\" > ");
				sw.WriteLine("			<settings /> ");
				sw.WriteLine("			<permissions> ");
				sw.WriteLine("				<permission name=\"CanOverrideSendToChannel\" value=\"True\" /> ");
				sw.WriteLine("			</permissions> ");
				sw.WriteLine("		</plugin>  ");

				sw.WriteLine("	</plugins>");
				sw.WriteLine("</config>");
				sw.Close();
			} 
			catch {}
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller1
			// 
			this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessInstaller1.Password = null;
			this.serviceProcessInstaller1.Username = null;
			// 
			// serviceInstaller1
			// 
			this.serviceInstaller1.ServiceName = "FnordBot";
			this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.serviceProcessInstaller1,
																					  this.serviceInstaller1});

		}
		#endregion
	}
}
