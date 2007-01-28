using System;
using NUnit.Framework;

namespace NielsRask.FnordBot.Users
{
	/// <summary>
	/// Summary description for Test.
	/// </summary>
	[TestFixture]
	public class Test
	{
		public Test()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		Module mdl;

		[TestFixtureSetUp]
		public void Setup() 
		{
			mdl = new Module( "..\\..\\users.xml" );
		}


		[Test]
		public void TestUserCount() 
		{
			Assert.IsTrue( mdl.Users.Count > 0, "count is "+mdl.Users.Count );
			Console.WriteLine( mdl.Users.ToXmlString() );
		}

		[Test]
		public void TestHostMatch() 
		{
			Assert.IsNotNull( mdl.Users.GetByHostMatch("foo@bar.dk") );
			Assert.IsNotNull( mdl.Users.GetByHostMatch("~foo@bar.dk") );
			Assert.IsNull( mdl.Users.GetByHostMatch("foo@bar.de") );

		}
	}
}
