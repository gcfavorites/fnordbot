using System;
using System.Collections;

namespace NielsRask.SortSnak
{
	/// <summary>
	/// Summary description for StringQueue.
	/// </summary>
	public class StringQueue : Queue 
	{
		Vocabulary vocab;
		public StringQueue(int i, Vocabulary vocab) : base(i) 
		{
			this.vocab = vocab; 
		}

		public void Enqueue(string str) 
		{
			base.Enqueue(str);
		}
		public void CreateFragment(bool canStart, bool canTerminate) 
		{
			object[] arr = base.ToArray();
			if (arr[0] == null || arr[1] == null || arr[2] == null || ((string)arr[0]).Length == 0 || ((string)arr[1]).Length == 0 || ((string)arr[2]).Length == 0 ) 
			{
				throw new Exception("forsøg på at indsætte null-fragment!");
			} 
			else 
			{
				vocab.AddFragment( (string)arr[0],(string)arr[1],(string)arr[2], canStart, canTerminate ) ;
			}
			//			object[] arr = base.ToArray();
			//			return new Triplet(, canStart,canTerminate);
		}
	}
}
