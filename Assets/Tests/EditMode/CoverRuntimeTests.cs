#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CoverRuntimeTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="T",teamId="P",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="P",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"T","C"});return s;}
  static CoverContract Cover(CoverLifetimeKind life=CoverLifetimeKind.PERSISTENT)=>new CoverContract{id="CV",protectorId="C",protectedActorId="T",lifetime=life,active=true,remainingUses=2,remainingTicks=2};
  [TestCase(SkillTargetRange.SINGLE,true)][TestCase(SkillTargetRange.BACK,true)][TestCase(SkillTargetRange.RANDOM,true)][TestCase(SkillTargetRange.FRONT,false)][TestCase(SkillTargetRange.ALL,false)]public void AppliesOnlyToFormalRanges(SkillTargetRange range,bool covered){var r=CoverRuntime.Resolve(Snapshot(),"T",range,new[]{Cover()});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(covered?"C":"T",r.targetId);}
  [Test] public void RandomDuplicateDrawsConsumeUsesSeparately(){var s=Snapshot();var c=Cover(CoverLifetimeKind.USES);var first=CoverRuntime.Resolve(s,"T",SkillTargetRange.RANDOM,new[]{c});var second=CoverRuntime.Resolve(s,"T",SkillTargetRange.RANDOM,new[]{c});var third=CoverRuntime.Resolve(s,"T",SkillTargetRange.RANDOM,new[]{c});Assert.AreEqual("C",first.targetId);Assert.AreEqual("C",second.targetId);Assert.AreEqual("T",third.targetId);Assert.AreEqual(0,c.remainingUses);Assert.IsFalse(c.active);}
  [Test] public void MultipleCandidatesFailWithoutInventingPriority(){var s=Snapshot();s.actors.Add(new BattleActorSaveRecord{actorId="C2",teamId="P",hp=100,maxHp=100,alive=true});var a=Cover();var b=Cover();b.id="CV2";b.protectorId="C2";var r=CoverRuntime.Resolve(s,"T",SkillTargetRange.SINGLE,new[]{a,b});Assert.IsFalse(r.ok);Assert.AreEqual("COVER_PRIORITY_CONTRACT_REQUIRED",r.reason);}
  [Test] public void DeadProtectorCannotCover(){var s=Snapshot();s.actors[1].alive=false;s.actors[1].hp=0;var r=CoverRuntime.Resolve(s,"T",SkillTargetRange.SINGLE,new[]{Cover()});Assert.IsTrue(r.ok);Assert.AreEqual("T",r.targetId);}
 }
}
#endif
