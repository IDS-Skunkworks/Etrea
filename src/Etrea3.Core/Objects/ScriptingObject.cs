using Newtonsoft.Json;
using System;
using NLua;

namespace Etrea3.Objects
{
    [Serializable]
    public abstract class ScriptingObject
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Script { get; set; }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
        [NonSerialized]
        protected Lua _lua = null;

        public abstract void Init();
        public void Dispose()
        {
            if (_lua != null)
            {
                _lua.Dispose();
            }
        }
    }
}
