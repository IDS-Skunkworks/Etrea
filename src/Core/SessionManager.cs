using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

namespace Etrea2.Core
{
    internal class SessionManager
    {
        private static SessionManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<Guid, Descriptor> Descriptors;
        internal List<Descriptor> Connections => Descriptors.Values.ToList();

        private SessionManager()
        {
            Descriptors = new Dictionary<Guid, Descriptor>();
        }

        internal static SessionManager Instance
        {
            get
            {
                lock ( _lock)
                {
                    if ( _instance == null )
                    {
                        _instance = new SessionManager();
                    }
                    return _instance;
                }
            }
        }

        internal List<Descriptor> GetAllPlayers()
        {
            lock (_lock)
            {
                return Instance.Descriptors.Values.Where(x => x.IsConnected && x.Player != null).ToList();
            }
        }

        internal Descriptor GetPlayerByGUID(Guid guid)
        {
            lock (_lock)
            {
                return Descriptors.ContainsKey(guid) ? Descriptors[guid] : null;
            }
        }

        internal List<Descriptor> GetDisconnectedSessions()
        {
            lock(_lock)
            {
                return Instance.Descriptors.Values.Where(x => !x.IsConnected || (x.Player == null && x.State != ConnectionState.CreatingCharacter)).ToList();
            }
        }

        internal Descriptor GetPlayer(string name)
        {
            lock (_lock)
            {
                return Instance.Descriptors.Values.Where(x => x.Player != null && Regex.IsMatch(x.Player.Name, name, RegexOptions.IgnoreCase)).FirstOrDefault();
            }
        }

        internal async Task NewDescriptor(TcpClient client)
        {
            var newDescriptor = new Descriptor(client);
            lock(_lock)
            {
                Instance.Descriptors.Add(newDescriptor.ID, newDescriptor);
                _ = new Connection(newDescriptor);
            }
        }

        internal List<Descriptor> GetPlayersInRoom(uint rid)
        {
            lock (_lock)
            {
                return Instance.Descriptors.Values.Where(x => x.Player != null && x.Player.CurrentRoom == rid).ToList();
            }
        }

        internal void Close(Descriptor desc)
        {
            EndPoint endPoint = null;
            try
            {
                endPoint = desc.Client.Client.RemoteEndPoint;
                var charName = desc.Player == null ? string.Empty : desc.Player.Name;
                if (desc.Player != null)
                {
                    if (desc.Player.FollowerID != Guid.Empty)
                    {
                        NPCManager.Instance.SetNPCFollowing(ref desc, false);
                    }
                    if (DatabaseManager.SavePlayer(ref desc, false))
                    {
                        Game.LogMessage($"INFO: Player {desc.Player.Name} at {desc.Client.Client.RemoteEndPoint} saved on disconnection", LogLevel.Info, true);
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Player {desc.Player.Name} at {desc.Client.Client.RemoteEndPoint} failed to save on disconnect, player data may be out of date on reload", LogLevel.Error, true);
                    }
                }
                desc.Client.Close();
                lock (_lock)
                {
                    Instance.Descriptors.Remove(desc.ID);
                }
                desc = null;
                string logMsg = string.IsNullOrEmpty(charName) ? $"CONNECTION: Connection from {endPoint} closed, no player associated with the connection"
                    : $"CONNECTION: Connection from {endPoint} closed, player {charName} has left the game";
                Game.LogMessage(logMsg, LogLevel.Connection, true);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Exception at SessionManager.Close() processing disconnection of {endPoint}: {ex.Message}", LogLevel.Error , true);
            }
        }

        internal void SendToAllClients(string msg)
        {
            lock(_lock)
            {
                foreach(var client in from client in Instance.Descriptors.Values where client.IsConnected && client.Player != null select client)
                {
                    client.Send(msg);
                }
            }
        }
    }
}
