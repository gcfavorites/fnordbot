using System;
using System.Collections;

namespace NielsRask.SortSnak
{
	/// <summary>
	/// ordforråddet. tre collections for hurtig lookup
	/// </summary>
	public class Vocabulary 
	{
		private FragmentList prevSortedFragments;
		private FragmentList centerSortedFragments;
		private FragmentList nextSortedFragments;
		private FragmentList canStartFragments;
		Random rnd;


		public FragmentList PrevSortedFragments 
		{
			get { return prevSortedFragments; }
		}
		public FragmentList CenterSortedFragments 
		{
			get { return centerSortedFragments; }
		}
		public FragmentList NextSortedFragments 
		{
			get { return nextSortedFragments; }
		}

		public int MinimumOverlap 
		{
			get { return centerSortedFragments.MinimumOverlap; }
			set 
			{ 
				prevSortedFragments.MinimumOverlap = value; 
				centerSortedFragments.MinimumOverlap = value; 
				nextSortedFragments.MinimumOverlap = value; 
			}
		}

		public int SimpleMatchChance 
		{
			get { return centerSortedFragments.SimpleMatchChance; }
			set 
			{ 
				prevSortedFragments.SimpleMatchChance = value; 
				centerSortedFragments.SimpleMatchChance = value; 
				nextSortedFragments.SimpleMatchChance = value; 
			}		
		}

		public int AmbientSimpleMatchChance 
		{
			get { return centerSortedFragments.AmbientSimpleMatchChance; }
			set 
			{ 
				prevSortedFragments.AmbientSimpleMatchChance = value; 
				centerSortedFragments.AmbientSimpleMatchChance = value; 
				nextSortedFragments.AmbientSimpleMatchChance = value; 
			}		
		}

		private WordList wordlist;

		public WordList Words 
		{
			get{ return wordlist; }
		}

		internal Vocabulary() 
		{
			prevSortedFragments = new FragmentList( new FragmentComparer( SortWord.PreviousWord ) );
			centerSortedFragments = new FragmentList( new FragmentComparer( SortWord.CurrentWord  ) );
			nextSortedFragments = new FragmentList( new FragmentComparer( SortWord.NextWord ) );
			canStartFragments = new FragmentList( null);
			wordlist = new WordList();
			rnd = new Random();
		}

		public Fragment GetRandomStartFragment() 
		{
			return canStartFragments[rnd.Next(canStartFragments.Count)];
		}

		public Fragment GetNextFragment(Fragment frag) 
		{
			FragmentList list = prevSortedFragments.GetSomeNextFragments(frag);
			return list[rnd.Next(list.Count)];
		}

		public Fragment GetPreviousFragment(Fragment frag) 
		{
			FragmentList list = nextSortedFragments.GetSomePreviousFragments(frag);
			return list[rnd.Next(list.Count)];
		}

		public bool KnowsWord(string word) 
		{
			return wordlist.ContainsWord(word);
		}

		public Fragment GetFragmentByWord(string word) 
		{
			wordlist.Add(word);
			FragmentList list = centerSortedFragments.GetSomeFragmentsByWord( new Fragment(new Word(""),new Word(word),new Word(""),false,false) ); // dummy-fragment
//			FragmentList list = centerSortedFragments.GetSomeFragmentsByWord( new Fragment(wordlist[""],wordlist[word],wordlist[""],false,false) ); // dummy-fragment
			if (list.Count > 0) return list[rnd.Next(list.Count)];
			else 
			{
				Console.WriteLine("fandt ikke '"+word+"' i ordlisten");
				return null;
			}
		}

		public void AddFragment(string prev, string word, string next, bool canStart, bool canEnd) 
		{
			wordlist.Add(prev);
			wordlist.Add(word);
			wordlist.Add(next);

			Word wp = wordlist[prev];
			Word w = wordlist[word];
			Word wn = wordlist[next];

			if (wp == null || w == null || wn == null ) 
			{
				throw new Exception("addfragment fandt ikke word!");
			}

			Fragment frg = new Fragment(wp,w,wn,canStart, canEnd);
			prevSortedFragments.Add(frg);
			centerSortedFragments.Add(frg);
			nextSortedFragments.Add(frg);
			if (canStart) canStartFragments.Add(frg);
		}

		private void AddFragment(Fragment frag) 
		{
			prevSortedFragments.Add(frag);
			centerSortedFragments.Add(frag);
			nextSortedFragments.Add(frag);
		}
	}
}
