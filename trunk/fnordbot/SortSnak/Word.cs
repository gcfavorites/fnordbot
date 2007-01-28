using System;
using System.Collections;

namespace NielsRask.SortSnak
{
	/// <summary>
	/// indeholder et ord
	/// </summary>
	public class Word 
	{
		private string val;
		private int score = 1;

		public string Value 
		{
			get { return val;}//+"("+score+")"; }
			set { val = value; } 
		}

		public Word (string value) 
		{
			val = value;
		}

		public int Score 
		{
			get {return score;}
		}

		public void AddHit() 
		{ 
			score++; 
		}

	}

	/// <summary>
	/// liste af words, f.eks hashtable
	/// </summary>
	public class WordList : Hashtable
	{
		public void Add(string word) 
		{
			if ( !base.ContainsKey(word) ) 
			{
				base.Add( word, new Word(word) );
			}
			this[word].AddHit();
		}

		public bool ContainsWord(string word) 
		{
			return base.ContainsKey(word);
		}

		public Word this[string word] 
		{
			get 
			{
				return (Word)base[word];
			}
		}
	}
}
