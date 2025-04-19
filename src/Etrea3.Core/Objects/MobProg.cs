using Newtonsoft.Json;
using System;
using NLua;
using Etrea3.Core;

namespace Etrea3.Objects
{
    [Serializable]
    public class MobProg : ScriptingObject
    {
        [JsonProperty]
        public MobProgTrigger Triggers { get; set; }

        public MobProg()
        {

        }

        public override void Init()
        {
            if (_lua == null)
            {
                _lua = new Lua();
                _lua.RegisterFunction("MobEmote", typeof(ScriptingFunctions).GetMethod("MobProgEmote"));
                _lua.RegisterFunction("MobMove", typeof(ScriptingFunctions).GetMethod("MobProgMove"));
                _lua.RegisterFunction("MobTakeItem", typeof(ScriptingFunctions).GetMethod("MobProgTakeItem"));
                _lua.RegisterFunction("MobDropItem", typeof(ScriptingFunctions).GetMethod("MobProgDropItem"));
                _lua.RegisterFunction("MobGiveItem", typeof(ScriptingFunctions).GetMethod("MobProgGiveItem"));
                _lua.RegisterFunction("MobAttack", typeof(ScriptingFunctions).GetMethod("MobProgAttack"));
                _lua.RegisterFunction("MobCastSpell", typeof(ScriptingFunctions).GetMethod("MobProgCastSpell"));
                _lua.RegisterFunction("MobSay", typeof(ScriptingFunctions).GetMethod("MobProgSay"));
                _lua.RegisterFunction("MobYell", typeof(ScriptingFunctions).GetMethod("MobProgYell"));
                _lua.RegisterFunction("MobWhisper", typeof(ScriptingFunctions).GetMethod("MobProgWhisper"));
                _lua.RegisterFunction("MobTeleportPlayer", typeof(ScriptingFunctions).GetMethod("MobProgTeleportPlayer"));
                _lua.RegisterFunction("MobRememberPlayer", typeof(ScriptingFunctions).GetMethod("MobProgRememberPlayer"));
                _lua.RegisterFunction("MobForgetPlayer", typeof(ScriptingFunctions).GetMethod("MobProgForgetPlayer"));
                _lua.RegisterFunction("MobRemembersPlayer", typeof(ScriptingFunctions).GetMethod("MobProgRemembersPlayer"));
                _lua.RegisterFunction("MobGetRememberedPlayerTick", typeof(ScriptingFunctions).GetMethod("MobProgGetRememberPlayerTickCount"));
                _lua.RegisterFunction("MobHasItem", typeof(ScriptingFunctions).GetMethod("MobProgMobHasItem"));
                _lua.RegisterFunction("GetMUDTick", typeof(ScriptingFunctions).GetMethod("GetMudTick"));
                _lua.RegisterFunction("RollDice", typeof(ScriptingFunctions).GetMethod("RollDice"));
                _lua.RegisterFunction("GetRandomNumber", typeof(ScriptingFunctions).GetMethod("GetRandomNumber"));
                _lua.RegisterFunction("MobCheckIfPlayerIsImm", typeof(ScriptingFunctions).GetMethod("MobProgCheckPlayerIsImm"));
                _lua.RegisterFunction("MobGetPlayerName", typeof(ScriptingFunctions).GetMethod("MobProgGetPlayerName"));
                _lua.RegisterFunction("GetRandomPlayer", typeof(ScriptingFunctions).GetMethod("GetRandomPlayerID"));
                _lua.RegisterFunction("GetItemName", typeof(ScriptingFunctions).GetMethod("MobProgGetItemName"));
                _lua.RegisterFunction("IsItemInRoom", typeof(ScriptingFunctions).GetMethod("MobProgIsItemInRoom"));
                _lua.RegisterFunction("MobSellPlayerItem", typeof(ScriptingFunctions).GetMethod("MogProgMobSellPlayerItem"));
                _lua.RegisterFunction("GetCurrentTimeOfDay", typeof(ScriptingFunctions).GetMethod("GetCurrentTOD"));
                _lua.RegisterFunction("GetPreviousTimeOfDay", typeof(ScriptingFunctions).GetMethod("GetPreviousTOD"));
                _lua.RegisterFunction("PlayerHasQuest", typeof(ScriptingFunctions).GetMethod("PlayerHasQuest"));
            }
            _lua.DoString(Script);
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
