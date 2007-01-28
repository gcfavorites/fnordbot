using System;
using System.Collections;

namespace NielsRask.SortSnak
{
	/// <summary>
	/// Denne comparer kigger på unikhed af eks prevword. brug den til previousword-collections?
	/// </summary>
	public class FragmentComparer : IComparer 
	{
		private SortWord sortWord; 
		public FragmentComparer( SortWord sortWord ) 
		{
			this.sortWord = sortWord;
		}

		public int Compare(object obj1, object obj2) 
		{
			return Compare(obj1 as Fragment, obj2 as Fragment);
		}

		public int Compare(Fragment frag1, Fragment frag2) 
		{
			int i = 0;
			switch(sortWord) 
			{
				case SortWord.PreviousWord:
					i = string.Compare(frag1.PrevWord.Value, frag2.PrevWord.Value, true);
					if (i == 0) i = string.Compare(frag1.ThisWord.Value, frag2.ThisWord.Value, true);
					if (i == 0) i = string.Compare(frag1.NextWord.Value, frag2.NextWord.Value, true);
					break;
				case SortWord.CurrentWord:
					i = string.Compare(frag1.ThisWord.Value, frag2.ThisWord.Value, true);
					if (i == 0) i = string.Compare(frag1.NextWord.Value, frag2.NextWord.Value, true);
					if (i == 0) i = string.Compare(frag1.PrevWord.Value, frag2.PrevWord.Value, true);
					break;
				case SortWord.NextWord:
					i = string.Compare(frag1.NextWord.Value, frag2.NextWord.Value, true);
					if (i == 0) i = string.Compare(frag1.ThisWord.Value, frag2.ThisWord.Value, true);
					if (i == 0) i = string.Compare(frag1.PrevWord.Value, frag2.PrevWord.Value, true);
					break;
			}
			return i;

		}
	}

	public class MatchComparer : IComparer 
	{
		public int Compare(object x, object y)
		{
			Fragment frg1 = (Fragment)x;
			Fragment frg2 = (Fragment)y;
//			if (x==null)
//			{
//				Console.WriteLine("x = null");
//				return -1;
//			}
//			if (y==null) 
//			{
//				Console.WriteLine("y = null");
//				return 1;
//			}
//			Console.WriteLine("hverken x eller y er null: "+frg1.ToString()+" - "+frg2.ToString());
			return Compare(frg1,frg2);
		}

		public int Compare(Fragment f1, Fragment f2) 
		{
			try 
			{
				return string.Compare(f1.ThisWord.Value, f2.ThisWord.Value, true);
			} 
			catch (Exception e) {
				Console.WriteLine("matchcomparer: "+e.ToString());   
				throw;
			}
		}
	}

	public class NextComparer : IComparer
	{
		bool reverse = false;
		public NextComparer() {;}
		public NextComparer(bool reverse) 
		{
			this.reverse = reverse;
		}
		public int Compare(object x, object y)
		{
			Fragment frg1 = (Fragment)x;
			Fragment frg2 = (Fragment)y;
			if (reverse) return CompareReverse(frg1,frg2);
			else return CompareForward(frg1, frg2);
		}

		public int CompareForward(Fragment f1, Fragment f2) // der søges på f2 ...
		{
			return string.Compare(f1.PrevWord.Value, f2.ThisWord.Value, true);
			// reverse? string.Compare(f2.ThisWord.Value, f1.NextWord.Value, true);
		}
		public int CompareReverse(Fragment f1, Fragment f2) // der søges på f2 ...
		{
			return string.Compare(f1.NextWord.Value, f2.ThisWord.Value, true);
		}

	}


	public class OverlapComparer : IComparer 
	{
		private bool reverse = false;
		public OverlapComparer() {}
		public OverlapComparer(bool reverse) 
		{
			this.reverse = reverse;
		}

		// til forward compare skal listen være sorteret på previous
		// til reverse compare skal listen være sorteret på nextword
		public int Compare(object x, object y) 
		{
//			Console.WriteLine("x er type "+x.GetType().ToString());
//			Console.WriteLine("y er type "+y.GetType().ToString());
			Fragment frg1 = (Fragment)x;
			Fragment frg2 = (Fragment)y;
			if (reverse) return CompareReverse(frg1,frg2);
			else return CompareForward(frg1, frg2);
		}
		// 1: w1 w2 w3		denne er mindre end trp2
		// 2:    w1 w2 w3	denne er større end trp1

		public int CompareReverse(Fragment f1, Fragment f2) // der søges på f2 ...
		{
			int i = string.Compare(f2.ThisWord.Value, f1.NextWord.Value, true);
//			Console.WriteLine("stage 1 compare '"+f2.ThisWord.Value+"','"+f1.NextWord.Value+"' yields "+i);
			if ( i == 0 ) 
			{
				i = string.Compare(f2.PrevWord.Value, f1.ThisWord.Value, true);
//				Console.WriteLine("stage 2 compare '"+f2.PrevWord.Value+"','"+f1.ThisWord.Value+"' yields "+i);
			} 
//			Console.WriteLine("frg1: "+f1.ToString()+" <-> frg2: "+f2.ToString()+" => "+i);
			return -1*i;
		}

		public int CompareForward(Fragment f1, Fragment f2) 
		{
			int i = string.Compare(f2.ThisWord.Value, f1.PrevWord.Value, true);
//			Console.WriteLine("stage 1 compare '"+f2.ThisWord.Value+"','"+f1.PrevWord.Value+"' yields "+i);
			if ( i == 0 ) 
			{
				i = string.Compare(f2.NextWord.Value, f1.ThisWord.Value, true);
//				Console.WriteLine("stage 2 compare '"+f2.NextWord.Value+"','"+f1.ThisWord.Value+"' yields "+i);
			} 
//			Console.WriteLine("frg1: "+f1.ToString()+" <-> frg2: "+f2.ToString()+" => "+i);
			return -1*i;
		}



//		public int CompareForward(Fragment f2, Fragment f1) burde virke
//		{
//			try 
//			{
//				int i = string.Compare(f1.ThisWord.Value, f2.PrevWord.Value, true);
//				Console.WriteLine("stage 1 compare '"+f1.ThisWord.Value+"','"+f2.PrevWord.Value+"' yields "+i);
//				if ( i == 0 ) 
//				{
//					i = string.Compare(f1.NextWord.Value, f2.ThisWord.Value, true);
//					Console.WriteLine("stage 2 compare '"+f1.NextWord.Value+"','"+f2.ThisWord.Value+"' yields "+i);
//				} 
//				Console.WriteLine("frg1: "+f1.ToString()+" <-> frg2: "+f2.ToString()+" => "+i);
//				return -1*i;
//			} 
//			catch (Exception e) 
//			{
//				Console.WriteLine("exception in Compare():");
//				Console.WriteLine(e.ToString());
//				throw;
//			}
//		}

		/*
		 * 
		 				int i = string.Compare(f1.NextWord.Value, f2.ThisWord.Value, true);
				Console.WriteLine("stage 1 compare '"+f1.NextWord.Value+"','"+f2.ThisWord.Value+"' yields "+i);
				if ( i == 0 ) 
				{
					i = string.Compare(f1.ThisWord.Value, f2.PrevWord.Value, true);
					Console.WriteLine("stage 2 compare '"+f1.ThisWord.Value+"','"+f2.PrevWord.Value+"' yields "+i);
				} 
				Console.WriteLine("frg1: "+f1.ToString()+" <-> frg2: "+f2.ToString()+" => "+i);
				return -1*i;*/

		// trp1: w1 w2 w3		denne er mindre end trp2
		// trp2:    w1 w2 w3	denne er større end trp1
//		public int CompareOld(Fragment frg1, Fragment frg2) 
//		{
//			try 
//			{
//				Console.WriteLine("comparing "+frg2+"");
//				Console.WriteLine("       to "+frg1+" ");
//				//			int i = string.Compare(frg2.ThisWord.Value,frg1.PrevWord.Value);
//				//			if (i == 0) i = string.Compare(frg2.NextWord.Value,frg1.ThisWord.Value);
//
//				//			int i = string.Compare(frg2.PrevWord.Value,frg1.ThisWord.Value);
//				//			if (i == 0) i = string.Compare(frg2.ThisWord.Value,frg1.NextWord.Value);
//
//				int i = string.Compare( frg2.ThisWord.Value, frg1.PrevWord.Value );		// kører baglæns ..
//				if(i==0) i = string.Compare( frg2.NextWord.Value, frg1.ThisWord.Value );
//
//				//			int i = string.Compare( frg1.ThisWord.Value, frg2.NextWord.Value );
//				//			if (i==0) i = string.Compare( frg1.PrevWord.Value, frg2.ThisWord.Value );	
//				if (reverse) i*=-1;
//				Console.WriteLine("compare-resultat: "+i);
//				return i;
//			} 
//			catch (Exception e) 
//			{
//				Console.WriteLine("comparer: "+e.ToString());
//				throw e;
//			}
//			return -1;
//		}
	}

}
