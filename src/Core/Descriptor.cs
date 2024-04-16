using System;
using System.Net.Sockets;
using System.Text;
using Etrea2.Entities;

namespace Etrea2.Core
{
    internal class Descriptor
    {
        internal Guid ID;
        internal TcpClient Client { get; private set; }
        internal DateTime ConnectionTime { get; private set; }
        internal DateTime LastInputTime { get; set; }
        internal Player Player { get; set; }
        internal ConnectionState State { get; set; }
        internal bool IsConnected => Client != null && Client.Connected;

        internal Descriptor(TcpClient client)
        {
            Client = client ?? throw new ArgumentNullException("client cannot be null");
            ConnectionTime = DateTime.UtcNow;
            State = ConnectionState.GetUsername;
            ID = Guid.NewGuid();
            Player = null;
        }

        internal void Send(string message)
        {
            if (IsConnected)
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);
                Client.GetStream().Write(msgBytes, 0, msgBytes.Length);
            }
        }

        internal string Read()
        {
            try
            {
                if (!IsConnected)
                {
                    return null;
                }
                byte[] buffer = new byte[4096];
                string input = string.Empty;
                bool validLine = false;
                while (!validLine)
                {
                    try
                    {
                        int byteCount = Client.GetStream().Read(buffer, 0, buffer.Length);
                        input = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        if (byteCount > 1)
                        {
                            validLine = buffer[byteCount - 1] == 10;
                        }
                    }
                    catch (Exception ex)
                    {
                        Game.LogMessage($"ERROR: Error reading from socket {this.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
                        SessionManager.Instance.Close(this);
                    }
                }
                return input;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error reading from socket at Descriptor.Read(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal void Disconnect()
        {
            if (IsConnected)
            {
                Game.LogMessage($"CONNECTION: Disconnecting socket {this.Client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                Client.Close();
            }
        }
    }
}
