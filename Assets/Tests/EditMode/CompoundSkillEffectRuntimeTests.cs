#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CompoundSkillEffectRuntimeTests
 {
  [Test] public void ExecutesEffectsByDeclaredOrder()
  {
   var s=Snapshot(false);var t=s.actors[0];
   var r=CompoundSkillEffectRuntime.Execute(s,t,new[]{
    new CompoundSkillEffectEntry{order=20,effect=new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=50}},
    new CompoundSkillEffectEntry{order=10,effect=new SkillEffectRequest{kind=SkillEffectKind.REVIVE,power=25}}
   });
   Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(2,r.executedCount);Assert.AreEqual(75,t.hp);
  }
  [Test] public void EqualOrderPreservesDefinitionSequence()
  {
   var s=Snapshot(true);var t=s.actors[0];t.hp=50;
   var r=CompoundSkillEffectRuntime.Execute(s,t,new[]{
    new CompoundSkillEffectEntry{order=10,effect=new SkillEffectRequest{kind=SkillEffectKind.RESOURCE_CHANGE,hpDelta=-40}},
    new CompoundSkillEffectEntry{order=10,effect=new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=20}}
   });
   Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(30,t.hp);
  }
  [Test] public void FailureStopsRemainingEffectsWithoutReordering()
  {
   var s=Snapshot(false);var t=s.actors[0];
   var r=CompoundSkillEffectRuntime.Execute(s,t,new[]{
    new CompoundSkillEffectEntry{order=10,effect=new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=50}},
    new CompoundSkillEffectEntry{order=20,effect=new SkillEffectRequest{kind=SkillEffectKind.REVIVE,power=25}}
   });
   Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_EFFECT_HEAL_TARGET_DEAD",r.reason);Assert.AreEqual(0,r.executedCount);Assert.AreEqual(0,t.hp);Assert.IsFalse(t.alive);
  }
  static BattleSnapshotSaveRecord Snapshot(bool alive){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="T",hp=alive?100:0,maxHp=100,mp=0,maxMp=20,alive=alive});s.fixedActorOrder.Add("T");return s;}
 }
}
#endif
