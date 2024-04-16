using System.Collections.Generic;
using System.Linq;

namespace Etrea2.Core
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

        internal static bool HasCharacterAchievedNewLevel(uint xp, uint currLevel, out uint calcLevel)
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
}
