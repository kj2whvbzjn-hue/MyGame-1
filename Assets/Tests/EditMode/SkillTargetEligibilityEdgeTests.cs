#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillTargetEligibilityEdgeTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="T1",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",teamId=null,hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="T2",hp=10,maxHp=10,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B","C"});return s;}
  [Test] public void Enemy_DoesNotTreatMissingTeamAsOpponent(){var s=Snapshot();var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.ALL},null);Assert.IsTrue(r.ok);CollectionAssert.AreEqual(new[]{"C"},r.targetIds);}
  [Test] public void DeadSelf_IsNotValidNormalSelfTarget(){var s=Snapshot();s.actors[0].alive=false;s.actors[0].hp=0;var r=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.SELF,range=SkillTargetRange.SINGLE},null);Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_TARGET_SELF_NOT_ALIVE",r.reason);}
 }
}
#endif
