using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal static partial class CommandParser
    {
        private static void ExorciseCursedItem(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Exorcist))
            {
                var verb = GetVerb(ref input);
                var targetSlot = input.Remove(0, verb.Length).Trim();
                if (!string.IsNullOrEmpty(targetSlot))
                {
                    if (targetSlot.ToLower() == "price")
                    {
                        var p = Helpers.GetNewPurchasePrice(ref desc, 2500);
                        desc.Send($"The priest looks solemn. \"Removing this curse will cost {p:N0} gold.\"{Constants.NewLine}");
                        return;
                    }
                    InventoryItem i = null;
                    switch (targetSlot.ToLower())
                    {
                        case "head":
                            if (desc.Player.EquipHead != null && desc.Player.EquipHead.IsCursed)
                            {
                                i = desc.Player.EquipHead;
                            }
                            else
                            {
                                desc.Send($"You aren't wearing anything cursed on your head!{Constants.NewLine}");
                            }
                            break;

                        case "neck":
                            if (desc.Player.EquipNeck != null && desc.Player.EquipNeck.IsCursed)
                            {
                                i = desc.Player.EquipNeck;
                            }
                            else
                            {
                                desc.Send($"You aren't wearing anything cursed around your neck!{Constants.NewLine}");
                            }
                            break;

                        case "armour":
                            if (desc.Player.EquipArmour != null && desc.Player.EquipArmour.IsCursed)
                            {
                                i = desc.Player.EquipArmour;
                            }
                            else
                            {
                                desc.Send($"Your armour isn't cursed!{Constants.NewLine}");
                            }
                            break;

                        case "weapon":
                            if (desc.Player.EquipWeapon != null && desc.Player.EquipWeapon.IsCursed)
                            {
                                i = desc.Player.EquipWeapon;
                            }
                            else
                            {
                                desc.Send($"Your weapon isn't cursed!{Constants.NewLine}");
                            }
                            break;

                        case "held":
                            if (desc.Player.EquipHeld != null && desc.Player.EquipHeld.IsCursed)
                            {
                                i = desc.Player.EquipHeld;
                            }
                            else
                            {
                                desc.Send($"You aren't holding anything cursed!{Constants.NewLine}");
                            }
                            break;

                        case "fingerright":
                            if (desc.Player.EquipRightFinger != null && desc.Player.EquipRightFinger.IsCursed)
                            {
                                i = desc.Player.EquipRightFinger;
                            }
                            else
                            {
                                desc.Send($"You aren't wearing anything with a curse on your right hand!{Constants.NewLine}");
                            }
                            break;

                        case "fingerleft":
                            if (desc.Player.EquipLeftFinger != null && desc.Player.EquipLeftFinger.IsCursed)
                            {
                                i = desc.Player.EquipLeftFinger;
                            }
                            else
                            {
                                desc.Send($"You aren't wearing anything with a curse on your left hand!{Constants.NewLine}");
                            }
                            break;
                    }
                    if (i != null)
                    {
                        var removePrice = Helpers.GetNewPurchasePrice(ref desc, 2500);
                        if (removePrice > desc.Player.Gold)
                        {
                            desc.Send($"The exorcist gives you a solemn look. \"You can't afford that, it seems.\"{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"The exorcist stashes your gold in his robe. \"Excellent! One moment...\"{Constants.NewLine}");
                            desc.Player.Gold -= removePrice;
                            desc.Player.Inventory.Add(i);
                            switch (targetSlot.ToLower())
                            {
                                case "head":
                                    desc.Player.EquipHead = null;
                                    break;

                                case "neck":
                                    desc.Player.EquipNeck = null;
                                    break;

                                case "armour":
                                    desc.Player.EquipArmour = null;
                                    break;

                                case "weapon":
                                    desc.Player.EquipWeapon = null;
                                    break;

                                case "held":
                                    desc.Player.EquipHeld = null;
                                    break;

                                case "fingerright":
                                    desc.Player.EquipRightFinger = null;
                                    break;

                                case "fingerleft":
                                    desc.Player.EquipLeftFinger = null;
                                    break;

                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"Usage: exorcise <price | equipment slot>{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no one here that can help exorcise cursed items...{Constants.NewLine}");
            }
        }

        private static void PlayerLanguages(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input);
            var line = input.Remove(0, verb.Length).Trim();
            if (string.IsNullOrEmpty(line))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"You are currently speaking {desc.Player.SpokenLanguage}");
                sb.AppendLine($"You know the following languages: {desc.Player.KnownLanguages}");
                desc.Send(sb.ToString());
            }
            else
            {
                var lang = ParseEnumValue<Languages>(ref line);
                if (desc.Player.KnownLanguages.HasFlag(lang))
                {
                    desc.Player.SpokenLanguage = lang;
                    desc.Send($"You are now speaking {lang}.{Constants.NewLine}");
                }
                else
                {
                    desc.Send($"You don't know that language!{Constants.NewLine}");
                }
            }
        }

        private static void PlayerVault(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.ItemVault))
            {
                var verb = GetVerb(ref input);
                var line = input.Remove(0, verb.Length).Trim();
                var tokens = TokeniseInput(ref line);
                // vault deposit fox
                if (tokens.Length >= 1)
                {
                    switch (tokens[0].ToLower().Trim())
                    {
                        case "check":
                        case "c":
                            if (desc.Player.VaultStore != null && desc.Player.VaultStore.Count > 0)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"The vault wardern rifles through his paperwork. \"You have the following things in storage.\"");
                                sb.AppendLine($"  {new string('=', 77)}");
                                foreach (var o in desc.Player.VaultStore.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                                {
                                    var cnt = desc.Player.VaultStore.Where(x => x.ID == o.ID).Count();
                                    sb.AppendLine($"|| {cnt} x {o.Name}, {o.ShortDescription}");
                                }
                                sb.AppendLine($"  {new string('=', 77)}");
                                desc.Send(sb.ToString());
                            }
                            else
                            {
                                desc.Send($"The vault wardern rifles through his paperwork. \"You don't have anything in storage,\" he says.{Constants.NewLine}");
                            }
                            break;

                        case "store":
                        case "s":
                            var item = line.Remove(0, tokens[0].Length).Trim();
                            var i = GetTargetItem(ref desc, item, true);
                            if (i != null)
                            {
                                desc.Player.Inventory.Remove(i);
                                desc.Player.VaultStore.Add(i);
                                desc.Send($"The vault warden smiles, \"Certainly, we'll keep that safe for you, you can collect it any time.\"{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You don't seem to be carrying anything like that.{Constants.NewLine}");
                            }
                            break;

                        case "withdraw":
                        case "w":
                            var vItem = line.Remove(0, tokens[0].Length).Trim();
                            var vaultItem = GetTargetItem(ref desc, vItem, false, true);
                            if (vaultItem != null)
                            {
                                desc.Player.VaultStore.Remove(vaultItem);
                                desc.Player.Inventory.Add(vaultItem);
                                desc.Send($"The vault warden smiles, \"Certainly, returned to you safe and sound!\"{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"The vault warden tuts, \"You don't have anything like that in storage.\"{Constants.NewLine}");
                            }
                            break;

                        default:
                            desc.Send($"Usage: vault <<check> | <store | withdraw> <item>{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    desc.Send($"Usage: vault <<check> | <store | withdraw> <item>{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no vault here...{Constants.NewLine}");
            }
        }

        private static void PlayerBanking(ref Descriptor desc, ref string input)
        {
            // bank balance
            // bank <deposit | withdraw> <amount>
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Banker))
            {
                var verb = GetVerb(ref input);
                var line = input.Remove(0, verb.Length).Trim();
                var tokens = TokeniseInput(ref line);
                if (tokens.Length >= 1)
                {
                    switch (tokens[0].ToLower().Trim())
                    {
                        case "b":
                        case "balance":
                            desc.Send($"The bank teller files through some papers. \"Your current balance is {desc.Player.BankBalance:N0} gold coins.\"{Constants.NewLine}");
                            break;

                        case "d":
                        case "deposit":
                            if (tokens.Length == 2)
                            {
                                if (uint.TryParse(tokens[1].Trim(), out uint depositAmount))
                                {
                                    if (depositAmount > 0 && depositAmount <= desc.Player.Gold)
                                    {
                                        desc.Player.Gold -= depositAmount;
                                        desc.Player.BankBalance += depositAmount;
                                        desc.Send($"You have successfully deposited {depositAmount} gold in your account!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"The bank teller looks confused. \"You must deposit at least 1 gold, and you can't depsoit more gold than you have!\"{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"The bank teller looks confused and mutters \"That doesn't seem right...\"{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"Usage: bank balance - view your current bank balance{Constants.NewLine}");
                                desc.Send($"{Constants.TabStop}bank <deposit | withdraw> <amount> - to deposit or withdraw gold{Constants.NewLine}");
                            }
                            break;

                        case "w":
                        case "withdraw":
                            if (tokens.Length == 2)
                            {
                                if (uint.TryParse(tokens[1].Trim(), out uint withdrawAmount))
                                {
                                    if (withdrawAmount > 0 && withdrawAmount <= desc.Player.BankBalance)
                                    {
                                        desc.Player.BankBalance -= withdrawAmount;
                                        desc.Player.Gold += withdrawAmount;
                                        desc.Send($"You have successfully withdrawn {withdrawAmount} gold from your account!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"The bank teller looks confused. \"You must withdraw at least 1 gold, and you can't withdraw more gold than is in your account!\"{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"The bank teller looks confused and mutters \"That doesn't seem right...\"{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"Usage: bank balance - view your current bank balance{Constants.NewLine}");
                                desc.Send($"{Constants.TabStop}bank <deposit | withdraw> <amount> - to deposit or withdraw gold{Constants.NewLine}");
                            }
                            break;

                        default:
                            desc.Send($"Sorry, I don't understand...{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    desc.Send($"Usage: bank balance - view your current bank balance{Constants.NewLine}");
                    desc.Send($"{Constants.TabStop}bank <deposit | withdraw> <amount> - to deposit or withdraw gold{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no banker here...{Constants.NewLine}");
            }
        }

        private static void MovePlayer(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = string.Empty;
            if (verb.Length <= 2)
            {
                switch (verb)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            else
            {
                direction = verb;
            }
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection(direction))
            {
                if (desc.Player.Position == ActorPosition.Standing)
                {
                    var s = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RequiredSkill;
                    if (s == null || desc.Player.HasSkill(s.Name))
                    {
                        var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor;
                        if (d == null || (d != null && d.IsOpen))
                        {
                            desc.Player.Move(desc.Player.CurrentRoom, RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).DestinationRoomID, false);
                        }
                        else
                        {
                            desc.Send($"A doorway blocks your path in that direction!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You need the {s.Name} skill to go that way!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You are {desc.Player.Position.ToString().ToLower()}, and don't feel like moving right now...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You cannot go that way!{Constants.NewLine}");
            }
        }

        private static void OpenOrCloseDoor(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = input.Remove(0, verb.Length).Trim().ToLower();
            if (direction.Length <= 2)
            {
                switch (direction)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            if (!string.IsNullOrEmpty(direction))
            {
                var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if (r.HasExitInDirection(direction))
                {
                    var d = r.GetRoomExit(direction).RoomDoor;
                    if (d != null)
                    {
                        List<Descriptor> localPlayers;
                        switch (verb.ToLower())
                        {
                            case "open":
                                if (d.IsLocked)
                                {
                                    desc.Send($"The door is locked!{Constants.NewLine}");
                                }
                                else
                                {
                                    RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsOpen = true;
                                    desc.Send($"You open the door {direction}.{Constants.NewLine}");
                                    localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                    if (localPlayers != null && localPlayers.Count > 1)
                                    {
                                        var pn = desc.Player.Name;
                                        foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                        {
                                            if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                            {
                                                p.Send($"{pn} opens the door {direction}.{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                p.Send($"Something opens the door {direction}.{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                break;

                            case "close":
                                RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsOpen = false;
                                desc.Send($"You close the door {direction}.{Constants.NewLine}");
                                localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if (localPlayers != null && localPlayers.Count > 1)
                                {
                                    var pn = desc.Player.Name;
                                    foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                    {
                                        if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                        {
                                            p.Send($"{pn} closes the door {direction}.{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            p.Send($"Something closes the door {direction}.{Constants.NewLine}");
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"There is no door on the exit {direction}...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"There is no exit {direction}...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void LockOrUnlockDoor(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = input.Remove(0, verb.Length).Trim().ToLower();
            if (direction.Length <= 2)
            {
                switch (direction)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            if (!string.IsNullOrEmpty(direction))
            {
                var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if (r.HasExitInDirection(direction))
                {
                    var d = r.GetRoomExit(direction).RoomDoor;
                    if (d != null)
                    {
                        switch (verb.ToLower())
                        {
                            case "lock":
                                if (!d.IsOpen)
                                {
                                    if (!d.IsLocked)
                                    {
                                        if (d.RequiredItemID == 0 || desc.Player.HasItemInInventory(d.RequiredItemID))
                                        {
                                            RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsLocked = true;
                                            desc.Send($"You lock the door {direction}.{Constants.NewLine}");
                                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                            if (localPlayers != null && localPlayers.Count > 1)
                                            {
                                                var pn = desc.Player.Name;
                                                foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                                {
                                                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                    {
                                                        p.Send($"{pn} locks the door {direction}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        p.Send($"Something locks the door {direction}.{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"You lack the correct key to lock the door...{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"The door is already locked...{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"The door must be shut first...{Constants.NewLine}");
                                }
                                break;

                            case "unlock":
                                if (d.IsOpen)
                                {
                                    desc.Send($"The door is already open...{Constants.NewLine}");
                                }
                                else
                                {
                                    if (!d.IsLocked)
                                    {
                                        desc.Send($"The door is already unlocked...{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        if (d.RequiredItemID == 0 || desc.Player.HasItemInInventory(d.RequiredItemID))
                                        {
                                            RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsLocked = false;
                                            desc.Send($"You unlock the door {direction}.{Constants.NewLine}");
                                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                            if (localPlayers != null && localPlayers.Count > 1)
                                            {
                                                var pn = desc.Player.Name;
                                                foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                                {
                                                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                    {
                                                        p.Send($"{pn} unlocks the door {direction}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        p.Send($"Something unlocks the door {direction}.{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"You lack the correct key to unlock the door...{Constants.NewLine}");
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    desc.Send($"There is no exit to the {direction}...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void PlayerQuests(ref Descriptor desc, ref string input)
        {
            // usage quest(s) - show current active quests
            // quest(s) <list | accept | abandon | return>
            var verb = GetVerb(ref input).Trim();
            var line = input.Remove(0, verb.Length).Trim();
            var elements = TokeniseInput(ref line);
            var operation = elements.Length >= 1 ? elements[0].ToLower().Trim() : string.Empty;
            if (!string.IsNullOrEmpty(operation))
            {
                var questList = QuestManager.Instance.GetQuestsForZone(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ZoneID);
                switch (operation)
                {
                    case "list":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var completedQuestIds = desc.Player.CompletedQuests;
                            var zoneQuests = new List<Quest>();
                            if (questList != null && questList.Count > 0)
                            {
                                foreach (var q in questList)
                                {
                                    if (!completedQuestIds.Contains(q.QuestGUID))
                                    {
                                        zoneQuests.Add(q);
                                    }
                                }
                            }
                            if (zoneQuests != null && zoneQuests.Count > 0)
                            {
                                int i = 0, n = 1;
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"  {new string('=', 77)}");
                                foreach (var q in zoneQuests)
                                {
                                    sb.AppendLine($"|| ID: {n}");
                                    sb.AppendLine($"|| Name: {q.QuestName}{Constants.TabStop}{Constants.TabStop}Quest Type: {q.QuestType}");
                                    sb.AppendLine($"|| Description:");
                                    var descLines = q.QuestText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                    foreach (var l in descLines)
                                    {
                                        sb.AppendLine($"|| {l}");
                                    }
                                    if (q.Monsters != null && q.Monsters.Count > 0)
                                    {
                                        sb.AppendLine($"|| Kill Monsters:");
                                        foreach (var m in q.Monsters)
                                        {
                                            sb.AppendLine($"|| {m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                                        }
                                    }
                                    if (q.FetchItems != null && q.FetchItems.Count > 0)
                                    {
                                        sb.AppendLine($"|| Obtain Items:");
                                        foreach (var item in q.FetchItems)
                                        {
                                            sb.AppendLine($"|| {item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                                        }
                                    }
                                    sb.AppendLine($"|| Gold: {q.RewardGold}{Constants.TabStop}Exp: {q.RewardExp}");
                                    if (q.RewardItems != null && q.RewardItems.Count > 0)
                                    {
                                        sb.AppendLine($"|| Reward Items:");
                                        foreach (var ri in q.RewardItems)
                                        {
                                            sb.AppendLine($"|| {ri.Name}");
                                        }
                                    }
                                    i++;
                                    n++;
                                    if (i < zoneQuests.Count)
                                    {
                                        sb.AppendLine($"||{new string('=', 77)}");
                                    }
                                }
                                sb.AppendLine($"  {new string('=', 77)}");
                                desc.Send(sb.ToString());
                            }
                            else
                            {
                                desc.Send($"There are no Quests available at the moment.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"There is no Questmaster here!{Constants.NewLine}");
                        }
                        break;

                    case "accept":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var iIndex = elements.Last();
                            if (int.TryParse(iIndex, out int qID))
                            {
                                if (qID > 0 && qID <= questList.Count)
                                {
                                    var q = questList[qID - 1];
                                    if (!desc.Player.ActiveQuests.Any(x => x.QuestGUID == q.QuestGUID))
                                    {
                                        desc.Player.ActiveQuests.Add(q);
                                        desc.Send($"You have accepted the quest!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"You have already accepted that Quest!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Sorry, that Quest doesn't seem to exist...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                        break;

                    case "return":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var iIndex = elements.Last();
                            if (int.TryParse(iIndex, out int qID))
                            {
                                if (qID > 0 && qID <= desc.Player.ActiveQuests.Count)
                                {
                                    var q = desc.Player.ActiveQuests[qID - 1];
                                    bool isComplete = true;
                                    if (q.Monsters != null && q.Monsters.Count > 0)
                                    {
                                        if (q.Monsters.Values.Any(x => x > 0))
                                        {
                                            isComplete = false;
                                        }
                                    }
                                    if (q.FetchItems != null && q.FetchItems.Count > 0)
                                    {
                                        foreach (var i in q.FetchItems)
                                        {
                                            if (desc.Player.Inventory.Where(x => x.ID == i.Key).Count() < i.Value)
                                            {
                                                isComplete = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (isComplete)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        desc.Player.CompletedQuests.Add(q.QuestGUID);
                                        desc.Player.ActiveQuests.Remove(q);
                                        desc.Player.AddExp(q.RewardExp, true, true);
                                        desc.Player.AddGold(q.RewardGold, true);
                                        if (q.FetchItems != null && q.FetchItems.Count > 0)
                                        {
                                            foreach (var i in q.FetchItems)
                                            {
                                                var item = desc.Player.Inventory.Where(x => x.ID == i.Key).FirstOrDefault();
                                                for (int n = 0; n < i.Value; n++)
                                                {
                                                    desc.Player.Inventory.Remove(item);
                                                }
                                            }
                                        }
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        sb.AppendLine($"|| Quest Completed!");
                                        sb.AppendLine($"|| Exp: {q.RewardExp}");
                                        sb.AppendLine($"|| Gold: {q.RewardGold}");
                                        if (q.RewardItems != null && q.RewardItems.Count > 0)
                                        {
                                            sb.AppendLine($"|| You are awarded the following items:");
                                            foreach (var i in q.RewardItems)
                                            {
                                                sb.AppendLine($"|| {i}");
                                                desc.Player.Inventory.Add(i);
                                            }
                                        }
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        desc.Send(sb.ToString());
                                    }
                                    else
                                    {
                                        desc.Send($"You have not met the requirements to complete this Quest!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Sorry, you don't seem to have that Quest active...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                        break;

                    case "abandon":
                        var inputIndex = elements.Last();
                        if (int.TryParse(inputIndex, out int qid))
                        {
                            if (qid > 0 && qid <= desc.Player.ActiveQuests.Count)
                            {
                                var q = desc.Player.ActiveQuests[qid - 1];
                                desc.Player.ActiveQuests.Remove(q);
                                desc.Send($"You have abandonned the Quest!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"Sorry, you don't seem to have that Quest active...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                        break;

                    default:
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        break;
                }
            }
            else
            {
                // no operation so show list of active quests the player has
                if (desc.Player.ActiveQuests != null && desc.Player.ActiveQuests.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    int i = 0, n = 1;
                    foreach (var q in desc.Player.ActiveQuests)
                    {
                        sb.AppendLine($"|| Number: {n}");
                        sb.AppendLine($"|| Name: {q.QuestName}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(q.QuestZone).ZoneName}");
                        if (q.Monsters != null && q.Monsters.Count > 0)
                        {
                            sb.AppendLine($"|| Kill Monsters:");
                            foreach (var m in q.Monsters)
                            {
                                sb.AppendLine($"|| {m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                            }
                        }
                        if (q.FetchItems != null && q.FetchItems.Count > 0)
                        {
                            sb.AppendLine("|| Obtain Items:");
                            foreach (var item in q.FetchItems)
                            {
                                sb.AppendLine($"|| {item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                            }
                        }
                        sb.AppendLine($"|| Gold: {q.RewardGold}{Constants.TabStop}Exp: {q.RewardExp}");
                        if (q.RewardItems != null && q.RewardItems.Count > 0)
                        {
                            sb.AppendLine($"|| Items:");
                            foreach (var item in q.RewardItems)
                            {
                                sb.AppendLine($"|| {item.Name}");
                            }
                        }
                        i++;
                        n++;
                        if (i < desc.Player.ActiveQuests.Count)
                        {
                            sb.AppendLine($"||{new string('=', 77)}");
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    desc.Send($"You have no active Quests at the moment.{Constants.NewLine}");
                }
            }
        }

        private static void PlayerMail(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.PostBox))
            {
                // usage: mail <list | read | write | delete>
                var elements = TokeniseInput(ref input);
                string operation = string.Empty;
                uint id = 0;
                if (elements.Length > 1)
                {
                    operation = elements[1].Trim();
                }
                if (elements.Length >= 3)
                {
                    if (!uint.TryParse(elements[2].Trim(), out id))
                    {
                        desc.Send($"The given ID could not be understood.{Constants.NewLine}");
                        return;
                    }
                }
                if (string.IsNullOrEmpty(operation))
                {
                    desc.Send($"Usage: mail <list | read | write | delete>{Constants.NewLine}");
                    desc.Send($"mail list - to list all your mails{Constants.NewLine}");
                    desc.Send($"mail read <id> - to read the specified mail{Constants.NewLine}");
                    desc.Send($"mail write - to start writing a mail to another player{Constants.NewLine}");
                    desc.Send($"mail delete <id> - to delete the specified mail{Constants.NewLine}");
                    return;
                }
                switch (operation.ToLower())
                {
                    case "list":
                        var allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        if (allMails != null && allMails.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            int index = 0;
                            foreach (var m in allMails)
                            {
                                sb.AppendLine($"|| ID: {m.Key}{Constants.TabStop}{Constants.TabStop}From: {m.Value.MailFrom}{Constants.TabStop}Sent: {m.Value.MailSent}");
                                sb.AppendLine($"|| Subject: {m.Value.MailSubject}");
                                sb.AppendLine($"|| Attached Items: {m.Value.AttachedItems != null && m.Value.AttachedItems.Count > 0}{Constants.TabStop}Attached Gold: {m.Value.AttachedGold}");
                                index++;
                                if (index < allMails.Count)
                                {
                                    sb.AppendLine($"||{new string('=', 77)}");
                                }
                            }
                            sb.AppendLine($"  {new string('=', 77)}");
                            desc.Send(sb.ToString());
                        }
                        else
                        {
                            desc.Send($"It doesn't look like you have any mail...{Constants.NewLine}");
                        }
                        break;

                    case "read":
                        allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        var mailItem = allMails.ContainsKey(id) ? allMails[id] : null;
                        if (mailItem != null)
                        {
                            if (!mailItem.MailRead)
                            {
                                DatabaseManager.MarkMailAsRead(ref desc, mailItem.MailID);
                                if (mailItem.AttachedGold > 0)
                                {
                                    desc.Send($"You take {mailItem.AttachedGold} gold from the mail!{Constants.NewLine}");
                                    desc.Player.Gold += mailItem.AttachedGold;
                                }
                                if (mailItem.AttachedItems != null && mailItem.AttachedItems.Count > 0)
                                {
                                    foreach (var i in mailItem.AttachedItems)
                                    {
                                        desc.Player.Inventory.Add(i);
                                        desc.Send($"You take {i.Name} from the mail!{Constants.NewLine}");
                                    }
                                }
                            }
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            sb.AppendLine($"|| From: {mailItem.MailFrom}{Constants.TabStop}{Constants.TabStop}Sent: {mailItem.MailSent}");
                            sb.AppendLine($"|| Subject: {mailItem.MailSubject}");
                            sb.AppendLine($"|| Message:");
                            if (!string.IsNullOrEmpty(mailItem.MailBody))
                            {
                                var bodyLines = mailItem.MailBody.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in bodyLines)
                                {
                                    sb.AppendLine($"||{Constants.TabStop}{line}");
                                }
                            }
                            sb.AppendLine($"  {new string('=', 77)}");
                            desc.Send(sb.ToString());
                        }
                        else
                        {
                            desc.Send($"Couldn't find a mail with that ID number.{Constants.NewLine}");
                        }
                        break;

                    case "write":
                        if (desc.Player.Gold < 5)
                        {
                            desc.Send($"It costs 5 gold to send a mail!{Constants.NewLine}");
                            return;
                        }
                        var newMail = Mail.Compose(ref desc);
                        if (newMail != null)
                        {
                            bool ok = false;
                            bool returnItems = false;
                            while (!ok)
                            {
                                desc.Send($"Send this mail (Y/N)?{Constants.NewLine}");
                                var response = desc.Read().Trim();
                                if (Helpers.ValidateInput(response))
                                {
                                    if (response.ToLower() == "y" || response.ToLower() == "yes")
                                    {
                                        // check we have 5 gold and send
                                        if (desc.Player.Gold >= 5)
                                        {
                                            newMail.MailSent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                                            if (DatabaseManager.SendNewMail(ref desc, ref newMail))
                                            {
                                                desc.Player.Gold -= 5;
                                                desc.Send($"The Winds of Magic swirl around your letter and it vanishes!{Constants.NewLine}");
                                                ok = true;
                                            }
                                            else
                                            {
                                                desc.Send($"The Winds of Magic sputter and die, your message was not taken!.{Constants.NewLine}");
                                                returnItems = true;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"You don't have enough gold to send a mail!{Constants.NewLine}");
                                            returnItems = true;
                                        }
                                    }
                                    if (response.ToLower() == "n" || response.ToLower() == "no")
                                    {
                                        returnItems = true;
                                    }
                                    if (returnItems)
                                    {
                                        // give the player back any attached items and gold
                                        if (newMail.AttachedGold > 0)
                                        {
                                            desc.Player.Gold += newMail.AttachedGold;
                                            desc.Send($"{newMail.AttachedGold} gold has been returned to you.{Constants.NewLine}");
                                        }
                                        if (newMail.AttachedItems != null && newMail.AttachedItems.Count > 0)
                                        {
                                            foreach (var i in newMail.AttachedItems)
                                            {
                                                desc.Player.Inventory.Add(i);
                                                desc.Send($"{i.Name} has been returned to you.{Constants.NewLine}");
                                            }
                                        }
                                        ok = true;
                                    }
                                }
                                else
                                {
                                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                                }
                            }
                        }
                        break;

                    case "delete":
                        allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        mailItem = allMails.ContainsKey(id) ? allMails[id] : null;
                        if (mailItem != null)
                        {
                            if (DatabaseManager.DeleteMailByID(ref desc, mailItem.MailID))
                            {
                                desc.Send($"The Winds of Magic swirl and swallow the message, it is gone forever!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"The Winds of Magic sputter and die, your message remains!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"No such mail could be found in your mailbox!{Constants.NewLine}");
                        }
                        break;

                    default:
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        break;
                }
            }
            else
            {
                desc.Send($"There is no Mailbox here...{Constants.NewLine}");
            }
        }

        private static void ShowOrTogglePVPFlag(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim();
            var op = input.Remove(0, verb.Length).Trim();
            if (!string.IsNullOrEmpty(op))
            {
                if (op.ToLower() == "on")
                {
                    desc.Player.PVP = true;
                    desc.Send($"PVP flag set to ON, PVP is enabled.{Constants.NewLine}");
                }
                else
                {
                    desc.Player.PVP = false;
                    desc.Send($"PVP flag set to OFF, PVP is disabled.{Constants.NewLine}");
                }
            }
            else
            {
                if (desc.Player.PVP)
                {
                    desc.Send($"Your PVP flag is currently enabled!{Constants.NewLine}");
                }
                else
                {
                    desc.Send($"Your PVP flag is currently disabled!{Constants.NewLine}");
                }
            }
        }

        private static void ShowFollowerInfo(ref Descriptor desc, ref string input)
        {
            StringBuilder sb = new StringBuilder();
            var n = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
            if (n != null)
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var elements = TokeniseInput(ref line);
                if (elements.Length >= 2)
                {
                    if (elements[0].ToLower() == "show" || elements[0].ToLower() == "use" || elements[0].ToLower() == "give" || elements[0].ToLower() == "remove")
                    {
                        switch (elements.First().ToLower())
                        {
                            case "show":
                                switch (elements[1].ToLower())
                                {
                                    case "stats":
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        sb.AppendLine($"|| Name: {n.Name}");
                                        sb.AppendLine($"||");
                                        sb.AppendLine($"|| Stats:");
                                        sb.AppendLine($"|| Strength: {n.Strength} ({Helpers.CalculateAbilityModifier(n.Strength)}){Constants.TabStop}{Constants.TabStop}Dexterity: {n.Dexterity} ({Helpers.CalculateAbilityModifier(n.Dexterity)})");
                                        sb.AppendLine($"|| Constitution: {n.Constitution} ({Helpers.CalculateAbilityModifier(n.Constitution)}){Constants.TabStop}{Constants.TabStop}Intelligence: {n.Intelligence} ({Helpers.CalculateAbilityModifier(n.Intelligence)})");
                                        sb.AppendLine($"|| Wisdom: {n.Wisdom} ({Helpers.CalculateAbilityModifier(n.Wisdom)}){Constants.TabStop} {Constants.TabStop}Charisma: {n.Charisma} ({Helpers.CalculateAbilityModifier(n.Charisma)})");
                                        sb.AppendLine($"|| HP: {n.NumberOfHitDice}d{n.HitDieSize} ({n.CurrentHP}/{n.MaxHP})");
                                        sb.AppendLine($"|| MP: {n.NumberOfHitDice}d8 ({n.CurrentMP}/{n.MaxMP})");
                                        sb.AppendLine($"|| Armour Class: {n.ArmourClass}{Constants.TabStop}{Constants.TabStop}No. Of Attacks: {n.NumberOfAttacks}");
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        desc.Send(sb.ToString());
                                        break;

                                    case "equip":
                                    case "eq":
                                    case "equipment":
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        sb.AppendLine($"|| Head: {n.EquipHead?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Neck: {n.EquipNeck?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Armour: {n.EquipArmour?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Finger (L): {n.EquipLeftFinger?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Finger (R): {n.EquipRightFinger?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Weapon: {n.EquipWeapon?.Name ?? "Nothing"}");
                                        sb.AppendLine($"|| Held: {n.EquipHeld?.Name ?? "Nothing"}");
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        desc.Send(sb.ToString());
                                        break;

                                    case "inv":
                                    case "inventory":
                                        if (n.Inventory != null && n.Inventory.Count > 0)
                                        {
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            sb.AppendLine($"|| {n.Name} is carrying:");
                                            foreach (var i in n.Inventory.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                                            {
                                                var cnt = n.Inventory.Where(y => y.ID == i.ID).Count();
                                                sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription}");
                                            }
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            desc.Send(sb.ToString());
                                        }
                                        else
                                        {
                                            desc.Send($"Your follower is not carrying any items.{Constants.NewLine}");
                                        }
                                        break;
                                }
                                break;

                            case "use":
                                var itemStr = line.Replace("use", string.Empty).Trim();
                                if (!string.IsNullOrEmpty(itemStr))
                                {
                                    var invItem = n.Inventory.Where(x => Regex.Match(x.Name, itemStr, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                    if (invItem != null)
                                    {
                                        if (invItem.Slot != WearSlot.None)
                                        {
                                            switch (invItem.Slot)
                                            {
                                                case WearSlot.Head:
                                                    if (n.EquipHead == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipHead = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing an item on their head!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Neck:
                                                    if (n.EquipNeck == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipNeck = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing something around their neck!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Weapon:
                                                    if (n.EquipWeapon == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipWeapon = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already using a weapon!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Armour:
                                                    if (n.EquipArmour == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipArmour = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing something as their armour!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.FingerLeft:
                                                case WearSlot.FingerRight:
                                                    if (n.EquipLeftFinger == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipLeftFinger = invItem;
                                                        n.CalculateArmourClass();
                                                        RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        if (n.EquipRightFinger == null)
                                                        {
                                                            n.Inventory.Remove(invItem);
                                                            n.EquipRightFinger = invItem;
                                                            n.CalculateArmourClass();
                                                            RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                            desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                        }
                                                        else
                                                        {
                                                            desc.Send($"Your follower is already wearing a ring on each hand!{Constants.NewLine}");
                                                        }
                                                    }
                                                    break;

                                                case WearSlot.Held:
                                                    if (n.EquipHeld == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquipHeld = invItem;
                                                        n.CalculateArmourClass();
                                                        RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                        desc.Send($"{n.Name} starts using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already holding something!{Constants.NewLine}");
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"Your follower can't use that item!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Your follower doesn't seem to be carrying that.{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Usage: follower use <item>{Constants.NewLine}");
                                }
                                break;

                            case "give":
                                itemStr = line.Replace("give", string.Empty).Trim();
                                if (!string.IsNullOrEmpty(itemStr))
                                {
                                    var tradeItem = n.Inventory.Where(x => Regex.Match(x.Name, itemStr, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                    if (tradeItem != null)
                                    {
                                        n.Inventory.Remove(tradeItem);
                                        desc.Player.Inventory.Add(tradeItem);
                                        desc.Send($"{n.Name} gives you {tradeItem.Name}.{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"{n.Name} doesn't seem to be carrying that.{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Usage: follower give <item>{Constants.NewLine}");
                                }
                                break;

                            case "remove":
                                var slot = TokeniseInput(ref line).LastOrDefault();
                                if (!string.IsNullOrEmpty(slot))
                                {
                                    InventoryItem eqItem = null;
                                    switch (slot.Trim().ToLower())
                                    {
                                        case "head":
                                            eqItem = n.EquipHead;
                                            if (eqItem != null)
                                            {
                                                n.EquipHead = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their head!{Constants.NewLine}");
                                            }
                                            break;

                                        case "neck":
                                            eqItem = n.EquipNeck;
                                            if (eqItem != null)
                                            {
                                                n.EquipNeck = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything around their neck!{Constants.NewLine}");
                                            }
                                            break;

                                        case "armour":
                                            eqItem = n.EquipArmour;
                                            if (eqItem != null)
                                            {
                                                n.EquipArmour = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't wearing any armour!{Constants.NewLine}");
                                            }
                                            break;

                                        case "weapon":
                                            eqItem = n.EquipWeapon;
                                            if (eqItem != null)
                                            {
                                                n.EquipWeapon = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't using a weapon!{Constants.NewLine}");
                                            }
                                            break;

                                        case "held":
                                            eqItem = n.EquipHeld;
                                            if (eqItem != null)
                                            {
                                                n.EquipHeld = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't holding anything!{Constants.NewLine}");
                                            }
                                            break;

                                        case "fingerleft":
                                        case "leftfinger":
                                            eqItem = n.EquipLeftFinger;
                                            if (eqItem != null)
                                            {
                                                n.EquipLeftFinger = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their fingers!{Constants.NewLine}");
                                            }
                                            break;

                                        case "fingerright":
                                        case "rightfinger":
                                            eqItem = n.EquipRightFinger;
                                            if (eqItem != null)
                                            {
                                                n.EquipRightFinger = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their fingers!{Constants.NewLine}");
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    desc.Send($"Remove an item from which equipment slot?{Constants.NewLine}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        sb.Clear();
                        sb.AppendLine($"Usage: follower <show | use | give | remove> <command specific args>");
                        sb.AppendLine($"Usage: follower show <stats | equip | inv>: show follower stats, equipment or inventory");
                        sb.AppendLine($"Usage: follower use <item>: have your follower equip an item in their inventory");
                        sb.AppendLine($"Usage: follower give <item>: have your follower hand an item from their inventory to you");
                        sb.AppendLine($"Usage: follower remove <slot>: remove an equipped item back to the inventory");
                        desc.Send(sb.ToString());
                    }
                }
                else
                {
                    sb.Clear();
                    sb.AppendLine($"Usage: follower <show | use | give> <command specific args>");
                    sb.AppendLine($"Usage: follower show <stats | equip | inv>: show follower stats, equipment or inventory");
                    sb.AppendLine($"Usage: follower use <item>: have your follower equip an item in their inventory");
                    sb.AppendLine($"Usage: follower give <item>: have your follower hand an item from their inventory to you");
                    desc.Send(sb.ToString());
                }
            }
            else
            {
                desc.Send($"You don't have a follower at the moment.{Constants.NewLine}");
            }
        }

        private static void MineResourceNode(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode != null)
            {
                var nodeName = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeName;
                if (desc.Player.HasSkill("Mining"))
                {
                    var i = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.Mine();
                    if (i != null)
                    {
                        desc.Player.Inventory.Add(i);
                        RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth--;
                        desc.Send($"You mine the {nodeName} node and find {i.Name}!{Constants.NewLine}");
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth == 0)
                        {
                            RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode = null;
                        }
                    }
                }
                else
                {
                    desc.Send($"You lack the skills to do that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is nothing to mine here!{Constants.NewLine}");
            }
        }

        private static void CraftItem(ref Descriptor desc, ref string input)
        {
            var recipeName = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if (!string.IsNullOrEmpty(recipeName))
            {
                var recipe = RecipeManager.Instance.GetRecipe(recipeName);
                if (recipe != null)
                {
                    if (desc.Player.KnowsRecipe(recipe.RecipeName))
                    {
                        bool canCraft = false;
                        switch (recipe.RecipeType)
                        {
                            case RecipeType.Jewelcrafting:
                                canCraft = desc.Player.HasSkill("Jewelcrafting");
                                break;

                            case RecipeType.Scribing:
                                canCraft = desc.Player.HasSkill("Scribing");
                                break;

                            case RecipeType.Blacksmithing:
                                canCraft = desc.Player.HasSkill("Blacksmithing");
                                break;

                            case RecipeType.Alchemy:
                                canCraft = desc.Player.HasSkill("Alchemy");
                                break;
                        }
                        if (canCraft)
                        {
                            bool hasMats = true;
                            foreach (var mat in recipe.RequiredMaterials)
                            {
                                if (desc.Player.HasItemInInventory(mat.Key))
                                {
                                    var cnt = Convert.ToUInt32((from i in desc.Player.Inventory where i.ID == mat.Key select i).Count());
                                    if (cnt < mat.Value)
                                    {
                                        hasMats = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    hasMats = false;
                                    break;
                                }
                            }
                            if (hasMats)
                            {
                                foreach (var mat in recipe.RequiredMaterials)
                                {
                                    for (int i = 0; i < mat.Value; i++)
                                    {
                                        var remItem = (from invItem in desc.Player.Inventory where invItem.ID == mat.Key select invItem).FirstOrDefault();
                                        desc.Player.Inventory.Remove(remItem);
                                    }
                                }
                                var item = ItemManager.Instance.GetItemByID(recipe.RecipeResult);
                                desc.Player.Inventory.Add(item);
                                desc.Send($"You have successfully crafted {item.Name}!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You don't have all the materials needed to craft that.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You don't have the skill to craft that!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You don't know the recipe for that!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"That Recipe doesn't seem to exist.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void HireFollower(ref Descriptor desc, ref string input)
        {
            var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if (string.IsNullOrEmpty(target))
            {
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Mercenary))
                {
                    if (desc.Player.FollowerID == Guid.Empty)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("The Mercenary Commander grins. 'You want hired muscle? Sure...'");
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| Price{Constants.TabStop}|| Mercenary");
                        sb.AppendLine($"||==============||{new string('=', 61)}");
                        var p = Helpers.GetNewPurchasePrice(ref desc, desc.Player.Level * 1000);
                        if (p.ToString().Length > 4)
                        {
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Fighter");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Thief");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Mage");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Priest");
                        }
                        else
                        {
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Fighter");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Thief");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Mage");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Priest");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"There is no Mercenary Commander to hire from!{Constants.NewLine}");
                }
            }
            else
            {
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Mercenary))
                {
                    if (desc.Player.FollowerID == Guid.Empty)
                    {
                        var p = Helpers.GetNewPurchasePrice(ref desc, desc.Player.Level * 1000);
                        if (desc.Player.Gold >= p)
                        {
                            NPC hireling = null;
                            if (Regex.Match(target, "fighter", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "fighter");
                            }
                            if (Regex.Match(target, "thief", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "thief");
                            }
                            if (Regex.Match(target, "mage", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "mage");
                            }
                            if (Regex.Match(target, "priest", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "priest");
                            }
                            if (hireling != null)
                            {
                                desc.Player.Gold -= p;
                                NPCManager.Instance.AddNPCToWorld(hireling, desc.Player.CurrentRoom);
                                desc.Player.FollowerID = hireling.NPCGuid;
                                desc.Send($"You have hired {hireling.Title} {hireling.Name} as your follower!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"Something went wrong hiring a follower, please check with an Imm!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"The Mercenary Commander laughs. 'No gold! You must have gold!'{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                    }
                }
                else
                {
                    var n = GetTargetNPC(ref desc, target);
                    if (n != null)
                    {
                        if (desc.Player.FollowerID == Guid.Empty)
                        {
                            if (n.BehaviourFlags.HasFlag(NPCFlags.Mercenary))
                            {
                                var baseCost = n.NumberOfHitDice * 1000;
                                var modCost = Helpers.GetNewPurchasePrice(ref desc, baseCost);
                                if (desc.Player.Gold >= modCost)
                                {
                                    desc.Player.Gold -= modCost;
                                    desc.Player.FollowerID = n.NPCGuid;
                                    NPCManager.Instance.SetNPCFollowing(ref desc, true);
                                    desc.Send($"You hand over {modCost} gold and hire {n.Name} as your follower!{Constants.NewLine}");
                                }
                                else
                                {
                                    desc.Send($"You can't afford to hire {n.Name} as your follower!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{n.Name} isn't interested in being your follower!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That doesn't seem to be here...{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DismissFollower(ref Descriptor desc, ref string input)
        {
            if (desc.Player.FollowerID != Guid.Empty)
            {
                var n = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                desc.Send($"You dismiss {n.Name} from your service.{Constants.NewLine}");
                NPCManager.Instance.RemoveNPCFromWorld(desc.Player.FollowerID);
                desc.Player.FollowerID = Guid.Empty;
                desc.Send($"{n.Name} is swallowed by the Winds of Magic!{Constants.NewLine}");
            }
            else
            {
                desc.Send($"You don't have a follower to dismiss right now.{Constants.NewLine}");
            }
        }

        private static void ChangeCharacterTitle(ref Descriptor desc, ref string input)
        {
            desc.Send($"Your current Title is: {desc.Player.Title}{Constants.NewLine}");
            bool titleOK = false;
            while (!titleOK)
            {
                desc.Send($"Enter new Title (exit to abort): ");
                var newTitle = desc.Read().Trim();
                if (ValidateInput(newTitle))
                {
                    if (newTitle.ToLower() == "exit")
                    {
                        titleOK = true;
                    }
                    else
                    {
                        if (newTitle.Length <= 15)
                        {
                            desc.Player.Title = newTitle;
                            titleOK = true;
                        }
                        else
                        {
                            desc.Send($"That is too long to be a title. Titles must be 15 characters or less.{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void ChangePlayerPassword(ref Descriptor desc, ref string input)
        {
            bool pwOK = false;
            while (!pwOK)
            {
                desc.Send($"Enter current password: ");
                var curPW = desc.Read().Trim();
                if (ValidateInput(curPW))
                {
                    if (DatabaseManager.ValidatePlayerPassword(desc.Player.Name, curPW))
                    {
                        desc.Send($"Enter new password: ");
                        var newPW = desc.Read().Trim();
                        if (ValidateInput(newPW))
                        {
                            if (DatabaseManager.UpdatePlayerPassword(ref desc, newPW))
                            {
                                desc.Send($"Your password has been updated successfully.{Constants.NewLine}");
                                Game.LogMessage($"INFO: Player {desc.Player} has successfully changed their password", LogLevel.Info, true);
                                pwOK = true;
                            }
                            else
                            {
                                desc.Send($"Failed to updated your password. Please see an Imm for advice.{Constants.NewLine}");
                                Game.LogMessage($"WARN: Failed to update password for {desc.Player}", LogLevel.Warning, true);
                                pwOK = true;
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Validation failed, this has been logged.{Constants.NewLine}");
                        Game.LogMessage($"WARN: Player {desc.Player} failed password validation while trying to change their password", LogLevel.Warning, true);
                        pwOK = true;
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void ShowPlayerRecipes(ref Descriptor desc, ref string input)
        {
            if (desc.Player.Recipes != null && desc.Player.Recipes.Count > 0)
            {
                var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (string.IsNullOrEmpty(target))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    var sk = (from kr in desc.Player.Recipes select kr.RecipeType.ToString()).Distinct().ToList();
                    bool first = true;
                    foreach (var skill in sk)
                    {
                        if (first)
                        {
                            sb.AppendLine($"|| {Constants.GreenText}{skill}{Constants.PlainText}");
                            sb.AppendLine($"||{new string('=', 77)}");
                            foreach (var r in desc.Player.Recipes.Where(x => x.RecipeType.ToString() == skill).ToList())
                            {
                                sb.AppendLine($"|| {r.RecipeName}");
                            }
                            first = false;
                        }
                        else
                        {
                            sb.AppendLine($"||{new string('=', 77)}");
                            sb.AppendLine($"|| {Constants.GreenText}{skill}{Constants.PlainText}");
                            sb.AppendLine($"||{new string('=', 77)}");
                            foreach (var r in desc.Player.Recipes.Where(x => x.RecipeType.ToString() == skill).ToList())
                            {
                                sb.AppendLine($"|| {r.RecipeName}");
                            }
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    var r = (from kr in desc.Player.Recipes where Regex.Match(kr.RecipeName, target, RegexOptions.IgnoreCase).Success select kr).FirstOrDefault();
                    if (r != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| Name: {r.RecipeName}");
                        sb.AppendLine($"|| Description: {r.RecipeDescription}");
                        sb.AppendLine($"|| Produces: {ItemManager.Instance.GetItemByID(r.RecipeResult).Name}");
                        sb.AppendLine($"|| Requires:");
                        foreach (var req in r.RequiredMaterials)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{req.Value} x {ItemManager.Instance.GetItemByID(req.Key).Name}");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"You don't know that recipe!{Constants.NewLine}");
                    }
                }
            }
            else
            {
                desc.Send($"You don't know any recipes!{Constants.NewLine}");
            }
        }

        private static void ReadScroll(ref Descriptor desc, ref string input)
        {
            if (desc.Player.HasSkill("Read"))
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var target = TokeniseInput(ref input).Last().Trim();
                var scrollName = line.Replace(target, string.Empty).Trim();
                var scr = GetTargetItem(ref desc, scrollName, true);
                if (scr != null && scr.ItemType == ItemType.Scroll)
                {
                    if (!string.IsNullOrEmpty(target))
                    {
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe) || RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
                        {
                            desc.Send($"Some mystical force prevents you from reading the scroll...{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Player.Inventory.Remove(scr);
                            CastSpell(ref desc, ref input, true);
                        }
                    }
                    else
                    {
                        desc.Send($"Cast the scroll at what?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You don't have that scroll in your inventory!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't know how to read the magic of the scrolls{Constants.NewLine}");
            }
        }

        private static void ChangeCharacterLongDesc(ref Descriptor desc)
        {
            desc.Send($"Your current Long Description is:{Constants.NewLine}{desc.Player.LongDescription}");
            desc.Player.LongDescription = Helpers.GetLongDescription(ref desc);
        }

        private static void ChangeCharacterShortDesc(ref Descriptor desc)
        {
            desc.Send($"Your current Short Description is: {desc.Player.ShortDescription}");
            desc.Send($"Enter a new short description for your character. This should be");
            desc.Send($"no longer than 30 characters:");
            var sDesc = desc.Read().Trim();
            if (Helpers.ValidateInput(sDesc))
            {
                desc.Player.ShortDescription = sDesc;
            }
        }

        private static void Backstab(ref Descriptor desc, ref string input)
        {
            Skill backstab = SkillManager.Instance.GetSkill("Backstab");
            if (!desc.Player.HasSkill("Backstab"))
            {
                desc.Send($"You lack the skill to do that!{Constants.NewLine}");
                return;
            }
            if (desc.Player.EquipWeapon == null)
            {
                desc.Send($"Backstab with what? You don't have a weapon!{Constants.NewLine}");
                return;
            }
            if (desc.Player.EquipWeapon.BaseWeaponType != WeaponType.Dagger || desc.Player.EquipWeapon.BaseWeaponType != WeaponType.Sword)
            {
                desc.Send($"You can't backstab someone with that weapon!{Constants.NewLine}");
                return;
            }
            if (desc.Player.Visible || desc.Player.Position != ActorPosition.Standing)
            {
                desc.Send($"You're not in a position to do that right now!{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                desc.Send($"Some mystical force prevents you from doing that!{Constants.NewLine}");
                return;
            }
            var verb = GetVerb(ref input);
            var target = input.Remove(0, verb.Length).Trim();
            if (string.IsNullOrEmpty(target))
            {
                desc.Send($"Backstab what, exactly?{Constants.NewLine}");
                return;
            }
            var npc = GetTargetNPC(ref desc, target);
            if (npc != null)
            {
                if (desc.Player.CurrentMP < backstab.MPCost)
                {
                    desc.Send($"You don't have the energy for that right now!{Constants.NewLine}");
                    return;
                }
                if (npc.BehaviourFlags.HasFlag(NPCFlags.NoAttack) || npc.IsInCombat)
                {
                    desc.Send($"Your target isn't available...{Constants.NewLine}");
                    return;
                }
                if (npc.IsFollower && (!SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Player.PVP) || !desc.Player.PVP)
                {
                    desc.Send($"Some mysical force prevents you from harming {npc.Name}...{Constants.NewLine}");
                    return;
                }
                desc.Player.AdjustMP((int)backstab.MPCost * -1);
                desc.Send($"You become visible again...{Constants.NewLine}");
                desc.Player.Visible = true;
                bool hits = desc.Player.DoHitRoll(npc, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                string wpn = desc.Player.EquipWeapon.Name.ToLower();
                if (hits)
                {
                    var damage = desc.Player.DoDamageRoll(npc);
                    damage *= 4;
                    if (isCritical)
                    {
                        damage *= 2;
                    }
                    var pDmg = (uint)Math.Round((double)damage / npc.CurrentHP * 100, 0);
                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                    {
                        desc.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): You backstab {npc.Name} with your {wpn}, {Helpers.GetDamageString(pDmg)} them for {damage} damage!{Constants.NewLine}");
                    }
                    else
                    {
                        desc.Send($"You backstab {npc.Name} with your {wpn}, {Helpers.GetDamageString(pDmg)} them!{Constants.NewLine}");
                    }
                    if (npc.IsFollower)
                    {
                        SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Send($"{desc.Player.Name}'s backstab {Helpers.GetDamageString(pDmg)} your follower for {damage} damage!{Constants.NewLine}");
                    }
                    npc.AdjustHP((int)damage * -1, out bool isKilled);
                    if (isKilled)
                    {
                        desc.Send($"{npc.Name}'s wounds are many and serious and they give in to death...{Constants.NewLine}");
                        desc.Send($"You gain {npc.BaseExpAward} Exp and {npc.Gold} gold{Constants.NewLine}");
                        desc.Player.AddExp(npc.BaseExpAward, false, false);
                        desc.Player.AddGold(npc.Gold, false);
                        npc.Kill(true, ref desc);
                        return;
                    }
                    else
                    {
                        CombatSession s1 = new CombatSession(desc, npc, desc.ID, npc.NPCGuid);
                        CombatSession s2 = new CombatSession(npc, desc, npc.NPCGuid, desc.ID);
                        CombatManager.Instance.AddCombatSession(s1);
                        CombatManager.Instance.AddCombatSession(s2);
                        if (desc.Player.FollowerID != Guid.Empty)
                        {
                            var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                            CombatSession s3 = new CombatSession(fNPC, npc, fNPC.NPCGuid, npc.NPCGuid);
                            CombatSession s4 = new CombatSession(npc, fNPC, npc.NPCGuid, fNPC.NPCGuid);
                            CombatManager.Instance.AddCombatSession(s3);
                            CombatManager.Instance.AddCombatSession(s4);
                        }
                    }
                    return;
                }
                else
                {
                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                    {
                        desc.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): Your backstab misses {npc.Name} - prepare to fight!{Constants.NewLine}");
                    }
                    else
                    {
                        desc.Send($"Your backstab missed {npc.Name} - prepare to fight!{Constants.NewLine}");
                    }
                    if (npc.IsFollower)
                    {
                        SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Send($"{desc.Player.Name}'s backstab attempt missed your follower!{Constants.NewLine}");
                    }
                    CombatSession s1 = new CombatSession(desc, npc, desc.ID, npc.NPCGuid);
                    CombatSession s2 = new CombatSession(npc, desc, npc.NPCGuid, desc.ID);
                    CombatManager.Instance.AddCombatSession(s1);
                    CombatManager.Instance.AddCombatSession(s2);
                    if (desc.Player.FollowerID != Guid.Empty)
                    {
                        var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                        CombatSession s3 = new CombatSession(fNPC, npc, fNPC.NPCGuid, npc.NPCGuid);
                        CombatSession s4 = new CombatSession(npc, fNPC, npc.NPCGuid, fNPC.NPCGuid);
                        CombatManager.Instance.AddCombatSession(s3);
                        CombatManager.Instance.AddCombatSession(s4);
                    }
                    return;
                }
            }
            var pid = desc.ID;
            var targetPlayer = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom.Where(x => Regex.IsMatch(x.Player.Name, target, RegexOptions.IgnoreCase) && x.ID != pid && x.Player.Visible).FirstOrDefault();
            if (targetPlayer != null)
            {
                if (desc.Player.CurrentMP < backstab.MPCost)
                {
                    desc.Send($"You don't have the energy for that right now!{Constants.NewLine}");
                    return;
                }
                if (!desc.Player.PVP || !targetPlayer.Player.PVP)
                {
                    desc.Send($"Some mystical force prevents you from harming {desc.Player.Name}...{Constants.NewLine}");
                    return;
                }
                desc.Player.AdjustMP((int)backstab.MPCost * -1);
                desc.Send($"You become visible again...{Constants.NewLine}");
                desc.Player.Visible = true;
                bool hits = desc.Player.DoHitRoll(targetPlayer.Player, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                if (hits)
                {
                    string wpn = desc.Player.EquipWeapon.Name.ToLower();
                    var damage = desc.Player.DoDamageRoll(targetPlayer.Player);
                    damage *= 4;
                    if (isCritical)
                    {
                        damage *= 2;
                    }
                    var pDmg = (uint)Math.Round((double)damage / targetPlayer.Player.CurrentHP * 100, 0);
                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                    {
                        desc.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): You backstab {targetPlayer.Player.Name} with your {wpn}, {Helpers.GetDamageString(pDmg)} them for {damage} damage!{Constants.NewLine}");
                    }
                    else
                    {
                        desc.Send($"You backstab {targetPlayer.Player.Name} with your {wpn}, {Helpers.GetDamageString(pDmg)} them!{Constants.NewLine}");
                    }
                    if (targetPlayer.Player.Level >= Constants.ImmLevel || targetPlayer.Player.ShowDetailedRollInfo)
                    {
                        desc.Send($"{desc.Player.Name}'s backstab {Helpers.GetDamageString(pDmg)} you for {damage} damage!{Constants.NewLine}");
                    }
                    else
                    {
                        desc.Send($"{desc.Player.Name}'s backstab {Helpers.GetDamageString(pDmg)} you!{Constants.NewLine}");
                    }
                    targetPlayer.Player.AdjustHP((int)damage * -1, out bool isKilled);
                    if (isKilled)
                    {
                        desc.Send($"Your strike has dealt lethal damage to {targetPlayer.Player.Name} and they are taken by Death!{Constants.NewLine}");
                        targetPlayer.Send($"{desc.Player.Name} has dealt lethal damage and you feel the icy hand of Death upon you...{Constants.NewLine}");
                        targetPlayer.Player.Kill();
                        return;
                    }
                    else
                    {
                        CombatSession s1 = new CombatSession(desc, targetPlayer, desc.ID, targetPlayer.ID);
                        CombatSession s2 = new CombatSession(targetPlayer, desc, targetPlayer.ID, desc.ID);
                        CombatManager.Instance.AddCombatSession(s1);
                        CombatManager.Instance.AddCombatSession(s2);
                        if (desc.Player.FollowerID != Guid.Empty)
                        {
                            var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                            CombatSession s3 = new CombatSession(fNPC, targetPlayer, fNPC.NPCGuid, targetPlayer.ID);
                            CombatSession s4 = new CombatSession(targetPlayer, fNPC, targetPlayer.ID, fNPC.NPCGuid);
                            CombatManager.Instance.AddCombatSession(s3);
                            CombatManager.Instance.AddCombatSession(s4);
                        }
                        if (targetPlayer.Player.FollowerID != Guid.Empty)
                        {
                            var tfNPC = NPCManager.Instance.GetNPCByGUID(targetPlayer.Player.FollowerID);
                            CombatSession s5 = new CombatSession(tfNPC, desc, tfNPC.NPCGuid, desc.ID);
                            CombatSession s6 = new CombatSession(desc, tfNPC, desc.ID, tfNPC.NPCGuid);
                            CombatManager.Instance.AddCombatSession(s5);
                            CombatManager.Instance.AddCombatSession(s6);
                        }
                        return;
                    }
                }
                else
                {
                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                    {
                        desc.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): Your backstab misses {targetPlayer.Player.Name} - prepare to fight!{Constants.NewLine}");
                    }
                    targetPlayer.Send($"{desc.Player.Name}'s backstab attempt on you missed!{Constants.NewLine}");
                    CombatSession s1 = new CombatSession(desc, targetPlayer, desc.ID, targetPlayer.ID);
                    CombatSession s2 = new CombatSession(targetPlayer, desc, targetPlayer.ID, desc.ID);
                    CombatManager.Instance.AddCombatSession(s1);
                    CombatManager.Instance.AddCombatSession(s2);
                    if (desc.Player.FollowerID != Guid.Empty)
                    {
                        var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                        CombatSession s3 = new CombatSession(fNPC, targetPlayer, fNPC.NPCGuid, targetPlayer.ID);
                        CombatSession s4 = new CombatSession(targetPlayer, fNPC, targetPlayer.ID, fNPC.NPCGuid);
                        CombatManager.Instance.AddCombatSession(s3);
                        CombatManager.Instance.AddCombatSession(s4);
                    }
                    if (targetPlayer.Player.FollowerID != Guid.Empty)
                    {
                        var tfNPC = NPCManager.Instance.GetNPCByGUID(targetPlayer.Player.FollowerID);
                        CombatSession s5 = new CombatSession(tfNPC, desc, tfNPC.NPCGuid, desc.ID);
                        CombatSession s6 = new CombatSession(desc, tfNPC, desc.ID, tfNPC.NPCGuid);
                        CombatManager.Instance.AddCombatSession(s5);
                        CombatManager.Instance.AddCombatSession(s6);
                    }
                    return;
                }
            }
            else
            {
                desc.Send($"Backstab who, exactly?{Constants.NewLine}");
                return;
            }
        }

        private static void Pickpocket(ref Descriptor desc, ref string input)
        {
            Skill pickpocket = SkillManager.Instance.GetSkill("Pickpocket");
            if (!desc.Player.HasSkill(pickpocket.Name))
            {
                desc.Send($"You lack the skill to do that!{Constants.NewLine}");
                return;
            }
            if (desc.Player.Position != ActorPosition.Standing)
            {
                desc.Send($"You're not in a position to do that right now!{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                desc.Send($"Some mystical force prevents you from doing that...{Constants.NewLine}");
                return;
            }
            if (desc.Player.CurrentMP < pickpocket.MPCost)
            {
                desc.Send($"You don't have the energy to do that right now!{Constants.NewLine}");
                return;
            }
            var verb = GetVerb(ref input);
            var target = input.Remove(0, verb.Length).Trim();
            if (string.IsNullOrEmpty(target))
            {
                desc.Send($"Pickpocket who, exactly?{Constants.NewLine}");
                return;
            }
            var targetNPC = GetTargetNPC(ref desc, target);
            if (targetNPC == null)
            {
                desc.Send($"You can't pickpocket something that isn't here!{Constants.NewLine}");
                return;
            }
            if (targetNPC.BehaviourFlags.HasFlag(NPCFlags.NoAttack) || targetNPC.IsFollower)
            {
                desc.Send($"Some mystical force prevents you from doing that to {targetNPC.Name}...{Constants.NewLine}");
                return;
            }
            if (targetNPC.Gold == 0 || targetNPC.Inventory == null || targetNPC.Inventory.Count == 0)
            {
                desc.Send($"{targetNPC.Name} doesn't have anything to steal!{Constants.NewLine}");
                return;
            }
            desc.Player.AdjustMP((int)pickpocket.MPCost * -1);
            int playerSkillRoll = (int)Helpers.RollDice(1, 20);
            playerSkillRoll += Helpers.CalculateAbilityModifier(desc.Player.Dexterity);
            if (!desc.Player.Visible)
            {
                playerSkillRoll += 4;
            }
            int npcSkillRoll = (int)Helpers.RollDice(1, 20);
            npcSkillRoll += Helpers.CalculateAbilityModifier(targetNPC.Dexterity);
            if (playerSkillRoll >= npcSkillRoll)
            {
                if (targetNPC.Gold > 0)
                {
                    var stolenGold = Helpers.RollDice(1, (uint)targetNPC.Gold);
                    desc.Player.AddGold(stolenGold, true);
                    targetNPC.Gold -= stolenGold;
                    desc.Send($"You have stolen {stolenGold:N0} gold from {targetNPC.Name}!{Constants.NewLine}");
                    return;
                }
                if (targetNPC.Inventory != null && targetNPC.Inventory.Count > 0)
                {
                    var item = targetNPC.Inventory[new Random(DateTime.UtcNow.GetHashCode()).Next(targetNPC.Inventory.Count)];
                    desc.Player.Inventory.Add(item);
                    targetNPC.Inventory.Remove(item);
                    var article = Helpers.IsCharAVowel(item.Name[0]) ? "an" : "a";
                    desc.Send($"You have stolen {article} {item.Name.ToLower()} from {targetNPC.Name}!{Constants.NewLine}");
                    return;
                }
            }
            else
            {
                desc.Send($"You have failed to steal anything from {targetNPC.Name}!{Constants.NewLine}");
                if (playerSkillRoll <= 1)
                {
                    desc.Send($"{targetNPC.Name} has spotted you trying to steal from them!{Constants.NewLine}");
                    CombatSession s1 = new CombatSession(targetNPC, desc, targetNPC.NPCGuid, desc.ID);
                    CombatSession s2 = new CombatSession(desc, targetNPC, desc.ID, targetNPC.NPCGuid);
                    CombatManager.Instance.AddCombatSession(s1);
                    CombatManager.Instance.AddCombatSession(s2);
                }
            }
        }

        private static void TrainPlayerStat(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.StatTrainer))
            {
                var stat = input.Replace(GetVerb(ref input), string.Empty).Trim();
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(stat))
                {
                    uint cost = 0;
                    switch (stat.ToLower())
                    {
                        case "str":
                        case "strength":
                            cost = Convert.ToUInt32((desc.Player.Strength + 1) * 1000);
                            break;

                        case "dex":
                        case "dexterity":
                            cost = Convert.ToUInt32((desc.Player.Dexterity + 1) * 1000);
                            break;

                        case "int":
                        case "intelligence":
                            cost = Convert.ToUInt32((desc.Player.Intelligence + 1) * 1000);
                            break;

                        case "wisdom":
                        case "wis":
                            cost = Convert.ToUInt32((desc.Player.Wisdom + 1) * 1000);
                            break;

                        case "constitution":
                        case "con":
                            cost = Convert.ToUInt32((desc.Player.Constitution + 1) * 1000);
                            break;

                        case "charisma":
                        case "cha":
                            cost = Convert.ToUInt32((desc.Player.Charisma + 1) * 1000);
                            break;

                        case "hp":
                        case "mp":
                        case "health":
                        case "mana":
                        case "stamina":
                        case "sp":
                            cost = 20000;
                            break;

                        default:
                            desc.Send($"'I can't help you train that,' the gym master says.{Constants.NewLine}");
                            return;
                    }
                    if (desc.Player.Gold >= cost)
                    {
                        desc.Send($"The gym master smiles. 'Certainly! Follow me...'{Constants.NewLine}");
                        desc.Player.Gold -= cost;
                        switch (stat.ToLower())
                        {
                            case "str":
                            case "strength":
                                desc.Player.Strength++;
                                desc.Send($"Your Strength increases to {desc.Player.Strength}{Constants.NewLine}");
                                break;

                            case "dex":
                            case "dexterity":
                                desc.Player.Dexterity++;
                                desc.Send($"Your Dexterity increases to {desc.Player.Dexterity}{Constants.NewLine}");
                                break;

                            case "int":
                            case "intelligence":
                                desc.Player.Intelligence++;
                                desc.Send($"Your Intelligence increases to {desc.Player.Intelligence}{Constants.NewLine}");
                                break;

                            case "wisdom":
                            case "wis":
                                desc.Player.Wisdom++;
                                desc.Send($"Your Wisdom increases to {desc.Player.Wisdom}{Constants.NewLine}");
                                break;

                            case "constitution":
                            case "con":
                                desc.Player.Constitution++;
                                desc.Send($"Your Constitution increases to {desc.Player.Constitution}{Constants.NewLine}");
                                break;

                            case "charisma":
                            case "cha":
                                desc.Player.Charisma++;
                                desc.Send($"Your Charisma increases to {desc.Player.Charisma}{Constants.NewLine}");
                                break;

                            case "hp":
                            case "health":
                                switch (desc.Player.Class)
                                {
                                    case ActorClass.Wizard:
                                        var hpInc = Helpers.RollDice(1, 4) + Helpers.CalculateAbilityModifier(desc.Player.Constitution);
                                        hpInc = hpInc <= 0 ? 1 : hpInc;
                                        desc.Player.CurrentHP += (int)hpInc;
                                        desc.Player.MaxHP += (int)hpInc;
                                        desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Cleric:
                                        hpInc = Helpers.RollDice(1, 8) + Helpers.CalculateAbilityModifier(desc.Player.Constitution);
                                        hpInc = hpInc <= 0 ? 1 : hpInc;
                                        desc.Player.CurrentHP += (int)hpInc;
                                        desc.Player.MaxHP += (int)hpInc;
                                        desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Thief:
                                        hpInc = Helpers.RollDice(1, 6) + Helpers.CalculateAbilityModifier(desc.Player.Constitution);
                                        hpInc = hpInc <= 0 ? 1 : hpInc;
                                        desc.Player.CurrentHP += (int)hpInc;
                                        desc.Player.MaxHP += (int)hpInc;
                                        desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Fighter:
                                        hpInc = Helpers.RollDice(1, 10) + Helpers.CalculateAbilityModifier(desc.Player.Constitution);
                                        hpInc = hpInc <= 0 ? 1 : hpInc;
                                        desc.Player.CurrentHP += (int)hpInc;
                                        desc.Player.MaxHP += (int)hpInc;
                                        desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                        break;
                                }
                                break;

                            case "mp":
                            case "mana":
                                switch (desc.Player.Class)
                                {
                                    case ActorClass.Wizard:
                                        var mpInc = Helpers.RollDice(1, 10) + Helpers.CalculateAbilityModifier(desc.Player.Intelligence);
                                        mpInc = mpInc <= 0 ? 1 : mpInc;
                                        desc.Player.CurrentMP += (int)mpInc;
                                        desc.Player.MaxMP += (int)mpInc;
                                        desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Cleric:
                                        mpInc = Helpers.RollDice(1, 8) + Helpers.CalculateAbilityModifier(desc.Player.Wisdom);
                                        mpInc = mpInc <= 0 ? 1 : mpInc;
                                        desc.Player.CurrentMP += (int)mpInc;
                                        desc.Player.MaxMP += (int)mpInc;
                                        desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Thief:
                                        mpInc = Helpers.RollDice(1, 6) + Helpers.CalculateAbilityModifier(desc.Player.Intelligence);
                                        mpInc = mpInc <= 0 ? 1 : mpInc;
                                        desc.Player.CurrentMP += (int)mpInc;
                                        desc.Player.MaxMP += (int)mpInc;
                                        desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                        break;

                                    case ActorClass.Fighter:
                                        mpInc = Helpers.RollDice(1, 4) + Helpers.CalculateAbilityModifier(desc.Player.Intelligence);
                                        mpInc = mpInc <= 0 ? 1 : mpInc;
                                        desc.Player.CurrentMP += (int)mpInc;
                                        desc.Player.MaxMP += (int)mpInc;
                                        desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                        break;
                                }
                                break;

                            case "sp":
                            case "stamina":
                                var spInc = Helpers.RollDice(1, 10);
                                var mod = Helpers.CalculateAbilityModifier(desc.Player.Constitution);
                                if (mod > 0)
                                {
                                    spInc += (uint)mod;
                                }
                                desc.Player.MaxSP += (int)spInc;
                                desc.Player.CurrentSP += (int)spInc;
                                desc.Send($"Your stamina increases by {spInc}!{Constants.NewLine}");
                                break;
                        }
                        desc.Player.CalculateArmourClass();
                    }
                    else
                    {
                        desc.Send($"The gym master frowns. 'Looks like you're short of funds!'{Constants.NewLine}");
                    }
                }
                else
                {
                    // no stat specified so show price for increasing all stats
                    sb.AppendLine($"The gym master flexes. 'Sure I can help you improve, but it will cost you...'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Stat");
                    var strIncPrice = (desc.Player.Strength + 1) * 1000;
                    var dexIncPrice = (desc.Player.Dexterity + 1) * 1000;
                    var intIncPrice = (desc.Player.Intelligence + 1) * 1000;
                    var wisIncPrice = (desc.Player.Wisdom + 1) * 1000;
                    var conIncPrice = (desc.Player.Constitution + 1) * 1000;
                    var chaIncPrice = (desc.Player.Charisma + 1) * 1000;
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    if (strIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Strength + 1) * 1000}{Constants.TabStop}|| Strength");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Strength + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Strength");
                    }
                    if (dexIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Dexterity + 1) * 1000}{Constants.TabStop}|| Dexterity");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Dexterity + 1) * 1000}{Constants.TabStop}{Constants.NewLine}|| Dexterity");
                    }
                    if (intIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Intelligence + 1) * 1000}{Constants.TabStop}|| Intelligence");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Intelligence + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Intelligence");
                    }
                    if (wisIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Wisdom + 1) * 1000}{Constants.TabStop}|| Wisdom");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Wisdom + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Wisdom");
                    }
                    if (conIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Constitution + 1) * 1000}{Constants.TabStop}|| Constitution");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Constitution + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Constitution");
                    }
                    if (chaIncPrice.ToString().Length > 4)
                    {
                        sb.AppendLine($"|| {(desc.Player.Charisma + 1) * 1000}{Constants.TabStop}|| Charisma");
                    }
                    else
                    {
                        sb.AppendLine($"|| {(desc.Player.Charisma + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Charisma");
                    }
                    sb.AppendLine($"|| 20000{Constants.TabStop}|| Extra HP");
                    sb.AppendLine($"|| 20000{Constants.TabStop}|| Extra MP");
                    sb.AppendLine($"|| 20000{Constants.TabStop}|| Extra SP");
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
            }
            else
            {
                desc.Send($"There is no one here to train you!{Constants.NewLine}");
            }
        }

        private static void ShowPlayerSkills(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if (desc.Player.Skills.Count > 0)
            {
                sb.AppendLine($"|| Skill {Constants.TabStop}{Constants.TabStop}|| MP{Constants.TabStop}|| Description");
                sb.AppendLine($"||{new string('=', 77)}");
                foreach (var s in desc.Player.Skills)
                {
                    if (s.Name.Length <= 4)
                    {
                        sb.AppendLine($"|| {s.Name}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                    }
                    else
                    {
                        if (s.Name.Length < 13)
                        {
                            sb.AppendLine($"|| {s.Name}{Constants.TabStop}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                        }
                        else
                        {
                            sb.AppendLine($"|| {s.Name}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("|| No skills known");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void ShowPlayerSpells(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if (desc.Player.Spells.Count > 0)
            {
                sb.AppendLine($"|| Spell {Constants.TabStop}{Constants.TabStop}|| MP{Constants.TabStop}|| Description");
                sb.AppendLine($"||{new string('=', 77)}");
                foreach (var s in desc.Player.Spells)
                {
                    if (s.SpellName.Length <= 4)
                    {
                        sb.AppendLine($"|| {s.SpellName}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                    }
                    else
                    {
                        if (s.SpellName.Length < 13)
                        {
                            sb.AppendLine($"|| {s.SpellName}{Constants.TabStop}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                        }
                        else
                        {
                            sb.AppendLine($"|| {s.SpellName}{Constants.TabStop}|| {s.MPCost}{Constants.TabStop}|| {s.Description}");
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("|| No spells known");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void ShowBuffs(ref Descriptor desc, ref string input)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if (desc.Player.Buffs != null && desc.Player.Buffs.Count > 0)
            {
                foreach (var b in desc.Player.Buffs)
                {
                    if (b.Value == -1)
                    {
                        sb.AppendLine($"|| {b.Key}: Permanent");
                    }
                    else
                    {
                        var line = b.Value > 1 ? $"|| {b.Key}: {b.Value} ticks remaining" : $"|| {b.Key}: {b.Value} tick remaining";
                        sb.AppendLine(line);
                    }
                }
            }
            else
            {
                sb.AppendLine("|| No buffs active");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void CastSpell(ref Descriptor desc, ref string input, bool overrideSkillCheck = false)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
            {
                desc.Send($"Some mystical force prevents the use of magic here...{Constants.NewLine}");
                return;
            }
            if (desc.Player.HasBuff("Silence"))
            {
                desc.Send($"You have been silenced and cannot use magic right now!{Constants.NewLine}");
                return;
            }
            var verb = GetVerb(ref input);
            var line = input.Remove(0, verb.Length);
            var lineElements = TokeniseInput(ref line);
            var spellName = GetSkillOrSpellName(ref line);
            spellName = string.IsNullOrEmpty(spellName) ? lineElements[0].Trim() : spellName;
            if (!string.IsNullOrEmpty(spellName))
            {
                var spell = SpellManager.Instance.GetSpell(spellName);
                if (spell == null)
                {
                    desc.Send($"No such spell exists in the Realms!{Constants.NewLine}");
                    return;
                }
                if (!desc.Player.HasSpell(spell.SpellName))
                {
                    desc.Send($"You don't know that spell!{Constants.NewLine}");
                    return;
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe) && (spell.SpellType == SpellType.Debuff || spell.SpellType == SpellType.Damage))
                {
                    desc.Send($"Some mystical force prevents you from casting that here...{Constants.NewLine}");
                    return;
                }
                if (desc.Player.CurrentMP < spell.MPCost)
                {
                    desc.Send($"You don't have the MP required to cast that spell!{Constants.NewLine}");
                    return;
                }
                var target = line.Replace(spellName, string.Empty).Replace("\"", string.Empty).Replace("'", string.Empty).Trim();
                if (string.IsNullOrEmpty(target) && !spell.AOESpell)
                {
                    desc.Send($"Cast that spell on what, exactly?{Constants.NewLine}");
                    return;
                }
                dynamic targetActor = null;
                bool targetIsSelf = false;
                if (target.ToLower() == "self" || target.ToLower() == desc.Player.Name.ToLower())
                {
                    targetActor = desc;
                    targetIsSelf = true;
                }
                if ((spell.SpellType == SpellType.Debuff || spell.SpellType == SpellType.Damage) && targetActor != null && targetIsSelf)
                {
                    desc.Send($"You can't cast damaging spells on yourself!{Constants.NewLine}");
                    return;
                }
                if (!spell.AOESpell && targetActor == null)
                {
                    // try to find a target NPC first
                    targetActor = GetTargetNPC(ref desc, target);
                }
                if (!spell.AOESpell && targetActor == null)
                {
                    // didn't get a target NPC so try a target player instead
                    var tp = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom.Where(x => Regex.IsMatch(x.Player.Name, target, RegexOptions.IgnoreCase) && x.Player.Visible).FirstOrDefault();
                    if (tp != null)
                    {
                        targetActor = tp;
                    }
                }
                if (!spell.AOESpell && targetActor == null)
                {
                    desc.Send($"The target of your magic cannot be found...{Constants.NewLine}");
                    return;
                }
                switch (spell.SpellType)
                {
                    case SpellType.Healing:
                        if (!spell.AOESpell)
                        {
                            int hpMod = 0;
                            bool hitsTarget = true;
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            if (targetActor is Descriptor)
                            {
                                Descriptor tDesc = (Descriptor)targetActor;
                                hpMod = spell.CalculateSpellHPEffect(desc.Player, tDesc.Player, out hitsTarget);
                                if (!targetIsSelf)
                                {
                                    desc.Send($"Calling on the power of {spell}, you heal {tDesc.Player.Name} for {hpMod} damage!{Constants.NewLine}");
                                    tDesc.Send($"Calling on the power of {spell}, {desc.Player.Name} heals you for {hpMod} damage!{Constants.NewLine}");
                                    tDesc.Player.AdjustHP(hpMod, out _);
                                }
                                else
                                {
                                    desc.Send($"Calling on the power of {spell} you heal yourself for {hpMod} damage!{Constants.NewLine}");
                                    desc.Player.AdjustHP(hpMod, out _);
                                }
                            }
                            else
                            {
                                NPC tNPC = (NPC)targetActor;
                                hpMod = spell.CalculateSpellHPEffect(desc.Player, tNPC, out hitsTarget);
                                desc.Send($"Calling on the power of {spell} you heal {tNPC.Name} for {hpMod} damage!{Constants.NewLine}");
                                if (tNPC.IsFollower && tNPC.FollowingPlayer != desc.ID)
                                {
                                    SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Send($"Calling on the power of {spell}, {desc.Player.Name} heals your follower for {hpMod} damage!{Constants.NewLine}");
                                }
                                tNPC.AdjustHP(hpMod, out _);
                            }
                        }
                        else
                        {
                            var targetNPCs = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom);
                            var targetPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            if (targetNPCs != null && targetNPCs.Count > 0)
                            {
                                foreach (var n in targetNPCs)
                                {
                                    var hpMod = spell.CalculateSpellHPEffect(desc.Player, n, out bool hits);
                                    if (hits)
                                    {
                                        n.AdjustHP(hpMod, out _);
                                    }
                                }
                            }
                            if (targetPlayers != null && targetPlayers.Count > 0)
                            {
                                foreach(var p in targetPlayers)
                                {
                                    var hpMod = spell.CalculateSpellHPEffect(desc.Player, p.Player, out bool hits);
                                    if (hits)
                                    {
                                        p.Player.AdjustHP(hpMod, out _);
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            p.Send($"{desc.Player.Name} bathes the area in holy light and your wounds are healed!{Constants.NewLine}");
                                        }
                                    }
                                }
                            }
                            desc.Send($"You bathe the area in holy light, healing everyone's wounds!{Constants.NewLine}");
                        }
                        break;

                    case SpellType.Damage:
                        if (!spell.AOESpell)
                        {
                            if (targetActor != null && targetActor is NPC)
                            {
                                var tNPC = NPCManager.Instance.GetNPCByGUID(((NPC)targetActor).NPCGuid);
                                if (tNPC.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                                {
                                    desc.Send($"Some mystical force prevents you from harming {tNPC.Name}...{Constants.NewLine}");
                                    return;
                                }
                                if (tNPC.IsFollower)
                                {
                                    if (!desc.Player.PVP || !SessionManager.Instance.GetPlayerByGUID(tNPC.NPCGuid).Player.PVP)
                                    {
                                        desc.Send($"Some mysitcal force prevents that from happening!{Constants.NewLine}");
                                        return;
                                    }
                                    var hpEffect = spell.CalculateSpellHPEffect(desc.Player, tNPC, out bool hits);
                                    desc.Player.AdjustMP((int)spell.MPCost * -1);
                                    if (hits)
                                    {
                                        tNPC.AdjustHP(hpEffect * -1, out bool isKilled);
                                        if (hpEffect <= 0)
                                        {
                                            desc.Send($"{tNPC.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                        }
                                        if (isKilled)
                                        {
                                            desc.Send($"Your {spell} spell strikes {tNPC.Name} for lethal damage, killing them!{Constants.NewLine}");
                                            SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Send($"{desc.Player.Name}'s {spell} spell has slain your follower!{Constants.NewLine}");
                                            tNPC.Kill();
                                            return;
                                        }
                                        else
                                        {
                                            if (hpEffect >= 1)
                                            {
                                                desc.Send($"Your {spell} spell strikes {tNPC.Name} causing {hpEffect} damage!{Constants.NewLine}");
                                                SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Send($"{desc.Player.Name}'s {spell} spell strikes your follower for {hpEffect} damage!{Constants.NewLine}");
                                            }
                                            var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, tNPC.NPCGuid);
                                            if (existingSessions == null || existingSessions.Count == 0)
                                            {
                                                CombatSession c1 = new CombatSession(desc, tNPC, desc.ID, tNPC.NPCGuid);
                                                CombatSession c2 = new CombatSession(tNPC, desc, tNPC.NPCGuid, desc.ID);
                                                CombatManager.Instance.AddCombatSession(c1);
                                                CombatManager.Instance.AddCombatSession(c2);
                                                if (desc.Player.FollowerID != Guid.Empty)
                                                {
                                                    var follower = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                    CombatSession c3 = new CombatSession(follower, tNPC, follower.NPCGuid, tNPC.NPCGuid);
                                                    CombatSession c4 = new CombatSession(tNPC, follower, tNPC.NPCGuid, follower.NPCGuid);
                                                    CombatManager.Instance.AddCombatSession(c3);
                                                    CombatManager.Instance.AddCombatSession(c4);
                                                }
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"The magic of your {spell} spell fizzles and fails!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    var hpEffect = spell.CalculateSpellHPEffect(desc.Player, tNPC, out bool hits);
                                    desc.Player.AdjustMP((int)spell.MPCost * -1);
                                    if (hits)
                                    {
                                        tNPC.AdjustHP(hpEffect * -1, out bool isKilled);
                                        if (hpEffect <= 0)
                                        {
                                            desc.Send($"{tNPC.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                        }
                                        if (isKilled)
                                        {
                                            desc.Send($"The power of your {spell} spell is too much for {tNPC.Name} and they give in to death!{Constants.NewLine}");
                                            desc.Send($"You have gained {tNPC.BaseExpAward} Exp and {tNPC.Gold} gold!{Constants.NewLine}");
                                            desc.Player.AddExp(tNPC.BaseExpAward, false, false);
                                            desc.Player.AddGold(tNPC.Gold, false);
                                            tNPC.Kill(true, ref desc);
                                        }
                                        else
                                        {
                                            if (hpEffect >= 1)
                                            {
                                                desc.Send($"The magic of your {spell} spell blasts {tNPC.Name} causing {hpEffect} damage!{Constants.NewLine}");
                                            }
                                            var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, tNPC.NPCGuid);
                                            if (existingSessions == null || existingSessions.Count == 0)
                                            {
                                                CombatSession c1 = new CombatSession(desc, tNPC, desc.ID, tNPC.NPCGuid);
                                                CombatSession c2 = new CombatSession(tNPC, desc, tNPC.NPCGuid, desc.ID);
                                                CombatManager.Instance.AddCombatSession(c1);
                                                CombatManager.Instance.AddCombatSession(c2);
                                                if (desc.Player.FollowerID != Guid.Empty)
                                                {
                                                    var follower = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                    CombatSession c3 = new CombatSession(follower, tNPC, follower.NPCGuid, tNPC.NPCGuid);
                                                    CombatSession c4 = new CombatSession(tNPC, follower, tNPC.NPCGuid, follower.NPCGuid);
                                                    CombatManager.Instance.AddCombatSession(c3);
                                                    CombatManager.Instance.AddCombatSession(c4);
                                                }
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        desc.Send($"The magic of your {spell} spell fizzles and fails!{Constants.NewLine}");
                                        return;
                                    }
                                }
                            }
                            if (targetActor != null && targetActor is Descriptor)
                            {
                                var tPlayer = (Descriptor)targetActor;
                                var hpEffect = spell.CalculateSpellHPEffect(desc.Player, tPlayer.Player, out bool hits);
                                desc.Player.AdjustMP((int)spell.MPCost * -1);
                                if (hits)
                                {
                                    tPlayer.Player.AdjustHP(hpEffect * -1, out bool isKilled);
                                    if (hpEffect <= 0)
                                    {
                                        desc.Send($"{tPlayer.Player.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                    }
                                    if (isKilled)
                                    {
                                        desc.Send($"The power of your {spell} spell slams into {tPlayer.Player.Name}, killing them instantly!{Constants.NewLine}");
                                        tPlayer.Send($"{desc.Player.Name}'s {spell} spell slams into you with lethal force, killing you instantly!{Constants.NewLine}");
                                        tPlayer.Player.Kill();
                                    }
                                    else
                                    {
                                        if (hpEffect >= 1)
                                        {
                                            desc.Send($"Your {spell} spell strikes {tPlayer.Player.Name} causing {hpEffect} damage!{Constants.NewLine}");
                                            tPlayer.Send($"{desc.Player.Name}'s {spell} spell strikes you for {hpEffect} damage!{Constants.NewLine}");
                                        }
                                        var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, tPlayer.ID);
                                        if (existingSessions == null || existingSessions.Count == 0)
                                        {
                                            CombatSession c1 = new CombatSession(desc, tPlayer, desc.ID, tPlayer.ID);
                                            CombatSession c2 = new CombatSession(tPlayer, desc, tPlayer.ID, desc.ID);
                                            CombatManager.Instance.AddCombatSession(c1);
                                            CombatManager.Instance.AddCombatSession(c2);
                                            if (desc.Player.FollowerID != Guid.Empty)
                                            {
                                                var fnpc = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                CombatSession c3 = new CombatSession(fnpc, tPlayer, fnpc.NPCGuid, tPlayer.ID);
                                                CombatSession c4 = new CombatSession(tPlayer, fnpc, tPlayer.ID, fnpc.NPCGuid);
                                                CombatManager.Instance.AddCombatSession(c3);
                                                CombatManager.Instance.AddCombatSession(c4);
                                            }
                                            if (tPlayer.Player.FollowerID != Guid.Empty)
                                            {
                                                var fnpc = NPCManager.Instance.GetNPCByGUID(tPlayer.Player.FollowerID);
                                                CombatSession c5 = new CombatSession(fnpc, desc, fnpc.NPCGuid, desc.ID);
                                                CombatSession c6 = new CombatSession(desc, fnpc, desc.ID, fnpc.NPCGuid);
                                                CombatManager.Instance.AddCombatSession(c5);
                                                CombatManager.Instance.AddCombatSession(c6);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"The magic of your {spell} spell fizzles and fails!{Constants.NewLine}");
                                    tPlayer.Send($"{desc.Player.Name} tries to fire a spell at you but their magic fails!{Constants.NewLine}");
                                }
                            }
                        }
                        else
                        {
                            var targetNPCs = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom);
                            var targetPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            if (targetNPCs != null && targetNPCs.Count > 0)
                            {
                                foreach(var npc in targetNPCs)
                                {
                                    if (npc.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                                    {
                                        desc.Send($"Some mystical force prevents your magic from harming {npc.Name}...{Constants.NewLine}");
                                        continue;
                                    }
                                    if (npc.FollowingPlayer == Guid.Empty || (SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Player.PVP && desc.Player.PVP))
                                    {
                                        var hpEffect = spell.CalculateSpellHPEffect(desc.Player, npc, out bool hits);
                                        if (hits)
                                        {
                                            if (hpEffect <= 0)
                                            {
                                                desc.Send($"{npc.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                            }
                                            npc.AdjustHP(hpEffect * -1, out bool isKilled);
                                            if (isKilled)
                                            {
                                                desc.Send($"Your magic deals lethal damage to {npc.Name}! You gain {npc.BaseExpAward} Exp and {npc.Gold} gold!{Constants.NewLine}");
                                                desc.Player.AddExp(npc.BaseExpAward, false, false);
                                                desc.Player.AddGold(npc.Gold, false);
                                                npc.Kill(true, ref desc);
                                            }
                                            else
                                            {
                                                if (hpEffect >= 1)
                                                {
                                                    desc.Send($"The magic of your {spell} spell deals {hpEffect} damage to {npc.Name}!{Constants.NewLine}");
                                                }
                                                if (npc.FollowingPlayer != Guid.Empty)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Send($"{desc.Player}'s {spell} deals {hpEffect} damage to {npc.Name}!{Constants.NewLine}");
                                                }
                                                var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, npc.NPCGuid);
                                                if (existingSessions == null || existingSessions.Count == 0)
                                                {
                                                    CombatSession s = new CombatSession(desc, npc, desc.ID, npc.NPCGuid);
                                                    CombatSession s1 = new CombatSession(npc, desc, npc.NPCGuid, desc.ID);
                                                    CombatManager.Instance.AddCombatSession(s);
                                                    CombatManager.Instance.AddCombatSession(s1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"Somehow the magic of your {spell} misses {npc.Name}!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Some mystical force prevents your magic from harming {npc.Name}...{Constants.NewLine}");
                                    }
                                }
                            }
                            foreach(var p in targetPlayers)
                            {
                                if (!desc.Player.PVP || !p.Player.PVP)
                                {
                                    if (desc.ID != p.ID)
                                    {
                                        desc.Send($"Some mystical force prevents your magic from harming {p.Player.Name}...{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    if (desc.ID != p.ID)
                                    {
                                        var hpEffect = spell.CalculateSpellHPEffect(desc.Player, p.Player, out bool hits);
                                        if (hits)
                                        {
                                            p.Player.AdjustHP(hpEffect * -1, out bool isKilled);
                                            if (hpEffect <= 0)
                                            {
                                                desc.Send($"{p.Player.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                            }
                                            if (isKilled)
                                            {
                                                desc.Send($"The power of your magic overcomes {p.Player.Name}!{Constants.NewLine}");
                                                p.Send($"The power of {desc.Player.Name}'s magic overcomes you and you give in to your wounds!{Constants.NewLine}");
                                                p.Player.Kill();
                                            }
                                            else
                                            {
                                                if (hpEffect >= 1)
                                                {
                                                    desc.Send($"The magic of your {spell} spell causes {hpEffect} damage to {p.Player.Name}!{Constants.NewLine}");
                                                    p.Send($"The magic of {desc.Player.Name}'s {spell} spell causes {hpEffect} damage to you!{Constants.NewLine}");
                                                }
                                                var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, p.ID);
                                                if (existingSessions == null || existingSessions.Count == 0)
                                                {
                                                    CombatSession a = new CombatSession(desc, p, desc.ID, p.ID);
                                                    CombatSession b = new CombatSession(p, desc, p.ID, desc.ID);
                                                    CombatManager.Instance.AddCombatSession(a);
                                                    CombatManager.Instance.AddCombatSession(b);
                                                    if (desc.Player.FollowerID != Guid.Empty)
                                                    {
                                                        var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                        CombatSession c = new CombatSession(f, p, f.NPCGuid, p.ID);
                                                        CombatSession d = new CombatSession(p, f, p.ID, f.NPCGuid);
                                                        CombatManager.Instance.AddCombatSession(c);
                                                        CombatManager.Instance.AddCombatSession(d);
                                                    }
                                                    if (p.Player.FollowerID != Guid.Empty)
                                                    {
                                                        var f = NPCManager.Instance.GetNPCByGUID(p.Player.FollowerID);
                                                        CombatSession e = new CombatSession(f, desc, f.NPCGuid, desc.ID);
                                                        CombatSession g = new CombatSession(desc, f, desc.ID, f.NPCGuid);
                                                        CombatManager.Instance.AddCombatSession(e);
                                                        CombatManager.Instance.AddCombatSession(g);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"The magic of your {spell} spell somehow misses {p.Player.Name}!{Constants.NewLine}");
                                            p.Send($"The magic of {desc.Player.Name}'s spell somehow misses you!{Constants.NewLine}");
                                        }
                                    }
                                }    
                            }
                        }
                        break;

                    case SpellType.Buff:
                        if (!spell.AOESpell)
                        {
                            int hpMod = 0;
                            bool hitsTarget = true;
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            if (targetActor is Descriptor)
                            {
                                Descriptor tDesc = (Descriptor)targetActor;
                                spell.ApplyBuffSpell(desc.Player, tDesc.Player, out hitsTarget, out hpMod);
                                if (hitsTarget)
                                {
                                    desc.Send($"You bless {tDesc.Player.Name} with the power of {spell}!{Constants.NewLine}");
                                    tDesc.Send($"{desc.Player.Name} blesses you with the power of {spell}!{Constants.NewLine}");
                                    if (hpMod > 0)
                                    {
                                        tDesc.Player.AdjustHP(hpMod, out _);
                                        tDesc.Send($"{desc.Player.Name}'s {spell} spell heals you for {hpMod} damage!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"You try to cast {spell} on {tDesc.Player.Name} but your magic fails and nothing happens!{Constants.NewLine}");
                                    tDesc.Send($"{desc.Player.Name} tries to cast a spell on you, but their magic fails and nothing happnes!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                NPC tNPC = (NPC)targetActor;
                                spell.ApplyBuffSpell(desc.Player, tNPC, out hitsTarget, out hpMod);
                                if (hitsTarget)
                                {
                                    desc.Send($"You bless {tNPC.Name} with the power of {spell}!{Constants.NewLine}");
                                    if (tNPC.IsFollower)
                                    {
                                        Descriptor owner = SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer);
                                        owner.Send($"{desc.Player.Name} has blessed your follower with the power of {spell}!{Constants.NewLine}");
                                        if (hpMod > 0)
                                        {
                                            owner.Send($"The magic of {desc.Player.Name}'s {spell} spell has healed your follower for {hpMod} damage!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"You try to cast {spell} on {tNPC.Name} but your magic fails and nothing happens!{Constants.NewLine}");
                                }
                            }
                        }
                        else
                        {
                            int hpMod = 0;
                            bool hitsTarget = true;
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            var targetNPCs = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).NPCsInRoom;
                            var targetPlayers = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom;
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            if (targetNPCs != null && targetNPCs.Count > 0)
                            {
                                foreach (var npc in targetNPCs)
                                {
                                    spell.ApplyBuffSpell(desc.Player, npc, out hitsTarget, out hpMod);
                                    if (hitsTarget)
                                    {
                                        desc.Send($"You bless {npc.Name} with the power of {spell}!{Constants.NewLine}");
                                        if (hpMod > 0)
                                        {
                                            npc.AdjustHP(hpMod, out _);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Somehow your {spell} spell misses {npc.Name}!{Constants.NewLine}");
                                    }
                                }
                            }
                            if (targetPlayers != null && targetPlayers.Count > 0)
                            {
                                foreach (var p in targetPlayers)
                                {
                                    spell.ApplyBuffSpell(desc.Player, p.Player, out hitsTarget, out hpMod);
                                    if (hitsTarget)
                                    {
                                        desc.Send($"You bless {p.Player.Name} with the power of {spell}!{Constants.NewLine}");
                                        p.Send($"{desc.Player.Name} blesses you with the power of {spell}!{Constants.NewLine}");
                                        if (hpMod > 0)
                                        {
                                            p.Send($"The magic of {desc.Player.Name}'s {spell} spell heals you for {hpMod} damage!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Somehow your {spell} spell misses {p.Player.Name}!{Constants.NewLine}");
                                        p.Send($"Somehow the magic of {desc.Player.Name}'s {spell} spell misses you!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        break;

                    case SpellType.Debuff:
                        if (!spell.AOESpell)
                        {
                            int hpMod = 0;
                            bool hitsTarget = true;
                            if (targetActor is Descriptor)
                            {
                                Descriptor tDesc = (Descriptor)targetActor;
                                desc.Player.AdjustMP((int)spell.MPCost * -1);
                                spell.ApplyBuffSpell(desc.Player, tDesc.Player, out hitsTarget, out hpMod);
                                if (hitsTarget)
                                {
                                    desc.Send($"You have cursed {tDesc.Player.Name} with the power of {spell}!{Constants.NewLine}");
                                    tDesc.Send($"{desc.Player.Name} has cursed you with the power of {spell}!{Constants.NewLine}");
                                    if (hpMod != 0)
                                    {
                                        tDesc.Player.AdjustHP(hpMod * -1, out bool isKilled);
                                        if (hpMod <= 0)
                                        {
                                            desc.Send($"{tDesc.Player.Name} has absorbed the power of your spell!{Constants.NewLine}");
                                            tDesc.Send($"{desc.Player.Name} hurls a spell at you, but you have absorbed its effect!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (isKilled)
                                            {
                                                desc.Send($"The power of your magic overcomes {tDesc.Player.Name} and they surrender to death!{Constants.NewLine}");
                                                tDesc.Send($"The power of {desc.Player.Name}'s {spell} spell overcomes you and you give in to death!{Constants.NewLine}");
                                                tDesc.Player.Kill();
                                                return;
                                            }
                                            else
                                            {
                                                desc.Send($"Your {spell} spell strikes {tDesc.Player.Name} causing {hpMod} damage!{Constants.NewLine}");
                                                tDesc.Send($"{desc.Player.Name}'s {spell} spell hits you for {hpMod} damage!{Constants.NewLine}");
                                            }
                                        }
                                        CombatSession s1 = new CombatSession(desc, tDesc, desc.ID, tDesc.ID);
                                        CombatSession s2 = new CombatSession(tDesc, desc, tDesc.ID, desc.ID);
                                        CombatManager.Instance.AddCombatSession(s1);
                                        CombatManager.Instance.AddCombatSession(s2);
                                        if (desc.Player.FollowerID != Guid.Empty)
                                        {
                                            NPC fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                            CombatSession s3 = new CombatSession(fNPC, tDesc, fNPC.NPCGuid, tDesc.ID);
                                            CombatSession s4 = new CombatSession(tDesc, fNPC, tDesc.ID, fNPC.NPCGuid);
                                            CombatManager.Instance.AddCombatSession(s3);
                                            CombatManager.Instance.AddCombatSession(s4);
                                        }
                                        if (tDesc.Player.FollowerID != Guid.Empty)
                                        {
                                            NPC tDescFNPC = NPCManager.Instance.GetNPCByGUID(tDesc.Player.FollowerID);
                                            CombatSession s5 = new CombatSession(desc, tDescFNPC, desc.ID, tDescFNPC.NPCGuid);
                                            CombatSession s6 = new CombatSession(tDescFNPC, desc, tDescFNPC.NPCGuid, desc.ID);
                                            CombatManager.Instance.AddCombatSession(s5);
                                            CombatManager.Instance.AddCombatSession(s6);
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"The Winds of Magic abandon you and your spell fails!{Constants.NewLine}");
                                    tDesc.Send($"{desc.Player.Name} seems to be trying to prepare a spell, but the Winds of Magic abandon them and the spell fails!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                NPC tNPC = (NPC)targetActor;
                                if (tNPC.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                                {
                                    desc.Send($"Some mystical force prevents you from harming {tNPC.Name}...{Constants.NewLine}");
                                    return;
                                }
                                if (tNPC.IsFollower)
                                {
                                    if (!desc.Player.PVP || !SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Player.PVP)
                                    {
                                        desc.Send($"Some mystical force prevents that from happening!{Constants.NewLine}");
                                        return;
                                    }
                                    spell.ApplyBuffSpell(desc.Player, tNPC, out hitsTarget, out hpMod);
                                    desc.Player.AdjustMP((int)spell.MPCost * -1);
                                    if (hitsTarget)
                                    {
                                        tNPC.AdjustHP(hpMod * -1, out bool isKilled);
                                        if (hpMod <= 0)
                                        {
                                            desc.Send($"{tNPC.Name} absorbs the power of your spell!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (isKilled)
                                            {
                                                desc.Send($"The power of your {spell} spell is too much for {tNPC.Name} and they die instantly!{Constants.NewLine}");
                                                SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Send($"{desc.Player.Name}'s {spell} spell kills your follower stone dead!{Constants.NewLine}");
                                                tNPC.Kill();
                                                return;
                                            }
                                            else
                                            {
                                                desc.Send($"Your {spell} spell strikes {tNPC.Name} causing {hpMod} damage!{Constants.NewLine}");
                                                SessionManager.Instance.GetPlayerByGUID(tNPC.FollowingPlayer).Send($"{desc.Player.Name}'s {spell} spell strikes your follower causing {hpMod} damage!{Constants.NewLine}");
                                            }
                                        }
                                        var existingSession = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, tNPC.NPCGuid);
                                        if (existingSession == null || existingSession.Count == 0)
                                        {
                                            CombatSession c1 = new CombatSession(desc, tNPC, desc.ID, tNPC.NPCGuid);
                                            CombatSession c2 = new CombatSession(tNPC, desc, tNPC.NPCGuid, desc.ID);
                                            CombatManager.Instance.AddCombatSession(c1);
                                            CombatManager.Instance.AddCombatSession(c2);
                                            if (desc.Player.FollowerID != Guid.Empty)
                                            {
                                                var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                CombatSession c3 = new CombatSession(fNPC, tNPC, fNPC.NPCGuid, tNPC.NPCGuid);
                                                CombatSession c4 = new CombatSession(tNPC, fNPC, tNPC.NPCGuid, fNPC.NPCGuid);
                                                CombatManager.Instance.AddCombatSession(c3);
                                                CombatManager.Instance.AddCombatSession(c4);
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        desc.Send($"The magic of your {spell} spell fizzles and fails!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    spell.ApplyBuffSpell(desc.Player, tNPC, out hitsTarget, out hpMod);
                                    desc.Player.AdjustMP((int)spell.MPCost * -1);
                                    if (hitsTarget)
                                    {
                                        tNPC.AdjustHP(hpMod * -1, out bool isKilled);
                                        if (hpMod <= 0)
                                        {
                                            desc.Send($"{tNPC.Name} absorbs the power of your spell!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (isKilled)
                                            {
                                                desc.Send($"The power of your {spell} spell is too much for {tNPC.Name} and they die instantly!{Constants.NewLine}");
                                                desc.Send($"You gain {tNPC.BaseExpAward} Exp and {tNPC.Gold} gold!{Constants.NewLine}");
                                                tNPC.Kill(true, ref desc);
                                                return;
                                            }
                                            else
                                            {
                                                desc.Send($"Your {spell} spell strikes {tNPC.Name} causing {hpMod} damage!{Constants.NewLine}");
                                            }
                                        }
                                        var existingSession = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, tNPC.NPCGuid);
                                        if (existingSession == null || existingSession.Count == 0)
                                        {
                                            CombatSession c1 = new CombatSession(desc, tNPC, desc.ID, tNPC.NPCGuid);
                                            CombatSession c2 = new CombatSession(tNPC, desc, tNPC.NPCGuid, desc.ID);
                                            CombatManager.Instance.AddCombatSession(c1);
                                            CombatManager.Instance.AddCombatSession(c2);
                                            if (desc.Player.FollowerID != Guid.Empty)
                                            {
                                                var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                CombatSession c3 = new CombatSession(fNPC, tNPC, fNPC.NPCGuid, tNPC.NPCGuid);
                                                CombatSession c4 = new CombatSession(tNPC, fNPC, tNPC.NPCGuid, fNPC.NPCGuid);
                                                CombatManager.Instance.AddCombatSession(c3);
                                                CombatManager.Instance.AddCombatSession(c4);
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        desc.Send($"The magic of your {spell} spell fizzles and fails!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            desc.Player.AdjustMP((int)spell.MPCost * -1);
                            int hpMod = 0;
                            bool hitsTarget = true;
                            var targetNPCs = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).NPCsInRoom;
                            var targetPlayers = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom;
                            if (targetNPCs != null && targetNPCs.Count > 0)
                            {
                                foreach(var npc in targetNPCs)
                                {
                                    if (npc.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                                    {
                                        desc.Send($"Some mystical force prevents you from harming {npc.Name}...{Constants.NewLine}");
                                        continue;
                                    }
                                    if (!npc.IsFollower || (SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Player.PVP && desc.Player.PVP))
                                    {
                                        spell.ApplyBuffSpell(desc.Player, npc, out hitsTarget, out hpMod);
                                        if (hitsTarget)
                                        {
                                            npc.AdjustHP(hpMod * -1, out bool isKilled);
                                            if (hpMod <= 0)
                                            {
                                                desc.Send($"{npc.Name} has absorbed the magic of your spell!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                if (isKilled)
                                                {
                                                    desc.Send($"Your {spell} spell deals lethal damage to {npc.Name}! You gain {npc.BaseExpAward} Exp and {npc.Gold} gold!{Constants.NewLine}");
                                                    desc.Player.AddExp(npc.BaseExpAward, false, false);
                                                    desc.Player.AddGold(npc.Gold, false);
                                                    npc.Kill(true, ref desc);
                                                }
                                                else
                                                {
                                                    desc.Send($"The magic of your {spell} spell deals {hpMod} damage to {npc.Name}!{Constants.NewLine}");
                                                    if (npc.FollowingPlayer != Guid.Empty)
                                                    {
                                                        SessionManager.Instance.GetPlayerByGUID(npc.FollowingPlayer).Send($"{desc.Player.Name}'s {spell} spell deals {hpMod} damage to {npc.Name}!{Constants.NewLine}");
                                                    }
                                                    var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, npc.NPCGuid);
                                                    if (existingSessions == null || existingSessions.Count == 0)
                                                    {
                                                        CombatSession s1 = new CombatSession(desc, npc, desc.ID, npc.NPCGuid);
                                                        CombatSession s2 = new CombatSession(npc, desc, npc.NPCGuid, desc.ID);
                                                        CombatManager.Instance.AddCombatSession(s1);
                                                        CombatManager.Instance.AddCombatSession(s2);
                                                        if (desc.Player.FollowerID != Guid.Empty)
                                                        {
                                                            var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                            CombatSession s3 = new CombatSession(fNPC, npc, fNPC.NPCGuid, npc.NPCGuid);
                                                            CombatSession s4 = new CombatSession(npc, fNPC, npc.NPCGuid, fNPC.NPCGuid);
                                                            CombatManager.Instance.AddCombatSession(s3);
                                                            CombatManager.Instance.AddCombatSession(s4);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"Somehow the magic of your {spell} spell misses {npc.Name}!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Some mystical force prevents your magic from harming {npc.Name}!{Constants.NewLine}");
                                    }
                                }
                            }
                            if (targetPlayers != null && targetPlayers.Count > 0)
                            {
                                foreach(var p in targetPlayers)
                                {
                                    if (!desc.Player.PVP || !p.Player.PVP)
                                    {
                                        if (desc.ID != p.ID)
                                        {
                                            desc.Send($"Some mystical force prevents your magic from harming {p.Player.Name}...{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        if (desc.ID != p.ID)
                                        {
                                            spell.ApplyBuffSpell(desc.Player, p.Player, out hitsTarget, out hpMod);
                                            if (hitsTarget)
                                            {
                                                p.Player.AdjustHP(hpMod * -1, out bool isKilled);
                                                if (hpMod <= 0)
                                                {
                                                    desc.Send($"{p.Player.Name} has absorbed the power of your {spell} spell!{Constants.NewLine}");
                                                    p.Send($"You have absorbed the magic of {desc.Player.Name}'s {spell} spell!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    if (isKilled)
                                                    {
                                                        desc.Send($"Your {spell} spell deals lethal damage to {p.Player.Name}, killing them instantly!{Constants.NewLine}");
                                                        p.Send($"{desc.Player.Name}'s {spell} spell deals lethal damage to you, killing you instantly!{Constants.NewLine}");
                                                        p.Player.Kill();
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"The magic of your {spell} spell strikes {p.Player.Name} for {hpMod} damage!{Constants.NewLine}");
                                                        p.Send($"The magic of {desc.Player.Name}'s {spell} spell strikes you for {hpMod} damage!{Constants.NewLine}");
                                                        var existingSessions = CombatManager.Instance.GetCombatSessionsForCombatantPairing(desc.ID, p.ID);
                                                        if (existingSessions == null || existingSessions.Count == 0)
                                                        {
                                                            CombatSession s1 = new CombatSession(desc, p, desc.ID, p.ID);
                                                            CombatSession s2 = new CombatSession(p, desc, p.ID, desc.ID);
                                                            CombatManager.Instance.AddCombatSession(s1);
                                                            CombatManager.Instance.AddCombatSession(s2);
                                                            if (desc.Player.FollowerID != Guid.Empty)
                                                            {
                                                                var fNPC = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                                CombatSession s3 = new CombatSession(fNPC, p, fNPC.NPCGuid, p.ID);
                                                                CombatSession s4 = new CombatSession(p, fNPC, p.ID, fNPC.NPCGuid);
                                                                CombatManager.Instance.AddCombatSession(s3);
                                                                CombatManager.Instance.AddCombatSession(s4);
                                                            }
                                                            if (p.Player.FollowerID != Guid.Empty)
                                                            {
                                                                var tfNPC = NPCManager.Instance.GetNPCByGUID(p.Player.FollowerID);
                                                                CombatSession s5 = new CombatSession(tfNPC, desc, tfNPC.NPCGuid, desc.ID);
                                                                CombatSession s6 = new CombatSession(desc, tfNPC, desc.ID, tfNPC.NPCGuid);
                                                                CombatManager.Instance.AddCombatSession(s5);
                                                                CombatManager.Instance.AddCombatSession(s6);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Somehow your magic misses {p.Player.Name}!{Constants.NewLine}");
                                                p.Send($"Somehow the magic of {desc.Player.Name}'s {spell} spell misses you!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void FleeCombat(ref Descriptor desc)
        {
            if (desc.Player.IsInCombat)
            {
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).RoomExits.Count > 0)
                {
                    var sessions = CombatManager.Instance.GetCombatSessionsForCombatant(desc.ID);
                    if (sessions != null && sessions.Count > 0)
                    {
                        foreach (var s in sessions)
                        {
                            CombatManager.Instance.RemoveCombatSession(s);
                        }
                    }
                    desc.Player.Position = ActorPosition.Standing;
                    var rndExit = Helpers.GetRandomExit(desc.Player.CurrentRoom);
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    desc.Player.Move(desc.Player.CurrentRoom, rndExit.DestinationRoomID, false);
                    desc.Send($"The fight is too much and you flee towards the {rndExit.ExitDirection}...{Constants.NewLine}");
                    if (localPlayers != null && localPlayers.Count > 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (!Regex.Match(desc.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"The fight is too much for {desc.Player.Name}, and they flee towards the {rndExit.ExitDirection}{Constants.NewLine}"
                                : $"The fight is too much for something and it flees towards the {rndExit.ExitDirection} like a chicken!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"There is nowhere to flee to, you must keep fighting!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Flee from what? You're not fighting!{Constants.NewLine}");
            }
        }

        private static void StartPVPCombat(ref Descriptor desc, ref string input)
        {
            if (desc.Player.PVP)
            {
                if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    var verb = GetVerb(ref input);
                    var target = input.Remove(0, verb.Length).Trim();
                    var tPlayer = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if (tPlayer != null)
                    {
                        if (tPlayer.Player.PVP)
                        {
                            var mySession = new CombatSession(desc, tPlayer, desc.ID, tPlayer.ID);
                            var tpSession = new CombatSession(tPlayer, desc, tPlayer.ID, desc.ID);
                            CombatManager.Instance.AddCombatSession(mySession);
                            CombatManager.Instance.AddCombatSession(tpSession);
                            desc.Player.Position = ActorPosition.Fighting;
                            tPlayer.Player.Position = ActorPosition.Fighting;
                            if (desc.Player.FollowerID != Guid.Empty)
                            {
                                var follower = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                var fSession = new CombatSession(follower, tPlayer, follower.NPCGuid, tPlayer.ID);
                                var pfSession = new CombatSession(tPlayer, follower, tPlayer.ID, follower.NPCGuid);
                                CombatManager.Instance.AddCombatSession(fSession);
                                CombatManager.Instance.AddCombatSession(pfSession);
                            }
                            if (tPlayer.Player.FollowerID != Guid.Empty)
                            {
                                var tpFollower = NPCManager.Instance.GetNPCByGUID(tPlayer.Player.FollowerID);
                                var tpfSession = new CombatSession(tpFollower, desc, tpFollower.NPCGuid, desc.ID);
                                var mtpfSession = new CombatSession(desc, tpFollower, desc.ID, tpFollower.NPCGuid);
                                CombatManager.Instance.AddCombatSession(tpfSession);
                                CombatManager.Instance.AddCombatSession(mtpfSession);
                            }
                        }
                        else
                        {
                            desc.Send($"That person doesn't want to fight you right now...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Some mystical force prevents fighting here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Please enable your PVP flag to start PVP combat.{Constants.NewLine}");
            }
        }

        private static void StartCombat(ref Descriptor desc, ref string input)
        {
            if (desc.Player.Position == ActorPosition.Standing)
            {
                if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    var verb = GetVerb(ref input).Trim();
                    var target = input.Remove(0, verb.Length).Trim();
                    if (!string.IsNullOrEmpty(target))
                    {
                        var t = GetTargetNPC(ref desc, target);
                        if (t != null && t.FollowingPlayer == Guid.Empty)
                        {
                            if (!t.BehaviourFlags.HasFlag(NPCFlags.NoAttack) && t.Position != ActorPosition.Fighting)
                            {
                                if (!desc.Player.Visible)
                                {
                                    desc.Player.Visible = true;
                                    desc.Send($"You shimmer and become visible again.{Constants.NewLine}");
                                }
                                var pSession = new CombatSession(desc, t, desc.ID, t.NPCGuid);
                                var mSession = new CombatSession(t, desc, t.NPCGuid, desc.ID);
                                CombatManager.Instance.AddCombatSession(pSession);
                                CombatManager.Instance.AddCombatSession(mSession);
                                if (desc.Player.FollowerID != Guid.Empty)
                                {
                                    var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                    var fSession = new CombatSession(f, t, f.NPCGuid, t.NPCGuid);
                                    var mfSession = new CombatSession(t, f, t.NPCGuid, f.NPCGuid);
                                    CombatManager.Instance.AddCombatSession(fSession);
                                    CombatManager.Instance.AddCombatSession(mfSession);
                                }
                                desc.Player.Position = ActorPosition.Fighting;
                            }
                            else
                            {
                                desc.Send($"Some otherworldly force prevents you from attacking {t.Name}...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            if (t == null)
                            {
                                desc.Send($"That doesn't seem to be here...{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You cannot attack someone's follower!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"Start a fight with what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"A feeling of peace comes over you... It would be impossible to fight here!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You're not in a position to do that right now.{Constants.NewLine}");
            }
        }

        private static void DescribeSkill(ref Descriptor desc, ref string input)
        {
            var skillName = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if (SkillManager.Instance.SkillExists(skillName))
            {
                var s = SkillManager.Instance.GetSkill(skillName);
                desc.Send($"{s.Name} {s.Description.ToLower()}{Constants.NewLine}");
            }
            else
            {
                if (SpellManager.Instance.SpellExists(skillName))
                {
                    var s = SpellManager.Instance.GetSpell(skillName);
                    desc.Send($"{s.SpellName} {s.Description.ToLower()}{Constants.NewLine}");
                }
                else
                {
                    var r = RecipeManager.Instance.GetRecipe(skillName);
                    if (r != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Name: {r.RecipeName}");
                        sb.AppendLine($"Description: {r.RecipeDescription}");
                        sb.AppendLine($"Produces: {ItemManager.Instance.GetItemByID(r.RecipeResult).Name}");
                        sb.AppendLine("Requires:");
                        foreach (var m in r.RequiredMaterials)
                        {
                            sb.AppendLine($"{Constants.TabStop}{m.Value} x {ItemManager.Instance.GetItemByID(m.Key).Name}");
                        }
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"No such skill, spell or recipe could be found.{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DoDiceGamble(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Gambler))
            {
                var amount = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (!string.IsNullOrEmpty(amount))
                {
                    if (uint.TryParse(amount, out uint gpBet))
                    {
                        if (gpBet > desc.Player.Gold)
                        {
                            desc.Send($"You can't wager more gold than you have!{Constants.NewLine}");
                        }
                        else
                        {
                            var playerRoll1 = Helpers.RollDice(1, 100);
                            var playerRoll2 = Helpers.RollDice(1, 100);
                            var dicerRoll = Helpers.RollDice(1, 100);
                            uint playerFinalRoll = 0;
                            if (desc.Player.HasSkill("Gambling"))
                            {
                                playerFinalRoll = playerRoll1 <= playerRoll2 ? playerRoll2 : playerRoll1;
                            }
                            else
                            {
                                playerFinalRoll = playerRoll1;
                            }
                            if (playerFinalRoll > dicerRoll)
                            {
                                var winnings = Convert.ToUInt32(Math.Round((double)gpBet / 2, 0));
                                desc.Player.Gold += winnings;
                                desc.Send($"You rolled {playerFinalRoll}, the Dicer rolled {dicerRoll}! You win {winnings} gold!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Player.Gold -= gpBet;
                                desc.Send($"You rolled {playerFinalRoll}, the Dicer rolled {dicerRoll}! You lose your wager of {gpBet} gold!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"You can't bet that, whole numbers of GP only!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You must place a wager to win anything!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no one here to wager with!{Constants.NewLine}");
            }
        }

        private static void ShowAllEmotes(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input);
            var line = input.Remove(0, verb.Length).Trim();
            if (string.IsNullOrEmpty(line))
            {
                // No input so show the list of configured emotes
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                var emotes = EmoteManager.Instance.GetAllEmotes(string.Empty).OrderBy(x => x.EmoteName).ToList();
                if (emotes != null && emotes.Count > 0)
                {
                    int i = 0;
                    string l = "|| ";
                    for (int t = 0; t < emotes.Count; t++)
                    {
                        l = $"{l}{emotes[t].EmoteName}{Constants.TabStop}{Constants.TabStop}";
                        i++;
                        if (i >= 5 && t < emotes.Count)
                        {
                            i = 0;
                            sb.AppendLine(l.Trim());
                            l = "|| ";
                        }
                        if (i == emotes.Count)
                        {
                            sb.AppendLine(l.Trim());
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    desc.Send($"No Emotes are currently in the game{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You {line}{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                var pName = desc.Player.Name;
                foreach (var lp in localPlayers.Where(x => !Regex.IsMatch(x.Player.Name, pName)))
                {
                    var msg = desc.Player.Visible || lp.Player.Level >= Constants.ImmLevel ? $"{pName} {line}{Constants.NewLine}" : $"Something {line}{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        private static void DoEmote(ref Descriptor desc, ref string input, Emote e)
        {
            var verb = GetVerb(ref input);
            var targetString = input.Remove(0, verb.Length).Trim();
            e.ShowEmoteMessage(ref desc, targetString);
        }

        private static void DoRecall(ref Descriptor desc)
        {
            var r = RoomManager.Instance.GetRoom(Constants.PlayerStartRoom());
            if (r != null && !r.Flags.HasFlag(RoomFlags.NoTeleport))
            {
                var cr = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if (!cr.Flags.HasFlag(RoomFlags.NoTeleport) && desc.Player.CurrentSP >= 5)
                {
                    if (cr.PlayersInRoom != null && cr.PlayersInRoom.Count > 0)
                    {
                        foreach (var p in cr.PlayersInRoom)
                        {
                            if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} offers a prayer to the Gods and vanishes!{Constants.NewLine}"
                                    : $"The Winds of Magic swirl and something is taken away!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                    desc.Player.Move(desc.Player.CurrentRoom, Constants.PlayerStartRoom(), true);
                    desc.Player.CurrentSP -= 5;
                    if (r.PlayersInRoom != null && r.PlayersInRoom.Count > 1)
                    {
                        foreach (var p in r.PlayersInRoom)
                        {
                            if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} appears in a flash of magic!{Constants.NewLine}"
                                    : $"There is a flash of magic as something arrives!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"The gods refuse to answer your prayers... You're on your own!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"The gods refuse to answer your prayers... You're on your own!{Constants.NewLine}");
            }
        }

        private static void DonateItem(ref Descriptor desc, ref string input)
        {
            var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
            var i = GetTargetItem(ref desc, target, true);
            var donRoom = RoomManager.Instance.GetRoom(Game.GetDonationRoomRID());
            if (donRoom != null)
            {
                if (i != null)
                {
                    desc.Player.Inventory.Remove(i);
                    RoomManager.Instance.AddItemToRoomInventory(donRoom.RoomID, ref i);
                    Game.LogMessage($"INFO: Player {desc.Player.Name} donated item {i.Name} ({i.ID})", LogLevel.Info, true);
                    desc.Send($"You offer up {i.ShortDescription} to the Winds of Magic!{Constants.NewLine}");
                    var donRoomPlayers = donRoom.PlayersInRoom;
                    var localPlayers = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom;
                    if (localPlayers != null && localPlayers.Count > 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} offers {i.ShortDescription} to the Winds of Magic!{Constants.NewLine}"
                                    : $"Something offers {i.ShortDescription} to the Winds of Magic!";
                                p.Send(msg);
                            }
                        }
                    }
                    if (donRoomPlayers != null && donRoomPlayers.Count > 0)
                    {
                        foreach (var p in donRoomPlayers)
                        {
                            p.Send($"The Winds of Magic swirl, depositing {i.ShortDescription} on the ground!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"You don't seem to be carrying that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no donation room in the world!{Constants.NewLine}");
                Game.LogMessage($"WARN: Player {desc.Player.Name} tried to donate an item, but there is no Donation Room in the game", LogLevel.Warning, true);
            }
        }

        private static void AppraiseItemForSale(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if (r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if (s != null)
                {
                    var criteria = input.Replace(GetVerb(ref input), string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(criteria))
                    {
                        var i = GetTargetItem(ref desc, criteria, true);
                        if (i != null)
                        {
                            s.AppraiseItemForSale(ref desc, i);
                        }
                        else
                        {
                            desc.Send($"You don't seem to be carrying anything like that...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Appraise what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void ListShopWares(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if (r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var criteria = input.Remove(0, GetVerb(ref input).Length).Trim();
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if (s != null)
                {
                    s.ListShopInventory(ref desc, criteria);
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void SellItemToShop(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if (r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if (s != null)
                {
                    var criteria = input.Remove(0, GetVerb(ref input).Length).Trim();
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        var i = GetTargetItem(ref desc, criteria, true);
                        if (i != null)
                        {
                            s.SellItem(ref desc, i);
                        }
                        else
                        {
                            desc.Send($"You don't seem to be carrying anything like that...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Sell what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"This shop appears to be broken, please tell an Imm!{Constants.NewLine}");
                    Game.LogMessage($"ERROR: Player {desc.Player.Name} tried to sell an item in Room {desc.Player.CurrentRoom} but ShopManager returned null", LogLevel.Error, true);
                }
            }
        }

        private static void BuyItemFromShop(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if (r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                var criteria = input.Remove(0, GetVerb(ref input).Length).Trim();
                if (s != null)
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        s.BuyItem(ref desc, criteria);
                    }
                    else
                    {
                        desc.Send($"Buy what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void LearnSkillOrSpell(ref Descriptor desc, ref string input)
        {
            // learn <skill | spell | recipe | language> <name>
            // TODO: Add support for learning languages (other than Common) if the player doesn't know
            var elements = TokeniseInput(ref input);
            if (elements.Length == 1)
            {
                // no criteria specified so show what we can learn here, if anything
                if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasTrainer)
                {
                    desc.Send($"There is no one here to teach you!{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.SkillTrainer))
                {
                    sb.AppendLine("The trainer smiles and says: 'I can teach you... For a price!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Skill");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint skillsAvailable = 0;
                    foreach (var s in SkillManager.Instance.GetAllSkills().OrderBy(x => x.Name).ToList())
                    {
                        if (!desc.Player.HasSkill(s.Name) || (s.Name == "Extra Attack" && desc.Player.NumberOfAttacks + 1 <= 5))
                        {
                            skillsAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {s.Name}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {s.Name}");
                            }
                        }
                    }
                    if (skillsAvailable == 0)
                    {
                        sb.AppendLine("|| No skills available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine();
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.MagicTrainer))
                {
                    sb.AppendLine("The sorceror smiles. 'Magic? I can teach you... For a price!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Spell");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint spellsAvailable = 0;
                    foreach (var s in SpellManager.Instance.GetAllSpells().OrderBy(x => x.SpellName).ToList())
                    {
                        if (!desc.Player.HasSpell(s.SpellName))
                        {
                            spellsAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {s.SpellName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {s.SpellName}");
                            }
                        }
                    }
                    if (spellsAvailable == 0)
                    {
                        sb.AppendLine("|| No spells available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Scribe))
                {
                    // show scribe recipes
                    sb.AppendLine("The scribe flashes a toothy grin. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Scribing).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipeName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Scribe recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist))
                {
                    // show alchemy recipes
                    sb.AppendLine("The alchemist gives you a sickly grin. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Alchemy).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipeName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Alchemy recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith))
                {
                    // show blacksmith recipes
                    sb.AppendLine("The blacksmith flexes his mighty arms. 'The secrets of the Forge can be yours...'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Blacksmithing).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipeName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Blacksmithing recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler))
                {
                    sb.AppendLine("The jeweler smiles broadly. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Jewelcrafting).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipeName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipeName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Jewelcrafting recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.LanguageTrainer))
                {
                    sb.AppendLine("The old diplomat gives you a smile. 'If you want to learn, I can teach!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint langsAvailable = 0;
                    foreach (Languages lang in Enum.GetValues(typeof(Languages)))
                    {
                        if (!desc.Player.KnownLanguages.HasFlag(lang))
                        {
                            langsAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 5000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {lang}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {lang}");
                            }
                        }
                    }
                    if (langsAvailable == 0)
                    {
                        sb.AppendLine("|| No languages available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                desc.Send(sb.ToString());
            }
            else
            {
                if (elements.Length >= 2)
                {
                    // we are learning a skill or a spell
                    var t = elements[1];
                    var toLearn = input.Replace(elements[0], string.Empty).Replace(elements[1], string.Empty).Trim();
                    switch (t.ToLower())
                    {
                        case "skill":
                            if (SkillManager.Instance.SkillExists(toLearn))
                            {
                                if (!desc.Player.HasSkill(toLearn) || (toLearn.ToLower() == "extra attack" && desc.Player.NumberOfAttacks + 1 <= 5))
                                {
                                    var s = SkillManager.Instance.GetSkill(toLearn);
                                    var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                                    if (desc.Player.Gold >= p)
                                    {
                                        // skill exists, player does not know it and has enough gold to buy it
                                        desc.Send($"The trainer smiles. 'Certainly I can teach you that!'{Constants.NewLine}");
                                        desc.Player.Gold -= p;
                                        if (s.Name == "Extra Attack")
                                        {
                                            desc.Player.NumberOfAttacks++;
                                        }
                                        else
                                        {
                                            desc.Player.AddSkill(s.Name);
                                        }
                                    }
                                    else
                                    {
                                        // skill exists, player does not know it but does not have enough gold to buy it
                                        desc.Send($"The trainer laughs, 'You're a little short of gold, my friend!'{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    // skill exists but the player already knows it
                                    desc.Send($"'You already know all that I can teach about that,' the trainer says.{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                // skill does not exist
                                desc.Send($"The trainer shakes their head. 'I don't think I can teach you that.'{Constants.NewLine}");
                            }
                            break;

                        case "spell":
                            if (SpellManager.Instance.SpellExists(toLearn))
                            {
                                if (!desc.Player.HasSpell(toLearn))
                                {
                                    var s = SpellManager.Instance.GetSpell(toLearn);
                                    var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                                    if (desc.Player.Gold >= p)
                                    {
                                        desc.Send($"The sorceror smiles. 'Certainly I can teach you that!'{Constants.NewLine}");
                                        desc.Player.Gold -= p;
                                        desc.Player.AddSpell(s.SpellName);
                                    }
                                    else
                                    {
                                        desc.Send($"The sorceror smiles. 'Come back with more gold.'{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"'You already know all I can teach about that spell,' the sorceror says.{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"The sorceror shakes his head. 'I don't think I can teach you that.'{Constants.NewLine}");
                            }
                            break;

                        case "recipe":
                            var r = RecipeManager.Instance.GetRecipe(toLearn);
                            if (r != null)
                            {
                                if (!desc.Player.KnowsRecipe(r.RecipeName))
                                {
                                    bool canLearn = false;
                                    switch (r.RecipeType)
                                    {
                                        case RecipeType.Scribing:
                                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Scribe) && desc.Player.HasSkill("Scribing"))
                                            {
                                                canLearn = true;
                                            }
                                            break;

                                        case RecipeType.Jewelcrafting:
                                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler) && desc.Player.HasSkill("Jewelcrafting"))
                                            {
                                                canLearn = true;
                                            }
                                            break;

                                        case RecipeType.Blacksmithing:
                                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith) && desc.Player.HasSkill("Blacksmithing"))
                                            {
                                                canLearn = true;
                                            }
                                            break;

                                        case RecipeType.Alchemy:
                                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist) && desc.Player.HasSkill("Alchemy"))
                                            {
                                                canLearn = true;
                                            }
                                            break;
                                    }
                                    if (canLearn)
                                    {
                                        var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                                        if (desc.Player.Gold >= p)
                                        {
                                            desc.Player.Gold -= p;
                                            desc.Player.Recipes.Add(r);
                                            desc.Send($"You gain knowledge of crafting {r.RecipeName}{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            desc.Send($"You don't have enough gold!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"You cannot learn that recipe right now.{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"You already know that recipe!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"That recipe doesn't exist!{Constants.NewLine}");
                            }
                            break;

                        case "language":
                            if (Enum.TryParse<Languages>(toLearn, true, out Languages lang))
                            {
                                if (!desc.Player.KnownLanguages.HasFlag(lang))
                                {
                                    var p = Helpers.GetNewPurchasePrice(ref desc, 5000);
                                    if (desc.Player.Gold >= p)
                                    {
                                        desc.Player.Gold -= p;
                                        desc.Player.KnownLanguages |= lang;
                                        desc.Send($"The old diplomat smiles and passes long his knowledge of the {lang} language.{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"The old diplomat chuckles. 'You need more gold to learn that!'{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"The old diplomat shakes his head. 'You already know all I can teach about that language.'{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"That language doesn't exist!{Constants.NewLine}");
                            }
                            break;

                        default:
                            desc.Send($"Usage: learn <skill | spell | recipe | language> <name>{Constants.NewLine}");
                            break;
                    }
                }
            }
        }

        private static void ConsumeItem(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input);
            var itemName = input.Remove(0, verb.Length).Trim();
            var i = GetTargetItem(ref desc, itemName, true);
            if (i != null)
            {
                if (i.ItemType == ItemType.Consumable)
                {
                    var pn = desc.Player.Name;
                    desc.Send($"You greedily consume the {i.Name}!{Constants.NewLine}");
                    desc.Player.Inventory.Remove(i);
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Name != pn).ToList();
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        foreach (var lp in localPlayers)
                        {
                            var msg = desc.Player.Visible || lp.Player.Level >= Constants.ImmLevel
                                ? $"{pn} greedily consume the {i.Name} they were carrying!{Constants.NewLine}"
                                : $"There is a strange noise as something greedily consumes something!{Constants.NewLine}";
                            lp.Send(msg);
                        }
                    }
                    if (i.NumberOfDamageDice > 0)
                    {
                        int modAmount = (int)Helpers.RollDice(i.NumberOfDamageDice, i.SizeOfDamageDice);
                        foreach (ConsumableEffect flag in Helpers.GetPotionFlags(i.ConsumableEffect))
                        {
                            switch (i.ConsumableEffect)
                            {
                                case ConsumableEffect.SPHealing:
                                    desc.Player.AdjustSP(modAmount);
                                    desc.Send($"You feel invigorated!{Constants.NewLine}");
                                    break;

                                case ConsumableEffect.Healing:
                                    desc.Player.AdjustHP(modAmount, out _);
                                    desc.Send($"You feel your wounds fading to memory...{Constants.NewLine}");
                                    break;

                                case ConsumableEffect.MPHealing:
                                    desc.Player.AdjustMP(modAmount);
                                    desc.Send($"You as though your spiritial force has been renewed!{Constants.NewLine}");
                                    break;

                                case ConsumableEffect.Death:
                                    desc.Send($"You feel the spectral hand of Death upon you...{Constants.NewLine}");
                                    desc.Player.Kill();
                                    break;

                                case ConsumableEffect.Poison:
                                    desc.Send($"The {i.Name} tastes foul as you swallow it...{Constants.NewLine}");
                                    desc.Player.AdjustHP(modAmount * -1, out bool isKilled);
                                    if (isKilled)
                                    {
                                        desc.Player.Kill();
                                    }
                                    break;

                                case ConsumableEffect.DrainSP:
                                    desc.Send($"You begin to feel your stamina drain away...{Constants.NewLine}");
                                    desc.Player.AdjustSP(modAmount * -1);
                                    break;

                                case ConsumableEffect.DrainMP:
                                    desc.Send($"You feel as though something was sapping your very spirit...{Constants.NewLine}");
                                    desc.Player.AdjustMP(modAmount * -1);
                                    break;

                                case ConsumableEffect.Restoration:
                                    desc.Send($"You feel holy power filling you, restoring you...{Constants.NewLine}");
                                    desc.Player.CurrentMP = desc.Player.MaxMP;
                                    desc.Player.CurrentHP = desc.Player.MaxHP;
                                    desc.Player.CurrentSP = desc.Player.MaxSP;
                                    break;
                            }
                        }
                    }
                    if (i.AppliesBuff)
                    {
                        desc.Send($"You feel the magic in the {i.Name} coursing through you!{Constants.NewLine}");
                        foreach (var b in i.AppliedBuffs)
                        {
                            var buff = BuffManager.Instance.GetBuff(b);
                            if (buff != null)
                            {
                                desc.Player.AddBuff(buff.BuffName, 0, false);
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"You can't consume that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't seem to be carrying that...{Constants.NewLine}");
            }
        }

        private static void GiveItemToTarget(ref Descriptor desc, ref string input)
        {
            // give bob chain shirt
            // give bob gold 400
            var line = input.Remove(0, GetVerb(ref input).Length).Trim();
            var targetPlayer = TokeniseInput(ref line).First().Trim();
            var obj = line.Remove(0, targetPlayer.Length).Trim();
            var objTokens = TokeniseInput(ref obj);
            if (objTokens.Length >= 1)
            {
                if (objTokens.First() != null && objTokens.First().ToLower() == "gold")
                {
                    if (objTokens.Length == 2)
                    {
                        if (uint.TryParse(objTokens.Last().Trim(), out uint gpToGive))
                        {
                            if (gpToGive <= desc.Player.Gold)
                            {
                                var p = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, targetPlayer, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                if (p != null)
                                {
                                    p.Player.Gold += gpToGive;
                                    desc.Player.Gold -= gpToGive;
                                    desc.Send($"You hand over {gpToGive} gold coins to {p.Player.Name}, how generous!{Constants.NewLine}");
                                    var msgToTarget = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} hands you {gpToGive} gold coins!{Constants.NewLine}"
                                        : $"Something hands you {gpToGive} gold coins, how odd!{Constants.NewLine}";
                                    p.Send(msgToTarget);
                                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                    if (localPlayers != null && localPlayers.Count > 0)
                                    {
                                        foreach (var lp in localPlayers)
                                        {
                                            if (lp.Player.Name != desc.Player.Name && lp.Player.Name != p.Player.Name)
                                            {
                                                if (lp.Player.Level >= Constants.ImmLevel)
                                                {
                                                    lp.Send($"{desc.Player.Name} hands {gpToGive} gold coins to {p.Player.Name}. Very generous!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    string gn = desc.Player.Visible ? desc.Player.Name : "Something";
                                                    string rn = p.Player.Visible ? p.Player.Name : "Something";
                                                    lp.Send($"{gn} hands {gpToGive} gold coins to {rn}. Very generous!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"That person doesn't seem to be here right now...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"You don't have that much gold to give!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That doesn't seem like a valid amount of gold to give...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Usage: Give <player> gold <amount>{Constants.NewLine}");
                    }
                }
                else
                {
                    var i = GetTargetItem(ref desc, obj, true);
                    if (i != null)
                    {
                        var p = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, targetPlayer, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        if (p != null)
                        {
                            if (desc.Player.Level >= Constants.ImmLevel || p.Player.Visible)
                            {
                                desc.Player.Inventory.Remove(i);
                                p.Player.Inventory.Add(i);
                                desc.Send($"You give {i.Name} to {p.Player.Name}{Constants.NewLine}");
                                string msgToSendToTarget = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} has given you {i.Name}! How kind!{Constants.NewLine}"
                                    : $"Something has given you {i.Name}. How strange!{Constants.NewLine}";
                                p.Send(msgToSendToTarget);
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if (localPlayers != null && localPlayers.Count - 2 >= 1)
                                {
                                    foreach (var lp in localPlayers)
                                    {
                                        if (lp.Player.Name != desc.Player.Name && lp.Player.Name != p.Player.Name)
                                        {
                                            if (lp.Player.Level >= Constants.ImmLevel)
                                            {
                                                lp.Send($"{desc.Player.Name} hands {i.Name} to {p.Player.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                var g = desc.Player.Visible ? desc.Player.Name : "Something";
                                                var r = p.Player.Visible ? p.Player.Name : "something";
                                                lp.Send($"{g} hands {i.Name} to {r}{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                desc.Send($"That person isn't around right now...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            var n = GetTargetNPC(ref desc, targetPlayer);
                            if (n != null)
                            {
                                desc.Send($"You hand over {i.Name} to {n.Name}!{Constants.NewLine}");
                                n.Inventory.Add(i);
                                desc.Player.Inventory.Remove(i);
                            }
                            else
                            {
                                desc.Send($"That person isn't around right now...{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"You don't seem to be carrying that...{Constants.NewLine}");
                    }
                }
            }
        }

        private static void RemoveEquippedItem(ref Descriptor desc, ref string input)
        {
            try
            {
                if (Enum.TryParse(TokeniseInput(ref input).Last(), true, out WearSlot targetSlot))
                {
                    string msgToSendToPlayer = string.Empty;
                    string[] msgToSendToOthers = { string.Empty, string.Empty };
                    switch (targetSlot)
                    {
                        case WearSlot.Head:
                            if (desc.Player.EquipHead != null)
                            {
                                var i = desc.Player.EquipHead;
                                desc.Player.EquipHead = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from your head{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from their head{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on your head!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Neck:
                            if (desc.Player.EquipNeck != null)
                            {
                                var i = desc.Player.EquipNeck;
                                desc.Player.EquipNeck = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from around your neck{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from around their neck{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything around your neck!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Armour:
                            if (desc.Player.EquipArmour != null)
                            {
                                var i = desc.Player.EquipArmour;
                                desc.Player.EquipArmour = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop wearing {i.Name} as your armour{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops wearing {i.Name} as their armour{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing any armour!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.FingerLeft:
                            if (desc.Player.EquipLeftFinger != null)
                            {
                                var i = desc.Player.EquipLeftFinger;
                                desc.Player.EquipLeftFinger = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from a finger on your left hand{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from a finger on their left hand{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on the fingers of your left hand!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.FingerRight:
                            if (desc.Player.EquipRightFinger != null)
                            {
                                var i = desc.Player.EquipRightFinger;
                                desc.Player.EquipRightFinger = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from a finger on your right hand{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from a finger on the right hand{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on the fingers of your right hand!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Weapon:
                            if (desc.Player.EquipWeapon != null)
                            {
                                var i = desc.Player.EquipWeapon;
                                desc.Player.EquipWeapon = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop wielding {i.Name} as your weapon{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops using {i.Name} as their weapon{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't using anything as a weapon!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Held:
                            if (desc.Player.EquipHeld != null)
                            {
                                var i = desc.Player.EquipHeld;
                                desc.Player.EquipHeld = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop holding {i.Name}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops holding {i.Name}{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't holding anything!{Constants.NewLine}";
                            }
                            break;

                        default:
                            msgToSendToPlayer = $"Sorry, I didn't understand that{Constants.NewLine}";
                            break;
                    }
                    desc.Send(msgToSendToPlayer);
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    if (localPlayers != null && localPlayers.Count - 1 >= 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (p != desc)
                            {
                                if (desc.Player.Visible)
                                {
                                    p.Send(msgToSendToOthers[0]);
                                }
                                else
                                {
                                    if (p.Player.Level >= Constants.ImmLevel)
                                    {
                                        p.Send(msgToSendToOthers[0]);
                                    }
                                    else
                                    {
                                        p.Send(msgToSendToOthers[1]);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"Remove that from where, exactly?{Constants.NewLine}");
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} had an error removing an item: {ex.Message}", LogLevel.Error, true);
                desc.Send($"That didn't work!{Constants.NewLine}");
            }
        }

        private static void EquipItem(ref Descriptor desc, ref string input)
        {
            try
            {
                if (Enum.TryParse(TokeniseInput(ref input).Last(), true, out WearSlot targetSlot))
                {
                    var itemName = input.Replace(GetVerb(ref input), string.Empty).Replace(targetSlot.ToString().ToLower(), string.Empty).Trim();
                    var item = desc.Player.Inventory.Where(x => Regex.Match(x.Name, itemName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    string msgToSendToPlayer = string.Empty;
                    string[] msgToSendToOthers = { string.Empty, string.Empty };
                    if (item != null && (item.ItemType == ItemType.Armour || item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Ring))
                    {
                        switch (targetSlot)
                        {
                            case WearSlot.Head:
                                if (item.CanPlayerEquip(ref desc, WearSlot.Head, out string reply))
                                {
                                    desc.Player.EquipHead = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on your head{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on their head{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Neck:
                                if (item.CanPlayerEquip(ref desc, WearSlot.Neck, out reply))
                                {
                                    desc.Player.EquipNeck = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} around your neck{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} around their neck{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Armour:
                                if (item.CanPlayerEquip(ref desc, WearSlot.Armour, out reply))
                                {
                                    desc.Player.EquipArmour = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You don {item.Name} as your armour{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} dons {item.Name} as their armour{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.FingerLeft:
                                if (item.CanPlayerEquip(ref desc, WearSlot.FingerLeft, out reply))
                                {
                                    desc.Player.EquipLeftFinger = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on a finger on your left hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on a finger on their left hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.FingerRight:
                                if (item.CanPlayerEquip(ref desc, WearSlot.FingerRight, out reply))
                                {
                                    desc.Player.EquipRightFinger = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on a finger on your right hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on a finger on their right hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Weapon:
                                if (item.CanPlayerEquip(ref desc, WearSlot.Weapon, out reply))
                                {
                                    desc.Player.EquipWeapon = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You wield {(item).Name} as your weapon{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} wields {(item).Name} as their weapon{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Held:
                                if (item.CanPlayerEquip(ref desc, WearSlot.Held, out reply))
                                {
                                    desc.Player.EquipHeld = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You hold {(item).Name} in your off-hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} holds {(item).Name} in their off-hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, 0, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            default:
                                desc.Send($"That doesn't seem to work...{Constants.NewLine}");
                                break;
                        }
                        desc.Send(msgToSendToPlayer);
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (localPlayers != null && localPlayers.Count - 1 >= 1)
                        {
                            foreach (var p in localPlayers)
                            {
                                if (p != desc)
                                {
                                    if (desc.Player.Visible)
                                    {
                                        p.Send(msgToSendToOthers[0]);
                                    }
                                    else
                                    {
                                        if (p.Player.Level >= Constants.ImmLevel)
                                        {
                                            p.Send(msgToSendToOthers[0]);
                                        }
                                        else
                                        {
                                            p.Send(msgToSendToOthers[1]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"Try as you might, you can't seem to make that work...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Equip that where, exactly?{Constants.NewLine}");
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} encountered an error equipping an item: {ex.Message}", LogLevel.Error, true);
                desc.Send($"Sorry, I didn't understand that{Constants.NewLine}");
            }
        }

        private static void ShowEquippedItems(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine("|| You are wearing the following equipment:");
            sb.AppendLine($"|| Head:{Constants.TabStop}{desc.Player.EquipHead?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Neck:{Constants.TabStop}{desc.Player.EquipNeck?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Armour:{Constants.TabStop}{desc.Player.EquipArmour?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Finger (L):{Constants.TabStop}{desc.Player.EquipLeftFinger?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Finger (R):{Constants.TabStop}{desc.Player.EquipRightFinger?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Weapon:{Constants.TabStop}{desc.Player.EquipWeapon?.Name ?? "Nothing"}");
            sb.AppendLine($"|| Held:{Constants.TabStop}{desc.Player.EquipHeld?.Name ?? "Nothing"}");
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void DeleteCharacter(ref Descriptor desc)
        {
            bool delChar = false;
            bool validInput = false;
            Game.LogMessage($"{desc.Player.Name} has requested character deletion", LogLevel.Info, true);
            while (!validInput)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("This will delete your character from the game and disconnect your session.");
                sb.AppendLine("This cannot be undone and deleted characters cannot be restored.");
                sb.Append("Do you wish to continue (yes/no): ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (ValidateInput(input))
                {
                    validInput = input.ToLower() == "yes" || input.ToLower() == "no";
                    delChar = validInput && input.ToLower() == "yes";
                }
            }
            if (delChar)
            {
                Game.LogMessage($"{desc.Player.Name} has confirmed requested deletion of character data.", LogLevel.Info, true);
                if (DatabaseManager.DeleteCharacter(ref desc))
                {
                    SessionManager.Instance.Close(desc);
                }
                else
                {
                    desc.Send($"There was an error deleting your character.{Constants.NewLine}");
                    desc.Send($"Please contact an Immortal for assistance.{Constants.NewLine}");
                }
            }
        }

        private static void GetItemFromRoom(ref Descriptor desc, ref string input)
        {
            var obj = input.Remove(0, GetVerb(ref input).Length).Trim();
            if (!string.IsNullOrEmpty(obj))
            {
                if (obj.ToLower().StartsWith("gold"))
                {
                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GoldInRoom > 0)
                    {
                        var tokens = TokeniseInput(ref obj);
                        ulong gpToGet;
                        if (tokens.Length > 1)
                        {
                            // player has specified an amount of gold to take
                            if (ulong.TryParse(tokens.Last().Trim(), out gpToGet))
                            {
                                if (gpToGet > RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GoldInRoom)
                                {
                                    desc.Send($"There isn't that much gold here!{Constants.NewLine}");
                                    return;
                                }
                            }
                            else
                            {
                                desc.Send($"That doesn't seem right...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            gpToGet = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GoldInRoom;
                        }
                        desc.Player.Gold += gpToGet;
                        RoomManager.Instance.RemoveGoldFromRoom(desc.Player.CurrentRoom, gpToGet);
                        desc.Send($"You greedily snatch up the {gpToGet} gold coins!{Constants.NewLine}");
                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (playersToNotify != null && playersToNotify.Count > 1)
                        {
                            foreach (var p in playersToNotify)
                            {
                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                {
                                    if (p.Player.Name != desc.Player.Name)
                                    {
                                        p.Send($"{desc.Player.Name} greedily snatches up {gpToGet} gold coins! So much for charity!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    p.Send($"Something snatches up {gpToGet} gold coins...{Constants.NewLine}");
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"There isn't any gold here to take...{Constants.NewLine}");
                    }
                }
                else
                {
                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom != null && RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(obj))
                        {
                            var i = GetTargetItem(ref desc, obj, false);
                            if (i != null)
                            {
                                RoomManager.Instance.RemoveItemFromRoomInventory(desc.Player.CurrentRoom, ref i);
                                desc.Player.Inventory.Add(i);
                                desc.Send($"You pick up {i.ShortDescription}{Constants.NewLine}");
                                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if (playersToNotify != null && playersToNotify.Count > 1)
                                {
                                    foreach (var p in playersToNotify)
                                    {
                                        if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                        {
                                            if (p.Player.Name != desc.Player.Name)
                                            {
                                                p.Send($"{desc.Player.Name} takes {i.ShortDescription}... Hope no one needed that!{Constants.NewLine}");
                                            }
                                        }
                                        else
                                        {
                                            p.Send($"Something takes {i.ShortDescription}...{Constants.NewLine}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                desc.Send($"That item doesn't seem to be here...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That item doesn't seem to be here...{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DropCharacterItem(ref Descriptor desc, ref string input)
        {
            // TODO: Needs to deal with dropping gold into the room
            // drop short sword
            // drop chain shirt
            // drop gold 500
            var line = input.Remove(0, GetVerb(ref input).Length).Trim();
            var tokens = TokeniseInput(ref line);
            if (tokens.First().ToLower() == "gold")
            {
                var amount = tokens.Last() ?? string.Empty;
                if (!string.IsNullOrEmpty(amount))
                {
                    if (uint.TryParse(amount, out uint gpToDrop))
                    {
                        if (gpToDrop <= desc.Player.Gold)
                        {
                            desc.Player.Gold -= gpToDrop;
                            RoomManager.Instance.AddGoldToRoom(desc.Player.CurrentRoom, gpToDrop);
                            desc.Send($"You drop {gpToDrop} gold to the floor!{Constants.NewLine}");
                            var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            if (playersToNotify.Count > 1)
                            {
                                foreach (var p in playersToNotify)
                                {
                                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                    {
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            p.Send($"{desc.Player.Name} drops {gpToDrop} gold coins to the floor. What a litterbug!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        p.Send($"Something drops {gpToDrop} gold coins to the floor... How strange!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            desc.Send($"You don't have that much gold to drop!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That doesn't seem like a valid amount of coins to drop...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You need to say how much gold you're dropping...{Constants.NewLine}");
                }
            }
            else
            {
                if (desc.Player.Inventory != null && desc.Player.Inventory.Count > 0)
                {
                    string obj = line;
                    if (!string.IsNullOrEmpty(obj))
                    {
                        var i = GetTargetItem(ref desc, obj, true);
                        if (i != null)
                        {
                            // we have an object from the inventory to drop, remove from player and add to room
                            desc.Player.Inventory.Remove(i);
                            RoomManager.Instance.AddItemToRoomInventory(desc.Player.CurrentRoom, ref i);
                            desc.Send($"You drop {i.Name} on the floor{Constants.NewLine}");
                            var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            if (playersToNotify.Count > 1)
                            {
                                foreach (var p in playersToNotify)
                                {
                                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                    {
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            p.Send($"{desc.Player.Name} drops {i.ShortDescription} to the floor. What a litterbug!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        p.Send($"Something drops {i.ShortDescription} to the floor... How strange!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            // couldn't find a matching item in the player's inventory to drop
                            desc.Send($"You don't seem to be carrying that with you...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        // not able to determine the object to drop based off player input
                        desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You aren't carrying anything to drop...{Constants.NewLine}");
                }
            }
        }

        private static void DoHideSkill(ref Descriptor desc)
        {
            if (desc.Player.Position == ActorPosition.Standing && desc.Player.HasSkill("Hide") && !desc.Player.IsInCombat)
            {
                var pname = desc.Player.Name;
                if (desc.Player.Visible)
                {
                    if (desc.Player.CurrentMP > SkillManager.Instance.GetSkill("Hide").MPCost)
                    {
                        desc.Player.Visible = false;
                        desc.Player.CurrentMP -= (int)SkillManager.Instance.GetSkill("Hide").MPCost;
                        desc.Send($"With cunning and skill you hide yourself from view!{Constants.NewLine}");
                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (playersToNotify != null && playersToNotify.Count > 1)
                        {
                            foreach (var p in playersToNotify.Where(x => x.Player.Name != pname))
                            {
                                p.Send($"{pname} hides and becomes impossible to see!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"You don't have the energy to do that!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Player.Visible = true;
                    desc.Send($"You stop hiding and become visible again!{Constants.NewLine}");
                    var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    if (playersToNotify != null && playersToNotify.Count > 1)
                    {
                        foreach (var p in playersToNotify.Where(x => x.Player.Name != pname))
                        {
                            p.Send($"{pname} stops hiding and becomes visible again!{Constants.NewLine}");
                        }
                    }
                }
            }
            else
            {
                desc.Send($"You cannot use that skill right now.{Constants.NewLine}");
            }
        }

        private static void ShowCharInventory(ref Descriptor desc)
        {
            if (desc.Player.Inventory != null && desc.Player.Inventory.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| You are carrying:");
                foreach (var i in desc.Player.Inventory.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                {
                    var cnt = desc.Player.Inventory.Where(y => y.ID == i.ID).Count();
                    sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
            }
            else
            {
                desc.Send($"You are not carrying anything{Constants.NewLine}");
            }
        }

        private static void Who(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| The world currently contains the following beings:");
            var allPlayers = SessionManager.Instance.GetAllPlayers();
            bool isImm = desc.Player.Level >= Constants.ImmLevel;
            if (allPlayers != null && allPlayers.Count > 0)
            {
                foreach (var p in SessionManager.Instance.GetAllPlayers().OrderByDescending(x => x.Player.Level).ToList())
                {
                    switch (isImm)
                    {
                        case true:
                            if (p.Player.Visible)
                            {
                                sb.AppendLine($"|| {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in room {p.Player.CurrentRoom}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in room {p.Player.CurrentRoom} (Invisible)");
                            }
                            break;

                        case false:
                            if (p.Player.Name == desc.Player.Name || p.Player.Visible)
                            {
                                var playerZone = ZoneManager.Instance.GetZoneForRID(p.Player.CurrentRoom).ZoneName;
                                if (p.Player.Name == desc.Player.Name && !p.Player.Visible)
                                {
                                    // player doing who is not an Imm and is invisible
                                    sb.AppendLine($"|| {p.Player.Name}, the {p.Player.Class} (Invisible) in {playerZone}");
                                }
                                else
                                {
                                    sb.AppendLine($"|| {p.Player.Name}, the {p.Player.Class} in {playerZone}");
                                }
                            }
                            break;
                    }
                }
            }
            sb.AppendLine($"  {new string('=', 77)}{Constants.NewLine}");
            desc.Send(sb.ToString());
        }

        internal static void PushTarget(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim();
            var line = input.Remove(0, verb.Length).ToLower().Trim();
            string target = string.Empty;
            string direction = string.Empty;
            if (line.IndexOf('\"') > -1)
            {
                target = GetSkillOrSpellName(ref line);
                direction = line.Replace(target, string.Empty).Replace("\"", string.Empty).Trim();
            }
            else
            {
                var elements = TokeniseInput(ref line);
                if (elements != null)
                {
                    try
                    {
                        target = elements[0];
                        direction = elements[elements.Length - 1];
                    }
                    catch
                    {
                        desc.Send($"Usage: Push <target> <direction>{Constants.NewLine}");
                    }
                }
            }
            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(direction))
            {
                // get a reference to the target player or NPC
                object objTgt = null;
                var playersInRoom = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                var npcsInRoom = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom);
                int targetRID = -1;
                string roomDirection = string.Empty;
                if (playersInRoom != null && playersInRoom.Count > 1)
                {
                    var p = playersInRoom.Where(x => Regex.Match(target, x.Player.Name, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if (p != null && (p.Player.Visible || desc.Player.Level >= Constants.ImmLevel))
                    {
                        objTgt = p;
                    }
                }
                if (objTgt == null)
                {
                    objTgt = GetTargetNPC(ref desc, target);
                }
                if (objTgt != null)
                {
                    // check to see if we have an exit in the specified direction
                    switch (direction)
                    {
                        case "u":
                        case "up":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("up"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").ExitDirection;
                                }
                            }
                            break;

                        case "d":
                        case "down":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("down"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").ExitDirection;
                                }
                            }
                            break;

                        case "n":
                        case "north":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("north"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").ExitDirection;
                                }
                            }
                            break;

                        case "nw":
                        case "northwest":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("northwest"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").ExitDirection;
                                }
                            }
                            break;

                        case "w":
                        case "west":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("west"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").ExitDirection;
                                }
                            }
                            break;

                        case "sw":
                        case "southwest":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("southwest"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").ExitDirection;
                                }
                            }
                            break;

                        case "s":
                        case "south":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("south"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").ExitDirection;
                                }
                            }
                            break;

                        case "se":
                        case "southeast":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("southeast"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").ExitDirection;
                                }
                            }
                            break;

                        case "e":
                        case "east":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("east"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").ExitDirection;
                                }
                            }
                            break;

                        case "ne":
                        case "northeast":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("northeast"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").RoomDoor;
                                if (d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").ExitDirection;
                                }
                            }
                            break;

                        default:
                            desc.Send($"You can't push something in that direction!{Constants.NewLine}");
                            return;

                    }
                    if (targetRID > -1)
                    {
                        uint newRID = Convert.ToUInt32(targetRID);
                        if (RoomManager.Instance.RoomExists(newRID))
                        {
                            bool destRoomIsNoMob = RoomManager.Instance.GetRoom(newRID).Flags.HasFlag(RoomFlags.NoMobs);
                            var playerRoll = Helpers.RollDice(1, 20);
                            int playerFinalRoll = Convert.ToInt32(playerRoll);
                            var playerStrModifier = Helpers.CalculateAbilityModifier(desc.Player.Strength);
                            playerFinalRoll += Convert.ToInt32(playerStrModifier);
                            playerFinalRoll = playerFinalRoll < 1 ? 1 : playerFinalRoll;
                            var targetRoll = Helpers.RollDice(1, 20);
                            int targetFinalRoll = Convert.ToInt32(targetRoll);
                            bool okToPush = false;
                            bool targetIsPlayer = objTgt.GetType() == typeof(Descriptor);
                            string targetName = string.Empty;
                            if (playerRoll > 1 && playerRoll < 20)
                            {
                                // player rolled between 2 and 19 inclusive so compare STR rolls to see who wins
                                if (targetIsPlayer)
                                {
                                    // target is a player
                                    targetName = (objTgt as Descriptor).Player.Name;
                                    var targetStrModifier = Helpers.CalculateAbilityModifier((objTgt as Descriptor).Player.Strength);
                                    targetFinalRoll += targetStrModifier;
                                    targetFinalRoll = targetFinalRoll < 1 ? 1 : targetFinalRoll;
                                    okToPush = playerFinalRoll > targetFinalRoll;
                                }
                                else
                                {
                                    // target is an npc
                                    targetName = (objTgt as NPC).Name;
                                    if (!(objTgt as NPC).BehaviourFlags.HasFlag(NPCFlags.NoPush))
                                    {
                                        var targetStrModifier = Helpers.CalculateAbilityModifier((objTgt as NPC).Strength);
                                        targetFinalRoll += targetStrModifier;
                                        targetFinalRoll = targetFinalRoll < 1 ? 1 : targetFinalRoll;
                                        okToPush = playerFinalRoll > targetFinalRoll;
                                    }
                                    else
                                    {
                                        desc.Send($"Some mystical force prevents you from doing that!{Constants.NewLine}");
                                    }
                                }
                            }
                            if (playerRoll == 20)
                            {
                                // player rolled a nautral 20 so automatically wins
                                okToPush = true;
                            }
                            if (okToPush && targetIsPlayer || (okToPush && !destRoomIsNoMob))
                            {
                                // push the target in the specified direction
                                desc.Send($"With a mighty effort you push {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                if (targetIsPlayer)
                                {
                                    // notify the target player
                                    if (desc.Player.Visible || ((objTgt as Descriptor).Player.Level >= Constants.ImmLevel))
                                    {
                                        (objTgt as Descriptor).Send($"{desc.Player.Name} pushes you {roomDirection.ToLower()}!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        (objTgt as Descriptor).Send($"Something pushes you {roomDirection.ToLower()}!{Constants.NewLine}");
                                    }
                                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom.Count > 2)
                                    {
                                        foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom)
                                        {
                                            if (p.Player.Name != desc.Player.Name && p.Player.Name != (objTgt as Descriptor).Player.Name)
                                            {
                                                if (!desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something pushes something else {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (!desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} pushes something {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // target is an NPC so just notify any other players in the room
                                    foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom)
                                    {
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            if (desc.Player.Visible)
                                            {
                                                p.Send($"{desc.Player.Name} pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                p.Send($"Something pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                if (targetIsPlayer)
                                {
                                    var p = SessionManager.Instance.GetPlayer((objTgt as Descriptor).Player.Name);
                                    p.Player.Move(p.Player.CurrentRoom, newRID, false);
                                }
                                else
                                {
                                    var n = NPCManager.Instance.GetNPCByGUID((objTgt as NPC).NPCGuid);
                                    n.Move(ref n, desc.Player.CurrentRoom, newRID, false);
                                }
                            }
                            else
                            {
                                // we failed
                                if (!targetIsPlayer && destRoomIsNoMob)
                                {
                                    desc.Send($"Some mysterious force prevents you from doing that!{Constants.NewLine}");
                                }
                                else
                                {
                                    desc.Send($"Try as you might, you just can't summon the strength to do that!{Constants.NewLine}");
                                }
                                if (targetIsPlayer)
                                {
                                    // notify the target player
                                    if (desc.Player.Visible || ((objTgt as Descriptor).Player.Level >= Constants.ImmLevel))
                                    {
                                        (objTgt as Descriptor).Send($"{desc.Player.Name} tries to push you {roomDirection.ToLower()} but isn't strong enough!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        (objTgt as Descriptor).Send($"Something tries to push you {roomDirection.ToLower()} but isn't strong enough!{Constants.NewLine}");
                                    }
                                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom.Count > 2)
                                    {
                                        // if we have more than the player and the target here, notify them of what happened
                                        foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom)
                                        {
                                            if (p.Player.Name != desc.Player.Name && p.Player.Name != (objTgt as Descriptor).Player.Name)
                                            {
                                                if (!desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something tries to push something else {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (!desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} tries to push something {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // target is an NPC so just notify any other players in the room
                                    foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom)
                                    {
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            if (desc.Player.Visible)
                                            {
                                                p.Send($"{desc.Player.Name} tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                p.Send($"Something tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            desc.Send($"Some mysterious force prevents you from pushing {target} that way...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"There is no way to push {target} that way!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You don't see {target} here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Usage: Push <target> <direction>{Constants.NewLine}");
            }
        }

        internal static void ChangePlayerPosition(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).ToLower();
            if (desc.Player.Position == ActorPosition.Fighting)
            {
                desc.Send($"You're in combat and can't do that right now!{Constants.NewLine}");
                return;
            }
            switch (verb)
            {
                case "sit":
                    desc.Player.Position = ActorPosition.Sitting;
                    desc.Send($"You sit down to take the load off your feet for a while.{Constants.NewLine}");
                    break;

                case "rest":
                    desc.Player.Position = ActorPosition.Resting;
                    desc.Send($"You stop and take a nice rest.{Constants.NewLine}");
                    break;

                case "stand":
                    SessionManager.Instance.GetPlayerByGUID(desc.ID).Player.Position = ActorPosition.Standing;
                    desc.Send($"You stand up, ready to go!{Constants.NewLine}");
                    break;
            }
        }

        private static void Look(ref Descriptor desc, ref string input)
        {
            string verb = GetVerb(ref input).ToLower().Trim();
            string target = input.Remove(0, verb.Length).Trim().ToLower();
            string msgToSendToPlayer = string.Empty;
            string msgToSendToTarget = string.Empty;
            string[] msgToSendToOthers = new string[] { string.Empty, string.Empty };
            if (!string.IsNullOrEmpty(target))
            {
                switch (target.ToLower())
                {
                    case "up":
                    case "u":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("up"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look up...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view up...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                            }
                        }
                        break;

                    case "down":
                    case "d":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("down"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as your look down...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view down...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                            }
                        }
                        break;

                    case "west":
                    case "w":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("west"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look towards the west...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view west...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                            }
                        }
                        break;

                    case "east":
                    case "e":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("east"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look towards the east...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view east...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                            }
                        }
                        break;

                    case "north":
                    case "n":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("north"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look north...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view north...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                            }
                        }
                        break;

                    case "south":
                    case "s":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("south"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look south...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view south...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                            }
                        }
                        break;

                    case "northwest":
                    case "nw":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("northwest"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look northwest...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view northwest...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                            }
                        }
                        break;

                    case "northeast":
                    case "ne":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("northeast"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look northeast...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view northeast...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                            }
                        }
                        break;

                    case "southeast":
                    case "se":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("southeast"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look southeast...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view southeast...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                            }
                        }
                        break;

                    case "southwest":
                    case "sw":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDirection("southwest"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look southwest...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view southwest...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                            }
                        }
                        break;

                    case "node":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode != null)
                        {
                            string amount = string.Empty;
                            uint d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth;
                            switch (d)
                            {
                                case 1:
                                    amount = "almost no";
                                    break;

                                case 2:
                                    amount = "a little bit of";
                                    break;

                                case 3:
                                    amount = "quite a bit of";
                                    break;

                                default:
                                    amount = "a lot of";
                                    break;
                            }
                            if (desc.Player.Level >= Constants.ImmLevel)
                            {
                                desc.Send($"The {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode} node can be mined {d} more times.{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"The {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode} node has {amount} resources left{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"There is no resource node here!{Constants.NewLine}");
                        }
                        break;

                    default:
                        // we aren't looking in a direction so try looking at other things, first players, then NPCs then items
                        var p = desc.Player.Level >= Constants.ImmLevel ?
                            RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault() :
                            RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Visible && Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        if (p != null)
                        {
                            if (Regex.IsMatch(desc.Player.Name, target, RegexOptions.IgnoreCase))
                            {
                                msgToSendToPlayer = $"You look yourself up and down. Vain, much?{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} looks themselves up and down. How vain.{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something looks at itself... Very creepy...{Constants.NewLine}";
                            }
                            else
                            {
                                var targetHP = (double)p.Player.CurrentHP / p.Player.MaxHP * 100;
                                string stateMsg = Helpers.GetActorStateMessage(p.Player.Name, targetHP);
                                string pAlignString = p.Player.Alignment == Alignment.Evil ? $"{Constants.NewLine}{p.Player} gives off a dark aura.{Constants.NewLine}"
                                    : p.Player.Alignment == Alignment.Good ? $"{Constants.NewLine}{p.Player} radiates a holy glow.{Constants.NewLine}" : string.Empty;
                                msgToSendToPlayer = $"{p.Player.LongDescription}{pAlignString}{stateMsg}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}{p.Player.Name} is using:{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Head: {p.Player.EquipHead?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Neck: {p.Player.EquipNeck?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Armour: {p.Player.EquipArmour?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Left Finger: {p.Player.EquipLeftFinger?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Right Finger: {p.Player.EquipRightFinger?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Weapon: {p.Player.EquipWeapon?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Held: {p.Player.EquipHeld?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} looks {p.Player.Name} up and down...{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something looks {p.Player.Name} up and down...{Constants.NewLine}";
                                msgToSendToTarget = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} gives you a studious look. Strange person.{Constants.NewLine}" : $"You feel a chill as though something was looking at you...{Constants.NewLine}";
                                p.Send(msgToSendToTarget);
                            }
                        }
                        else
                        {
                            // p was null so the target string doesn't match a player in the room, try finding a matching NPC
                            var n = GetTargetNPC(ref desc, target);
                            if (n != null)
                            {
                                var targetHP = (double)n.CurrentHP / n.MaxHP * 100;
                                string stateMsg = Helpers.GetActorStateMessage(n.Name, targetHP);
                                string pAlignString = n.Alignment == Alignment.Evil ? $"{Constants.NewLine}{n.Name} gives off a dark aura.{Constants.NewLine}"
                                    : n.Alignment == Alignment.Good ? $"{Constants.NewLine}{n.Name} radiates a holy glow.{Constants.NewLine}" : string.Empty;
                                msgToSendToPlayer = $"{n.LongDescription}{pAlignString}{stateMsg}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gives {n.Name} a studious look{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something gives {n.Name} a studious look{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}{n.Name} is using:{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Head: {n.EquipHead?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Neck: {n.EquipHead?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Armour: {n.EquipArmour?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Left Finger: {n.EquipLeftFinger?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Right Finger: {n.EquipRightFinger?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Weapon: {n.EquipWeapon?.Name ?? "Nothing"}{Constants.NewLine}";
                                msgToSendToPlayer = $"{msgToSendToPlayer}Held: {n.EquipHeld?.Name ?? "Nothing"}{Constants.NewLine}";
                            }
                            else
                            {
                                // n was null, try looking for an item in the room
                                var i = GetTargetItem(ref desc, target, false);
                                if (i != null)
                                {
                                    string modMsg = i.LongDescription;
                                    if (i.IsMagical)
                                    {
                                        modMsg = $"{modMsg}{Constants.NewLine}This item appears magical...";
                                        if (i.IsCursed)
                                        {
                                            modMsg = $"{modMsg}And possibly cursed!";
                                        }
                                    }
                                    msgToSendToPlayer = $"{modMsg}{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} looks longingly at {i.Name}{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"Something looks longingly at {i.Name}";
                                }
                                else
                                {
                                    // no matching item in the room, so look in player inventory instead
                                    var ii = GetTargetItem(ref desc, target, true);
                                    if (ii != null)
                                    {
                                        string modMsg = ii.LongDescription;
                                        if (ii.IsMagical)
                                        {
                                            modMsg = $"{modMsg}{Constants.NewLine}This item appears magical...";
                                            if (ii.IsCursed)
                                            {
                                                modMsg = $"{modMsg}And possibly cursed!";
                                            }
                                        }
                                        msgToSendToPlayer = $"{modMsg}{Constants.NewLine}";
                                        msgToSendToOthers[0] = $"{desc.Player.Name} looks longling at {ii.Name} that they're holding{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"Something looks longingly at something else... Very strange{Constants.NewLine}";
                                    }
                                    else
                                    {
                                        // no matching player, npc, room or inventory item...
                                        msgToSendToPlayer = $"You look about, but you can't find anything like that!{Constants.NewLine}";
                                        msgToSendToOthers[0] = $"{desc.Player.Name} looks about, searching for something they can't seem to find...{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"The air shifts as something looks around, searching...{Constants.NewLine}";
                                    }
                                }
                            }
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(msgToSendToPlayer))
                {
                    desc.Send(msgToSendToPlayer);
                }
                var playersInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom;
                if (playersInRoom != null && playersInRoom.Count > 1)
                {
                    foreach (var p in playersInRoom)
                    {
                        if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success && !Regex.Match(p.Player.Name, target, RegexOptions.IgnoreCase).Success)
                        {
                            if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                            {
                                p.Send(msgToSendToOthers[0]);
                            }
                            else
                            {
                                p.Send(msgToSendToOthers[1]);
                            }
                        }
                    }
                }
            }
            else
            {
                RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).DescribeRoom(ref desc, true);
            }
        }

        private static void DoPlayerAlias(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input);
            var line = input.Remove(0, verb.Length).Trim();
            if (string.IsNullOrEmpty(line))
            {
                // show configured aliases
                StringBuilder sb = new StringBuilder();
                if (desc.Player.CommandAliases != null && desc.Player.CommandAliases.Count > 0)
                {
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Alias{Constants.TabStop}{Constants.TabStop}|| Command");
                    sb.AppendLine($"  {new string('=', 77)}");
                    foreach (var alias in desc.Player.CommandAliases)
                    {
                        if (alias.Key.Length < 7)
                        {
                            sb.AppendLine($"|| {alias.Key}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}|| {alias.Value}");
                        }
                        else
                        {
                            sb.AppendLine($"|| {alias.Key}{Constants.TabStop}{Constants.TabStop}|| {alias.Value}");
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    desc.Send($"You haven't configured any aliases yet...{Constants.NewLine}");
                }
            }
            else
            {
                var tokens = TokeniseInput(ref line);
                if (tokens.Length > 1)
                {
                    var operation = tokens[0].Trim().ToLower();
                    switch (operation)
                    {
                        case "add":
                        case "create":
                            var alias = tokens[1].Trim().ToLower();
                            var command = line.Remove(0, operation.Length).Trim().Remove(0, alias.Length).ToLower().Trim();
                            if (!desc.Player.CommandAliases.ContainsKey(alias))
                            {
                                desc.Player.CommandAliases.Add(alias, command);
                                desc.Send($"A new alias for '{alias}' has been created.{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You already have an alias configured for '{alias}', please remove this before adding a new one.{Constants.NewLine}");
                            }
                            break;

                        case "remove":
                        case "delete":
                            alias = tokens[1].Trim().ToLower();
                            if (desc.Player.CommandAliases.ContainsKey(alias))
                            {
                                desc.Player.CommandAliases.Remove(alias);
                                desc.Send($"The alias '{alias}' has been successfully removed.{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"No alias matching '{alias}' could be found to remove.{Constants.NewLine}");
                            }
                            break;

                        default:
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void ShowCharSheet(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            string dmg = desc.Player.EquipWeapon != null ? $"{desc.Player.EquipWeapon.NumberOfDamageDice}D{desc.Player.EquipWeapon.SizeOfDamageDice}" : "1D2";
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| Name: {desc.Player.Name}{Constants.TabStop}{Constants.TabStop}Gender: {desc.Player.Gender}{Constants.TabStop}Class: {desc.Player.Class}{Constants.TabStop}Race: {desc.Player.Race}");
            sb.AppendLine($"|| Level: {desc.Player.Level}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}Exp: {desc.Player.Exp:N0} / {LevelTable.GetExpForNextLevel(desc.Player.Level, desc.Player.Exp):N0}");
            sb.AppendLine($"|| Alignment: {desc.Player.Alignment}{Constants.TabStop}{Constants.TabStop}Gold: {desc.Player.Gold:N0}");
            sb.AppendLine($"|| Position: {desc.Player.Position}{Constants.TabStop}{Constants.TabStop}Base Armour Class: {desc.Player.BaseArmourClass}{Constants.TabStop}Armour Class: {desc.Player.ArmourClass}");
            sb.AppendLine($"||");
            sb.AppendLine($"|| Stats:");
            sb.AppendLine($"|| STR: {desc.Player.Strength} ({Helpers.CalculateAbilityModifier(desc.Player.Strength)}){Constants.TabStop}{Constants.TabStop}DEX: {desc.Player.Dexterity} ({Helpers.CalculateAbilityModifier(desc.Player.Dexterity)}){Constants.TabStop}{Constants.TabStop}CON: {desc.Player.Constitution} ({Helpers.CalculateAbilityModifier(desc.Player.Constitution)})");
            sb.AppendLine($"|| INT: {desc.Player.Intelligence} ({Helpers.CalculateAbilityModifier(desc.Player.Intelligence)}){Constants.TabStop}{Constants.TabStop}WIS: {desc.Player.Wisdom} ({Helpers.CalculateAbilityModifier(desc.Player.Wisdom)}){Constants.TabStop} {Constants.TabStop}CHA: {desc.Player.Charisma} ({Helpers.CalculateAbilityModifier(desc.Player.Charisma)})");
            sb.AppendLine($"|| ");
            sb.AppendLine($"|| Health  : {desc.Player.CurrentHP} / {desc.Player.MaxHP}{Constants.TabStop}Mana: {desc.Player.CurrentMP} / {desc.Player.MaxMP}");
            sb.AppendLine($"|| Stamina : {desc.Player.CurrentSP} / {desc.Player.MaxSP}{Constants.TabStop}Attacks: {desc.Player.NumberOfAttacks}{Constants.TabStop}Damage: {dmg}");
            sb.AppendLine($"|| Languages: {desc.Player.KnownLanguages}");
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void SayToRoom(ref Descriptor desc, ref string line)
        {
            var verb = GetVerb(ref line).Trim();
            var msg = line.Remove(0, verb.Length).Trim();
            foreach (var p in RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom))
            {
                if (Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                {
                    if (desc.Player.SpokenLanguage == Languages.Common)
                    {
                        p.Send($"You say \"{msg}\"{Constants.NewLine}");
                    }
                    else
                    {
                        p.Send($"In {desc.Player.SpokenLanguage}, you say \"{msg}\"{Constants.NewLine}");
                    }

                }
                else
                {
                    if (desc.Player.SpokenLanguage == Languages.Common)
                    {
                        string msgToSend = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} says, \"{msg}\"{Constants.NewLine}"
                            : $"Something says, \"{msg}\"{Constants.NewLine}";
                        p.Send(msgToSend);
                    }
                    else
                    {
                        if (p.Player.KnownLanguages.HasFlag(desc.Player.SpokenLanguage))
                        {
                            string msgToSend = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"In {desc.Player.SpokenLanguage}, {desc.Player.Name} says, \"{msg}\"{Constants.NewLine}"
                                : $"In {desc.Player.SpokenLanguage}, Something says, \"{msg}\"{Constants.NewLine}";
                            p.Send(msgToSend);
                        }
                        else
                        {
                            string msgToSend = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} says something in {desc.Player.SpokenLanguage} but you don't understand.{Constants.NewLine}"
                                : $"Something says something in {desc.Player.SpokenLanguage}, but you don't understand.{Constants.NewLine}";
                            p.Send(msgToSend);
                        }
                    }
                }
            }
        }

        private static void SayToCharacter(ref Descriptor desc, ref string line)
        {
            var inputElements = TokeniseInput(ref line);
            string target = inputElements.Length > 1 ? inputElements[1].Trim() : string.Empty;
            var playersInRoom = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
            if (Regex.Match(desc.Player.Name, target, RegexOptions.IgnoreCase).Success)
            {
                desc.Send($"You mumble incoherently to yourself. Strange person.{Constants.NewLine}");
                if (playersInRoom.Count - 1 > 1)
                {
                    foreach (var pd in playersInRoom)
                    {
                        if (!Regex.Match(pd.Player.Name, desc.Player.Name, RegexOptions.IgnorePatternWhitespace).Success)
                        {
                            var msgToSend = desc.Player.Visible || pd.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} mumbles to themselves like a weirdo.{Constants.NewLine}"
                                : $"Something mumbles to itself incoherently. Strange.{Constants.NewLine}";
                            pd.Send(msgToSend);
                        }
                    }
                }
                return;
            }
            var verb = GetVerb(ref line);
            var toSend = line.Remove(0, verb.Length).Trim().Remove(0, target.Length).Trim();
            var p = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
            if (p != null)
            {
                if (desc.Player.SpokenLanguage == Languages.Common)
                {
                    var msgToPlayer = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} whispers \"{toSend}\"{Constants.NewLine}"
                        : $"Something whispers \"{toSend}\"{Constants.NewLine}";
                    p.Send(msgToPlayer);
                }
                else
                {
                    if (p.Player.KnownLanguages.HasFlag(desc.Player.SpokenLanguage))
                    {
                        var msgToPlayer = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"In {desc.Player.SpokenLanguage}, {desc.Player.Name} whispers \"{toSend}\"{Constants.NewLine}"
                            : $"In {desc.Player.SpokenLanguage}, something whispers \"{toSend}\"{Constants.NewLine}";
                        p.Send(msgToPlayer);
                    }
                    else
                    {
                        var msgToPlayer = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} whispers something in {desc.Player.SpokenLanguage} but you can't understand.{Constants.NewLine}"
                            : $"Something whispers something in {desc.Player.SpokenLanguage}, but you can't understand.{Constants.NewLine}";
                        p.Send(msgToPlayer);
                    }
                }
                if (playersInRoom.Count > 2)
                {
                    foreach (var player in playersInRoom)
                    {
                        string msgToOthers = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} whispers something to "
                            : $"Something whispers something to ";
                        msgToOthers = p.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{msgToOthers}{p.Player.Name}.{Constants.NewLine}"
                            : $"{msgToOthers}something else.{Constants.NewLine}";
                        if (!Regex.Match(player.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success && !Regex.Match(desc.Player.Name, player.Player.Name, RegexOptions.IgnoreCase).Success)
                        {
                            player.Send(msgToOthers);
                        }
                    }
                }
            }
            else
            {
                desc.Send($"That person isn't here...{Constants.NewLine}");
                if (playersInRoom.Count - 1 > 1)
                {
                    foreach (var player in playersInRoom)
                    {
                        if (!Regex.Match(player.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                        {
                            var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} mumbles something to themselves... What a strange person.{Constants.NewLine}"
                                : $"Something mumbles to itself. How strange.{Constants.NewLine}";
                        }
                    }
                }
            }
        }
    }
}
