using System;
using System.Collections;
using System.Xml;

namespace NielsRask.FnordBot.Users
{
	/// <summary>
	/// Summary description for User.
	/// </summary>
	public class User
	{
		string name = "";
		string nickName = "";
		string password = "";
		HostmaskCollection hostmasks;
		CustomSettingCollection customSettings;
		bool isCitizen = false;
		Module mdl;

		public string Name 
		{
			get { return name; }
			set { name = value; }
		}

		public string NickName 
		{
			get { return nickName; }
			set { nickName = value; }
		}

		public string Password  
		{
			get { return password; }
			set { password = value; }
		}

		public HostmaskCollection Hostmasks
		{
			get { return hostmasks; }
		}

		public CustomSettingCollection CustomSettings
		{
			get { return customSettings; }
		}

		/// <summary>
		/// If true, indicates that this user exists in the userfile. if false, it is an unregistered user
		/// </summary>
		public bool IsCitizen 
		{
			get{ return isCitizen; }
		}

		public User( string name ) 
		{
			this.name = name;
			this.nickName = name;
			this.password = name;
			hostmasks = new	HostmaskCollection();
			customSettings = new CustomSettingCollection( null );
		}

		public void MakeCitizen() 
		{
			isCitizen = true;
		}

		internal User(XmlNode node, Module mdl)
		{
			isCitizen = true; // a registered user, loaded from the userfile
			name = node.SelectSingleNode("name/text()").Value;
			password = node.SelectSingleNode("password/text()").Value;
			hostmasks = HostmaskCollection.UnpackHostmasks( node.SelectNodes("hostmasks/hostmask") );
			customSettings = CustomSettingCollection.UnpackSettings( node.SelectNodes("custom/*"), mdl );
			this.mdl = mdl;
		}

		public string ToXmlString() 
		{
			string xml = "<user>";
			xml += "<name>"+name+"</name>";
			xml += "<password>"+password+"</password>";
			xml += hostmasks.ToXmlString();
			xml += customSettings.ToXmlString();
			return xml;
		}

		public void Save() 
		{
			if (mdl!= null) mdl.Save();
		}

	}

	public class UserCollection : CollectionBase
	{
		public UserCollection() {}

		internal static UserCollection UnpackUsers( XmlNodeList usrs, Module mdl ) 
		{
			UserCollection usrcol = new UserCollection();
			for (int i=0; i<usrs.Count; i++) 
			{
				Console.WriteLine("Unpacking a user");
				usrcol.Add( usrs[i], mdl );
			}
			return usrcol;
		}

		public void Add(User user) 
		{
			List.Add(user);
		}

		internal void Add( XmlNode userNode, Module mdl ) 
		{
			Add( new User( userNode, mdl ) );
		}

		public void Remove(User user) 
		{
			List.Remove(user);
		}

		public User this[int i] 
		{
			get { return (User)List[i]; }
			set { List[i] = value; }
		}

		public User GetByName(string name, bool ignoreCase) 
		{
			bool found = false;
			int i=0;
			while ( ! found && i < Count) 
			{
				if ( String.Compare( this[i].Name, name, ignoreCase ) == 0 ) 
				{
					found = true;
				} 
				else 
				{
					i++;
				}
			}
			if ( found ) 
			{
				return this[i];
			} 
			else 
			{
				return null;					
			}
		}

		public User GetByHostMatch( string host ) 
		{
			bool found = false;
			int i=0;
			while ( ! found && i < Count) 
			{
				if ( this[i].Hostmasks.IsMatch( host, false, false ) ) 
				{
					found = true;
				} 
				else 
				{
					i++;
				}
			}
			if ( found ) 
			{
				return this[i];
			} 
			else 
			{
				return null;					
			}
		}

		public string ToXmlString() 
		{
			string xml = "<users>";

			for(int i=0; i<Count; i++) if (this[i].IsCitizen) xml += this[i].ToXmlString(); // only save citizens
			xml += "</users>";
			return xml;
		}
	}

}
