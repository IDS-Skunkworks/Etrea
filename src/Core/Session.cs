using System;
using System.Net.Sockets;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class Session
    {
        public Guid ID;
        public TcpClient Client { get; set; }
        public DateTime ConnectionTime { get; set; }
        public DateTime LastInputTime { get; set; }
        public Player Player { get; set; }
        public ConnectionState State { get; set; }
        public bool IsConnected => Client != null && Client.Connected;

        public Session(TcpClient client)
        {
            Client = client;
            ConnectionTime = DateTime.UtcNow;
            State = ConnectionState.MainMenu;
            ID = Guid.NewGuid();
            Player = null;
        }

        public void Send(string message)
        {
            if (IsConnected)
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(Helpers.ParseColourCodes(message));
                Client.GetStream().Write(msgBytes, 0, msgBytes.Length);
            }
        }

        public string Read()
        {
            try
            {
                if (!IsConnected)
                {
                    return null;
                }
                byte[] buffer = new byte[4096];
                string input = string.Empty;
                try
                {
                    int byteCount = Client.GetStream().Read(buffer, 0, buffer.Length);
                    input = Encoding.UTF8.GetString(buffer, 0, byteCount);
                }
                catch (Exception ex)
                {
                    Game.LogMessage($"ERROR: Error reading from socket {Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
                    SessionManager.Instance.Close(this);
                    return null;
                }
                return input;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error reading from socket at Session.Read(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        public void Disconnect()
        {
            /// Should not be called directly - close all sessions via the SessionManager!
            if (IsConnected)
            {
                Game.LogMessage($"CONNECTION: Disconnecting socket {Client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                Client.Client.Dispose();
            }
        }
    }
}
