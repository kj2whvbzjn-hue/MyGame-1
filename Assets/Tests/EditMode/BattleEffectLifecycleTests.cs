#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class BattleEffectLifecycleTests
    {
        [Test] public void DamageHit_DispatchesHitReceivedAndDamageEvents_InFormalOrder()
        {
            var hit=new ResolvedHitSaveRecord{actionId="R",hitIndex=0,sourceId="A",targetId="B",judgement="HIT",actualHpLoss=10};
            var regs=new[]{
                new TriggerRegistration{id="D",ownerId="A",trigger=TriggerEvent.ON_DAMAGE_DEALT,priority=2,sequence=2},
                new TriggerRegistration{id="H2",ownerId="A",trigger=TriggerEvent.ON_HIT_DEALT,priority=2,sequence=2},
                new TriggerRegistration{id="H1",ownerId="A",trigger=TriggerEvent.ON_HIT_DEALT,priority=2,sequence=1,once=true},
                new TriggerRegistration{id="R",ownerId="B",trigger=TriggerEvent.ON_HIT_RECEIVED,priority=1,sequence=1}
            };
            var all=BattleEffectLifecycle.DispatchResolvedHitEvents(hit,regs,new Dictionary<string,int>{{"A",0},{"B",1}});
            Assert.AreEqual(3,all.Count);
            Assert.AreEqual(TriggerEvent.ON_HIT_DEALT,all[0].trigger);
            Assert.AreEqual("H1",all[0].registrations[0].id);
            Assert.AreEqual("H2",all[0].registrations[1].id);
            Assert.AreEqual(TriggerEvent.ON_HIT_RECEIVED,all[1].trigger);
            Assert.AreEqual(TriggerEvent.ON_DAMAGE_DEALT,all[2].trigger);
            Assert.IsTrue(regs[2].consumed);
        }

        [Test] public void CriticalHit_AlsoDispatchesCriticalBeforeDamage()
        {
            var hit=new ResolvedHitSaveRecord{actionId="R",sourceId="A",targetId="B",judgement="CRITICAL",actualHpLoss=4};
            var all=BattleEffectLifecycle.DispatchResolvedHitEvents(hit,new TriggerRegistration[0],new Dictionary<string,int>());
            Assert.AreEqual(4,all.Count);
            Assert.AreEqual(TriggerEvent.ON_CRITICAL,all[2].trigger);
            Assert.AreEqual(TriggerEvent.ON_DAMAGE_DEALT,all[3].trigger);
        }

        [Test] public void Miss_DoesNotDispatchOnHit()
        {
            var hit=new ResolvedHitSaveRecord{actionId="R",sourceId="A",targetId="B",judgement="MISS",actualHpLoss=0};
            Assert.AreEqual(0,BattleEffectLifecycle.DispatchResolvedHitEvents(hit,null,new Dictionary<string,int>()).Count);
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
            var settings=new PassiveRuntimeSettings{maxPassiveSlots=4};
            var v=BattleEffectLifecycle.PassiveProperty(new[]{
                new PassiveContribution{passiveId="P1",seriesId="S1",property="ACTION_GAUGE_GAIN",value=2},
                new PassiveContribution{passiveId="P2",seriesId="S2",property="ACTION_GAUGE_GAIN",value=3}
            },"ACTION_GAUGE_GAIN",settings);
            Assert.AreEqual(5,v);
        }
    }
}
#endif
