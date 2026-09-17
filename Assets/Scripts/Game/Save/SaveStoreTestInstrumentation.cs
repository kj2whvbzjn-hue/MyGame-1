using System;

namespace GuildAdventure.Game.Save
{
    public sealed class RecordingSaveStore<T> : ISaveStore<T>
    {
        private T value;

        public RecordingSaveStore(T initialValue)
        {
            value = initialValue;
        }

        public int LoadCount { get; private set; }
        public int WriteCount { get; private set; }
        public bool FailNextWrite { get; set; }

        public T Load()
        {
            LoadCount++;
            return value;
        }

        public void Write(T next)
        {
            WriteCount++;
            if (FailNextWrite)
            {
                FailNextWrite = false;
                throw new InvalidOperationException("Injected save write failure.");
            }
            value = next;
        }
    }
}
