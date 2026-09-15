#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.AI;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class AiDecisionTargetRuntimeTests
    {
        sealed class FixedRng:IRandomSource
        {
            readonly double value; public int calls; public string purpose;
            public FixedRng(double value){this.value=value;}
            public double Next01(string p){calls++;purpose=p;return value;}
        }

        [Test] public void OneCandidate_DoesNotConsumeAiDecisionRng()
        {
            var s=Snapshot(); var rng=new FixedRng(.9);
            var r=AiDecisionTargetRuntime.Select(s,new[]{"B"},rng);
            Assert.IsTrue(r.ok); Assert.AreEqual("B",r.targetId); Assert.AreEqual(0,rng.calls);
        }

        [Test] public void MultipleCandidates_UseFixedOrderThenAiDecisionRng()
        {
            var s=Snapshot(); var rng=new FixedRng(.75);
            var r=AiDecisionTargetRuntime.Select(s,new[]{"C","B"},rng);
            Assert.IsTrue(r.ok); CollectionAssert.AreEqual(new[]{"B","C"},r.candidateIds);
            Assert.AreEqual("C",r.targetId); Assert.AreEqual(1,rng.calls);
            Assert.AreEqual(AiDecisionTargetRuntime.RngPurpose,rng.purpose);
        }

        [Test] public void DeadCandidates_AreExcluded()
        {
            var s=Snapshot(); s.actors.Find(x=>x.actorId=="B").alive=false;
            var r=AiDecisionTargetRuntime.Select(s,new[]{"B","C"},null);
            Assert.IsTrue(r.ok); Assert.AreEqual("C",r.targetId);
        }

        static BattleSnapshotSaveRecord Snapshot()
        {
            var s=new BattleSnapshotSaveRecord();
            s.fixedActorOrder.AddRange(new[]{"A","B","C"});
            s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10});
            s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10});
            s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10});
            return s;
        }
    }
}
#endif
