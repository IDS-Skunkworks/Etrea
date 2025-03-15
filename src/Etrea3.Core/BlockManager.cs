using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea3.Core
{
    public class BlockManager
    {
        private static BlockManager instance = null;
        private ConcurrentDictionary<string, BlockedIPAddress> BlockList { get; set; }
        public int Count => BlockList.Count;

        public BlockManager()
        {
            BlockList = new ConcurrentDictionary<string, BlockedIPAddress>();
        }

        public static BlockManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlockManager();
                }
                return instance;
            }
        }

        public bool LoadBlockList()
        {
            if (!DatabaseManager.LoadBlockList(out var blocklist) || blocklist == null)
            {
                return false;
            }
            foreach (var block in blocklist)
            {
                Instance.BlockList.TryAdd(block.Key, block.Value);
            }
            return true;
        }

        public bool IsIPAddressBanned(string ip)
        {
            return Instance.BlockList.ContainsKey(ip);
        }

        public BlockedIPAddress GetBlockedIP(string ip)
        {
            if (Instance.BlockList.TryGetValue(ip, out BlockedIPAddress address))
            {
                return address;
            }
            return null;
        }

        public List<BlockedIPAddress> GetBlockedIPs(string ip)
        {
            var ips = Instance.BlockList.Values.Where(x => Regex.IsMatch(x.IPAddress, ip)).ToList();
            return ips;
        }

        public List<BlockedIPAddress> GetBlockedIPs()
        {
            return Instance.BlockList.Values.ToList();
        }

        public bool AddBlockedIPAddress(string ip, Session session)
        {
            try
            {
                BlockedIPAddress blockAddress = new BlockedIPAddress
                {
                    BlockedBy = session.Player.Name,
                    IPAddress = ip,
                    BlockedDateTime = DateTime.UtcNow
                };
                if (Instance.BlockList.ContainsKey(ip))
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} attempted to block IP address {ip}, but it is already in the block list", LogLevel.Error);
                    session.Send($"%BRT%The IP address {ip} is already in the block list.%PT%{Constants.NewLine}");
                    return false;
                }
                if (!DatabaseManager.AddBlockedIPAddress(blockAddress, out string errMsg))
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} attempted to block IP address {ip} but an error was encountered saving to the World Database: {errMsg}", LogLevel.Error);
                    session.Send($"%BRT%An error was encountered attempting to add the IP address to the block list.%PT%{Constants.NewLine}");
                    return false;
                }
                if (!Instance.BlockList.TryAdd(ip, blockAddress))
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} attempted to block IP address {ip} but BlockManager could not be updated", LogLevel.Error);
                    session.Send($"%BRT%Failed to add {ip} to the list of blocked IP addresses.%PT%{Constants.NewLine}");
                    return false;
                }
                Game.LogMessage($"INFO: Player {session.Player.Name} has added IP address {ip} to the block list", LogLevel.God);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in BlockManager.AddBlockedIPAddress() attempting to block {ip}: {ex.Message}", LogLevel.Error);
                session.Send($"%BRT%An error was encountered attempting to add the IP address to the block list.%PT%{Constants.NewLine}");
                return false;
            }
        }

        public bool RemoveBlockedIPAddress(string ip, Session session)
        {
            try
            {
                if (!Instance.BlockList.ContainsKey(ip))
                {
                    Game.LogMessage($"WARN: Player {session.Player.Name} attempted to remove IP address {ip} from the block list, but no such entry was found", LogLevel.Warning);
                    session.Send($"%BRT%No such IP address was found in the block list.%PT%{Constants.NewLine}");
                    return false;
                }
                if (!DatabaseManager.RemoveBlockedIPAddress(ip, out string errMsg))
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} attempted to remove {ip} from the block list, but an error was encountered updating the World Database: {errMsg}", LogLevel.Error);
                    session.Send($"%BRT%An error was encountered attempting to remove the IP address from the block list.%PT%{Constants.NewLine}");
                    return false;
                }
                if (!Instance.BlockList.TryRemove(ip, out _))
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} attempted to remove {ip} from the block list, but an Block Manager could not be updated", LogLevel.Error);
                    session.Send($"%BRT%An error was encountered attempting to remove the IP address from the block list.%PT%{Constants.NewLine}");
                    return false;
                }
                Game.LogMessage($"INFO: Player {session.Player.Name} has removed IP address {ip} from the block list", LogLevel.God);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in BlockManager.RemoveBlockedIPAddress(): {ex.Message}", LogLevel.Error);
                session.Send($"%BRT%An error was encountered attempting to remove the IP address from the block list.%PT%{Constants.NewLine}");
                return false;
            }
        }
    }
}
