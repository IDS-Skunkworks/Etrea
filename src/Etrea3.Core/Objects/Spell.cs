using Etrea3.Core;
using NCalc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea3.Objects
{
    [Serializable]
    public class Spell
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public SpellType SpellType { get; set; } = SpellType.Undefined;
        [JsonProperty]
        public ActorClass AvailableToClass { get; set; } = ActorClass.Undefined;
        [JsonProperty]
        public string MPCostExpression { get; set; }
        [JsonProperty]
        public string DamageExpression { get; set; }
        [JsonProperty]
        public bool AutoHitTarget { get; set; }
        [JsonProperty]
        public int LearnCost { get; set; }
        [JsonProperty]
        public bool IsAOE { get; set; }
        [JsonProperty]
        public bool ApplyAbilityModifier { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<string, bool> AppliedBuffs { get; set; } = new ConcurrentDictionary<string, bool>();
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        public int MPCost(Actor caster)
        {
            if (int.TryParse(MPCostExpression, out int cost))
            {
                return cost;
            }
            return EvaluateCostExpression(caster);
        }

        public void Cast(Actor caster, Actor target)
        {
            if (RoomManager.Instance.GetRoom(caster.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
            {
                // caster is in a no magic room, no spells here
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"Some mystical force prevents the use of your magic here...{Constants.NewLine}");
                }
                else
                {
                    Game.LogMessage($"DEBUG: NPC {caster.Name} tried to cast {Name} in Room {caster.CurrentRoom} which is flagged NoMagic", LogLevel.Debug);
                }
                return;
            }
            if (RoomManager.Instance.GetRoom(caster.CurrentRoom).Flags.HasFlag(RoomFlags.Safe) && (SpellType == SpellType.Damage || SpellType == SpellType.Debuff))
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"Some mystical force prevents you casting that type of magic here!{Constants.NewLine}");
                }
                else
                {
                    Game.LogMessage($"DEBUG: NPC {caster.Name} tried to cast {Name} in Room {caster.CurrentRoom} which is flagged Safe and the Spell is {SpellType}", LogLevel.Debug);
                }
                return;
            }
            if (!IsAOE && target == null)
            {
                // spell needs a target but none was given
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"Cast that on what? You need to target your spells!{Constants.NewLine}");
                }
                else
                {
                    Game.LogMessage($"DEBUG: NPC {caster.Name} tried to cast {Name} in Room {caster.CurrentRoom} but no target was specified", LogLevel.Debug);
                }
                return;
            }
            if (caster.CurrentMP < MPCost(caster))
            {
                // caster doesn't have enough MP to cast this spell
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"You don't have enough MP to use that spell right now!{Constants.NewLine}");
                }
                return;
            }
            if (target != null && target.ActorType == ActorType.NonPlayer && ((NPC)target).Flags.HasFlag(NPCFlags.NoAttack))
            {
                if (SpellType == SpellType.Damage || SpellType == SpellType.Debuff)
                {
                    // target is an NPC with the NoAttack flag, can't cast against this target if the type is damage or debuff
                    if (caster.ActorType == ActorType.Player)
                    {
                        ((Player)caster).Send($"Some mystical force prevents you from harming {target.Name}...{Constants.NewLine}");
                    }
                    else
                    {
                        Game.LogMessage($"DEBUG: NPC {caster.Name} tried to cast {Name} against {target.Name} but the target has the NoAttack flag", LogLevel.Debug);
                    }
                    return;
                }
            }
            if ((SpellType == SpellType.Damage || SpellType == SpellType.Debuff) && caster.ActorType == ActorType.Player && target != null && target.ActorType == ActorType.Player)
            {
                // trying to cast harmful spells on ourself or another player is a no-go
                ((Player)caster).Send($"The Gods forbid such actions in the Realms!{Constants.NewLine}");
                return;
            }
            switch(SpellType)
            {
                case SpellType.Damage:
                    if (IsAOE)
                    {
                        CastDamageSpell(caster);
                    }
                    else
                    {
                        CastDamageSpell(caster, target);
                    }
                    break;

                case SpellType.Debuff:
                    if (IsAOE)
                    {
                        CastDebuffSpell(caster);
                    }
                    else
                    {
                        CastDebuffSpell(caster, target);
                    }
                    break;

                case SpellType.Buff:
                    if (IsAOE)
                    {
                        CastBuffSpell(caster);
                    }
                    else
                    {
                        CastBuffSpell(caster, target);
                    }
                    break;

                case SpellType.Healing:
                    if (IsAOE)
                    {
                        CastHealingSpell(caster);
                    }
                    else
                    {
                        CastHealingSpell(caster, target);
                    }
                    break;
            }
        }

        private int EvaluateCostExpression(Actor caster)
        {
            Expression e = new Expression(MPCostExpression);
            e.Parameters["level"] = caster != null ? caster.Level : 1;
            e.Parameters["INT"] = caster != null ? Helpers.CalculateAbilityModifier(caster.Intelligence) : 0;
            e.Parameters["WIS"] = caster != null ? Helpers.CalculateAbilityModifier(caster.Wisdom) : 0;
            return Convert.ToInt32(e.Evaluate());
        }

        private int CalculateDamage(Actor caster)
        {
            var dicePattern = new Regex(@"(.*)[dD](\d+)");
            var match = dicePattern.Match(DamageExpression);
            int ttlDamage = 0;
            string damExpr = string.Empty;
            if (match.Success)
            {
                string baseExpression = match.Groups[1].Value.Trim();
                int dSize = int.Parse(match.Groups[2].Value);
                int nDice = EvaluateExpression(baseExpression, caster);
                ttlDamage = Helpers.RollDice<int>(nDice, dSize);
                damExpr = DamageExpression.Replace(match.Value, ttlDamage.ToString());
            }
            if (!string.IsNullOrEmpty(damExpr.Trim()))
            {
                ttlDamage = EvaluateExpression(damExpr, caster);
            }
            if (ApplyAbilityModifier)
            {
                int abilityMod = 0;
                if (caster.ActorType == ActorType.NonPlayer)
                {
                    abilityMod = Helpers.CalculateAbilityModifier(Math.Max(caster.Intelligence, caster.Wisdom));
                }
                if (caster.ActorType == ActorType.Player)
                {
                    if (((Player)caster).Class == ActorClass.Wizard)
                    {
                        abilityMod = Helpers.CalculateAbilityModifier(caster.Intelligence);
                    }
                    else
                    {
                        abilityMod = Helpers.CalculateAbilityModifier(caster.Wisdom);
                    }
                }
                abilityMod = Math.Max(0, abilityMod);
                ttlDamage += abilityMod;
            }
            return ttlDamage;
        }

        private int EvaluateExpression(string expression, Actor caster)
        {
            int damage = 0;
            var expr = new Expression(expression);
            expr.Parameters["INT"] = Helpers.CalculateAbilityModifier(caster.Intelligence);
            expr.Parameters["WIS"] = Helpers.CalculateAbilityModifier(caster.Wisdom);
            expr.Parameters["level"] = caster.Level;
            expr.EvaluateFunction += (name, args) =>
            {
                if (name == "min" && args.Parameters.Length == 2)
                {
                    var arg1 = Convert.ToDouble(args.Parameters[0].Evaluate());
                    var arg2 = Convert.ToDouble(args.Parameters[1].Evaluate());
                    args.Result = Math.Min(arg1, arg2);
                }
                if (name == "max" && args.Parameters.Length == 2)
                {
                    var arg1 = Convert.ToDouble(args.Parameters[0].Evaluate());
                    var arg2 = Convert.ToDouble(args.Parameters[1].Evaluate());
                    args.Result = Math.Max(arg1, arg2);
                }
                if (name == "rand" && args.Parameters.Length > 0)
                {
                    List<double> values = new List<double>();
                    Random rnd = new Random(DateTime.Now.GetHashCode());
                    foreach(var arg in args.Parameters)
                    {
                        values.Add(Convert.ToDouble(arg.Evaluate()));
                    }
                    args.Result = values[rnd.Next(values.Count)];
                }
                if (name == "randbetween" && args.Parameters.Length == 2)
                {
                    var arg1 = Convert.ToInt32(args.Parameters[0].Evaluate());
                    var arg2 = Convert.ToInt32(args.Parameters[1].Evaluate());
                    Random rnd = new Random(DateTime.Now.GetHashCode());
                    args.Result = arg1 < arg2 ? rnd.Next(arg1, arg2) : rnd.Next(arg2, arg1);
                }
            };
            damage = Convert.ToInt32(expr.Evaluate());
            return damage;
        }

        private void CastDamageSpell(Actor caster)
        {
            if (caster.ActorType == ActorType.NonPlayer)
            {
                // NPCs should not be using AOE damage spells, log and return
                Game.LogMessage($"DEBUG: NPC {caster.Name} in Room {caster.CurrentRoom} attempted to cast '{Name}'", LogLevel.Debug);
                return;
            }
            var targets = RoomManager.Instance.GetRoom(caster.CurrentRoom).NPCsInRoom.Where(x => !x.Flags.HasFlag(NPCFlags.NoAttack)).ToList();
            if (targets == null || targets.Count == 0)
            {
                ((Player)caster).Send($"There are no viable targets for that spell here...{Constants.NewLine}");
                return;
            }
            caster.AdjustMP(MPCost(caster) * -1);
            foreach (var t in targets)
            {
                bool startCombat = true;
                bool hitsTarget = AutoHitTarget || caster.HitsTarget(t, out _, out _, out _);
                if (!hitsTarget)
                {
                    switch(t.CanBeSeenBy(caster))
                    {
                        case true:
                            ((Player)caster).Send($"Mighty though your magic is, your spell misses {t.Name}...{Constants.NewLine}");
                            break;

                        case false:
                            ((Player)caster).Send($"Mighty though your magic is, your spell misses something...{Constants.NewLine}");
                            break;
                    }
                    continue;
                }
                var dmg = Math.Max(0, CalculateDamage(caster));
                t.AdjustHP(dmg * -1, out bool isKilled);
                if (isKilled)
                {
                    startCombat = false;
                    switch(t.CanBeSeenBy(caster))
                    {
                        case true:
                            ((Player)caster).Send($"The magic of your {Name} spell strikes {t.Name} for lethal damage!{Constants.NewLine}");
                            break;

                        case false:
                            ((Player)caster).Send($"The magic of your {Name} spell strikes something for lethal damage!{Constants.NewLine}");
                            break;
                    }
                    t.Kill(caster, true);
                    continue;
                }
                else
                {
                    switch (t.CanBeSeenBy(caster))
                    {
                        case true:
                            ((Player)caster).Send($"The magic of your {Name} spell strikes {t.Name} for {dmg:N0} damage!{Constants.NewLine}");
                            break;

                        case false:
                            ((Player)caster).Send($"The magic of your {Name} spell strikes something for {dmg:N0} damage!{Constants.NewLine}");
                            break;
                    }
                }
                if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                {
                    foreach(var b in AppliedBuffs.Keys)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            var duration = buff.CalculateDuration(caster, this);
                            t.ApplyBuff(buff.Name, duration);
                            switch(t.CanBeSeenBy(caster))
                            {
                                case true:
                                    ((Player)caster).Send($"{t.Name} has been affected by {buff.Name}!{Constants.NewLine}");
                                    break;

                                case false:
                                    ((Player)caster).Send($"Something has been affected by {buff.Name}!{Constants.NewLine}");
                                    break;
                            }
                        }
                        else
                        {
                            Game.LogMessage($"DEBUG: Spell {Name} cast by {caster.Name} in Room {caster.CurrentRoom} tried to apply Buff {b} but no such Buff was found in Buff Manager", LogLevel.Debug);
                        }
                    }
                }
                if (startCombat)
                {
                    caster.AddToTargetQueue(t);
                    t.AddToTargetQueue(caster);
                }
            }
        }

        private void CastDamageSpell(Actor caster, Actor target)
        {
            // remember CalculateDamage returns a positive number so convert to negative before calling ModifyHP on the target
            bool startCombat = true;
            caster.AdjustMP(MPCost(caster) * -1);
            bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
            if (hitsTarget)
            {
                var dmg = Math.Max(0, CalculateDamage(caster));
                target.AdjustHP(dmg * -1, out bool isKilled);
                if (caster.ActorType == ActorType.NonPlayer && target.ActorType == ActorType.Player)
                {
                    if (isKilled)
                    {
                        ((Player)target).Send($"The magic of {caster.Name}'s {Name} spell strikes you for lethal damage, killing you instantly!{Constants.NewLine}");
                        startCombat = false;
                        target.Kill(caster, true);
                    }
                    else
                    {
                        ((Player)target).Send($"The magic of {caster.Name}'s {Name} spell strikes you for {Math.Abs(dmg):N0} damage!{Constants.NewLine}");
                        if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                        {
                            foreach(var b in AppliedBuffs.Keys)
                            {
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    var duration = buff.CalculateDuration(caster, this);
                                    target.ApplyBuff(buff.Name, duration);
                                    ((Player)target).Send($"{caster.Name}'s magic has affected you with {buff.Name}!{Constants.NewLine}");
                                }
                                else
                                {
                                    Game.LogMessage($"DEBUG: Spell '{Name}' cast by {caster.Name} in Room {caster.CurrentRoom} attempted to apply Buff {b} but no such Buff was found in Buff Manager", LogLevel.Debug);
                                }
                            }
                        }
                    }
                }
                if (caster.ActorType == ActorType.NonPlayer && target.ActorType == ActorType.NonPlayer && isKilled)
                {
                    startCombat = false;
                    target.Kill(caster, true);
                }
                if (caster.ActorType == ActorType.Player)
                {
                    if (isKilled)
                    {
                        startCombat = false;
                        ((Player)caster).Send($"The magic of your {Name} spell strikes {target.Name} for lethal damage!{Constants.NewLine}");
                        target.Kill(caster, true);
                    }
                    else
                    {
                        ((Player)caster).Send($"The magic of your {Name} spell strikes {target.Name} for {Math.Abs(dmg):N0} damage!{Constants.NewLine}");
                        if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                        {
                            foreach(var b in AppliedBuffs.Keys)
                            {
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    var duration = buff.CalculateDuration(caster, this);
                                    target.ApplyBuff(buff.Name, duration);
                                    ((Player)caster).Send($"Your magic has affected {target.Name} with {buff.Name}!{Constants.NewLine}");
                                }
                                else
                                {
                                    Game.LogMessage($"DEBUG: Spell '{Name}' cast by {caster.Name} in Room {caster.CurrentRoom} attempted to apply Buff {b} but no such Buff was found in Buff Manager", LogLevel.Debug);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"The Winds of Magic turn against you and your spell fizzles and misses!{Constants.NewLine}");
                }
            }
            if (startCombat)
            {
                caster.AddToTargetQueue(target);
                target.AddToTargetQueue(caster);
            }
        }

        private void CastHealingSpell(Actor caster)
        {
            var targets = RoomManager.Instance.GetRoom(caster.CurrentRoom).AllActorsInRoom;
            caster.AdjustMP(MPCost(caster) * -1);
            foreach(var target in targets)
            {
                bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
                if (hitsTarget)
                {
                    var healing = CalculateDamage(caster);
                    target.AdjustHP(healing, out _);
                    if (caster.ActorType == ActorType.Player)
                    {
                        // tell the caster they healed the target
                        switch(target.CanBeSeenBy(caster))
                        {
                            case true:
                                ((Player)caster).Send($"The magic of your {Name} spell heals {target.Name} for {healing:N0} damage!{Constants.NewLine}");
                                break;

                            case false:
                                ((Player)caster).Send($"The magic of your {Name} spell heals something for {healing:N0} damage!{Constants.NewLine}");
                                break;
                        }
                    }
                    if (target.ActorType == ActorType.Player)
                    {
                        // tell the target they've been healed
                        switch(caster.CanBeSeenBy(target))
                        {
                            case true:
                                ((Player)target).Send($"{caster.Name}'s {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                break;

                            case false:
                                ((Player)target).Send($"Something's {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                break;
                        }
                    }
                    if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                    {
                        foreach(var b in AppliedBuffs.Keys)
                        {
                            var buff = BuffManager.Instance.GetBuff(b);
                            if (buff != null)
                            {
                                int duration = buff.CalculateDuration(caster, this);
                                target.ApplyBuff(buff.Name, duration);
                                if (caster.ActorType == ActorType.Player)
                                {
                                    switch(target.CanBeSeenBy(caster))
                                    {
                                        case true:
                                            ((Player)caster).Send($"Your {Name} spell has granted {target.Name} the boon of {buff.Name}!{Constants.NewLine}");
                                            break;

                                        case false:
                                            ((Player)caster).Send($"Your {Name} spell has granted the boon of {buff.Name} to something!{Constants.NewLine}");
                                            break;
                                    }
                                }
                                if (target.ActorType == ActorType.Player)
                                {
                                    switch(caster.CanBeSeenBy(target))
                                    {
                                        case true:
                                            ((Player)target).Send($"{caster.Name}'s {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                            break;

                                        case false:
                                            ((Player)target).Send($"Something's {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                            }
                        }
                    }
                }
                else
                {
                    if (caster.ActorType == ActorType.Player)
                    {
                        ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} spell misses!{Constants.NewLine}");
                    }
                }
            }
        }

        private void CastHealingSpell(Actor caster, Actor target)
        {
            caster.AdjustMP(MPCost(caster) * -1);
            bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
            if (hitsTarget)
            {
                var healing = CalculateDamage(caster);
                target.AdjustHP(healing, out _);
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"The magic of your {Name} spell heals {target.Name} for {healing:N0} damage!{Constants.NewLine}");
                }
                if (target.ActorType == ActorType.Player)
                {
                    // tell the target they've been healed
                    switch (caster.CanBeSeenBy(target))
                    {
                        case true:
                            ((Player)target).Send($"{caster.Name}'s {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                            break;

                        case false:
                            ((Player)target).Send($"Something's {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                            break;
                    }
                }
                if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                {
                    foreach(var b in AppliedBuffs.Keys)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            int duration = buff.CalculateDuration(caster, this);
                            target.ApplyBuff(buff.Name, duration);
                            if (caster.ActorType == ActorType.Player)
                            {
                                ((Player)caster).Send($"The magic of your {Name} spell has granted the boon of {buff.Name} to {target.Name}!{Constants.NewLine}");
                            }
                            if (target.ActorType == ActorType.Player)
                            {
                                switch (caster.CanBeSeenBy(target))
                                {
                                    case true:
                                        ((Player)target).Send($"The magic of {caster.Name}'s {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                        break;

                                    case false:
                                        ((Player)target).Send($"The magic of {Name} cast by something has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                        }
                    }
                }
            }
            else
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} spell misses!{Constants.NewLine}");
                }
            }
        }

        private void CastBuffSpell(Actor caster)
        {
            var targets = RoomManager.Instance.GetRoom(caster.CurrentRoom).AllActorsInRoom;
            caster.AdjustMP(MPCost(caster) * -1);
            if (targets != null && targets.Count > 0)
            {
                foreach(var target in targets)
                {
                    bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
                    if (hitsTarget)
                    {
                        if (!string.IsNullOrEmpty(DamageExpression))
                        {
                            var healing = CalculateDamage(caster);
                            target.AdjustHP(healing, out _);
                            if (caster.ActorType == ActorType.Player)
                            {
                                // tell the player they healed the target
                                switch (target.CanBeSeenBy(caster))
                                {
                                    case true:
                                        ((Player)caster).Send($"The magic of your {Name} spell heals {target.Name} for {healing:N0} damage!{Constants.NewLine}");
                                        break;

                                    case false:
                                        ((Player)caster).Send($"The magic of your {Name} spell heals something for {healing:N0} damage!{Constants.NewLine}");
                                        break;
                                }
                            }
                            if (target.ActorType == ActorType.Player)
                            {
                                // tell the target they've been healed
                                switch (caster.CanBeSeenBy(target))
                                {
                                    case true:
                                        ((Player)target).Send($"{caster.Name}'s {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                        break;

                                    case false:
                                        ((Player)target).Send($"Something's {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                        break;
                                }
                            }
                        }
                        if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                        {
                            foreach(var b in AppliedBuffs.Keys)
                            {
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    int duration = buff.CalculateDuration(caster, this);
                                    target.ApplyBuff(buff.Name, Math.Max(1, duration));
                                    if (caster.ActorType == ActorType.Player)
                                    {
                                        switch(target.CanBeSeenBy(caster))
                                        {
                                            case true:
                                                ((Player)caster).Send($"The magic of your {Name} spell has granted the boon of {buff.Name} to {target.Name}!{Constants.NewLine}");
                                                break;

                                            case false:
                                                ((Player)caster).Send($"Something grows stronger as the magic of your {Name} spell grants the boon of {buff.Name}!{Constants.NewLine}");
                                                break;
                                        }
                                    }
                                    if (target.ActorType == ActorType.Player)
                                    {
                                        // tell the player they have been buffed
                                        switch (caster.CanBeSeenBy(target))
                                        {
                                            case true:
                                                ((Player)target).Send($"The magic of {caster.Name}'s {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                                break;

                                            case false:
                                                ((Player)target).Send($"The magic of something's {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (caster.ActorType == ActorType.Player)
                        {
                            ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} spell misses!{Constants.NewLine}");
                        }
                    }
                }
            }
            else
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"There are no targets for that spell here!{Constants.NewLine}");
                }
            }
        }

        private void CastBuffSpell(Actor caster, Actor target)
        {
            bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
            caster.AdjustMP(MPCost(caster) * -1);
            if (hitsTarget)
            {
                if (!string.IsNullOrEmpty(DamageExpression))
                {
                    var healing = CalculateDamage(caster);
                    target.AdjustHP(healing, out _);
                    if (caster.ActorType == ActorType.Player)
                    {
                        // tell the player they healed the target
                        switch (target.CanBeSeenBy(caster))
                        {
                            case true:
                                ((Player)caster).Send($"The magic of your {Name} spell heals {target.Name} for {healing:N0} damage!{Constants.NewLine}");
                                break;

                            case false:
                                ((Player)caster).Send($"The magic of your {Name} spell heals something for {healing:N0} damage!{Constants.NewLine}");
                                break;
                        }
                    }
                    if (target.ActorType == ActorType.Player)
                    {
                        // tell the target they've been healed
                        switch (caster.CanBeSeenBy(target))
                        {
                            case true:
                                ((Player)target).Send($"{caster.Name}'s {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                break;

                            case false:
                                ((Player)target).Send($"Something's {Name} has healed you for {healing:N0} damage!{Constants.NewLine}");
                                break;
                        }
                    }
                }
                if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                {
                    foreach (var b in AppliedBuffs.Keys)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            int duration = buff.CalculateDuration(caster, this);
                            target.ApplyBuff(buff.Name, duration);
                            if (caster.ActorType == ActorType.Player)
                            {
                                switch (target.CanBeSeenBy(caster))
                                {
                                    case true:
                                        ((Player)caster).Send($"The magic of your {Name} spell has granted the boon of {buff.Name} to {target.Name}!{Constants.NewLine}");
                                        break;

                                    case false:
                                        ((Player)caster).Send($"Something grows stronger as it gains the boon of {buff.Name} from your {Name} spell!{Constants.NewLine}");
                                        break;
                                }
                            }
                            if (target.ActorType == ActorType.Player)
                            {
                                // tell the player they have been buffed
                                switch (caster.CanBeSeenBy(target))
                                {
                                    case true:
                                        ((Player)target).Send($"The magic of {caster.Name}'s {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                        break;

                                    case false:
                                        ((Player)target).Send($"The magic of something's {Name} spell has granted you the boon of {buff.Name}!{Constants.NewLine}");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                        }
                    }
                }
            }
            else
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} spell misses!{Constants.NewLine}");
                }
            }
        }

        private void CastDebuffSpell(Actor caster)
        {
            if (caster.ActorType == ActorType.NonPlayer)
            {
                // NPCs should not be able to cast AOE debuff spells, log and return
                Game.LogMessage($"DEBUG: NPC {caster.Name} in Room {caster.CurrentRoom} attempted to case {Name}", LogLevel.Debug);
                return;
            }
            var targets = RoomManager.Instance.GetRoom(caster.CurrentRoom).NPCsInRoom.Where(x => !x.Flags.HasFlag(NPCFlags.NoAttack)).ToList();
            if (targets == null || targets.Count == 0)
            {
                ((Player)caster).Send($"There are no viable targets for that spell here...{Constants.NewLine}");
                return;
            }
            caster.AdjustMP(MPCost(caster) * -1);
            foreach(var target in targets)
            {
                bool startCombat = true;
                bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
                if (!hitsTarget)
                {
                    switch(target.CanBeSeenBy(caster))
                    {
                        case true:
                            ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} misses {target.Name}!{Constants.NewLine}");
                            break;

                        case false:
                            ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} misses something...{Constants.NewLine}");
                            break;
                    }
                    continue;
                }
                if (!string.IsNullOrEmpty(DamageExpression))
                {
                    var dmg = CalculateDamage(caster) * -1;
                    target.AdjustHP(dmg, out bool isKilled);
                    if (isKilled)
                    {
                        startCombat = false;
                        if (target.CanBeSeenBy(caster))
                        {
                            ((Player)caster).Send($"The might of your {Name} spell is too much for {target.Name} and they succumb to their wounds!{Constants.NewLine}");
                        }
                        else
                        {
                            ((Player)caster).Send($"The might of your {Name} spell is too much and something succumbs to its wounds...{Constants.NewLine}");
                        }
                        target.Kill(caster, true);
                        continue;
                    }
                    else
                    {
                        if (target.CanBeSeenBy(caster))
                        {
                            ((Player)caster).Send($"Your {Name} spell strikes {target.Name} for {dmg:N0} damage!{Constants.NewLine}");
                        }
                        else
                        {
                            ((Player)caster).Send($"Your {Name} spell strikes something for {dmg:N0} damage!{Constants.NewLine}");
                        }
                    }
                }
                if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                {
                    foreach(var b in AppliedBuffs.Keys)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            var duration = buff.CalculateDuration(caster, this);
                            target.ApplyBuff(buff.Name, duration);
                            if (target.CanBeSeenBy(caster))
                            {
                                ((Player)caster).Send($"The magic of your {Name} spell has afflicted {target.Name} with {buff.Name}!{Constants.NewLine}");
                            }
                            else
                            {
                                ((Player)caster).Send($"The magic of your {Name} spell has afflicted something with {buff.Name}!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                        }
                    }
                }
                if (startCombat)
                {
                    caster.AddToTargetQueue(target);
                    target.AddToTargetQueue(caster);
                }
            }
        }

        private void CastDebuffSpell(Actor caster, Actor target)
        {
            bool hitsTarget = AutoHitTarget || caster.HitsTarget(target, out _, out _, out _);
            caster.AdjustMP(MPCost(caster) * -1);
            bool startCombat = true;
            if (hitsTarget)
            {
                if (!string.IsNullOrEmpty(DamageExpression))
                {
                    var dam = CalculateDamage(caster) * -1;
                    target.AdjustHP(dam, out bool isKilled);
                    if (isKilled)
                    {
                        startCombat = false;
                        if (caster.ActorType == ActorType.Player)
                        {
                            ((Player)caster).Send($"The power of your magic is too much for {target.Name}!{Constants.NewLine}");
                        }
                        if (target.ActorType == ActorType.Player)
                        {
                            ((Player)target).Send($"The power of {caster.Name}'s magic is too much for you and you succumb to your wounds!{Constants.NewLine}");
                        }
                        target.Kill(caster, true);
                        return;
                    }
                    else
                    {
                        if (caster.ActorType == ActorType.Player)
                        {
                            ((Player)caster).Send($"The power of your magic causes {dam:N0} damage to {target.Name}!{Constants.NewLine}");
                        }
                        if (target.ActorType == ActorType.Player)
                        {
                            ((Player)target).Send($"{caster.Name}'s magic strikes you for {dam:N0} damage!{Constants.NewLine}");
                        }
                    }
                }
                if (AppliedBuffs != null && AppliedBuffs.Count > 0)
                {
                    foreach(var b in AppliedBuffs.Keys)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            int duration = buff.CalculateDuration(caster, this);
                            target.ApplyBuff(buff.Name, duration);
                            if (caster.ActorType == ActorType.Player)
                            {
                                ((Player)caster).Send($"Your magic has afflicted {target.Name} with {buff.Name}!{Constants.NewLine}");
                            }
                            if (target.ActorType == ActorType.Player)
                            {
                                ((Player)target).Send($"The power of {caster.Name}'s magic has afflicted you with {buff.Name}!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            Game.LogMessage($"DEBUG: Spell '{Name}' cast by '{caster.Name}' cannot apply Buff '{b}', no such Buff in Buff Manager", LogLevel.Debug);
                        }
                    }
                }
            }
            else
            {
                if (caster.ActorType == ActorType.Player)
                {
                    ((Player)caster).Send($"The Winds of Magic turn against you and your {Name} spell misses!{Constants.NewLine}");
                }
                if (target.ActorType == ActorType.Player)
                {
                    ((Player)target).Send($"The Winds of Magic turn against {caster.Name} and their spell misses you!{Constants.NewLine}");
                }
            }
            if (startCombat)
            {
                caster.AddToTargetQueue(target);
                target.AddToTargetQueue(caster);
            }
        }
    }
}
