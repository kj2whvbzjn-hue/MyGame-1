using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Core
{
    public static class RandomPurpose
    {
        public const string BattleOrder = "battle_order";
        public const string Hit = "hit";
        public const string Critical = "critical";
        public const string TargetSelection = "target_selection";
        public const string AiTieBreak = "ai_tie_break";
        public const string Growth = "growth";
        public const string Reward = "reward";
    }

    public sealed class RandomCallRecord
    {
        public readonly string purpose;
        public readonly double value;

        public RandomCallRecord(string purpose, double value)
        {
            this.purpose = purpose;
            this.value = value;
        }
    }

    public sealed class RecordingRandomSource : IRandomSource
    {
        private readonly IRandomSource inner;
        private readonly List<RandomCallRecord> calls = new List<RandomCallRecord>();
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

        public RecordingRandomSource(IRandomSource inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IReadOnlyList<RandomCallRecord> Calls => calls;
        public int TotalCount => calls.Count;

        public int CountFor(string purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose)) return 0;
            return counts.TryGetValue(purpose, out var count) ? count : 0;
        }

        public double Next01(string purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
                throw new ArgumentException("Random purpose is required.", nameof(purpose));

            var value = inner.Next01(purpose);
            calls.Add(new RandomCallRecord(purpose, value));
            counts[purpose] = CountFor(purpose) + 1;
            return value;
        }
    }
}
