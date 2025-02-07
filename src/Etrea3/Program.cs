using System;
using System.Configuration;
using Etrea3.Networking;
using Etrea3.Core;
using System.Threading.Tasks;
using Etrea3.Networking.API;
using System.IO;
using System.Reflection;

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
            Game.LogMessage($"INFO: TCP server stopped, shutdown complete", LogLevel.Info);
        }

        static bool Setup()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Console.WriteLine("Starting file check...");
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world"));
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\welcome.txt")))
                {
                    using (Stream rStream = assembly.GetManifestResourceStream("Etrea3.Resources.welcome.txt"))
                    {
                        if (rStream == null)
                        {
                            Console.WriteLine("ERROR: Cannot open default welcome message resource");
                            return false;
                        }
                        Console.WriteLine("Creating default welcome message file...");
                        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\welcome.txt"), FileMode.Create, FileAccess.Write))
                        {
                            rStream.CopyTo(fs);
                        }
                    }
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\players.db")))
                {
                    using (Stream rStream = assembly.GetManifestResourceStream("Etrea3.Resources.players.db"))
                    {
                        if (rStream == null)
                        {
                            Console.WriteLine("ERROR: Cannot open default player database resource");
                            return false;
                        }
                        Console.WriteLine("Creating blank player database...");
                        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\players.db"), FileMode.Create, FileAccess.Write))
                        {
                            rStream.CopyTo(fs);
                        }
                    }
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\world.db")))
                {
                    using (Stream rStream = assembly.GetManifestResourceStream("Etrea3.Resources.world.db"))
                    {
                        if (rStream == null)
                        {
                            Console.WriteLine("ERROR: Cannot open default world database resource");
                            return false;
                        }
                        Console.WriteLine("Creating default world database...");
                        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\world.db"), FileMode.Create, FileAccess.Write))
                        {
                            rStream.CopyTo(fs);
                        }
                    }
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\logs.db")))
                {
                    using (Stream rStream = assembly.GetManifestResourceStream("Etrea3.Resources.logs.db"))
                    {
                        if (rStream == null)
                        {
                            Console.WriteLine("ERROR: Cannot open default logs database resoure");
                            return false;
                        }
                        Console.WriteLine("Creating default logging database...");
                        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world\\logs.db"), FileMode.Create, FileAccess.Write))
                        {
                            rStream.CopyTo(fs);
                        }
                    }
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
