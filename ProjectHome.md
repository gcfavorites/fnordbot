## FnordBot ##
Fnordbot started out as a simple client for testing my chat-algorithms, but evolved into an extensible IRC bot, based upon a layered IRC protocol framework suitable for implementing other clients.
The bot runs as a windows service, and a NSIS installer is included.

## LibIrc2 ##
LibIrc2 is a layered IRC protocol framework. You can decide how much of the framework to use:
  * Network layer contains simple methods for sending and receiving data from the server.
  * Protocol layer contains logic for parsing received data as IRC messages and for sending IRC commands.
  * Client layer adds an object model of joined channels and their users

## Plugins ##
Several plugins are included in the base distribution:
  * **Logger** - for logging to disk
  * **Sortsnak** - Non-Intelligent, Artificial Language Learner-style chatbot. Inspired by Monty the PIRC-bot
  * **Stat** - maintains statistics on most active users and most used words
  * **Wordgame** - a word guessing game, inspired by the eggdrop script of the same name.
  * **Voter** - Hold a referendum in a channel