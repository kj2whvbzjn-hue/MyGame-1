using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Game.Character
{
    [Serializable]
    public sealed class StatGrowthResult
    {
        public CharacterStat stat;
        public int growthValue;
        public int guaranteedIncrease;
        public double randomRoll;
        public bool extraIncrease;
        public int finalIncrease;
        public int beforeValue;
        public int afterValue;
    }

    [Serializable]
    public sealed class LevelGrowthResult
    {
        public bool grew;
        public string reason;
        public int fromLevel;
        public int toLevel;
        public string jobId;
        public List<StatGrowthResult> stats = new List<StatGrowthResult>();
    }

    public static class CharacterGrowth
    {
        public const int DefaultInitialStat = 10;
        public const int DefaultInitialLevel = 1;
        public const int DefaultMaxLevel = 50;

        public static int CalculateIncrement(int growthValue, double randomRoll)
        {
            if (growthValue < 0)
                throw new ArgumentOutOfRangeException(nameof(growthValue));
            if (randomRoll < 0d || randomRoll >= 1d)
                throw new ArgumentOutOfRangeException(nameof(randomRoll));

            int guaranteed = growthValue / 10;
            int remainder = growthValue % 10;
            bool extra = remainder > 0 && randomRoll < remainder / 10d;
            return guaranteed + (extra ? 1 : 0);
        }

        public static double CalculateExpectedStat(
            int level,
            int growthValue,
            int initialStat = DefaultInitialStat,
            int initialLevel = DefaultInitialLevel)
        {
            if (level < initialLevel) throw new ArgumentOutOfRangeException(nameof(level));
            if (growthValue < 0) throw new ArgumentOutOfRangeException(nameof(growthValue));
            return initialStat + (level - initialLevel) * growthValue / 10d;
        }

        public static LevelGrowthResult ResolveLevelUp(
            int level,
            int maxLevel,
            string jobId,
            IDictionary<CharacterStat, int> currentStats,
            IDictionary<CharacterStat, int> growthByStat,
            IRandomSource random)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (maxLevel < 1 || level > maxLevel) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            if (string.IsNullOrWhiteSpace(jobId)) throw new ArgumentException("jobId is required.", nameof(jobId));
            if (currentStats == null) throw new ArgumentNullException(nameof(currentStats));
            if (growthByStat == null) throw new ArgumentNullException(nameof(growthByStat));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var result = new LevelGrowthResult
            {
                grew = level < maxLevel,
                reason = level < maxLevel ? null : "max_level",
                fromLevel = level,
                toLevel = level < maxLevel ? level + 1 : level,
                jobId = jobId
            };

            if (!result.grew)
                return result;

            foreach (CharacterStat stat in Enum.GetValues(typeof(CharacterStat)))
            {
                if (!currentStats.TryGetValue(stat, out int before))
                    throw new ArgumentException($"Missing current stat: {stat}", nameof(currentStats));
                if (!growthByStat.TryGetValue(stat, out int growth))
                    throw new ArgumentException($"Missing growth value: {stat}", nameof(growthByStat));
                if (before < 0 || growth < 0)
                    throw new ArgumentOutOfRangeException(stat.ToString());

                double roll = random.Next01($"growth:{stat}");
                int guaranteed = growth / 10;
                int remainder = growth % 10;
                bool extra = remainder > 0 && roll < remainder / 10d;
                int increase = guaranteed + (extra ? 1 : 0);

                result.stats.Add(new StatGrowthResult
                {
                    stat = stat,
                    growthValue = growth,
                    guaranteedIncrease = guaranteed,
                    randomRoll = roll,
                    extraIncrease = extra,
                    finalIncrease = increase,
                    beforeValue = before,
                    afterValue = before + increase
                });
            }

            return result;
        }
    }
}
