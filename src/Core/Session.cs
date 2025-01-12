using System;
using System.Buffers;
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

        private const int MaxBufferSize = 0x1000;
        private static readonly object _lockObject = new object();

        public Session(TcpClient client)
        {
            Client = client;
            ConnectionTime = DateTime.UtcNow;
            State = ConnectionState.MainMenu;
            ID = Guid.NewGuid();
            Player = null;
        }

        public void SendSystem(string message)
        {
            Send(message, true);
        }

        public void Send(string message, bool bypass = false)
        {
            if (IsConnected)
            {
                if (Player != null && (Player.Flags.HasFlag(PlayerFlags.UsingOLC) || Player.Flags.HasFlag(PlayerFlags.WritingMail)) && !bypass)
                {
                    return;
                }
                string parsedMessage = Helpers.ParseColourCodes(message);
                int maxByteCount = Encoding.UTF8.GetMaxByteCount(parsedMessage.Length);
                byte[] heapBuffer = maxByteCount >= MaxBufferSize ? ArrayPool<byte>.Shared.Rent(maxByteCount) : ArrayPool<byte>.Shared.Rent(MaxBufferSize);
                try
                {
                    int byteCount = Encoding.UTF8.GetBytes(parsedMessage, 0, parsedMessage.Length, heapBuffer, 0);
                    Client.GetStream().Write(heapBuffer, 0, byteCount);
                }
                catch (Exception ex)
                {
                    string endpoint = "unknown";
                    try
                    {
                        endpoint = Client?.Client?.RemoteEndPoint?.ToString() ?? "null client";
                    }
                    catch (Exception exc)
                    {
                        endpoint = $"Error retrieving endpoint: {exc.Message}";
                    }
                    Game.LogMessage($"ERROR: Error sending to Client {endpoint}: {ex.Message}", LogLevel.Error, true);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(heapBuffer);
                }
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
                byte[] heapBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
                try
                {
                    int byteCount = Client.GetStream().Read(heapBuffer, 0, heapBuffer.Length);
                    if (byteCount == 0)
                    {
                        return null;
                    }
                    return Encoding.UTF8.GetString(heapBuffer, 0, byteCount);
                }
                catch (Exception ex)
                {
                    string endpoint = "unknown";
                    try
                    {
                        endpoint = Client?.Client?.RemoteEndPoint?.ToString() ?? "null client";
                    }
                    catch (Exception exc)
                    {
                        endpoint = $"Error retrieving endpoint: {exc.Message}";
                    }
                    Game.LogMessage($"ERROR: Error reading from socket {endpoint}: {ex.Message}", LogLevel.Error, true);
                    SessionManager.Instance.Close(this);
                    return null;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(heapBuffer);
                }
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
