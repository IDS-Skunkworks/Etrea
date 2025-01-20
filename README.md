# Kingdoms of Etrea
Kingdoms of Etrea is a MUD server written entirely in C#.

I have fond memories of playing MUDs based off Circle and Smaug from way back when, and for some reason I thought it might be a nice challenge to try and write a MUD server based entirely in C# since almost all of the others are based on C/C++.

If you want to have a play with the code yourself, just clone the repo and open with Visual Studio 2022. Any project dependencies (such as KerLua, NCalc, NewtonSoft JSON and SQLite) should be restored from NuGet. Build and run.

If you want to just host your own version, the latest release contains a ZIP with everything you should need to get going, including the dependences. Just download it, extract it to a suitable location and run the Etrea3.exe file. On first start-up it will create its directory structure and some default SQLite databases to hold world assets and player characters. Then you can just log in with your favourite MUD client, create a character and start playing.

If you want to see what the game is like, I have the current build running on etrea.endoftheinternet.org port 12345.

If you want to learn more about how to play, or if you have comments, suggestions or other feedback, please pop along to the project website where I'm putting together some guides suitable for both players and Immortals.

# Development Goals
1. Add more zones, NPCs, Items, Quests etc. to the default world
2. Move the game objects to a shared library project to properly support the Admin console
3. Complete the REST API for the Admin console
4. Complete the Admin console
5. Tidy the menus in OLC

I'm not a trained developer, so these goals might take me longer to achieve than if I was. Naturally all this will be updated as progress is made.
