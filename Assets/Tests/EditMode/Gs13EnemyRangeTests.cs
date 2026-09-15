#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs13EnemyRangeTests
 {
  sealed class FixedRng:IRandomSource{readonly double v;public FixedRng(double v){this.v=v;}public double Next01(string purpose){Assert.AreEqual(SkillTargetRuntime.RngPurpose,purpose);return v;}}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",formationRow=1,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="F1",teamId="E",formationRow=0,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="F2",teamId="E",formationRow=0,hp=10,maxHp=10});s.actors.Add(new BattleActorSaveRecord{actorId="B1",teamId="E",formationRow=1,hp=10,maxHp=10});s.fixedActorOrder.AddRange(new[]{"A","F1","F2","B1"});return s;}
  [Test] public void Single_IsFrontlineOnly(){var r=SkillTargetRuntime.Resolve(S(),new SkillTargetRequest{sourceId="A",selectedTargetId="B1",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.SINGLE},null);Assert.IsFalse(r.ok);}
  [Test] public void Back_IsSingleAcrossBothRows(){var r=SkillTargetRuntime.Resolve(S(),new SkillTargetRequest{sourceId="A",selectedTargetId="F2",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.BACK},null);Assert.IsTrue(r.ok);CollectionAssert.AreEqual(new[]{"F2"},r.targetIds);}
  [Test] public void Random_PreservesDuplicateDraws(){var r=SkillTargetRuntime.Resolve(S(),new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.RANDOM,randomCount=3},new FixedRng(.99));Assert.IsTrue(r.ok);CollectionAssert.AreEqual(new[]{"B1","B1","B1"},r.targetIds);}
 }
}
#endif
