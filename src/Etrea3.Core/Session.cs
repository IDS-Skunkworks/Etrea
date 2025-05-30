﻿using System;
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
        private static ArrayPool<byte> bufferPool = ArrayPool<byte>.Shared;
        private Guid Snooper { get; set; } = Guid.Empty;

        public Session(TcpClient client)
        {
            Client = client;
            ConnectionTime = DateTime.UtcNow;
            State = ConnectionState.MainMenu;
            ID = Guid.NewGuid();
            Player = null;
        }

        public bool SetSnooper(Guid snooper, bool clearSnoop, out string response)
        {
            if (clearSnoop)
            {
                if (Snooper == snooper)
                {
                    Snooper = Guid.Empty;
                    response = "Snooper cleared";
                    return true;
                }
                response = "Cannot clear some else's snoop session";
                return false;
            }
            if (Snooper != Guid.Empty)
            {
                if (Snooper != snooper)
                {
                    response = "Someone else is already snooping that connection";
                    return false;
                }
                response = "You are already snooping that connection";
                return false;
            }
            Snooper = snooper;
            response = "Snooping OK";
            return true;
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
                byte[] buffer = maxByteCount >= MaxBufferSize ? bufferPool.Rent(maxByteCount) : bufferPool.Rent(MaxBufferSize);
                try
                {
                    int byteCount = Encoding.UTF8.GetBytes(parsedMessage, 0, parsedMessage.Length, buffer, 0);
                    Client?.GetStream()?.Write(buffer, 0, byteCount);
                    if (Snooper != Guid.Empty)
                    {
                        SessionManager.Instance.GetSession(Snooper)?.SendSystem($"%BMT%<<{message}<<%PT%");
                    }
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
                    Game.LogMessage($"ERROR: Error sending to Client {endpoint}: {ex.Message}", LogLevel.Error);
                }
                finally
                {
                    bufferPool.Return(buffer);
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
                byte[] buffer = bufferPool.Rent(MaxBufferSize);
                try
                {
                    int? byteCount = Client?.GetStream()?.Read(buffer, 0, buffer.Length);
                    if (byteCount == null || byteCount == 0)
                    {
                        return null;
                    }
                    LastInputTime = DateTime.UtcNow;
                    string msg = Encoding.UTF8.GetString(buffer, 0, byteCount.Value);
                    if (Snooper != Guid.Empty)
                    {
                        SessionManager.Instance.GetSession(Snooper)?.SendSystem($"%BMT%>>{msg}>>%PT%");
                    }
                    return msg;
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
                    Game.LogMessage($"ERROR: Error reading from socket {endpoint}: {ex.Message}", LogLevel.Error);
                    SessionManager.Instance.Close(this);
                    return null;
                }
                finally
                {
                    bufferPool.Return(buffer);
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error reading from socket at Session.Read(): {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        public void Disconnect()
        {
            /// Should not be called directly - close all sessions via the SessionManager!
            if (IsConnected)
            {
                Game.LogMessage($"CONNECTION: Disconnecting socket {Client.Client.RemoteEndPoint}", LogLevel.Connection);
                Client.Client.Dispose();
            }
        }
    }
}
