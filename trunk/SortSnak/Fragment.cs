using System;
using System.Collections;
using System.Diagnostics;

namespace NielsRask.SortSnak
{
	/// <summary>
	/// en triplet af 3 words
	/// </summary>
	public class Fragment 
	{
		Word prevWord;
		Word thisWord;
		Word nextWord;
		bool canEnd = false;
		bool canStart = false;

		public Fragment(Word pw, Word cw, Word nw, bool canStart, bool canEnd) 
		{
			if (pw == null || cw == null || nw == null)
			{
				Console.WriteLine("*** forsøg på at oprette fragment med null-værdi!! unwinding the stack:");

				StackTrace st = new StackTrace(true);
				string indent = "";
				for(int i =0; i< st.FrameCount; i++ )
				{
					StackFrame sf = st.GetFrame(i);
					if (sf.GetFileName() != "") 
					{
						Console.WriteLine(indent+" Method: {0} File: {1} : {2}",sf.GetMethod(),sf.GetFileName(), sf.GetFileLineNumber() );
						indent += "*";
					}
				}
			}
			prevWord = pw;
			thisWord = cw;
			nextWord = nw;
			this.canEnd = canEnd;
			this.canStart = canStart;
		}

		public Word PrevWord 
		{
			get { return prevWord; }
			set { prevWord = value; }
		}
		public Word ThisWord 
		{
			get { return thisWord; }
			set { thisWord = value; }
		}
		public Word NextWord 
		{
			get { return nextWord; }
			set { nextWord = value; }
		}
		public bool CanEnd 
		{
			get { return canEnd; }
		}
		public bool CanStart 
		{
			get { return canStart; }
		}

		public override string ToString()
		{
			return "["+prevWord.Value+"¤"+thisWord.Value+"¤"+nextWord.Value+"¤"+CanStart+"¤"+CanEnd+"]";
		}

	}

	/// <summary>
	/// collection af fragmenter, sorteret
	/// </summary>
	public class FragmentList 
	{
		ArrayList list;
		IComparer cmp;
		Random rnd;
		int lowerBound = 3; // hvis ikke der findes så mange overlaps, prøver vi at finde en simple-next
		int simpleChance = 35; // procent chance for at vi enabler simple-motoren hvis lowerbound ikke er mødt
		int ambientSimpleChance = 10; // chance der altid er for at få simple-options med

		/// <summary>
		/// we need at least this many fragments to choose from, to use normal 2-word matching.
		/// </summary>
		public int MinimumOverlap 
		{
			get { return lowerBound; }
			set { lowerBound = value; }
		}

		/// <summary>
		/// the chance, in % that we use simple (1-word) matching if MinimumOverlap isnt met
		/// </summary>
		public int SimpleMatchChance 
		{
			get { return simpleChance; }
			set { simpleChance = value; }
		}

		/// <summary>
		/// The chance, in %, that we use simple (1-word) matching for a given match
		/// </summary>
		/// <remarks>This property serves to inject some real randomness in the generated strings</remarks>
		public int AmbientSimpleMatchChance 
		{
			get { return ambientSimpleChance; }
			set { ambientSimpleChance = value; }
		}

		public FragmentList(IComparer cmp) 
		{
			list = new ArrayList();
			this.cmp = cmp;
			rnd = new Random();
		}

		public IEnumerator GetEnumerator(int index, int count) 
		{
			return list.GetEnumerator(index,count);
		}
		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator();
		}

		public void Add(Fragment frag) 
		{
			if (cmp != null) 
			{
				int i = list.BinarySearch(frag,cmp);
				if (i < 0) // indsæt på korrekt plads, hvis den ikke findes
				{
//					Console.WriteLine("binsearch-add: indsat ("+i+") "+frag.ToString());
					list.Insert(~i,frag);
				} 
				else 
				{
//					Console.WriteLine("binsearch-add: kunne ikke indsætte ("+i+") "+frag.ToString());
				}
			} 
			else 
			{
				list.Add(frag);
			}
		}

		public FragmentList GetSomeFragmentsByWord(Fragment frag) 
		{
			MatchComparer mc = new MatchComparer();
//			
			FragmentList lst = new FragmentList(null);

			int i = list.BinarySearch( frag, mc );
			if (i < 0) Console.WriteLine("GetSomeFragmentsByWord fandt ikke ord, binsearch returnerede "+i+" ("+~i+")");
			while (i>1 && mc.Compare(this[i-1], frag)==0) i--; 

			while(i>0 && mc.Compare(this[i],frag) == 0) 
			{
				lst.Add(this[i]);
				i++;
			}
			return lst;
		}

		public FragmentList GetSomePreviousFragments(Fragment frag) 
		{
			FragmentList lst = new FragmentList(null);
			OverlapComparer olc = new OverlapComparer(true);
			int i = list.BinarySearch(frag, olc);
			if (i<0) throw new Exception("tilsyneladende blev "+frag+" ikke fundet i denne liste");
			int o = ~i;
			while (i > 1 && olc.Compare(this[i-1],frag) == 0 ) i--; 

			while(olc.Compare(this[i],frag) == 0) 
			{
				lst.Add(this[i]);
				i++;
			}
			if ( (lst.Count < lowerBound && (simpleChance-1 >= rnd.Next(100))) || rnd.Next(100)<ambientSimpleChance ) // 5% chance for spring 
			{
				NextComparer ncmp = new NextComparer(true);
				int j = list.BinarySearch(frag, ncmp);
				while (j > 1 && ncmp.Compare(this[j-1],frag) == 0 )  j--;
				while(ncmp.Compare(this[j],frag) == 0) 
				{
					lst.Add(this[j]);
					j++;
				}
			}
			return lst;
		}

		// w1 w2 w3
		//    w1 w2 w3
		public FragmentList GetSomeNextFragments(Fragment frag) 
		{
			FragmentList lst = new FragmentList(null);
			OverlapComparer olc = new OverlapComparer(false);
			int i = list.BinarySearch(frag, olc);
			if (i<0) throw new Exception("tilsyneladende blev "+frag+" ikke fundet i denne liste");
			//			Console.WriteLine("fandt next: "+list[i]);
			int o = ~i;
			while (i > 1 && olc.Compare(this[i-1],frag) == 0 )  
			{
				//				Console.WriteLine("backtrack!");
				i--; // ?
			}

			while(olc.Compare(this[i],frag) == 0) 
			{
				lst.Add(this[i]);
				i++;
			}
			if ( (lst.Count < lowerBound && (simpleChance-1 >= rnd.Next(100))) || rnd.Next(100)<ambientSimpleChance ) // 5% chance for spring 
			{
//				Console.Write("enabling simple-compare for "+frag.ToString()+" - it only had "+lst.Count+" nextfragments...");
				NextComparer ncmp = new NextComparer(false);
				int j = list.BinarySearch(frag, ncmp);
				while (j > 1 && ncmp.Compare(this[j-1],frag) == 0 )  j--;
				while(ncmp.Compare(this[j],frag) == 0) 
				{
					lst.Add(this[j]);
					j++;
				}
//				Console.WriteLine(" now it has "+lst.Count+" to choose from");
			}
			return lst;
		}

//		public FragmentList GetSomeNextFragments(Fragment frag) 
//		{
//			bool simple = true;
//			FragmentList lst = new FragmentList(null);
//			if (simple)
//			{
//				NextComparer ncmp = new NextComparer(false);
//				int i = list.BinarySearch(frag, ncmp);
//				while (i > 1 && ncmp.Compare(this[i-1],frag) == 0 )  i--;
//				while(ncmp.Compare(this[i],frag) == 0) 
//				{
//					lst.Add(this[i]);
//					i++;
//				}
//			} 
//			else 
//			{
//				//			Console.WriteLine("getsomenext("+frag+") looking in list of "+Count);
//				OverlapComparer olc = new OverlapComparer(false);
//				int i = list.BinarySearch(frag, olc);
//				if (i<0) throw new Exception("tilsyneladende blev "+frag+" ikke fundet i denne liste");
//				//			Console.WriteLine("fandt next: "+list[i]);
//				int o = ~i;
//				while (i > 1 && olc.Compare(this[i-1],frag) == 0 )  
//				{
//					//				Console.WriteLine("backtrack!");
//					i--; // ?
//				}
//
//				while(olc.Compare(this[i],frag) == 0) 
//				{
//					lst.Add(this[i]);
//					i++;
//				}
//			}
//			return lst;
//		}

		public int BinarySearch(Fragment frag, IComparer cmp) 
		{
			return list.BinarySearch(frag,cmp);
		}

		/*
		 * 			OverlapComparer ocmp = new OverlapComparer();
			int i = mainList.BinarySearch(trp,ocmp);
//			Console.WriteLine("binsearch found "+i+" ("+~i+") ");
//			//i = ~i;
//			Console.WriteLine("-> value ("+this[i].ToString()+")");
			while (ocmp.Compare(trp,this[i-1]) == 0) i--;
			//i++;
			ArrayList lst = new ArrayList();
//			Console.WriteLine("sweeping from index "+i+" ("+this[i].ToString()+")");
//			Console.WriteLine("comparing "+trp.ToString());
//			Console.WriteLine("with      "+this[i].ToString());
//			Console.WriteLine("overlap compare returns "+ocmp.Compare(this[i],trp));
			while(ocmp.Compare(this[i],trp) == 0) {
//				Console.WriteLine("overlap match at index "+i);
				lst.Add(this[i]);
				i++;
			}
//			Console.WriteLine("found "+lst.Count+" randoms");
			return (Triplet)lst[rnd.Next(lst.Count)];
		 * */

		public Fragment this[int index] 
		{
			get 
			{ 
				try 
				{
					return (Fragment)list[index]; 								  
				} 
				catch (Exception e) 
				{
					Console.WriteLine("fragment["+index+"]: "+e.ToString());
					throw;
				}
			}
			set { list[index] = value; }
		}
		
		public int Count
		{
			get { return list.Count; }
		}

		public void Show() 
		{
			foreach(object ofrag in list) 
			{
				Fragment frag = (Fragment)ofrag;
				Console.WriteLine( frag.ToString() );
			}
		}
	}
}
