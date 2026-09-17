using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using NUnit.Framework;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FoundationF02InstrumentationTests
    {
        private sealed class CloneableState : IDeepCloneable<CloneableState>
        {
            public int value;
            public CloneableState DeepClone() => new CloneableState { value = value };
        }

        [Test]
        public void RecordingRandomSource_CountsCallsByPurpose()
        {
            var rng = new RecordingRandomSource(new SequenceRandomSource(new[] { 0.1, 0.2, 0.3 }));

            Assert.AreEqual(0.1, rng.Next01(RandomPurpose.BattleOrder));
            Assert.AreEqual(0.2, rng.Next01(RandomPurpose.Hit));
            Assert.AreEqual(0.3, rng.Next01(RandomPurpose.Hit));

            Assert.AreEqual(3, rng.TotalCount);
            Assert.AreEqual(1, rng.CountFor(RandomPurpose.BattleOrder));
            Assert.AreEqual(2, rng.CountFor(RandomPurpose.Hit));
            Assert.AreEqual(0, rng.CountFor(RandomPurpose.Reward));
        }

        [Test]
        public void RecordingSaveStore_CountsWritesAndCanInjectFailure()
        {
            var store = new RecordingSaveStore<CloneableState>(new CloneableState { value = 1 });
            store.FailNextWrite = true;

            var failed = SaveTransaction.Execute(
                store,
                state => { state.value = 2; return state; },
                state => null);

            Assert.IsFalse(failed.ok);
            Assert.AreEqual("save_write_failed", failed.reason);
            Assert.AreEqual(1, store.WriteCount);

            var succeeded = SaveTransaction.Execute(
                store,
                state => { state.value = 3; return state; },
                state => null);

            Assert.IsTrue(succeeded.ok);
            Assert.AreEqual(2, store.WriteCount);
            Assert.AreEqual(3, store.Load().value);
        }
    }
}
