using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Etrea2.Core;
using System.Threading;

namespace Etrea2.Networking
{
    internal class TcpServer
    {
        private bool _accept = false;
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private string _ip;
        private int _port;
        private byte[] _buffer = new byte[1024];

        internal TcpServer(string ipAddress, int port)
        {
            _tokenSource = new CancellationTokenSource();
            _ip = ipAddress;
            _port = port;
        }

        internal void StartServer(uint startRoom)
        {
            try
            {
                IPAddress listenerIP = IPAddress.Parse(_ip);
                _listener = new TcpListener(listenerIP, _port);
                _listener.Start();
                _accept = true;
                Game.LogMessage($"INFO: Listening for connections on {_listener.LocalEndpoint}", LogLevel.Info, true);
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error starting listener: {ex.Message}", LogLevel.Error, true);
                _accept = false;
            }
        }

        internal void Listen()
        {
            Task.Run(async () =>
            {
                if (_listener != null && _accept)
                {
                    while (true)
                    {
                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            Game.LogMessage($"INFO: Stopping TCP listener...", LogLevel.Info, true);
                            _accept = false;
                            _listener.Stop();
                            break;
                        }
                        var clientTask = _listener.AcceptTcpClientAsync();
                        if (clientTask.Result != null)
                        {
                            var client = clientTask.Result;
                            Game.LogMessage($"CONNECTION: Accepting new connection from {client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                            await SessionManager.Instance.NewDescriptor(client);
                        }
                    }
                }
            }, _tokenSource.Token);
        }

        internal void Shutdown()
        {
            Game.LogMessage($"INFO: Requesting TCP listener stop", LogLevel.Info, true);
            _tokenSource.Cancel();
        }
    }
}
