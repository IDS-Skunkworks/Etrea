using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Etrea3.Core;

namespace Etrea3.Networking
{
    public class TcpServer
    {
        private static bool accept = false;
        private static TcpListener listener;
        private static CancellationTokenSource tokenSource;
        private static string ip;
        private static int port;

        public bool Init(out string errMsg)
        {
            errMsg = string.Empty;
            if (!IPAddress.TryParse(ConfigurationManager.AppSettings["ListenerIP"], out IPAddress listenerIP))
            {
                errMsg = $"Unable to parse ListenerIP: value {ConfigurationManager.AppSettings["ListenerIP"]} is not a valid IP address";
                return false;
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["ListenerPort"], out int listenerPort))
            {
                errMsg = $"Unable to parse ListenerPort: value {ConfigurationManager.AppSettings["ListenerPort"]} is not a valid number";
                return false;
            }
            if (listenerPort <= 0 || listenerPort >= 65000)
            {
                errMsg = $"Port {listenerPort} is not valid. Value must be >0 and <65000";
                return false;
            }
            ip = listenerIP.ToString();
            port = listenerPort;
            tokenSource = new CancellationTokenSource();
            return true;
        }

        public void Start()
        {
            try
            {
                IPAddress listenerIP = IPAddress.Parse(ip);
                listener = new TcpListener(listenerIP, port);
                listener.Start();
                accept = true;
                Game.LogMessage($"INFO: Listening for connections on {listener.LocalEndpoint}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error starting listener: {ex.Message}", LogLevel.Error);
                accept = false;
            }
        }

        public void Listen()
        {
            Task.Run(async () =>
            {
                while (listener != null && accept)
                {
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        Game.LogMessage($"INFO: Stopping TCP listener", LogLevel.Info);
                        accept = false;
                        listener.Stop();
                        break;
                    }
                    var clientTask = listener.AcceptTcpClientAsync();
                    if (clientTask.Result != null)
                    {
                        var client = clientTask.Result;
                        Game.LogMessage($"CONNECTION: Accepting new connection from {client.Client.RemoteEndPoint}", LogLevel.Connection);
                        await SessionManager.Instance.NewSession(client);
                    }
                }
            }, tokenSource.Token);
        }

        public void ShutDown()
        {
            Game.LogMessage($"INFO: Requesting the TCP listener to stop", LogLevel.Info);
            tokenSource.Cancel();
        }
    }
}
