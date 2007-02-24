using System;
using NielsRask.FnordBot;

namespace NielsRask.Stat
{
	public class StatPlugin : IPlugin
	{
		NielsRask.FnordBot.FnordBot bot;

		public StatPlugin()
		{
		}
		#region IPlugin Members

		public void Attach(NielsRask.FnordBot.FnordBot bot)
		{
			this.bot = bot;
		}

		public void Init(System.Xml.XmlNode pluginNode)
		{
			// TODO:  Add StatPlugin.Init implementation
		}

		#endregion

	}
}
