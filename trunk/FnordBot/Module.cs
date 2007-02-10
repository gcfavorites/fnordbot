//using System;
//using System.Xml;
//
//namespace NielsRask.FnordBot.Users
//{
//	/// <summary>
//	/// Summary description for Class1.
//	/// </summary>
//	public class Module
//	{
//		XmlDocument xdoc;
//		UserCollection users;
//		string defaultPath;
//
//		public UserCollection Users 
//		{
//			get { return users; }
//		}
//
//		public Module (string path)
//		{
//			defaultPath = path;
//			XmlDocument xdoc = new XmlDocument();
//			xdoc.Load( path );
//
//			this.xdoc = xdoc;
//
//			XmlNodeList usrlst = xdoc.DocumentElement.SelectNodes("user");
//			Console.WriteLine("found "+usrlst.Count+" usernodes");
//			
//
//			users = UserCollection.UnpackUsers( usrlst, this );		
//		}
//
//		public Module (XmlDocument xdoc) 
//		{
//			this.xdoc = xdoc;
//
//			XmlNodeList usrlst = xdoc.DocumentElement.SelectNodes("user");
//
//			users = UserCollection.UnpackUsers( usrlst, this );
//		}
//
//		public void Save() 
//		{
//			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>"+users.ToXmlString();
//			xdoc.LoadXml( xml );
//			xdoc.Save( defaultPath );
//		}
//
//		public User GetUserByHostMask(string hostmask) 
//		{
//			return users.GetByHostMatch( hostmask );
//		}
//
//		#region old code
////		XmlDocument xdoc;
////		public Module()
////		{
////			xdoc = new XmlDocument();
////			xdoc.Load("..\\..\\Users.xml");
////		}
////
////		private void SaveXml() 
////		{
////			xdoc.Save("..\\..\\Users.xml");
////		}
////
////		// indlæs data fra en xml
////
////		// returner fnordbot-username udfra hostmask
////		// opret user udfra hostmask
////		// tilføj hostmask til eksisterende user
////		// parsing af hostmask m/ wildcard support
////		// gemme custom data for en user - eks wordgame score eller dungeon status
////
////		public string AddHostmask(string user, string hostmask) 
////		{
////			XmlNode usr = GetUserNode( user );
////			XmlNode mask = usr.SelectSingleNode("hostmasks/hostmask[text()='"+hostmask+"']");
////			if (mask == null) usr.SelectSingleNode("hostmask").InnerXml += "<hostmask>"+hostmask+"</hostmask>";
////			SaveXml();
////		}
////
////		private XmlNode GetUserNode(string user) 
////		{
////			return xdoc.DocumentElement.SelectSingleNode("user[name='"+name+"']"");
////		}
////
////		public string GetUserByHostmask(string hostmask) 
////		{
////			XmlNodeList users = xdoc.DocumentElement.SelectNodes("user");
////			for (int i=0; i<users.Count; i++) 
////			{
////				XmlNodeList hostmasks = users[i].SelectNodes("hostmasks/hostmask/text()");
////				for (int j=0; j<hostmasks.Count; j++) 
////				{
////					if ( wildcmp(hostmasks[i].Value, hostmask, false) ) return users[i].SelectSingleNode("name/text()");
////				}
////			}
////
////			return "";
////		}
////
////		public string GetCustomSetting(string user, string setting) 
////		{
////			XmlNode node = xdoc.DocumentElement.SelectSingleNode("user[name='"+user+"']/custom/"+setting+"/text()" );
////			if (node != null) 
////				return node.Value;
////			else
////				return "";
////		}
////
////		public void SetCustomString(string user, string setting, string value) 
////		{
////			XmlNode cstm = xdoc.SelectSingleNode("user[name='"+user+"']/custom/"+setting+"/text()");
////			if ( cstm == null ) 
////			{
////				xdoc.SelectSingleNode("user[name='"+user+"']/custom/").InnerXml += "<"+setting+">"+value+"</"+setting+">";
////			} 
////			else 
////			{
////				cstm.Value = value;
////			}
////			SaveXml();
////		}
////
////		public string AddUser( string name, string hostmask ) 
////		{
////			string xml = "";
////			xml += "<user>";
////			xml += "<name>"+name+"</name>";
////			xml += "<password>123</password>";
////			xml += "<hostmasks>";
////			xml += "<hostmask>"+hostmask+"</hostmask>";
////			xml += "</hostmasks>";
////			xml += "<custom></custom>";
////			xml += "</user>";
////			xdoc.DocumentElement.InnerXml += xml;
////			SaveXml();
////		}
////
////		private bool wildcmp(string wild, string str, bool case_sensitive)
////		{
////			int cp=0, mp=0;
////	
////			int i=0;
////			int j=0;
////
////			if (! case_sensitive)
////			{
////				wild = wild.ToLower();
////				str = str.ToLower();
////			}
////			
////			while (i < str.Length && j < wild.Length && wild[j] != '*')
////			{
////				if ((wild[j] != str[i]) && (wild[j] != '?')) 
////				{
////					return false;
////				}
////				i++;
////				j++;
////			}
////		
////			while (i<str.Length) 
////			{
////				if (j<wild.Length && wild[j] == '*') 
////				{
////					if ((j++)>=wild.Length) 
////					{
////						return true;
////					}
////					mp = j;
////					cp = i+1;
////				} 
////				else if (j<wild.Length && (wild[j] == str[i] || wild[j] == '?')) 
////				{
////					j++;
////					i++;
////				} 
////				else 
////				{
////					j = mp;
////					i = cp++;
////				}
////			}
////		
////			while (j
////			{
////				j++;
////			}
////			return j>=wild.Length;
////		}
//		#endregion
//	}
//}
//
