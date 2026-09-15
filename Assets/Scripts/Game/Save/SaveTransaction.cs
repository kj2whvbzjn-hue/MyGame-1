using System;

namespace GuildAdventure.Game.Save
{
    public interface ISaveStore<T>
    {
        T Load();
        void Write(T value);
    }

    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }

    public sealed class SaveTransactionResult<T>
    {
        public bool ok;
        public string reason;
        public T committed;
    }

    public static class SaveTransaction
    {
        public static SaveTransactionResult<T> Execute<T>(
            ISaveStore<T> store,
            Func<T,T> mutate,
            Func<T,string> validate)
            where T : IDeepCloneable<T>
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (mutate == null) throw new ArgumentNullException(nameof(mutate));
            if (validate == null) throw new ArgumentNullException(nameof(validate));

            var current = store.Load();
            if (current == null)
                return new SaveTransactionResult<T>{ok=false,reason="save_missing"};

            var working = current.DeepClone();
            T next;
            try { next = mutate(working); }
            catch { return new SaveTransactionResult<T>{ok=false,reason="mutation_failed"}; }

            if (next == null)
                return new SaveTransactionResult<T>{ok=false,reason="mutation_returned_null"};

            var error = validate(next);
            if (!string.IsNullOrEmpty(error))
                return new SaveTransactionResult<T>{ok=false,reason=error};

            try { store.Write(next); }
            catch { return new SaveTransactionResult<T>{ok=false,reason="save_write_failed"}; }

            return new SaveTransactionResult<T>{ok=true,committed=next};
        }
    }
}
