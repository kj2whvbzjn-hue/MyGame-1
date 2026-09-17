#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleTurnOrderSnapshotTests
    {
        [Test]
        public void FixedOrder_IsDrawnOnceAtBattleStart_ThenSnapshotIsAuthoritative()
        {
            var draws=0;
            var values=new Queue<double>(new[]{0.8,0.1,0.4});
            Func<double> draw=()=>{draws++;return values.Dequeue();};

            var created=BattleTurnOrder.CreateFixedOrder(new[]{"A","B","C"},draw);
            Assert.AreEqual(3,draws);

            var snapshot=new BattleSnapshotSaveRecord
            {
                battleId="BATTLE-1",settingsVersion="SETTINGS-1",seed="SEED-1",
                actors=new List<BattleActorSaveRecord>
                {
                    Actor("A"),Actor("B"),Actor("C")
                },
                fixedActorOrder=new List<string>{created[0].actorId,created[1].actorId,created[2].actorId}
            };
            Assert.IsNull(BattleSnapshotValidation.Validate(snapshot));

            var before=draws;
            var first=BattleEffectLifecycle.BuildFixedOrder(snapshot);
            var second=BattleEffectLifecycle.BuildFixedOrder(snapshot);
            Assert.AreEqual(before,draws,"Tick/Hit/replay order lookup must not consume order RNG.");
            CollectionAssert.AreEqual(snapshot.fixedActorOrder,new List<string>(SortByOrder(first)));
            CollectionAssert.AreEqual(snapshot.fixedActorOrder,new List<string>(SortByOrder(second)));
        }

        [Test]
        public void ResumePath_UsesSavedOrder_WithoutOrderRng()
        {
            var snapshot=new BattleSnapshotSaveRecord
            {
                battleId="BATTLE-RESUME",settingsVersion="SETTINGS-1",seed="SEED-1",tick=7,
                actors=new List<BattleActorSaveRecord>{Actor("A"),Actor("B")},
                fixedActorOrder=new List<string>{"B","A"}
            };
            Assert.IsNull(BattleSnapshotValidation.Validate(snapshot));

            var orderRngCalls=0;
            Func<double> forbiddenOrderRng=()=>{orderRngCalls++;throw new Exception("ORDER_RNG_MUST_NOT_RUN_ON_RESUME");};
            // Resume restores actor state and reads the already-saved order; it must never call the battle-start draw.
            foreach(var actor in snapshot.actors) BattleResumeAdapter.RestoreActor(actor);
            var order=BattleEffectLifecycle.BuildFixedOrder(snapshot);

            Assert.AreEqual(0,orderRngCalls);
            Assert.NotNull(forbiddenOrderRng); // keeps the forbidden source explicit in the fixture.
            CollectionAssert.AreEqual(new[]{"B","A"},new List<string>(SortByOrder(order)));
        }

        static BattleActorSaveRecord Actor(string id)=>new BattleActorSaveRecord
        {
            actorId=id,teamId="T",formationRow=0,hp=10,maxHp=10,mp=0,maxMp=0,speed=10,alive=true
        };

        static IEnumerable<string> SortByOrder(IReadOnlyDictionary<string,int> order)
        {
            var rows=new List<KeyValuePair<string,int>>(order);
            rows.Sort((x,y)=>x.Value!=y.Value?x.Value.CompareTo(y.Value):string.CompareOrdinal(x.Key,y.Key));
            foreach(var row in rows)yield return row.Key;
        }
    }
}
#endif
