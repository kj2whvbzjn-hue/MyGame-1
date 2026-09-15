#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleEffectLifecycleTests
    {
        [Test] public void DamageHit_DispatchesOnDamage_InFormalOrder_AndConsumesOnce()
        {
            var hit=new ResolvedHitSaveRecord{actionId="R",hitIndex=0,sourceId="A",targetId="B",judgement="HIT",actualHpLoss=10};
            var regs=new[]{
                new TriggerRegistration{id="Y",ownerId="B",trigger=TriggerEvent.ON_DAMAGE,priority=2},
                new TriggerRegistration{id="X",ownerId="A",trigger=TriggerEvent.ON_DAMAGE,priority=2,once=true}
            };
            var order=new Dictionary<string,int>{{"A",0},{"B",1}};
            var d=BattleEffectLifecycle.DispatchResolvedHit(hit,regs,order);
            Assert.AreEqual(TriggerEvent.ON_DAMAGE,d.trigger);
            Assert.AreEqual("X",d.registrations[0].id);
            Assert.IsTrue(regs[1].consumed);
        }

        [Test] public void Miss_DispatchesOnHitWithoutDamage()
        {
            var hit=new ResolvedHitSaveRecord{actionId="R",sourceId="A",targetId="B",judgement="MISS",actualHpLoss=0};
            var regs=new[]{new TriggerRegistration{id="T",ownerId="A",trigger=TriggerEvent.ON_HIT}};
            var d=BattleEffectLifecycle.DispatchResolvedHit(hit,regs,new Dictionary<string,int>{{"A",0}});
            Assert.AreEqual(TriggerEvent.ON_HIT,d.trigger);
            Assert.AreEqual(1,d.registrations.Count);
        }

        [Test] public void EffectTick_RemovesExpiredAndConsumedEffects()
        {
            var a=new BattleActorSaveRecord();
            a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="1",remainingTicks=2});
            a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="2",remainingTicks=1});
            a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="3",remainingTicks=5,consumed=true});
            BattleEffectLifecycle.TickEffects(a);
            Assert.AreEqual(1,a.appliedEffects.Count);
            Assert.AreEqual(1,a.appliedEffects[0].remainingTicks);
        }

        [Test] public void PassiveProperty_UsesCompiledContributions()
        {
            var v=BattleEffectLifecycle.PassiveProperty(new[]{
                new PassiveContribution{passiveId="P1",property="ACTION_GAUGE_GAIN",value=2},
                new PassiveContribution{passiveId="P2",property="ACTION_GAUGE_GAIN",value=3}
            },"ACTION_GAUGE_GAIN");
            Assert.AreEqual(5,v);
        }
    }
}
#endif
