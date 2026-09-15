using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Core
{
    public sealed class SequenceRandomSource : IRandomSource
    {
        private readonly Queue<double> values;

        public SequenceRandomSource(IEnumerable<double> values)
        {
            this.values = new Queue<double>(values ?? throw new ArgumentNullException(nameof(values)));
        }

        public double Next01(string purpose)
        {
            if (values.Count == 0)
                throw new InvalidOperationException("No random values remain.");

            var value = values.Dequeue();
            if (value < 0d || value >= 1d)
                throw new InvalidOperationException("Random value must be >= 0 and < 1.");

            return value;
        }
    }
}
