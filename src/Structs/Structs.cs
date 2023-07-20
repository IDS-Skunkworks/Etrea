using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdoms_of_Etrea
{
    internal static class LevelTable
    {
        private static Dictionary<uint, uint> _levelTable = new Dictionary<uint, uint>
        {
            { 1, 0 },
            { 2, 1000 },
            { 3, 4000 },
            { 4, 10000 },
            { 5, 20000 },
            { 6, 35000 },
            { 7, 56000 },
            { 8, 84000 },
            { 9, 120000 },
            { 10, 165000 },
            { 11, 220000 },
            { 12, 286000 },
            { 13, 364000 },
            { 14, 455000 },
            { 15, 560000 },
            { 16, 680000 },
            { 17, 816000 },
            { 18, 969000 },
            { 19, 1140000 },
            { 20, 1330000 }
        };

        internal static bool HasCharAchievedNewLevel(uint xp, uint currLevel, out uint calcLevel)
        {
            calcLevel = _levelTable.Where(x => x.Value <= xp).Max(y => y.Key);
            return calcLevel > currLevel;
        }

        internal static uint GetExpForNextLevel(uint currentLevel, uint currentExp)
        {
            var maxLevel = _levelTable.Max(x => x.Key);
            if (currentLevel < maxLevel)
            {
                var nextLevelXp = _levelTable.Where(x => x.Key == currentLevel + 1).First().Value;
                return nextLevelXp - currentExp;
            }
            return 0;
        }
    }

    [Serializable]
    internal class ActorStats
    {
        [JsonProperty]
        internal uint Strength { get; set; }
        [JsonProperty]
        internal uint Dexterity { get; set; }
        [JsonProperty]
        internal uint Intelligence { get; set; }
        [JsonProperty]
        internal uint Wisdom { get; set; }
        [JsonProperty]
        internal uint Constitution { get; set; }
        [JsonProperty]
        internal uint Charisma { get; set; }
        [JsonProperty]
        internal uint MaxHP { get; set; }
        [JsonProperty]
        internal uint MaxMP { get; set; }
        [JsonProperty]
        internal uint CurrentMaxHP { get; set; }      // in case of effects which alter max hp from base max
        [JsonProperty]
        internal uint CurrentMaxMP { get; set; }      // in case of effects which alter max mp from base max
        [JsonProperty]
        internal int CurrentHP { get; set; }
        [JsonProperty]
        internal int CurrentMP { get; set; }
        [JsonProperty]
        internal uint ArmourClass { get; set; }
        [JsonProperty]
        internal uint Gold { get; set; }
        [JsonProperty]
        internal uint Level { get; set; }
        [JsonProperty]
        internal uint Exp { get; set; }


        internal static int CalculateAbilityModifier(uint abilityScore)
        {
            return ((int)abilityScore - 10) / 2;
        }
    }
}