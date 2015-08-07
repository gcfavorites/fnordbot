# New plugin howto #

Start a new project in VS.Net.

Add a reference to the file FnordBot.dll.

Add the line `using NielsRask.FnordBot;` to the top of your file.

Now make your class implement the interface IPlugin (NielsRask.FnordBot.IPlugin)
VS.NET should generate stubs for the Init and Attach methods. If not, look at the code listing further down.

for this demonstration we'll make a plugin that simply makes the bot say the word "bar" on a channel if it sees the word "foo".

Its simple, really. We need to subscribe to the OnPublicMessage event, and put the answer logic in the subscribing method.

```
		// store a reference to the bot instance
		NielsRask.FnordBot.FnordBot bot;
		public void Attach(NielsRask.FnordBot.FnordBot bot)
		{
			this.bot = bot;
			// subscribe to the OnPublicMessage event
			bot.OnPublicMessage += new NielsRask.FnordBot.FnordBot.MessageHandler(bot_OnPublicMessage);
		}
		private void bot_OnPublicMessage(User user, string channel, string message)
		{
			if (message == "foo")
				bot.SendToChannel(channel, "bar");
		}
		public void Init(System.Xml.XmlNode pluginNode)
		{
			// no logic needed in this example
		}
```


The Init method passes an xml node from the bot configuration file that can contain plugin-specific settings. more on this later.

To use your new plugin, copy a plugin-section from the default config and modify it for your plugin. Then start the bot and test