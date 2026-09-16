#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class FormationDamageRuntimeTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord();s.actors.Add(new BattleActorSaveRecord{actorId="F",teamId="P",formationRow=0,hp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="R",teamId="P",formationRow=1,hp=100,alive=true});return s;}
  [Test] public void FrontAttacker_IsAlwaysFullDamage(){var s=Snapshot();Assert.AreEqual(1d,FormationDamageRuntime.Resolve(s,"F",SkillTargetRange.SINGLE));Assert.AreEqual(1d,FormationDamageRuntime.Resolve(s,"F",SkillTargetRange.RANDOM));}
  [Test] public void RearAttacker_SingleFrontRandomAreHalf(){var s=Snapshot();Assert.AreEqual(.5d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.SINGLE));Assert.AreEqual(.5d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.FRONT));Assert.AreEqual(.5d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.RANDOM));}
  [Test] public void RearAttacker_BackAndAllAreFullDamage(){var s=Snapshot();Assert.AreEqual(1d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.BACK));Assert.AreEqual(1d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.ALL));}
  [Test] public void RearBecomesFrontAfterFrontlineDies(){var s=Snapshot();s.actors[0].hp=0;s.actors[0].alive=false;Assert.AreEqual(1d,FormationDamageRuntime.Resolve(s,"R",SkillTargetRange.SINGLE));}
 }
}
#endif
