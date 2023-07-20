using Kingdoms_of_Etrea.Networking;
using Kingdoms_of_Etrea.Core;
using System.Configuration;
using System.Net;

namespace Kingdoms_of_Etrea
{
    internal class Program
    {
        private static TcpServer _server;
        private static Game _game;
        private static uint _startRoom;

        static void Main(string[] args)
        {
            // https://www.dandwiki.com/wiki/3e_SRD:Armor
            var serverIP = ConfigurationManager.AppSettings["listenerIP"];
            var serverPort = ConfigurationManager.AppSettings["listenPort"];
            var startRoom = ConfigurationManager.AppSettings["playerStartRoom"];
            int port;
            if(!string.IsNullOrEmpty(serverIP))
            {
                if(!IPAddress.TryParse(serverIP, out _))
                {
                    serverIP = "0.0.0.0";
                }
            }
            else
            {
                serverIP = "0.0.0.0";
            }
            if(!string.IsNullOrEmpty(serverPort))
            {
                if(!int.TryParse(serverPort, out port))
                {
                    port = 12345;
                }
            }
            else
            {
                port = 12345;
            }
            if(!string.IsNullOrEmpty(startRoom))
            {
                if(!uint.TryParse(startRoom, out uint _startRoom))
                {
                    _startRoom = 100;
                }
            }
            else
            {
                _startRoom = 100;
            }
            _server = new TcpServer(serverIP, port);
            _server.StartServer(_startRoom);
            _server.Listen();

            // zone, npc, combat, autosave, buff
            var confZonePulse = ConfigurationManager.AppSettings["zonePulse"];
            var confNpcPulse = ConfigurationManager.AppSettings["npcPulse"];
            var confCombatPulse = ConfigurationManager.AppSettings["combatPulse"];
            var confAutoSavePulse = ConfigurationManager.AppSettings["autoSavePulse"];
            var confBuffPulse = ConfigurationManager.AppSettings["buffPulse"];
            var confBackupPulse = ConfigurationManager.AppSettings["backupPulse"];
            var confBackupsRetained = ConfigurationManager.AppSettings["backupsRetained"];
            uint zonePulse, npcPulse, combatPulse, autoSavePulse, buffPulse, backupPulse, backupsRetained;
            if(!string.IsNullOrEmpty(confBackupPulse))
            {
                if(!uint.TryParse(confBackupPulse, out backupPulse))
                {
                    backupPulse = 3600;
                }
            }
            else
            {
                backupPulse = 3600;
            }
            if(!string.IsNullOrEmpty(confBackupsRetained))
            {
                if(!uint.TryParse(confBackupsRetained, out backupsRetained))
                {
                    backupsRetained = 10;
                }
            }
            else
            {
                backupsRetained = 10;
            }
            if(!string.IsNullOrEmpty(confZonePulse))
            {
                if(!uint.TryParse(confZonePulse, out zonePulse))
                {
                    zonePulse = 600;
                }
            }
            else
            {
                zonePulse = 600;
            }
            if(!string.IsNullOrEmpty(confNpcPulse))
            {
                if(!uint.TryParse(confNpcPulse, out npcPulse))
                {
                    npcPulse = 120;
                }
            }
            else
            {
                npcPulse = 120;
            }
            if(!string.IsNullOrEmpty(confCombatPulse))
            {
                if(!uint.TryParse(confCombatPulse, out combatPulse))
                {
                    combatPulse = 6;
                }
            }
            else
            {
                combatPulse = 6;
            }
            if(!string.IsNullOrEmpty(confAutoSavePulse))
            {
                if(!uint.TryParse(confAutoSavePulse, out autoSavePulse))
                {
                    autoSavePulse = 180;
                }
            }
            else
            {
                autoSavePulse = 180;
            }
            if(!string.IsNullOrEmpty(confBuffPulse))
            {
                if(!uint.TryParse(confBuffPulse, out buffPulse))
                {
                    buffPulse = 60;
                }    
            }
            else
            {
                buffPulse = 60;
            }
            Game.LogMessage($"INFO: Starting game with following parameters: zonePulse = {zonePulse}; npcPulse = {npcPulse}; combatPulse = {combatPulse}; buffPulse = {buffPulse}; backupPulse = {backupPulse}; backupsRetained = {backupsRetained}", LogLevel.Info, true);
            _game = new Game(zonePulse, npcPulse, combatPulse, autoSavePulse, buffPulse, backupPulse, backupsRetained);
            _game.Run();

            _server.Shutdown();
        }

        internal static void Stop()
        {
            if(_game != null)
            {
                _game.Shutdown();
            }
        }
    }
}
