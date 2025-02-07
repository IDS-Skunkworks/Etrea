using Newtonsoft.Json;
using System;
using NLua;
using Etrea3.Core;

namespace Etrea3.Objects
{
    [Serializable]
    public class MobProg
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Script { get; set; }
        [JsonProperty]
        public MobProgTrigger Triggers { get; set; }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
        [NonSerialized]
        private Lua _lua = null;

        public MobProg()
        {

        }

        public void Init()
        {
            if (_lua == null)
            {
                _lua = new Lua();
                _lua.RegisterFunction("MobEmote", typeof(ActMob).GetMethod("MobProgEmote"));
                _lua.RegisterFunction("MobMove", typeof(ActMob).GetMethod("MobProgMove"));
                _lua.RegisterFunction("MobTakeItem", typeof(ActMob).GetMethod("MobProgTakeItem"));
                _lua.RegisterFunction("MobDropItem", typeof(ActMob).GetMethod("MobProgDropItem"));
                _lua.RegisterFunction("MobGiveItem", typeof(ActMob).GetMethod("MobProgGiveItem"));
                _lua.RegisterFunction("MobAttack", typeof(ActMob).GetMethod("MobProgAttack"));
                _lua.RegisterFunction("MobCastSpell", typeof(ActMob).GetMethod("MobProgCastSpell"));
                _lua.RegisterFunction("MobSay", typeof(ActMob).GetMethod("MobProgSay"));
                _lua.RegisterFunction("MobYell", typeof(ActMob).GetMethod("MobProgYell"));
                _lua.RegisterFunction("MobWhisper", typeof(ActMob).GetMethod("MobProgWhisper"));
                _lua.RegisterFunction("MobTeleportPlayer", typeof(ActMob).GetMethod("MobProgTeleportPlayer"));
                _lua.RegisterFunction("MobRememberPlayer", typeof(ActMob).GetMethod("MobProgRememberPlayer"));
                _lua.RegisterFunction("MobForgetPlayer", typeof(ActMob).GetMethod("MobProgForgetPlayer"));
                _lua.RegisterFunction("MobRemembersPlayer", typeof(ActMob).GetMethod("MobProgRemembersPlayer"));
                _lua.RegisterFunction("MobGetRememberedPlayerTick", typeof(ActMob).GetMethod("MobProgGetRememberPlayerTickCount"));
                _lua.RegisterFunction("MobHasItem", typeof(ActMob).GetMethod("MobProgMobHasItem"));
                _lua.RegisterFunction("MobGetMUDTick", typeof(ActMob).GetMethod("MobProgGetMudTick"));
                _lua.RegisterFunction("MobRollDice", typeof(ActMob).GetMethod("MobProgRollDice"));
                _lua.RegisterFunction("MobCheckIfPlayerIsImm", typeof(ActMob).GetMethod("MobProgCheckPlayerIsImm"));
                _lua.RegisterFunction("MobGetPlayerName", typeof(ActMob).GetMethod("MobProgGetPlayerName"));
                _lua.RegisterFunction("MobGetRandomPlayer", typeof(ActMob).GetMethod("MobProgGetRandomPlayerID"));
                _lua.RegisterFunction("MobGetItemName", typeof(ActMob).GetMethod("MobProgGetItemName"));
                _lua.RegisterFunction("MobItemInRoom", typeof(ActMob).GetMethod("MobProgItemInRoom"));
                _lua.RegisterFunction("MobSellPlayerItem", typeof(ActMob).GetMethod("MogProgMobSellPlayerItem"));
            }
            _lua.DoString(Script);
        }

        public void Dispose()
        {
            if (_lua != null)
            {
                _lua.Dispose();
            }
        }

        public void TriggerEvent(MobProgTrigger trigger, object parameters)
        {
            try
            {
                var func = _lua[trigger.ToString()] as LuaFunction;
                func?.Call(parameters);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProg.TriggerEvent() for MobProg {ID}: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
