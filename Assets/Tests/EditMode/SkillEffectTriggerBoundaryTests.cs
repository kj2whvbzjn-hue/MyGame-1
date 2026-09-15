#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillEffectTriggerBoundaryTests
 {
  [Test] public void AppliedStatus_RemainsCommittedWhenReactiveLimitStopsQueue(){var s=S();var a=s.actors[0];var action=new TriggerActionContext{actionId="X",activationLimit=1,activationCount=1};var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.APPLY,lifecycleKind=EffectLifecycleKind.STATUS,instanceId="ST",effectId="STUN",sourceId="SRC",durationTicks=3,triggerActionContext=action,triggerRegistrations=new[]{new TriggerRegistration{id="P",trigger=TriggerEvent.ON_STATUS_APPLIED,activationChance=1}}});Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(r.triggerOk);Assert.AreEqual(TriggerActivationRuntime.LimitReached,r.triggerReason);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual("STUN",a.appliedEffects[0].effectId);}
  [Test] public void GenericEvent_CanPreserveOriginalActionSource(){var s=S();var a=s.actors[0];var r=SkillEffectRuntime.Execute(s,a,new SkillEffectRequest{kind=SkillEffectKind.APPLY,lifecycleKind=EffectLifecycleKind.STATUS,instanceId="ST",effectId="STUN",sourceId="PASSIVE_OWNER",actionSourceId="ORIGINAL_ATTACKER",durationTicks=3});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual("PASSIVE_OWNER",r.triggerDispatches[0].context.sourceId);Assert.AreEqual("ORIGINAL_ATTACKER",r.triggerDispatches[0].context.actionSourceId);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
