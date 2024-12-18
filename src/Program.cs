using System;
using System.Configuration;
using Etrea3.Networking;
using Etrea3.Core;
using System.Threading.Tasks;
using Etrea3.Networking.API;

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
            tcpServer = new TcpServer();
            if (!tcpServer.Init(out string errReply))
            {
                Console.Write($"Error starting TCP server: {errReply}{Environment.NewLine}");
                Console.WriteLine("Press ENTER to close");
                _ = Console.ReadLine();
                Environment.Exit(-1);
            }
            apiServer = new APIServer("http://localhost:5000");
            apiServer.Init();
            tcpServer.Start();
            tcpServer.Listen();
            game = new Game();
            await game.Run();
            tcpServer.ShutDown();
            apiServer.Stop();
            Game.LogMessage($"INFO: TCP server stopped, shutdown complete", LogLevel.Info, true);
        }
    }
}
