#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Inventory;
using GuildAdventure.Game.Reward;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class RewardSaveTransactionTests
    {
        private sealed class Store : ISaveStore<GameSaveState>
        {
            public GameSaveState state;
            public GameSaveState Load() => state;
            public void Write(GameSaveState value) => state = value;
        }

        [Test]
        public void Commit_UsesExistingStaticSaveTransactionApi()
        {
            var store = new Store { state = new GameSaveState() };
            store.state.inventory.capacity = 2;
            var rewards = new List<RewardItem> { new RewardItem { refId = "R1", kind = "item", amount = 1 } };

            GameSaveState committed;
            string error;
            var ok = RewardSaveTransaction.Commit(store, rewards, r => "I1", out committed, out error);

            Assert.IsTrue(ok, error);
            Assert.IsNotNull(committed);
            Assert.AreEqual(1, store.state.inventory.items.Count);
        }

        [Test]
        public void Commit_CapacityFailure_DoesNotModifyStoredSave()
        {
            var original = new GameSaveState();
            original.inventory.capacity = 1;
            original.inventory.items.Add(new InventoryItem { instanceId = "OLD", masterId = "M0", kind = "item", amount = 1 });
            var store = new Store { state = original };
            var rewards = new List<RewardItem> { new RewardItem { refId = "R1", kind = "item", amount = 1 } };

            GameSaveState committed;
            string error;
            var ok = RewardSaveTransaction.Commit(store, rewards, r => "NEW", out committed, out error);

            Assert.IsFalse(ok);
            Assert.IsNull(committed);
            Assert.AreSame(original, store.state);
            Assert.AreEqual(1, store.state.inventory.items.Count);
            Assert.AreEqual("OLD", store.state.inventory.items[0].instanceId);
        }
    }
}
#endif
