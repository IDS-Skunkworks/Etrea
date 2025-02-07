using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Etrea3.Core
{
    public static class LevelTable
    {
        private static ConcurrentDictionary<int, uint> ExpTable = new ConcurrentDictionary<int, uint>
        (
            new Dictionary<int, uint>
            {
                // TODO: Add more levels as required
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
            }
        );

        public static bool HasAchievedNewLevel(uint charExp, int currLevel, out int calcLevel)
        {
            calcLevel = ExpTable.Where(x => x.Value <= charExp).Max(y => y.Key);
            return calcLevel > currLevel;
        }

        public static uint ExpForNextLevel(int currLevel, uint charExp)
        {
            var maxLevel = ExpTable.Max(x => x.Key);
            if (currLevel < maxLevel)
            {
                var nextLevelExp = ExpTable.Where(x => x.Key == currLevel + 1).First().Value;
                return nextLevelExp - charExp;
            }
            return 0;
        }
    }
}
