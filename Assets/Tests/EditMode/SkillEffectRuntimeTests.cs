#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillEffectRuntimeTests
 {
  [Test] public void BarrierApply_PersistsSnapshotAmount(){var s=S();var a=s.actors[0];var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.APPLY,lifecycleKind=EffectLifecycleKind.BARRIER,instanceId="SH",effectId="BARRIER",sourceId="A",power=25,sequence=3});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(25,a.barrierLayers[0].remaining);Assert.AreEqual(4,a.barrierLayers[0].appliedTick);}
  [Test] public void Heal_UsesGs18FloorThenMagicCeilAndCapsMaxHp(){var s=S();var a=s.actors[0];a.hp=50;var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=20,magicIncreaseMultiplier=1.25});Assert.IsTrue(r.ok);Assert.AreEqual(75,a.hp);}
  [Test] public void Remove_NormalCleanse_DoesNotRemoveDot(){var s=S();var a=s.actors[0];EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="D",effectId="BURN",kind=EffectLifecycleKind.DOT,baseDurationTicks=5,value=3});var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.REMOVE});Assert.IsFalse(r.ok);Assert.AreEqual(1,a.appliedEffects.Count);}
  [Test] public void Revive_RestoresDeadActor(){var s=S();var a=s.actors[0];a.hp=0;a.alive=false;var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.REVIVE,power=30});Assert.IsTrue(r.ok);Assert.AreEqual(30,a.hp);Assert.IsTrue(a.alive);}
  [Test] public void StatusApply_DispatchesOnStatusApplied(){var s=S();var reg=R("P",TriggerEvent.ON_STATUS_APPLIED);var r=SkillEffectRuntime.Execute(s,s.actors[0],new SkillEffectRequest{kind=SkillEffectKind.APPLY,lifecycleKind=EffectLifecycleKind.STATUS,instanceId="ST",effectId="STUN",sourceId="A",durationTicks=3,triggerRegistrations=new[]{reg}});Assert.IsTrue(r.ok);Assert.AreEqual(1,r.triggerDispatches.Count);Assert.AreEqual(TriggerEvent.ON_STATUS_APPLIED,r.triggerDispatches[0].trigger);Assert.AreEqual(1,r.reactiveExecuted);}
  [Test] public void ResourceLethal_DispatchesDeathOnce(){var s=S();var reg=R("D",TriggerEvent.ON_DEATH);var r=SkillEffectRuntime.Execute(s,s.actors[0],new SkillEffectRequest{kind=SkillEffectKind.RESOURCE_CHANGE,sourceId="A",hpDelta=-999,triggerRegistrations=new[]{reg}});Assert.IsTrue(r.ok);Assert.AreEqual(0,s.actors[0].hp);Assert.IsFalse(s.actors[0].alive);Assert.AreEqual(1,r.triggerDispatches.Count);Assert.AreEqual(TriggerEvent.ON_DEATH,r.triggerDispatches[0].trigger);Assert.AreEqual(1,r.reactiveExecuted);}
  static TriggerRegistration R(string id,TriggerEvent e)=>new TriggerRegistration{id=id,trigger=e,activationChance=1};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S",tick=4};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=10,maxMp=10,alive=true});return s;}
 }
}
#endif
