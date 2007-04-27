using System;
using NielsRask.FnordBot;
using System.Collections;
using log4net;

namespace NielsRask.Stat
{
	public class StatPlugin : IPlugin
	{
		NielsRask.FnordBot.FnordBot bot;
		ChannelDictionary wordstat;
		ChannelDictionary userstat;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public StatPlugin()
		{
			wordstat = new ChannelDictionary();
			userstat  = new ChannelDictionary();
		}
		#region IPlugin Members

		public void Attach(NielsRask.FnordBot.FnordBot bot)
		{
			log.Info("Attaching stat plugin");
			try 
			{
				this.bot = bot;
				bot.OnPublicMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
				bot.OnPrivateMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPrivateMessage);
			} 
			catch (Exception e) 
			{
				log.Error("Error in Statplugin.attach", e);
			}
		}

		public void Init(System.Xml.XmlNode pluginNode)
		{
			// TODO:  Add StatPlugin.Init implementation
		}

		#endregion

		private void bot_OnPublicMessage(User user, string channel, string message)
		{
			try 
			{
				if ( !message.StartsWith("!") ) 
				{
					#region wordstat
					string[] parts = message.Split(' ');
					foreach (string part in parts)
						wordstat.Increment(channel, part); // bedre med en rigtig for-løkke
					#endregion

					#region userstat
					userstat.Increment(channel, user.NickName); // tæl linjer. evt en wordcol også
					#endregion
				} 
				else if (message == "!topwords") //eller toptalk{
				{
					log.Info("Listing top words on "+channel+"...");
					string output = "";
					StatCollection scol = wordstat[channel].GetTop(10);
					for (int i=0; i<scol.Count; i++) 
					{
						output += "\u0002"+(i+1)+"\u0002 - ["+scol[i].Key+"]: "+scol[i].Score+"; ";
					}
					bot.SendToChannel(channel, output, true);
				}
				else if (message == "!toptalk") 
				{
					log.Info("Listing top talkers on "+channel+"...");
					string output = "";
					StatCollection scol = userstat[channel].GetTop(10);
					for (int i=0; i<scol.Count; i++) 
					{
						output += "\u0002"+(i+1)+"\u0002 - ["+scol[i].Key+"]: "+scol[i].Score+"; ";
					}
					bot.SendToChannel(channel, output, true);
				}
				else if (message=="!chanstat") 
				{
					log.Info("Listing stat info");
					int chncount = wordstat[channel].Count;
					bot.SendToChannel(channel, chncount+" words known in this channel", true);
					int count = 0;
					foreach (object okey in wordstat.Keys) 
					{
						string key = okey as string;
						count += wordstat[key].Count;
					}
					if (count > chncount)
						bot.SendToChannel(channel, count+" words known in all channels", true);
				}
				else if (message=="!mystat") 
				{
					log.Info("Listing personal stats info");
					bot.SendToChannel(channel, "Your score: "+userstat[channel][user.NickName].Score+"");

				}
			} 
			catch (Exception e) 
			{
				log.Error("Error in statplugin.onpublicmessage('"+user.NickName+"','"+channel+"', '"+message+"')",e);
			}
		}

		private void bot_OnPrivateMessage(User user, string channel, string message)
		{
			if (message == "!colortest")
			{ 
				bot.SendToUser(user.NickName, "I am ^C4,7really upset^C at my brother for ^C0,1WASTING ^C15,000 sheets of paper!");
				bot.SendToUser(user.NickName, "dette er en \u0020Bold\u0020 test");
				bot.SendToUser(user.NickName, "dette er en \u0002Bold\u0002 test 2");
			}
		}

	}

	/// <summary>
	/// et dictionary over kanalnavne og statdictionaries
	/// </summary>
	public class ChannelDictionary : DictionaryBase 
	{
		public void Increment(string channel, string word) 
		{
			if (!Contains(channel))
				Add(channel, new StatCollection());

			this[channel].Increment(word);
		}

		public void Add(string channelname, StatCollection collection) 
		{
			Dictionary.Add(channelname, collection);
		}

		public StatCollection this[string channelname] 
		{
			get { return (StatCollection)Dictionary[channelname]; }
			set { Dictionary[channelname] = value; }
		}

		public bool Contains(string key)
		{
			return this.Dictionary.Contains(key);
		}

		public ICollection Keys
		{
			get {return this.Dictionary.Keys;}
		}

	}

	public class StatObject 
	{
		private string key;
		private int score;

		public string Key 
		{
			get { return key; }
			set { key = value; }
		}
		public int Score 
		{
			get { return score; }
			set { score = value; }
		}

		public StatObject(string key, int score) 
		{
			this.Key = key;
			this.score = score;
		}
	}

	public class StatCollection : CollectionBase 
	{
		public void Add(string key, int score) 
		{
			Add(new StatObject(key, score) );
		}
		public void Add(StatObject obj) 
		{
			List.Add( obj );
		}

		public void Increment(string key) 
		{
			StatObject obj= GetByKey(key);
			if (obj == null) 
			{
				obj = new StatObject(key, 0);
				Add( obj );
			}
			obj.Score++;
		}

		public StatCollection GetTop(int count) 
		{
			Sort();

			StatCollection col = new StatCollection();
			int max = count>10?10:count;

			for(int i=0; i<max; i++)
				col.Add( this[i] );

			return col; 
		}

		private void Sort()	// bubblesort, bare for at få noget der virker. optimer senere!
		{
			int i, j;
			StatObject temp;
			for (i =Count - 1; i > 0; i--)
			{
				for (j = 0; j < i; j++)
				{
					if (this[j].Score.CompareTo(this[j + 1].Score) < 0)
					{
						temp = this[j];
						this[j] = this[j+1];
						this[j+1] = temp;
					}
				}
			}
		}
		public StatObject this[int i] 
		{
			get { return (StatObject)List[i]; }
			set { List[i] = value; }
		}

		public StatObject this[string key] 
		{
			get { return GetByKey(key); }
		}

		private StatObject GetByKey(string key) 
		{
			bool found = false;
			int i = 0;
			while (!found && i<Count) 
			{
				if (this[i].Key == key) 
					found = true;
				else
					i++;
			}
			if (found)
				return this[i];
			else
				return null;
		}
	}
}
