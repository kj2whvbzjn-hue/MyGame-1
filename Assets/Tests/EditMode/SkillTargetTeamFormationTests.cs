#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillTargetTeamFormationTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="T1",formationRow=0,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="B",teamId="T1",formationRow=1,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="T2",formationRow=0,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="D",teamId="T2",formationRow=1,hp=10,maxHp=10});s.fixedActorOrder.AddRange(new[]{"A","B","C","D"});return s;}
  [Test] public void AllyAndEnemy_AreSeparatedByTeam(){var s=Snapshot();var ally=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ALLY,range=SkillTargetRange.ALL},null);CollectionAssert.AreEqual(new[]{"B"},ally.targetIds);var enemy=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.ALL},null);CollectionAssert.AreEqual(new[]{"C","D"},enemy.targetIds);}
  [Test] public void FrontAndBack_ReturnFormationRows(){var s=Snapshot();var front=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.FRONT},null);CollectionAssert.AreEqual(new[]{"C"},front.targetIds);var back=SkillTargetRuntime.Resolve(s,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.BACK},null);CollectionAssert.AreEqual(new[]{"D"},back.targetIds);}
 }
}
#endif
