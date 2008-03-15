using System;
using System.Collections;
using System.Xml;
using System.Text;

namespace NielsRask.FnordBot
{
	/// <summary>
	/// This class represents a hostmask of a user
	/// </summary>
	public class Hostmask 
	{
		string mask;

		/// <summary>
		/// Gets the mask.
		/// </summary>
		/// <value>The mask.</value>
		public string Mask 
		{
			get  { return mask; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Hostmask"/> class.
		/// </summary>
		/// <param name="mask">The mask.</param>
		public Hostmask( string mask ) 
		{
			this.mask = mask;
		}

		internal Hostmask( XmlNode node ) 
		{
			mask = node.SelectSingleNode("./text()").Value;
		}

		/// <summary>
		/// Determines whether the specified host is match.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="exact">if set to <c>true</c> [exact].</param>
		/// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
		/// <returns>
		/// 	<c>true</c> if the specified host is match; otherwise, <c>false</c>.
		/// </returns>
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

		/// <summary>
		/// Returns this hostmask as xml
		/// </summary>
		/// <returns></returns>
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

	/// <summary>
	/// A collection of hostmasks
	/// </summary>
	public class HostmaskCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HostmaskCollection"/> class.
		/// </summary>
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

		/// <summary>
		/// Adds the specified mask.
		/// </summary>
		/// <param name="mask">The mask.</param>
		public void Add(Hostmask mask) 
		{
			List.Add(mask);
		}

		internal void Add( XmlNode node ) 
		{
			Add( new Hostmask( node ) );
		}

		/// <summary>
		/// Removes the specified mask.
		/// </summary>
		/// <param name="mask">The mask.</param>
		public void Remove(Hostmask mask) 
		{
			List.Remove(mask);
		}

		/// <summary>
		/// Gets or sets the <see cref="Hostmask"/> with the specified i.
		/// </summary>
		/// <value></value>
		public Hostmask this[int i] 
		{
			get { return (Hostmask)List[i]; }
			set { List[i] = value; }
		}

		/// <summary>
		/// Determines whether the specified host is match.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="exact">if set to <c>true</c> [exact].</param>
		/// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
		/// <returns>
		/// 	<c>true</c> if the specified host is match; otherwise, <c>false</c>.
		/// </returns>
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
		
		/// <summary>
		/// Returns this hostmaskcollection as xml
		/// </summary>
		/// <returns></returns>
		public string ToXmlString() 
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(" <hostmasks> ");
			for(int i=0; i<Count; i++) 
			{
				sb.Append( this[i].ToXmlString() );
			}
			sb.Append( "</hostmasks>" );
			return sb.ToString();
		}
	}


}
