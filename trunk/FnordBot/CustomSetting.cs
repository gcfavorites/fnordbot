using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace NielsRask.FnordBot.Users
{
	public class CustomSetting 
	{
		string name;
		NameValueCollection items;

		public string Name 
		{
			get { return name; }
		}

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

		public string GetValue(string name) 
		{
			return items.Get( name );
		}

		public void SetValue(string name, string value) 
		{
			items.Set( name, value );
		}

		public string ToXmlString() 
		{
			string xml = "<"+name+">";
			for (int i=0; i<items.Keys.Count; i++) 
			{
				string key = items.Keys[i];
				xml += "<"+key+">"+items[key]+"</"+key+">";
			}
			xml += "<"+name+">";
			return xml;
		}
	}

	public class CustomSettingCollection : CollectionBase
	{
		Module mdl;
		internal CustomSettingCollection( Module mdl ) 
		{
			this.mdl = mdl;
		}

		internal static CustomSettingCollection UnpackSettings( XmlNodeList settings, Module mdl) 
		{
			CustomSettingCollection cstmcol = new CustomSettingCollection( mdl );
			for (int i=0; i<settings.Count; i++) 
			{
				Console.WriteLine("Unpacking a setting");

				cstmcol.Add( settings[i] );
			}
			return cstmcol;
		}

		public void Add(CustomSetting setting) 
		{
			List.Add(setting);
		}

		public void SetCustomValue( string module, string key, string value ) 
		{
			CustomSetting cstm = new CustomSetting( module );
			cstm.SetValue( key, value );
			Add( cstm );
		}

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

		public void Remove(CustomSetting setting) 
		{
			List.Remove(setting);
		}

		public CustomSetting this[int i] 
		{
			get { return (CustomSetting)List[i]; }
			set { List[i] = value; }
		}

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

		public void Save() 
		{
			if (mdl!= null) mdl.Save();
		}
	}

}
