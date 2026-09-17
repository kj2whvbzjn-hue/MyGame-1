#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillTargetRuntimeTests
 {
  sealed class Rng:IRandomSource{public double value;public int calls;public double Next01(string purpose){Assert.AreEqual("TARGET_SELECTION",purpose);calls++;return value;}}
  [Test] public void All_ReturnsDeterministicBattleOrder(){var s=S();var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.ALL,candidateIds=new[]{"C","B"}},null);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"B","C"},r.targetIds);}
  [Test] public void RandomCount_SelectsWithReplacementAndPreservesDuplicateDraws(){var s=S();var rng=new Rng{value=.9};var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.RANDOM,randomCount=2,candidateIds=new[]{"B","C"}},rng);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"C","C"},r.targetIds);Assert.AreEqual(2,rng.calls);}
  [Test] public void Corpse_SelectsDeadOnly(){var s=S();s.actors[1].hp=0;s.actors[1].alive=false;var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.CORPSE,range=SkillTargetRange.ALL,candidateIds=new[]{"B","C"}},null);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"B"},r.targetIds);}
  [Test] public void Single_RejectsTargetOutsideEligibleSet(){var s=S();var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",selectedTargetId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.SINGLE,candidateIds=new[]{"A","B"}},null);Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_TARGET_SINGLE_INVALID",r.reason);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B","C"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
