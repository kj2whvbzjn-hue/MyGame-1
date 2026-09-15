#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickDotTriggerTests
 {
  [Test] public void LethalDot_DispatchesDamageThenDeathBeforeEffectCleanup(){var s=S(2);var a=s.actors[0];a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="DOT",sourceId="SRC",effectId="BURN",kind=EffectLifecycleKind.DOT.ToString(),value=5,remainingTicks=2});var executed=new List<TriggerEvent>();var regs=new[]{new TriggerRegistration{id="DAMAGE",trigger=TriggerEvent.ON_DAMAGE_DEALT,activationChance=1},new TriggerRegistration{id="DEATH",trigger=TriggerEvent.ON_DEATH,activationChance=1}};var r=BattleTickRuntime.Advance(s,new BattleTickOptions{triggerRegistrations=regs,executeReactive=x=>executed.Add(x.registration.trigger)});Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(a.alive);Assert.AreEqual(0,a.hp);CollectionAssert.AreEqual(new[]{TriggerEvent.ON_DAMAGE_DEALT,TriggerEvent.ON_DEATH},executed);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(2,r.triggerDispatches.Count);}
  [Test] public void BarrierAbsorbedDot_DoesNotDispatchDamageEvent(){var s=S(10);var a=s.actors[0];EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="SH",sourceId="A",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=5},9);a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="DOT",sourceId="SRC",effectId="POISON",kind=EffectLifecycleKind.DOT.ToString(),value=3,remainingTicks=2});var r=BattleTickRuntime.Advance(s,new BattleTickOptions{triggerRegistrations=new[]{new TriggerRegistration{id="D",trigger=TriggerEvent.ON_DAMAGE_DEALT,activationChance=1}}});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(10,a.hp);Assert.AreEqual(0,r.triggerDispatches.Count);}
  static BattleSnapshotSaveRecord S(int hp){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=hp,maxHp=10,alive=true});return s;}
 }
}
#endif
