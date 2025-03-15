using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etrea3.Core
{
    public class SessionManager
    {
        private static SessionManager instance = null;
        private ConcurrentDictionary<Guid, Session> Sessions;
        public int Count => Sessions.Count;
        public List<Session> AllSessions => Sessions.Values.ToList();
        public List<Session> ActivePlayers => Sessions.Values.Where(x => x.IsConnected && x.Player != null).ToList();
        public List<Session> IdleSessions => Sessions.Values.Where(x => x.IsConnected && x.Player != null && (DateTime.UtcNow - x.LastInputTime).TotalSeconds > Game.MaxIdleSeconds).ToList();
        public List<Session> DisconnectedSessions => Sessions.Values.Where(x => !x.IsConnected || ((x.Player == null && x.State != ConnectionState.CreatingCharacter)
        || (x.Player == null && (DateTime.UtcNow - x.ConnectionTime).TotalSeconds > Game.MaxIdleSeconds))).ToList();
        public List<Session> Immortals => Sessions.Values.Where(x => x.IsConnected && x.Player != null && x.Player.IsImmortal).ToList();

        private SessionManager()
        {
            Sessions = new ConcurrentDictionary<Guid, Session>();
        }

        public static SessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SessionManager();
                }
                return instance;
            }
        }

        public async Task NewSession(TcpClient tcpClient)
        {
            try
            {
                var newSession = new Session(tcpClient);
                Instance.Sessions.TryAdd(newSession.ID, newSession);
                _ = new Connection(newSession);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error setting up new Session for {tcpClient?.Client?.RemoteEndPoint}: {ex.Message}", LogLevel.Error);
            }
        }

        public void UpdateSessionStatus(Guid id, ConnectionState state)
        {
            if (Instance.Sessions.ContainsKey(id))
            {
                Instance.Sessions[id].State = state;
            }
        }

        public bool DisconnectBlockedIPSessions(string ip, out int droppedSessions)
        {
            droppedSessions = 0;
            try
            {
                var sessions = Instance.Sessions.Values.Where(x => x.IsConnected && Regex.IsMatch(x.Client.Client.RemoteEndPoint.ToString(), ip)).ToList();
                foreach (var session in sessions)
                {
                    try
                    {
                        session.Send($"Your IP address has been banned, this connection will be dropped.{Constants.NewLine}");
                        session.Disconnect();
                        droppedSessions++;
                    }
                    catch (Exception ex)
                    {
                        Game.LogMessage($"ERROR: Error disconnecting session {session?.Client?.Client?.RemoteEndPoint}: {ex.Message}", LogLevel.Error);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SessionManager.DisconnectBlockedIPSessions(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public void Close(Session session)
        {
            EndPoint endPoint = null;
            try
            {
                endPoint = session.Client.Client?.RemoteEndPoint;
                var charName = session.Player?.Name ?? string.Empty;
                if (session.Player != null)
                {
                    if (DatabaseManager.SavePlayer(session, false))
                    {
                        Game.LogMessage($"INFO: Player {session.Player.Name} at {session.Client.Client.RemoteEndPoint} saved on disconnection", LogLevel.Info);
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Player {session.Player.Name} at {session.Client.Client.RemoteEndPoint} failed to save on disconnection, player data may be out of date on reload", LogLevel.Error);
                    }
                    var localPlayers = Instance.GetPlayersInRoom(session.Player.CurrentRoom).Where(x => x.ID != session.ID).ToList();
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        foreach (var player in localPlayers)
                        {
                            string msg = session.Player.CanBeSeenBy(player.Player) ? $"{session.Player.Name} slowly fades out of reality.{Constants.NewLine}" :
                                $"Something slowly fades out of reality.{Constants.NewLine}";
                            player.Send(msg);
                        }
                    }
                }
                session.Disconnect();
                Instance.Sessions.TryRemove(session.ID, out _);
                session = null;
                string logMessage = string.IsNullOrEmpty(charName) ? $"CONNECTION: Connection from {endPoint} closed, no player associated with the connection" :
                    $"CONNECTION: Connection from {endPoint} closed, player {charName} has left the game";
                Game.LogMessage(logMessage, LogLevel.Connection);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SessionManager.Close(): {ex.Message}", LogLevel.Error);
            }
        }

        public void RemovePlayerCombatTarget(Guid id)
        {
            foreach (var p in Instance.Sessions.Values.Where(x => x.Player != null && x.Player.TargetQueue.Count> 0))
            {
                p.Player.TargetQueue.TryRemove(id, out _);
            }
        }

        public Session GetSession(string playerName)
        {
            return string.IsNullOrEmpty(playerName) ? null : Instance.Sessions.Values.FirstOrDefault(x => x.Player != null && x.Player.Name.IndexOf(playerName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public Session GetSession(Guid playerID)
        {
            if (Instance.Sessions.ContainsKey(playerID))
            {
                return Instance.Sessions[playerID];
            }
            return null;
        }

        public List<Session> GetPlayersInRoom(int roomId)
        {
            return Instance.Sessions.Values.Where(x => x.Player != null && x.Player.CurrentRoom == roomId).ToList();
        }

        public bool GetShopCustomers(Shop shop)
        {
            return Instance.Sessions.Values.Where(x => x.Player != null && x.Player.ShopContext == shop).Any();
        }

        public void UpdateSessionPlayer(Guid id, Player player)
        {
            Instance.GetSession(id).Player = player;
        }

        public void SetLastInputTime(Guid id, DateTime lastInputTime)
        {
            if (Instance.Sessions.ContainsKey(id))
            {
                Instance.Sessions[id].LastInputTime = lastInputTime;
            }
        }

        public void RemoveActorFromPlayerCombatQueue(Guid id)
        {
            var actorsInCombat = Instance.Sessions.Values.Where(x => x.Player.TargetQueue.ContainsKey(id)).ToList();
            if (actorsInCombat != null && actorsInCombat.Count > 0)
            {
                foreach(var a in actorsInCombat)
                {
                    a.Player.TargetQueue.TryRemove(id, out _);
                }
            }
        }

        public void SendToAllPlayers(string message)
        {
            foreach(var player in Instance.Sessions.Values.Where(x => x.Player != null && x.IsConnected))
            {
                player.Send(message);
            }
        }
    }
}
