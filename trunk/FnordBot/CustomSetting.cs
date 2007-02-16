using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace NielsRask.FnordBot
{
	/// <summary>
	/// This class holds an item of custom data for users, such as game scores
	/// </summary>
	public class CustomSetting 
	{
		string name;
		NameValueCollection items;

		/// <summary>
		/// Name of this customsetting section
		/// </summary>
		public string Name 
		{
			get { return name; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CustomSetting"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public CustomSetting( string name ) 
		{
			this.name = name;
			items =	new NameValueCollection();
		}

		internal CustomSetting( XmlNode node ) 
		{
			name = node.Name;
			items =	new NameValueCollection();
			for (int i=0; i<node.ChildNodes.Count; i++) 
			{
				items.Add( node.ChildNodes[i].Name, node.ChildNodes[i].FirstChild.Value );
				Console.WriteLine("found key: "+node.ChildNodes[i].Name+" = "+node.ChildNodes[i].FirstChild.Value);

			}
			Console.WriteLine("found setting: "+name);
		}

		/// <summary>
		/// Gets the value of a specific setting name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public string GetValue(string name) 
		{
			return items.Get( name );
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public void SetValue(string name, string value) 
		{
			items.Set( name, value );
		}

		/// <summary>
		/// returns the xml representation of this customsetting
		/// </summary>
		/// <returns></returns>
		public string ToXmlString() 
		{
			string xml = "<"+name+">";
			for (int i=0; i<items.Keys.Count; i++) 
			{
				string key = items.Keys[i];
				xml += "<"+key+">"+items[key]+"</"+key+">";
			}
			xml += "</"+name+">";
			return xml;
		}
	}

	/// <summary>
	/// Represents all the custom settings of a user
	/// </summary>
	public class CustomSettingCollection : CollectionBase
	{
		UserCollection.SaveUsersDelegate saveUsers;
		internal CustomSettingCollection( UserCollection.SaveUsersDelegate saveUsers ) 
		{
			this.saveUsers = saveUsers;
		}

		internal static CustomSettingCollection UnpackSettings( XmlNodeList settings, UserCollection.SaveUsersDelegate saveUsers) 
		{
			CustomSettingCollection cstmcol = new CustomSettingCollection( saveUsers );
			for (int i=0; i<settings.Count; i++) 
			{
				Console.WriteLine("Unpacking a setting");

				cstmcol.Add( settings[i] );
			}
			return cstmcol;
		}

		/// <summary>
		/// Adds the specified setting.
		/// </summary>
		/// <param name="setting">The setting.</param>
		public void Add(CustomSetting setting) 
		{
			List.Add(setting);
		}

		/// <summary>
		/// Sets the custom value.
		/// </summary>
		/// <param name="module">The module.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void SetCustomValue( string module, string key, string value ) 
		{
			CustomSetting cstm = new CustomSetting( module );
			cstm.SetValue( key, value );
			Add( cstm );
		}

		/// <summary>
		/// Gets the custom value.
		/// </summary>
		/// <param name="module">The module.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public string GetCustomValue( string module, string key ) 
		{
			CustomSetting cstm = GetByName( module );
			if (cstm != null) return cstm.GetValue( key );
			else return "";
		}

		internal void Add( XmlNode node ) 
		{
			Add( new CustomSetting( node ) );
		}

		/// <summary>
		/// Removes the specified setting.
		/// </summary>
		/// <param name="setting">The setting.</param>
		public void Remove(CustomSetting setting) 
		{
			List.Remove(setting);
		}

		/// <summary>
		/// Gets or sets the <see cref="CustomSetting"/> with the specified i.
		/// </summary>
		/// <value></value>
		public CustomSetting this[int i] 
		{
			get { return (CustomSetting)List[i]; }
			set { List[i] = value; }
		}

		/// <summary>
		/// Gets a customsetting section by name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public CustomSetting GetByName(string name) 
		{
			bool found = false;
			int i = 0;
			while ( !found && i<Count) 
			{
				if ( string.Compare(this[i].Name, name, true) == 0 )
				{
					found = true;
				} 
				else 
				{
					i++;
				}
			}
			if (found) 
			{
				return this[i];
			} 
			else 
			{
				return null;
			}
		}

		/// <summary>
		/// Returns an xml representtion of this customsettingcollection
		/// </summary>
		/// <returns></returns>
		public string ToXmlString() 
		{
			string xml = "<custom>";
			for(int i=0; i<Count; i++) 
			{
				xml += this[i].ToXmlString();
			}
			xml += "</custom>";
			return xml;
		}

		/// <summary>
		/// Saves the settings
		/// </summary>
		public void Save() 
		{
			saveUsers();
		}
	}

}
