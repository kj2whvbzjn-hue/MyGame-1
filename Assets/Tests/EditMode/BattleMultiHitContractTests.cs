#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleMultiHitContractTests
    {
        sealed class FixedRng : IRandomSource { readonly double v; public FixedRng(double value){v=value;} public double Next01(string purpose)=>v; }

        static BattleSnapshotSaveRecord Snapshot(int targetHp=100)
        {
            var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};
            s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=0,maxMp=0,alive=true});
            s.actors.Add(new BattleActorSaveRecord{actorId="T",hp=targetHp,maxHp=targetHp,mp=0,maxMp=0,alive=true});
            s.fixedActorOrder.AddRange(new[]{"A","T"});
            return s;
        }

        [Test] public void MultiHit_ProducesOneC03PerResolvedHit_WithStableIndices()
        {
            var s=Snapshot();
            var p=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="S",
                hitCount=3,damageType=DamageType.PHYSICAL,baseDamage=10,accuracy=100,evasion=0};
            var r=BattleStepExecutor.ExecuteAttack(s,p,new FixedRng(.99),new FixedRng(.01),new FixedRng(.99));
            Assert.IsTrue(r.ok);
            Assert.AreEqual(1,s.actionReservations.Count);
            Assert.AreEqual(3,r.resolvedHits.Count);
            Assert.AreEqual(3,s.resolvedHits.Count);
            for(var i=0;i<3;i++){Assert.AreEqual("C03",r.resolvedHits[i].contract);Assert.AreEqual(i,r.resolvedHits[i].hitIndex);Assert.AreEqual("R",r.resolvedHits[i].actionId);}
        }

        [Test] public void MultiHit_StopsAfterTargetDies()
        {
            var s=Snapshot(5);
            var p=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="S",
                hitCount=4,damageType=DamageType.PHYSICAL,baseDamage=20,accuracy=100,evasion=0};
            var r=BattleStepExecutor.ExecuteAttack(s,p,new FixedRng(.99),new FixedRng(.01),new FixedRng(.99));
            Assert.IsTrue(r.ok);
            Assert.AreEqual(1,r.resolvedHits.Count);
            Assert.IsFalse(s.actors[1].alive);
        }

        [Test] public void InvalidHitCount_DoesNotCommitC02OrC03()
        {
            var s=Snapshot();
            var p=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="S",hitCount=0};
            var r=BattleStepExecutor.ExecuteAttack(s,p,new FixedRng(.99),new FixedRng(.01),new FixedRng(.99));
            Assert.IsFalse(r.ok);
            Assert.AreEqual(0,s.actionReservations.Count);
            Assert.AreEqual(0,s.resolvedHits.Count);
        }
    }
}
#endif
