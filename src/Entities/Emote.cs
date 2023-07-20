using Kingdoms_of_Etrea.Core;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Emote
    {
        [JsonProperty]
        internal uint EmoteID { get; set; }
        [JsonProperty]
        internal string EmoteName { get; set; }
        [JsonProperty]
        internal string MsgToPlayerWithTarget { get; set; }
        [JsonProperty]
        internal string MsgToPlayerWithNoTarget { get; set; }
        [JsonProperty]
        internal string MsgToTarget { get; set; }
        [JsonProperty]
        internal string MsgToOthersWithTarget { get; set; }
        [JsonProperty]
        internal string MsgToOthersWithNoTarget { get; set; }
        [JsonProperty]
        internal string MsgToOthersWithVisPlayerAndInvisTarget { get; set; }
        [JsonProperty]
        internal string MsgToOthersWithInvisPlayerAndTarget { get; set; }
        [JsonProperty]
        internal string MsgToPlayerTargetNotFound { get; set; }
        [JsonProperty]
        internal string MsgToOthersTargetNotFound { get; set; }
        [JsonProperty]
        internal string MsgToOthersWhenTargetIsPlayer { get; set; }
        [JsonProperty]
        internal string MsgToPlayerWhenTargetIsPlayer { get; set; }

        internal void ShowEmoteMessage(ref Descriptor desc, string target)
        {
            var playerName = desc.Player.Name;
            if(string.IsNullOrEmpty(target))
            {
                desc.Send($"{MsgToPlayerWithNoTarget}{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                if(localPlayers != null && localPlayers.Count > 1)
                {
                    foreach(var player in localPlayers.Where(x => x.Player.Name != playerName))
                    {
                        var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ?
                            $"{MsgToOthersWithNoTarget.Replace("{Player}", playerName)}{Constants.NewLine}" :
                            $"{MsgToOthersWithNoTarget.Replace("{Player}", "Something")}{Constants.NewLine}";
                        player.Send(msg);
                    }
                }
            }
            else
            {
                var tp = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if(tp != null)
                {
                    // we have a target player
                    if(tp.Player.Name == desc.Player.Name)
                    {
                        desc.Send($"{MsgToPlayerWhenTargetIsPlayer.Replace("{Player", playerName).Replace("{Target}", playerName)}{Constants.NewLine}");
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Name != tp.Player.Name).ToList();
                        if(localPlayers != null && localPlayers.Count > 0)
                        {
                            foreach(var player in localPlayers)
                            {
                                var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ?
                                    $"{MsgToOthersWhenTargetIsPlayer.Replace("{Player}", playerName).Replace("{Target}", playerName)}{Constants.NewLine}" :
                                    $"{MsgToOthersWhenTargetIsPlayer.Replace("{Player}", "Something").Replace("{Target}", "Something")}{Constants.NewLine}";
                                player.Send(msg);
                            }
                        }
                    }
                    else
                    {
                        if (tp.Player.Visible)
                        {
                            desc.Send($"{MsgToPlayerWithTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}");
                            var targetMsg = desc.Player.Visible || tp.Player.Level >= Constants.ImmLevel ?
                                $"{MsgToTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}" :
                                $"{MsgToTarget.Replace("{Player}", "Something").Replace("{Target}", tp.Player.Name)}{Constants.NewLine}";
                            tp.Send(targetMsg);
                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            if (localPlayers != null && localPlayers.Count > 1)
                            {
                                foreach (var player in localPlayers.Where(x => x.Player.Name != playerName && x.Player.Name != tp.Player.Name))
                                {
                                    var msg = $"{MsgToOthersWithTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}";
                                    tp.Send(msg);
                                }
                            }
                        }
                        else
                        {
                            // target player is invisible
                            if (desc.Player.Level >= Constants.ImmLevel)
                            {
                                desc.Send($"{MsgToPlayerWithTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}");
                                var targetMsg = desc.Player.Visible || tp.Player.Level >= Constants.ImmLevel ?
                                    $"{MsgToTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}" :
                                    $"{MsgToTarget.Replace("{Player}", "Something").Replace("{Target}", tp.Player.Name)}{Constants.NewLine}";
                                tp.Send(targetMsg);
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if (localPlayers != null && localPlayers.Count > 1)
                                {
                                    foreach (var player in localPlayers.Where(x => x.Player.Name != playerName && x.Player.Name != tp.Player.Name))
                                    {
                                        if (player.Player.Level >= Constants.ImmLevel)
                                        {
                                            player.Send($"{MsgToOthersWithTarget.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (desc.Player.Visible)
                                            {
                                                // target is invis, player is vis, 3rd player does not have ability to see invis
                                                player.Send($"{MsgToOthersWithVisPlayerAndInvisTarget.Replace("{Player}", playerName).Replace("{Target}", "Something")}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                // target is invis, player invis, 3rd player does not have ability to see invis
                                                player.Send($"{MsgToOthersWithInvisPlayerAndTarget.Replace("{Player}", "Something").Replace("{Target}", "Something")}{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // target is invisible and we're not an Imm
                                desc.Send($"{MsgToPlayerTargetNotFound.Replace("{Player}", playerName).Replace("{Target}", target)}{Constants.NewLine}");
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if (localPlayers != null && localPlayers.Count > 1)
                                {
                                    foreach (var player in localPlayers.Where(x => x.Player.Name != playerName))
                                    {
                                        var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ?
                                            $"{MsgToOthersTargetNotFound.Replace("{Player}", playerName).Replace("{Target}", tp.Player.Name)}{Constants.NewLine}" :
                                            $"{MsgToOthersTargetNotFound.Replace("{Player}", "Something").Replace("{Target}", "Something")}{Constants.NewLine}";
                                        player.Send(msg);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    var tnpc = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if (tnpc != null)
                    {
                        // target is an npc
                        desc.Send($"{MsgToPlayerWithTarget.Replace("{Player}", playerName).Replace("{Target}", tnpc.Name)}{Constants.NewLine}");
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if(localPlayers != null && localPlayers.Count > 1)
                        {
                            foreach(var player in localPlayers.Where(x => x.Player.Name != playerName))
                            {
                                var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ?
                                    $"{MsgToOthersWithTarget.Replace("{Player}", playerName).Replace("{Target}", tnpc.Name)}{Constants.NewLine}" :
                                    $"{MsgToOthersWithTarget.Replace("{Player}", "Something").Replace("{Target}", tnpc.Name)}{Constants.NewLine}";
                                player.Send(msg);
                            }
                        }
                    }
                    else
                    {
                        // target can't be found
                        desc.Send($"{MsgToPlayerTargetNotFound.Replace("{Player}", desc.Player.Name).Replace("{Target}", target)}{Constants.NewLine}");
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if(localPlayers != null && localPlayers.Count > 1)
                        {
                            foreach(var player in localPlayers.Where(x => x.Player.Name != playerName))
                            {
                                var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ?
                                    $"{MsgToOthersTargetNotFound.Replace("{Player}", playerName).Replace("{Target}", target)}{Constants.NewLine}" :
                                    $"{MsgToOthersTargetNotFound.Replace("{Player}", "Something").Replace("{Target}", target)}{Constants.NewLine}";
                                player.Send(msg);
                            }
                        }
                    }
                }
            }
        }
    }
}
