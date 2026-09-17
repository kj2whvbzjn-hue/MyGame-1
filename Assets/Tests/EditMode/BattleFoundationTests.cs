#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleFoundationTests
    {
        [Test] public void Critical_IsGuaranteedHit_AndConsumesOnlyOneRoll()
        {
            int hitCalls=0;
            var r=HitCritical.Resolve(20,DamageType.PHYSICAL,1,999,0,0,()=>0.19,()=>{hitCalls++;return 0.99;});
            Assert.IsTrue(r.critical);Assert.IsTrue(r.hit);Assert.AreEqual(1,r.rngConsumed);Assert.AreEqual(0,hitCalls);
        }

        [Test] public void PhysicalHitRate_IsNotCappedAt100_InDomain()
        {
            Assert.AreEqual(200,HitCritical.PhysicalHitRatePercent(20,10));
        }

        [Test] public void MagicalHitRate_IsCappedAt100()
        {
            Assert.AreEqual(100,HitCritical.MagicalHitRatePercent(20,10));
        }

        [Test] public void ElementOmissions_ShareRemainingPercentageEqually()
        {
            var n=DamageDefense.NormalizeShares(new[]{
                new ElementShare{element=Element.FIRE,share=40},
                new ElementShare{element=Element.ICE,share=null},
                new ElementShare{element=Element.WIND,share=null}
            });
            Assert.AreEqual(30,n[1].share.Value);Assert.AreEqual(30,n[2].share.Value);
        }

        [Test] public void Resistance_IsClampedTo75()
        {
            Assert.AreEqual(75,DamageDefense.ClampResistance(100));
        }

        [Test] public void Block_IsAppliedAfterFinalFloorCandidate()
        {
            var r=DamageDefense.ResolveBlock(101,true,.5,.3,.49);
            Assert.IsTrue(r.blocked);Assert.AreEqual(70,r.damage);
        }

        [Test] public void Barrier_IsConsumedFifo()
        {
            var r=DamageDefense.ConsumeBarrierFifo(12,new[]{
                new BarrierLayer{id="A",remaining=5},new BarrierLayer{id="B",remaining=10}
            });
            Assert.AreEqual(12,r.absorbed);Assert.AreEqual(0,r.hpDamageCandidate);
            Assert.AreEqual("B",r.layers[0].id);Assert.AreEqual(3,r.layers[0].remaining);
        }

        [Test] public void DualWield_IsOnlyCurrentCombatCapability()
        {
            var r=CombatCapabilityResolver.Normalize(new[]{"DUAL_WIELD"});
            Assert.IsTrue(r.Contains(CombatCapability.DUAL_WIELD));
        }

        [Test] public void FixedBattleOrder_ConsumesRngOnlyDuringInitialization()
        {
            int orderDraws=0;
            var fixedOrder=BattleTurnOrder.CreateFixedOrder(
                new[]{"actor-c","actor-a","actor-b"},
                ()=>{ orderDraws++; return orderDraws * 0.1; });

            Assert.AreEqual(3,orderDraws);

            var living=new HashSet<string>{"actor-a","actor-c"};
            var firstTick=BattleTurnOrder.LivingActorsInFixedOrder(fixedOrder,living);
            var secondTick=BattleTurnOrder.LivingActorsInFixedOrder(fixedOrder,living);

            Assert.AreEqual(3,orderDraws,"Tick processing must not consume battle-order RNG after initialization.");
            CollectionAssert.AreEqual(firstTick,secondTick);
        }
    }
}
#endif
