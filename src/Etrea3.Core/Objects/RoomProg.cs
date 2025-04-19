using Etrea3.Core;
using Newtonsoft.Json;
using System;
using NLua;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etrea3.Objects
{
    [Serializable]
    public class RoomProg : ScriptingObject
    {
        [JsonProperty]
        public RoomProgTrigger Triggers { get; set; }

        public RoomProg()
        {

        }

        public override void Init()
        {
            if (_lua == null)
            {
                _lua = new Lua();
                _lua.RegisterFunction("ToggleRoomFlag", typeof(ScriptingFunctions).GetMethod("ToggleRoomFlag"));
                _lua.RegisterFunction("SetRoomFlag", typeof(ScriptingFunctions).GetMethod("SetRoomFlag"));
                _lua.RegisterFunction("SendEnvironmentMessage", typeof(ScriptingFunctions).GetMethod("SendEnvironmentMessage"));
                _lua.RegisterFunction("SpawnItemInRoom", typeof(ScriptingFunctions).GetMethod("SpawnItemInRoom"));
                _lua.RegisterFunction("SpawnNPCInRoom", typeof(ScriptingFunctions).GetMethod("SpawnNPCInRoom"));
                _lua.RegisterFunction("DespawnItemInRoom", typeof(ScriptingFunctions).GetMethod("DespawnItemInRoom"));
                _lua.RegisterFunction("DespawnNPCInRoom", typeof(ScriptingFunctions).GetMethod("DespawnNPCInRoom"));
                _lua.RegisterFunction("TeleportPlayer", typeof(ScriptingFunctions).GetMethod("TeleportPlayer"));
                _lua.RegisterFunction("TeleportItem", typeof(ScriptingFunctions).GetMethod("TeleportItem"));
                _lua.RegisterFunction("TeleportNPC", typeof(ScriptingFunctions).GetMethod("TeleportNPC"));
                _lua.RegisterFunction("PlayerHasQuest", typeof(ScriptingFunctions).GetMethod("PlayerHasQuest"));
                _lua.RegisterFunction("GetCurrentTOD", typeof(ScriptingFunctions).GetMethod("GetCurrentTOD"));
                _lua.RegisterFunction("GetPreviousTOD", typeof(ScriptingFunctions).GetMethod("GetPreviousTOD"));
                _lua.RegisterFunction("RollDice", typeof(ScriptingFunctions).GetMethod("RollDice"));
                _lua.RegisterFunction("GetRandomNumber", typeof(ScriptingFunctions).GetMethod("GetRandomNumber"));
                _lua.RegisterFunction("GetMudTick", typeof(ScriptingFunctions).GetMethod("GetMudTick"));

            }
            _lua.DoString(Script);
        }

        public void TriggerEvent(RoomProgTrigger trigger, object parameters)
        {
            try
            {
                var func = _lua[trigger.ToString()] as LuaFunction;
                func?.Call(parameters);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProg.TriggerEvent() for RoomProg {ID}: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
