using System;
using System.Configuration;
using Etrea3.Networking;
using Etrea3.Core;
using System.Threading.Tasks;
using Etrea3.Networking.API;
using System.IO;

namespace Etrea3
{
    public class Program
    {
        private static TcpServer tcpServer;
        private static APIServer apiServer;
        private static Game game;
        
        static async Task Main(string[] args)
        {
            Console.Title = ConfigurationManager.AppSettings["GameTitle"];
            if (!Setup())
            {
                Console.WriteLine("One or more required files was missing and could not be created from internal resources.");
                Console.WriteLine("MUD Server cannot start");
                Console.WriteLine("Press ENTER to close");
                _ = Console.ReadLine();
                Environment.Exit(-1);
            }
            tcpServer = new TcpServer();
            if (!tcpServer.Init(out string errReply))
            {
                Console.WriteLine($"Error starting TCP server: {errReply}");
                Console.WriteLine("Press ENTER to close");
                _ = Console.ReadLine();
                Environment.Exit(-1);
            }
            apiServer = new APIServer();
            apiServer.Init();
            tcpServer.Start();
            tcpServer.Listen();
            game = new Game();
            await game.Run();
            tcpServer.ShutDown();
            apiServer.Stop();
            Game.LogMessage($"INFO: TCP server stopped, shutdown complete", LogLevel.Info, true);
        }

        static bool Setup()
        {
            try
            {
                Console.WriteLine("Checking required files...");
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(Path.Combine(basePath, "resources\\welcome.txt")))
                {
                    byte[] welcomeBytes = Properties.Resources.welcome;
                    if (!Directory.Exists(Path.Combine(basePath, "resources")))
                    {
                        Directory.CreateDirectory(Path.Combine(basePath, "resources"));
                        Console.WriteLine("Creating default Welcome Message");
                        File.WriteAllBytes(Path.Combine(basePath, "resources\\welcome.txt"), welcomeBytes);
                    }
                }
                if (!Directory.Exists(Path.Combine(basePath, "world")))
                {
                    Directory.CreateDirectory(Path.Combine(basePath, "world"));
                }
                if (!File.Exists(Path.Combine(basePath, "world\\logs.db")))
                {
                    byte[] logDBBytes = Properties.Resources.logs;
                    Console.WriteLine("Creating logs database");
                    File.WriteAllBytes(Path.Combine(basePath, "world\\logs.db"), logDBBytes);
                }
                if (!File.Exists(Path.Combine(basePath, "world\\players.db")))
                {
                    byte[] playerDBBytes = Properties.Resources.players;
                    Console.WriteLine("Creating player database");
                    File.WriteAllBytes(Path.Combine(basePath, "world\\players.db"), playerDBBytes);
                }
                if (!File.Exists(Path.Combine(basePath, "world\\world.db")))
                {
                    byte[] worldDBBytes = Properties.Resources.world;
                    Console.WriteLine("Creating world database");
                    File.WriteAllBytes(Path.Combine(basePath, "world\\world.db"), worldDBBytes);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to create/verify required files: {ex.Message}");
                return false;
            }
        }
    }
}
