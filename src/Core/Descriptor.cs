using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Kingdoms_of_Etrea.Entities;

namespace Kingdoms_of_Etrea.Core
{
    internal class Descriptor
    {
        internal Descriptor(TcpClient client)
        {
            Client = client ?? throw new ArgumentNullException("client");
            ConnectionTime = DateTime.UtcNow;
            State = ConnectionState.GetUsername;
            Id = Guid.NewGuid();
            Player = null;
        }

        internal Guid Id;
        internal TcpClient Client { get; private set; }
        internal bool IsConnected => Client.Connected;
        internal DateTime ConnectionTime { get; private set; }
        internal DateTime LastInputTime { get; set; }
        internal Player Player { get; set; }
        internal ConnectionState State { get; set; }

        internal async Task SendAsync(string message)
        {
            if(IsConnected)
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);
                await Client.GetStream().WriteAsync(msgBytes, 0, msgBytes.Length);
            }
        }

        internal void Send(string message)
        {
            if(IsConnected && !string.IsNullOrEmpty(message))
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);
                Client.GetStream().Write(msgBytes, 0, msgBytes.Length);
            }
        }

        internal async Task<string> ReadAsync()
        {
            byte[] buffer = new byte[4096];
            if(!IsConnected)
            {
                return null;
            }
            string input = string.Empty;
            string request = string.Empty;
            while(input != Constants.NewLine)
            {
                try
                {
                    int byteCount = await Client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    input = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    if (input.ToCharArray()[0] == Constants.ASCIIDel)
                    {
                        request = request.Substring(0, request.Length - 1);
                    }
                    else
                    {
                        request = $"{request}{input}";
                    }
                }
                catch
                {
                    Game.LogMessage($"ERROR: Error: client {Client.Client.RemoteEndPoint} was closed from the remote side", LogLevel.Error, true);
                    Client.Client.Disconnect(false);
                    return null;
                }
            }
            return request;
        }

        internal string Read()
        {
            try
            {
                byte[] buffer = new byte[4096];
                if (!IsConnected)
                {
                    return null;
                }
                string input = string.Empty;
                bool validLine = false;
                while(!validLine)
                {
                    try
                    {
                        int byteCount = Client.GetStream().Read(buffer, 0, buffer.Length);
                        input = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        // if the last byte in the buffer is 10, user has pressed enter
                        if(byteCount > 1)
                        {
                            validLine = buffer[byteCount - 1] == 10;
                        }
                    }
                    catch(Exception ex)
                    {
                        Game.LogMessage($"ERROR: Error reading from socket: {this.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
                        SessionManager.Instance.Close(this);
                    }
                }
                return input;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error reading from socket at Descriptor.Read(): {ex.Message}", LogLevel.Error, true);
                return string.Empty;
            }
        }

        internal void Disconnect()
        {
            if(IsConnected)
            {
                Game.LogMessage($"INFO: Disconnecting socket: {this.Client.Client.RemoteEndPoint}", LogLevel.Info, true);
                Client.Close();
            }
        }
    }
}
