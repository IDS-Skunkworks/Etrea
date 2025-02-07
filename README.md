# Kingdoms of Etrea
Kingdoms of Etrea is a MUD server written entirely in C#.

I have fond memories of playing MUDs based off Circle and Smaug from way back when, and for some reason I thought it might be a nice challenge to try and write a MUD server based entirely in C# since almost all of the others are based on C/C++.

If you want to have a play with the code yourself, just clone the repo and open with Visual Studio 2022. Any project dependencies (such as KerLua, NCalc, NewtonSoft JSON and SQLite) should be restored from NuGet. Build and run, making sure your startup-project (if running in Debug) is Etrea and not Etrea Admin.

If you want to just host your own version, the latest release contains a ZIP with everything you should need to get going, including the dependences. Just download it, extract it to a suitable location and run the Etrea3.exe file. On first start-up it will create its directory structure and some default SQLite databases to hold world assets and player characters. Then you can just log in with your favourite MUD client, create a character and start playing.

If you want to see what the game is like, I have the current build running on etrea.endoftheinternet.org port 12345.

If you want to learn more about how to play, or if you have comments, suggestions or other feedback, please pop along to the project website where I'm putting together some guides suitable for both players and Immortals.

If you want to use the Admin tool, you will first need to ensure the MUD server is running as Admin with an appropriate value for APIUrl in the config file. Log in as your Immortal character and use the command: apikey generate <your player name> to generate a key. Add this key into the config file for the Admin tool, along with the web address of your MUD server (http://localhost:5000 is the default if running both on the same machine). The Admin tool will then be able to authenticate to the API as your player. Additional keys for other Immortal players can be generated the same way. API access is not allowed for non-Immortal players.

# Development Goals
1. Add more zones, NPCs, Items, Quests etc. to the default world
2. Tidy the menus in OLC

Unless there are any bug fixes or additional developments, future releases are likely to be updated world databases with more rooms, npcs, items, zones, quests etc. in them. If you would like to help build out the world or contribute to the development of the server, please get in touch!
