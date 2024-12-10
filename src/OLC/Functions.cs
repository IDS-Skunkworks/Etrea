using Etrea3.Core;
using System;
using Etrea3.Objects;


namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static T GetValue<T>(Session session, string msg)
        {
            session.Send(msg);
            var input = session.Read();
            if (string.IsNullOrEmpty(input))
            {
                return default(T);
            }
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(input.Trim(), out var val))
                {
                    return (T)(object)val;
                }
                Game.LogMessage($"ERROR: Error in OLC.GetValue(): {input.Trim()} could not be parsed to {typeof(T)}", LogLevel.Error, true);
                return default(T);
            }
            if (typeof(T) == typeof(string))
            {
                return (T)(object)input.Trim();
            }
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(input.Trim(), out var val))
                {
                    return (T)(object)val;
                }
                Game.LogMessage($"ERROR: Error in OLC.GetValue(): {input.Trim()} could not be parsed to {typeof(T)}", LogLevel.Error, true);
                return default(T);
            }
            if (typeof(T) == typeof(uint))
            {
                if (uint.TryParse(input.Trim(), out var val))
                {
                    return (T)(object)val;
                }
                Game.LogMessage($"ERROR: Error in OLC.GetValue(): {input.Trim()} could not be parsed to {typeof(T)}", LogLevel.Error, true);
                return default(T);
            }
            if (typeof(T) == typeof(ulong))
            {
                if (ulong.TryParse(input.Trim(), out var val))
                {
                    return (T)(object)val;
                }
                Game.LogMessage($"ERROR: Error in OLC.GetValue(): {input.Trim()} could not be parsed to {typeof(T)}", LogLevel.Error, true);
                return default(T);
            }
            if (typeof(T) == typeof(long))
            {
                if (long.TryParse(input.Trim(), out var val))
                {
                    return (T)(object)val;
                }
                Game.LogMessage($"ERROR: Error in OLC.GetValue(): {input.Trim()} could not be parsed to {typeof(T)}", LogLevel.Error, true);
                return default(T);
            }
            Game.LogMessage($"ERROR: Error in OLC.GetValue(): {typeof(T)} is not supported", LogLevel.Error, true);
            return default(T);
        }

        public static bool ValidateAsset<T>(Session session, T asset, bool isNew, out string reply)
        {
            reply = string.Empty;
            if (typeof(T) == typeof(ResourceNode))
            {
                var node = (ResourceNode)(object)asset;
                if (node.ID <= 0)
                {
                    reply = "Resource Node ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && NodeManager.Instance.NodeExists(node.ID))
                {
                    reply = "The ID of this Resource node is already in use.";
                    session?.Send($"%BRT%{reply}%PT%T%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(node.Name))
                {
                    reply = "The Resource Node must have a Name.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (node.ApperanceChance <= 0)
                {
                    reply = "The Appearance Chance for this Node must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (node.CanFind.Count == 0)
                {
                    reply = "The Resource Node must have items that can be obtained from mining.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Emote))
            {
                var emote = (Emote)(object)asset;
                if (emote.ID <= 0)
                {
                    reply = "Emote ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(emote.Name))
                {
                    reply = "Emote Name cannot be empty.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (emote.Name.IndexOf(' ') > -1)
                {
                    reply = "Emote Name cannot contain spaces.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                foreach (var m in emote.MessageToPerformer)
                {
                    if (string.IsNullOrEmpty(m))
                    {
                        reply = "Messages to Performer must be set.";
                        session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                        return false;
                    }
                }
                foreach (var m in emote.MessageToOthers)
                {
                    if (string.IsNullOrEmpty(m))
                    {
                        reply = "Messages to Others must be set.";
                        session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(emote.MessageToTarget))
                {
                    reply = "Message to Target must be set.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && EmoteManager.Instance.EmoteExists(emote.Name))
                {
                    reply = "An Emote with that name already exists.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && EmoteManager.Instance.EmoteExists(emote.ID))
                {
                    reply = "An Emote with the same ID already exists.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(NPC))
            {
                var npc = (NPC)(object)asset;
                if (npc.TemplateID <= 0)
                {
                    reply = "Template ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(npc.Name) || string.IsNullOrEmpty(npc.ShortDescription) || string.IsNullOrEmpty(npc.LongDescription))
                {
                    reply = "The NPC must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (npc.Strength <= 0 || npc.Dexterity <= 0 || npc.Constitution <= 0 || npc.Intelligence <= 0 || npc.Wisdom <= 0 || npc.Charisma <= 0)
                {
                    reply = "All Stats must be higher than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && NPCManager.Instance.NPCTemplateExists(npc.TemplateID))
                {
                    reply = "The Template ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (npc.TemplateID == 0)
                {
                    reply = "An ID for this Template must be provided.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (npc.Flags.HasFlag(NPCFlags.Hostile) && npc.Flags.HasFlag(NPCFlags.NoAttack))
                {
                    reply = "The NPC cannot have the Hostile and NoAttack Flags at the same time.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(npc.ArrivalMessage) || string.IsNullOrEmpty(npc.DepatureMessage))
                {
                    reply = "The NPC must have an Arrival and a Departure Message.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (npc.NumberOfAttacks < 1)
                {
                    reply = "The number of attacks for the NPC must be at least 1.";
                    session?.Send($"%BRT%{reply}%PT%T%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Quest))
            {
                Quest quest = (Quest)(object)asset;
                if (isNew && QuestManager.Instance.QuestExists(quest.ID))
                {
                    reply = "The ID of the Quest is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (quest.ID <= 0)
                {
                    reply = "The Quest ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(quest.Name) || string.IsNullOrEmpty(quest.FlavourText))
                {
                    reply = "The Quest must have a Name and some Flavour Text.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (quest.QuestType == QuestType.Undefined)
                {
                    reply = "The Quest Type cannot be Undefined.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (quest.RewardExp == 0)
                {
                    reply = "A Quest should always reward Exp.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (quest.RewardGold == 0 && quest.RequiredItems.Count == 0)
                {
                    reply = "A Quest must reward Gold and/or Items.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (quest.RequiredItems.Count == 0 && quest.RequiredMonsters.Count == 0)
                {
                    reply = "A Quest must require Monsters and/or Items to complete.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(CraftingRecipe))
            {
                CraftingRecipe recipe = (CraftingRecipe)(object)asset;
                if (recipe.ID <= 0)
                {
                    reply = "The ID of the Crafting Recipe must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && RecipeManager.Instance.RecipeExists(recipe.ID))
                {
                    reply = "The Recipe ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (recipe.RecipeType == RecipeType.Undefined)
                {
                    reply = "Recipe Type cannot be Undefined.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (recipe.LearnCost <= 0)
                {
                    reply = "The Learn Cost cannot be lower than 1 Gold.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (recipe.RecipeResult == 0)
                {
                    reply = "Recipe Result cannot be zero.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (recipe.RequiredItems.Count == 0)
                {
                    reply = "The Recipe must have at least one ingredient.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(recipe.Name) || string.IsNullOrEmpty(recipe.Description))
                {
                    reply = "The Recipe must have a Name and Description.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Spell))
            {
                var spell = (Spell)(object)asset;
                if (spell.ID <= 0)
                {
                    reply = "Spell ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && SpellManager.Instance.SpellExists(spell.ID))
                {
                    reply = "The specified Spell ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (spell.SpellType == SpellType.Undefined)
                {
                    reply = "Spell Type cannot be undefined.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(spell.DamageExpression) && (spell.SpellType == SpellType.Healing || spell.SpellType == SpellType.Damage))
                {
                    reply = "If the Spell type is Healing or Damage, a Damage Expression must be provided.";
                    session?.Send($"BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(spell.MPCostExpression))
                {
                    reply = "The Spell must have an MP Cost.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (spell.SpellType == SpellType.Buff || spell.SpellType == SpellType.Debuff && spell.AppliedBuffs.Count == 0)
                {
                    reply = "If the Spell Type is Buff or Debuff it must apply at least one Buff.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(spell.Name) || string.IsNullOrEmpty(spell.Description))
                {
                    reply = "The Spell must have a Name and Description.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (spell.AvailableToClass == ActorClass.Undefined)
                {
                    reply = "The Spell must be available to one or more Classes.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Room))
            {
                var room = (Room)(object)asset;
                if (isNew && RoomManager.Instance.RoomIDInUse(room.ID))
                {
                    reply = "The Room ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (room.ID < 0)
                {
                    reply = "The Room ID cannot be less than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                var z = ZoneManager.Instance.GetZoneForRID(room.ID);
                if (z == null)
                {
                    reply = $"No Zone with ID {room.ZoneID} was found in Zone Manager.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (z.ZoneID != room.ZoneID)
                {
                    reply = "The Room ID is not valid for the specified Zone ID.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(room.RoomName) || string.IsNullOrEmpty(room.ShortDescription)
                    || string.IsNullOrEmpty(room.LongDescription))
                {
                    reply = "The Room must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Zone))
            {
                var zone = (Zone)(object)asset;
                if (isNew && ZoneManager.Instance.ZoneExists(zone.ZoneID))
                {
                    reply = "The Zone ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (isNew && (zone.MinRoom <= ZoneManager.Instance.MaxAllocatedRoomID))
                {
                    reply = "Zone Start Room overlaps with an existing Zone.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (zone.MinRoom >= zone.MaxRoom)
                {
                    reply = "Zone Start Room ID must be less than Zone End Room ID.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(zone.ZoneName))
                {
                    reply = "Zone must have a Name.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Shop))
            {
                var shop = (Shop)(object)asset;
                if (isNew && ShopManager.Instance.ShopExists(shop.ID))
                {
                    reply = "The Shop ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (shop.ID <= 0)
                {
                    reply = "Shop ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(shop.ShopName))
                {
                    reply = "The Shop must have a Name.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (shop.BaseGold == 0)
                {
                    reply = "The Shop must have a positive, non-zero Gold balance.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Weapon))
            {
                var weapon = (Weapon)(object)asset;
                if (isNew && ItemManager.Instance.ItemExists(weapon.ID))
                {
                    reply = "The Item ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.ID <= 0)
                {
                    reply = "The Item ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.TabStop}");
                    return false;
                }
                if (weapon.BaseValue < 0)
                {
                    reply = "The Base Value of the Weapon must be 0 or higher.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.IsCursed && !weapon.IsMagical)
                {
                    reply = "If the Weapon has the Curse flag, it must also have the Magical flag.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.AppliedBuffs.Count > 0 && !weapon.IsMagical)
                {
                    reply = "If the Weapon applies Buffs, it should also have the Magical flag.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.NumberOfDamageDice == 0 || weapon.SizeOfDamageDice == 0)
                {
                    reply = "The number and size of damage dice must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(weapon.Name) || string.IsNullOrEmpty(weapon.ShortDescription) || string.IsNullOrEmpty(weapon.LongDescription))
                {
                    reply = "The Weapon must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.DamageModifier < 0 || weapon.HitModifier < 0)
                {
                    reply = "The Weapon's Damage and Hit modifiers must be 0 or greater.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (weapon.WeaponType == WeaponType.Undefined)
                {
                    reply = "The Weapon Type cannot be Undefined.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Ring))
            {
                var ring = (Ring)(object)asset;
                if (isNew && ItemManager.Instance.ItemExists(ring.ID))
                {
                    reply = "The Item ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (ring.ID <= 0)
                {
                    reply = "The Item ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (ring.BaseValue < 0)
                {
                    reply = "The Base Value of the Item must be 0 or higher.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(ring.Name) || string.IsNullOrEmpty(ring.ShortDescription) || string.IsNullOrEmpty(ring.LongDescription))
                {
                    reply = "The Item must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (ring.IsCursed && !ring.IsMagical)
                {
                    reply = "If the Ring has the Curse flag it must also have the Magical flag.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Scroll))
            {
                var scroll = (Scroll)(object)asset;
                if (isNew && ItemManager.Instance.ItemExists(scroll.ID))
                {
                    reply = "The Item ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(scroll.Name) || string.IsNullOrEmpty(scroll.ShortDescription) || string.IsNullOrEmpty(scroll.LongDescription))
                {
                    reply = "The Scroll must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(scroll.CastsSpell))
                {
                    reply = "The Scroll must have a Spell to cast.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (scroll.ID <= 0)
                {
                    reply = "The Item ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (scroll.BaseValue < 0)
                {
                    reply = "The Base Value must be 0 or higher.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(Consumable))
            {
                var item = (Consumable)(object)asset;
                if (isNew && ItemManager.Instance.ItemExists(item.ID))
                {
                    reply = "The Item ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (item.ID <= 0)
                {
                    reply = "The Item ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (item.BaseValue < 0)
                {
                    reply = "The Base Value of the Item must be 0 or higher.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(item.Name) || string.IsNullOrEmpty(item.ShortDescription) || string.IsNullOrEmpty(item.LongDescription))
                {
                    reply = "The Consumable must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (item.Effects == ConsumableEffect.Undefined)
                {
                    reply = "The Consumable Effect cannot be Undefined.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if ((item.Effects.HasFlag(ConsumableEffect.Death) || item.Effects.HasFlag(ConsumableEffect.Poison) && (item.Effects.HasFlag(ConsumableEffect.Healing) || item.Effects.HasFlag(ConsumableEffect.Restoration))))
                {
                    reply = "Consumable Effects cannot combine Poison/Death with Healing/Restoration.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (item.Effects.HasFlag(ConsumableEffect.MPRecovery) || item.Effects.HasFlag(ConsumableEffect.SPRecovery) && (item.Effects.HasFlag(ConsumableEffect.DrainMP) || item.Effects.HasFlag(ConsumableEffect.DrainSP)))
                {
                    reply = "Consumable Effects cannot combine MP/SP Drain with MP/SP Recovery.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            if (typeof(T) == typeof(InventoryItem))
            {
                var item = (InventoryItem)(object)asset;
                if (isNew && ItemManager.Instance.ItemExists(item.ID))
                {
                    reply = "The Item ID is already in use.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (item.ID <= 0)
                {
                    reply = "The Item ID must be greater than 0.";
                    session?.Send($"%BRT%{reply}%PT%T%{Constants.TabStop}");
                    return false;
                }
                if (item.BaseValue < 0)
                {
                    reply = "The Base Value of the Item must be 0 or higher.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(item.Name) || string.IsNullOrEmpty(item.ShortDescription) || string.IsNullOrEmpty(item.LongDescription))
                {
                    reply = "The Item must have a Name, Short and Long Descriptions.";
                    session?.Send($"%BRT%{reply}%PT%{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            Game.LogMessage($"ERROR: Error in OLC.ValidateAsset(): {typeof(T)} is not supported", LogLevel.Error, true);
            return false;
        }

        private static T GetEnumValue<T>(Session session, string prompt) where T : struct, Enum
        {
            session.Send(prompt);
            var input = session.Read();
            if (!string.IsNullOrEmpty(input) && Enum.TryParse(input.Trim(), true, out T val))
            {
                return val;
            }
            Game.LogMessage($"ERROR: Error in OLC.GetEnumValue(): '{input.Trim()}' cannot be parsed into {typeof(T)}", LogLevel.Error, true);
            return default(T);
        }
    }
}