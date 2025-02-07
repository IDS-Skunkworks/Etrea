using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class HelpArticle
    {
        [JsonProperty]
        public string Title { get; set; }
        [JsonProperty]
        public string ArticleText { get; set; }
        [JsonProperty]
        public bool ImmOnly { get; set; }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
    }
}
