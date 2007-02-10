using System;
using System.Collections;
using System.Xml;

namespace NielsRask.FnordBot
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
		UserCollection.SaveUsersDelegate saveUsers;
//		Module mdl;

		/// <summary>
		/// RealName of the user
		/// </summary>
		/// <value>The name.</value>
		public string Name 
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Nickname of the user
		/// </summary>
		public string NickName 
		{
			get { return nickName; }
			set { nickName = value; }
		}

		/// <summary>
		/// Password of the user
		/// </summary>
		public string Password  
		{
			get { return password; }
			set { password = value; }
		}

		/// <summary>
		/// Valid hostmasks for this user
		/// </summary>
		public HostmaskCollection Hostmasks
		{
			get { return hostmasks; }
		}

		/// <summary>
		/// Custom settings or this user
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="saveUsers">The save users.</param>
		public User( string name, UserCollection.SaveUsersDelegate saveUsers ) 
		{
			this.name = name;
			this.nickName = name;
			this.password = name;
			this.saveUsers = saveUsers;
			hostmasks = new	HostmaskCollection();
			customSettings = new CustomSettingCollection( saveUsers );
		}

		/// <summary>
		/// Makes the user a citizen.
		/// </summary>
		public void MakeCitizen() 
		{
			isCitizen = true;
		}

		internal User(XmlNode node, UserCollection.SaveUsersDelegate saveUsers )
		{
			this.saveUsers = saveUsers;
			isCitizen = true; // a registered user, loaded from the userfile
			name = node.SelectSingleNode("name/text()").Value;
			password = node.SelectSingleNode("password/text()").Value;
			hostmasks = HostmaskCollection.UnpackHostmasks( node.SelectNodes("hostmasks/hostmask") );
			customSettings = CustomSettingCollection.UnpackSettings( node.SelectNodes("custom/*"), saveUsers );
//			this.mdl = mdl;
		}

		/// <summary>
		/// Returns an xml representation of the user
		/// </summary>
		/// <returns></returns>
		public string ToXmlString() 
		{
			string xml = "<user>";
			xml += "<name>"+name+"</name>";
			xml += "<password>"+password+"</password>";
			xml += hostmasks.ToXmlString();
			xml += customSettings.ToXmlString();
			return xml;
		}

		/// <summary>
		/// Saves all users
		/// </summary>
		public void Save() 
		{
			saveUsers();
		}

	}

	/// <summary>
	/// A collecton of known users
	/// </summary>
	public class UserCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UserCollection"/> class.
		/// </summary>
		/// <param name="saveUsers">The saveusers delgate.</param>
		public UserCollection(SaveUsersDelegate saveUsers) 
		{
			this.saveUsers = saveUsers;
		}
		/// <summary>
		/// Delegate for saving the users.xml
		/// </summary>
		public delegate void SaveUsersDelegate();
		SaveUsersDelegate saveUsers;

		/// <summary>
		/// Unpacks the users.
		/// </summary>
		/// <param name="usrs">The usrs.</param>
		/// <param name="saveUsers">The save users.</param>
		/// <returns></returns>
		internal static UserCollection UnpackUsers( XmlNodeList usrs, SaveUsersDelegate saveUsers ) 
		{
			UserCollection usrcol = new UserCollection(saveUsers);
			for (int i=0; i<usrs.Count; i++) 
			{
				Console.WriteLine("Unpacking a user");
				usrcol.Add( usrs[i], saveUsers );
			}
			return usrcol;
		}

		/// <summary>
		/// Adds the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public void Add(User user) 
		{
			List.Add(user);
		}

		/// <summary>
		/// Adds the specified user node.
		/// </summary>
		/// <param name="userNode">The user node.</param>
		/// <param name="saveUsers">The delegate for saving the users file.</param>
		internal void Add( XmlNode userNode, SaveUsersDelegate saveUsers ) 
		{
			Add( new User( userNode, saveUsers ) );
		}

		/// <summary>
		/// Removes the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public void Remove(User user) 
		{
			List.Remove(user);
		}

		/// <summary>
		/// Gets or sets the <see cref="User"/> with the specified i.
		/// </summary>
		/// <value></value>
		public User this[int i] 
		{
			get { return (User)List[i]; }
			set { List[i] = value; }
		}

		/// <summary>
		/// Gets the user by name
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
		/// <returns></returns>
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

		/// <summary>
		/// Gets the user by host match.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Returns an xml representation of the collection.
		/// </summary>
		/// <returns></returns>
		public string ToXmlString() 
		{
			string xml = "<users>";

			for(int i=0; i<Count; i++) if (this[i].IsCitizen) xml += this[i].ToXmlString(); // only save citizens
			xml += "</users>";
			return xml;
		}

		/// <summary>
		/// Saves the usercollection.
		/// </summary>
		public void Save() 
		{
			saveUsers();
		}
	}
}
