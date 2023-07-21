using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Net;

namespace Kingdoms_of_Etrea.Core
{
    internal class SessionManager
    {
        private static SessionManager _instance = null;
        private static readonly object _lockObject = new object();

        private SessionManager()
        {
            Descriptors = new HashSet<Descriptor>();
        }

        internal static SessionManager Instance
        {
            get 
            {
                lock(_lockObject)
                {
                    if(_instance == null)
                    {
                        _instance = new SessionManager();
                    }
                    return _instance;
                }
            }
        }

        internal List<Descriptor> GetAllPlayers()
        {
            return Instance.Descriptors.Where(x => x.IsConnected && x.Player != null).ToList();
        }

        internal Descriptor GetPlayerByGUID(Guid guid)
        {
            return Instance.Descriptors.Where(x => x.Id == guid).FirstOrDefault();
        }

        internal List<Descriptor> GetDisconnectedSessions()
        {
            return Instance.Descriptors.Where(x => !x.IsConnected || (x.Player == null && x.State != ConnectionState.CreatingCharacter)).ToList();
        }

        internal Descriptor GetPlayer(string playerName)
        {
            var result = Instance.Descriptors.Where(x => x.Player != null && Regex.Match(x.Player.Name, playerName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
            return result;
        }

        internal HashSet<Descriptor> Descriptors { get; set; }

        internal async Task NewDescriptorAsync(TcpClient client)
        {
            var newDescriptor = new Descriptor(client);
            Descriptors.Add(newDescriptor);
            _ = new Connection(newDescriptor);
        }

        internal List<Descriptor> GetPlayersInRoom(uint rid)
        {
            List<Descriptor> result = new List<Descriptor>();
            lock(_lockObject)
            {
                result = Instance.Descriptors.Where(x => x.Player != null && x.Player.CurrentRoom == rid).ToList();
            }
            return result;
        }

        internal void Close(Descriptor descriptor)
        {
            EndPoint endpoint = null;
            try
            {
                endpoint = descriptor.Client.Client.RemoteEndPoint;
                var charName = descriptor.Player == null ? string.Empty : descriptor.Player.ToString();
                if (descriptor.Player != null)
                {
                    if(descriptor.Player.FollowerID != Guid.Empty)
                    {
                        NPCManager.Instance.SetNPCFollowing(ref descriptor, false);
                    }
                    if(DatabaseManager.SavePlayerNew(ref descriptor, false))
                    {
                        Game.LogMessage($"INFO: Client at {descriptor.Player.Name} on disconnection", LogLevel.Info, true);
                    }
                    else
                    {
                        Game.LogMessage($"INFO: Client at {descriptor.Player.Name} on disconnection. Player data may be out of date on reload", LogLevel.Error, true);
                    }
                }
                descriptor.Client.Close();
                Descriptors.Remove(descriptor);
                descriptor = null;
                string logMsg = string.IsNullOrEmpty(charName) ? $"INFO: Connection from {endpoint} closed, no player was associated with the connection"
                    : $"INFO: Connection from {endpoint} closed, player {charName} has left the game";
                Game.LogMessage(logMsg, LogLevel.Connection, true);
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Could not close connection from {endpoint}", LogLevel.Connection, true);
                Game.LogMessage($"ERROR: Exception at SessionManger.Close(): {ex.Message}", LogLevel.Connection, true);
            }
        }

        internal void SendToAllClients(string message)
        {
            foreach (var client in from client in Descriptors
                                   where client.IsConnected && client.Player != null
                                   select client)
            {
                client.Send(message);
            }
        }
    }
}
