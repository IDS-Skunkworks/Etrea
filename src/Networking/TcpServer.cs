using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kingdoms_of_Etrea.Core;

namespace Kingdoms_of_Etrea.Networking
{
    internal class TcpServer
    {
        private bool _accept = false;
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private string _ip;
        private int _port;
        private byte[] _buffer = new byte[256];

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
                if(_listener != null && _accept)
                {
                    while(true)
                    {
                        if(_tokenSource.Token.IsCancellationRequested)
                        {
                            Game.LogMessage("INFO: Stopping TCP listener...", LogLevel.Info, true);
                            _accept = false;
                            _listener.Stop();
                            break;
                        }
                        var clientTask = _listener.AcceptTcpClientAsync();

                        if(clientTask.Result != null)
                        {
                            var client = clientTask.Result;
                            Game.LogMessage($"INFO: Accepting new connection from {client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                            await SessionManager.Instance.NewDescriptorAsync(client);
                        }
                    }
                }
            }, _tokenSource.Token);
        }

        internal void Shutdown()
        {
            Game.LogMessage("SHUTDOWN: Requesting TCP listener stop", LogLevel.Info, true);
            _tokenSource.Cancel();
        }
    }
}
