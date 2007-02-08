using System;

namespace NielsRask.LibIrc
{
	/// <summary>
	/// Enumeration of supported replycoldes
	/// </summary>
	public enum ReplyCode :int 
	{
		// :panda.droso.net 353 BimseBot = #craYon :BimseBot NetRanger smcRanger NordCore Maverick cyberzed Pornoting |Hunter-| Modena
		/// <summary>
		/// Reply is a list of users in a channel
		/// </summary>
		RPL_NAMREPLY = 353,
		// :panda.droso.net 366 BimseBot #craYon :End of /NAMES list.
		/// <summary>
		/// A complete namelist has been sent
		/// </summary>
		RPL_ENDOFNAMES = 366,

		/// <summary>
		/// MOTD will be sent
		/// </summary>
		RPL_MOTDSTART = 375,
		/// <summary>
		/// MOTD of the server
		/// </summary>
		RPL_MOTD = 372,
		/// <summary>
		/// MOTD has been sent
		/// </summary>
		RPL_ENDOFMOTD = 376,

		/// <summary>
		/// Topic for a channel, triggered at join and change
		/// </summary>
		RPL_TOPIC = 332,
		/// <summary>
		/// The channel has no topic
		/// </summary>
		RPL_NOTOPIC = 331,

		/// <summary>
		/// 
		/// </summary>
		RPL_LIST = 322,
		/// <summary>
		/// 
		/// </summary>
		RPL_LISTEND = 323,

		/// <summary>
		/// 
		/// </summary>
		RPL_TIME = 391, 
		/// <summary>
		/// This nickname is in use, select another one
		/// </summary>
		ERR_NICKINUSE = 433,

		/// <summary>
		/// 
		/// </summary>
		None
	}

	/*
:koala.droso.net NOTICE AUTH :*** Looking up your hostname...
:koala.droso.net NOTICE AUTH :*** Checking Ident
:koala.droso.net NOTICE AUTH :*** No Ident response
:koala.droso.net NOTICE AUTH :*** Found your hostname
:koala.droso.net 001 BimseBot :Welcome to the Droso Net Internet Relay Chat Network BimseBot
:koala.droso.net 002 BimseBot :Your host is koala.droso.net[0.0.0.0/6667], running version hybrid-7.2.0
:koala.droso.net 003 BimseBot :This server rose from the ashes Jan 14 2006 at 20:16:20
:koala.droso.net 004 BimseBot koala.droso.net hybrid-7.2.0 DGabcdfghiklnorsuwxyz biklmnopstveI bkloveI
:koala.droso.net 005 BimseBot CALLERID CASEMAPPING=rfc1459 KICKLEN=160 MODES=4 NICKLEN=20 PREFIX=(ov)@+ STATUSMSG=@+ TOPICLEN=160 NETWORK=Droso Net MAXLIST=beI:25 MAXTARGETS=4 CHANTYPES=#& CHANLIMIT=#&:15 :are supported by this server
:koala.droso.net 005 BimseBot CHANNELLEN=50 CHANMODES=eIb,k,l,imnpst AWAYLEN=160 KNOCK ELIST=CMNTU SAFELIST EXCEPTS=e INVEX=I :are supported by this server
:koala.droso.net 251 BimseBot :There are 0 users and 21 invisible on 3 servers
:koala.droso.net 252 BimseBot 1 :Smurf Targets (IRC Operators) online
:koala.droso.net 254 BimseBot 7 :channels formed
:koala.droso.net 255 BimseBot :I have 13 clients and 2 servers
:koala.droso.net 265 BimseBot :Current local users: 13  Max: 30
:koala.droso.net 266 BimseBot :Current global users: 21  Max: 41
:koala.droso.net 250 BimseBot :Highest connection count: 32 (30 clients) (78667 connections received)
:koala.droso.net 375 BimseBot :- koala.droso.net Message of the Day - 
:koala.droso.net 372 BimseBot :- This is ircd-hybrid MOTD replace it with something better
:koala.droso.net 376 BimseBot :End of /MOTD command.
We're now registered!
:BimseBot MODE BimseBot :+i
:BimseBot!~bimmer@0x3e42a834.adsl.cybercity.dk JOIN :#bottest
:koala.droso.net 332 BimseBot #bottest :dingeling
:koala.droso.net 333 BimseBot #bottest NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk 1152616957
:koala.droso.net 353 BimseBot = #bottest :BimseBot @NordCore
:koala.droso.net 366 BimseBot #bottest :End of /NAMES list.
:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PART #bottest
:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk JOIN :#bottest
:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk PRIVMSG #bottest :fooo
-> :koala.droso.net NOTICE AUTH :*** No Ident response
-> :koala.droso.net 433 * BimseBot :Nickname is already in use.
-> :Bimsebot MODE Bimsebot :+i
-> :Bimsebot!~bimse@0x3e42a834.adsl.cybercity.dk JOIN :#craYon
-> :koala.droso.net 332 Bimsebot #craYon :http://www.elizium.nu/scripts/lemmings/index.html | http://www.nudisttrampolining.com/
-> :koala.droso.net 333 Bimsebot #craYon smcRanger!~smc@0x535e8436.hsnxx3.adsl-dhcp.tele.dk 1152601225
Bimsebot (craYon)
-> :koala.droso.net 353 Bimsebot = #craYon :Bimsebot Gazolini cyberzed NordCore smcRanger Modena PornoWork |Hunter-|
-> :koala.droso.net 366 Bimsebot #craYon :End of /NAMES list.
:NordCore!~nordcore@0x3e42a834.adsl.cybercity.dk TOPIC #bottest :fooo bar
	
	
	 * */
}
