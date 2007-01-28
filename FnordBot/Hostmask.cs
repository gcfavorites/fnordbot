using System;
using System.Collections;
using System.Xml;

namespace NielsRask.FnordBot.Users
{
	public class Hostmask 
	{

		string mask;

		public string Mask 
		{
			get  { return mask; }
		}

		public Hostmask( string mask ) 
		{
			this.mask = mask;
		}

		internal Hostmask( XmlNode node ) 
		{
			mask = node.SelectSingleNode("./text()").Value;
		}

		public bool IsMatch(string host, bool exact, bool caseSensitive) 
		{
			if (exact) 
			{
				return string.Compare(mask, host, caseSensitive) == 0;
			} 
			else 
			{
				return wildcmp(mask, host, caseSensitive);
			}
		}

		public string ToXmlString() 
		{
			return "<hostmask>"+mask+"</hostmask>";
		}


		private bool wildcmp(string wild, string str, bool case_sensitive)
		{
			int cp=0, mp=0;
	
			int i=0;
			int j=0;

			if (! case_sensitive)
			{
				wild = wild.ToLower();
				str = str.ToLower();
			}
			
			while (i < str.Length && j < wild.Length && wild[j] != '*')
			{
				if ((wild[j] != str[i]) && (wild[j] != '?')) 
				{
					return false;
				}
				i++;
				j++;
			}
		
			while (i<str.Length) 
			{
				if (j<wild.Length && wild[j] == '*') 
				{
					if ((j++)>=wild.Length) 
					{
						return true;
					}
					mp = j;
					cp = i+1;
				} 
				else if (j<wild.Length && (wild[j] == str[i] || wild[j] == '?')) 
				{
					j++;
					i++;
				} 
				else 
				{
					j = mp;
					i = cp++;
				}
			}
		
//			while (j
			while (j < wild.Length && wild[j] == '*')
			{
				j++;
			}
			return j>=wild.Length;
		}
	}

	public class HostmaskCollection : CollectionBase
	{
		public HostmaskCollection() {}

		internal static HostmaskCollection UnpackHostmasks( XmlNodeList masks ) 
		{
			HostmaskCollection mskcol = new HostmaskCollection();
			for (int i=0; i<masks.Count; i++) 
			{
				Console.WriteLine("Unpacking a hostmask");

				mskcol.Add( masks[i] );
			}
			return mskcol;
		}

		public void Add(Hostmask mask) 
		{
			List.Add(mask);
		}

		internal void Add( XmlNode node ) 
		{
			Add( new Hostmask( node ) );
		}

		public void Remove(Hostmask mask) 
		{
			List.Remove(mask);
		}

		public Hostmask this[int i] 
		{
			get { return (Hostmask)List[i]; }
			set { List[i] = value; }
		}

		public bool IsMatch(string host, bool exact, bool caseSensitive) 
		{
			bool found = false;
			int i = 0;
			while ( !found && i<Count) 
			{
				if ( this[i].IsMatch(host, exact, caseSensitive) )
				{
					found = true;
				} 
				else 
				{
					i++;
				}
			}
			return found;
		}
		
		public string ToXmlString() 
		{
			string xml = "<hostmasks>";
			for(int i=0; i<Count; i++) 
			{
				xml += this[i].ToXmlString();
			}
			xml += "</hostmasks>";
			return xml;
		}
	}


}
