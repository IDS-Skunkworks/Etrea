using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace Etrea3.Objects
{
    [Serializable]
    public class CraftingRecipe
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public RecipeType RecipeType { get; set; } = RecipeType.Undefined;
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public int RecipeResult { get; set; } = 0;
        [JsonProperty]
        public int LearnCost { get; set; } = 0;
        [JsonProperty]
        public ConcurrentDictionary<int, int> RequiredItems { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        private bool CanCraft(Session session)
        {
            if (!session.Player.KnowsRecipe(ID))
            {
                session.Send($"%BRT%You don't know how to make that!%PT%{Constants.NewLine}");
                return false;
            }
            switch(RecipeType)
            {
                case RecipeType.Alchemy:
                    if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist))
                    {
                        session.Send($"There is no alchemical equipment here!{Constants.NewLine}");
                        return false;
                    }
                    if (!session.Player.HasSkill("Alchemist"))
                    {
                        session.Send($"You lack the required skill to craft that item!{Constants.NewLine}");
                        return false;
                    }
                    break;

                case RecipeType.Blacksmith:
                    if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith))
                    {
                        session.Send($"There are no blacksmithing tools here!{Constants.NewLine}");
                        return false;
                    }
                    if (!session.Player.HasSkill("Blacksmith"))
                    {
                        session.Send($"You lack the required skill to craft that item!{Constants.NewLine}");
                        return false;
                    }
                    break;

                case RecipeType.Cooking:
                    if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Chef))
                    {
                        session.Send($"There is no kitchen here!{Constants.NewLine}");
                        return false;
                    }
                    if (!session.Player.HasSkill("Cooking"))
                    {
                        session.Send($"You lack the required skill to craft that item!{Constants.NewLine}");
                        return false;
                    }
                    break;

                case RecipeType.Jewelcraft:
                    if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler))
                    {
                        session.Send($"There are no jeweler's tools here!{Constants.NewLine}");
                        return false;
                    }
                    if (!session.Player.HasSkill("Jeweler"))
                    {
                        session.Send($"You lack the required skill to craft that item!{Constants.NewLine}");
                        return false;
                    }
                    break;

                case RecipeType.Scribe:
                    if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RecipeType.Scribe))
                    {
                        session.Send($"There are no Scribe's tools here!{Constants.NewLine}");
                        return false;
                    }
                    if (!session.Player.HasSkill("Scribe"))
                    {
                        session.Send($"You lack the required skill to craft that item!{Constants.NewLine}");
                        return false;
                    }
                    break;
            }
            foreach(var i in RequiredItems)
            {
                var item = ItemManager.Instance.GetItem(i.Key);
                if (item == null)
                {
                    Game.LogMessage($"ERROR: Player {session.Player.Name} encountered an error crafting Recipe {ID}: Item {i.Key} returned null from ItemManager", LogLevel.Error, true);
                    session.Send($"This Recipe appears to be broken, please let an Imm know!{Constants.NewLine}");
                    return false;
                }
                if (!session.Player.HasItemInInventory(i.Key))
                {
                    session.Send($"%BRT%You are missing some ingredients!%PT%{Constants.NewLine}");
                    return false;
                }
                if (session.Player.GetInventoryItemCount(i.Key) < i.Value)
                {
                    session.Send($"%BRT%You are missing some ingredients!%PT%{Constants.NewLine}");
                    return false;
                }
            }
            return true;
        }

        public bool Craft(Session session)
        {
            if (!CanCraft(session))
            {
                return false;
            }
            var i = ItemManager.Instance.GetItem(RecipeResult);
            if (i == null)
            {
                Game.LogMessage($"ERROR: Player {session.Player.Name} encountered an error crafting Recipe {ID}: Result Item {RecipeResult} returned null from ItemManager", LogLevel.Error, true);
                return false;
            }
            foreach (var requiredItem in RequiredItems)
            {
                for (int cnt = 0; cnt < requiredItem.Value; cnt++)
                {
                    session.Player.RemoveItemFromInventory(requiredItem.Key);
                }
            }
            session.Player.AddItemToInventory(i.ID);
            session.Send($"You have successfully crafted a new {i.Name}!{Constants.NewLine}");
            return true;
        }
    }
}
