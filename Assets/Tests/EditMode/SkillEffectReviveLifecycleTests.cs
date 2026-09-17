#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillEffectReviveLifecycleTests
 {
  [Test] public void ReviveEffect_UsesFormalLifecycleAndAppliesPostReviveEffect()
  {
   var s=Snapshot();var t=s.actors[0];t.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="OLD",effectId="POISON",kind="DOT",remainingTicks=5});t.barrierLayers.Add(new BarrierLayerSaveRecord{id="B",remaining=10});t.cast=new CastSaveRecord{active=true,remainingTicks=3};var post=false;
   var r=SkillEffectRuntime.Execute(s,t,new SkillEffectRequest{kind=SkillEffectKind.REVIVE,power=25,reviveMp=7,reviveEffect=a=>{post=true;Assert.AreEqual(25,a.hp);Assert.AreEqual(7,a.mp);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(0,a.barrierLayers.Count);Assert.IsNull(a.cast);}});
   Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(r.revive.revived);Assert.IsTrue(post);Assert.AreEqual(25,t.hp);Assert.AreEqual(7,t.mp);
  }
  [Test] public void NonReviveEffects_DoNotOperateOnCorpse()
  {
   var s=Snapshot();var t=s.actors[0];
   Assert.AreEqual("SKILL_EFFECT_HEAL_TARGET_DEAD",SkillEffectRuntime.Execute(s,t,new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=50}).reason);
   Assert.AreEqual("SKILL_EFFECT_APPLY_TARGET_DEAD",SkillEffectRuntime.Execute(s,t,new SkillEffectRequest{kind=SkillEffectKind.APPLY,instanceId="X",effectId="STUN",lifecycleKind=EffectLifecycleKind.STATUS,durationTicks=1}).reason);
   Assert.AreEqual("SKILL_EFFECT_RESOURCE_TARGET_DEAD",SkillEffectRuntime.Execute(s,t,new SkillEffectRequest{kind=SkillEffectKind.RESOURCE_CHANGE,mpDelta=1}).reason);
  }
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="T",hp=0,maxHp=100,mp=0,maxMp=20,alive=false});s.fixedActorOrder.Add("T");return s;}
 }
}
#endif
