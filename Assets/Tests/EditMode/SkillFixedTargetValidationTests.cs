#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillFixedTargetValidationTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord();s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",formationRow=0,hp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="F",teamId="E",formationRow=0,hp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="R",teamId="E",formationRow=1,hp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","F","R"});return s;}
  [Test] public void Completion_RemovesDeadWithoutReplenishing(){var s=Snapshot();s.actors[1].hp=0;s.actors[1].alive=false;var r=SkillFixedTargetValidation.Validate(s,new SkillFixedTargetValidationRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.RANDOM,fixedTargetIds=new[]{"F","R","F"}});Assert.IsTrue(r.ok);CollectionAssert.AreEqual(new[]{"R"},r.validTargetIds);}
  [Test] public void DuplicateFixedTargetsRemainDuplicate(){var s=Snapshot();var r=SkillFixedTargetValidation.Validate(s,new SkillFixedTargetValidationRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.RANDOM,fixedTargetIds=new[]{"R","R"}});CollectionAssert.AreEqual(new[]{"R","R"},r.validTargetIds);}
  [Test] public void EnemySingleBecomesInvalidWhenFrontlineChanges(){var s=Snapshot();var r=SkillFixedTargetValidation.Validate(s,new SkillFixedTargetValidationRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.SINGLE,fixedTargetIds=new[]{"R"}});Assert.AreEqual(0,r.validTargetIds.Count);}
  [Test] public void CorpseAcceptsOnlyDeadFixedTarget(){var s=Snapshot();s.actors[2].hp=0;s.actors[2].alive=false;var r=SkillFixedTargetValidation.Validate(s,new SkillFixedTargetValidationRequest{sourceId="A",category=SkillTargetCategory.CORPSE,range=SkillTargetRange.SINGLE,fixedTargetIds=new[]{"F","R"}});CollectionAssert.AreEqual(new[]{"R"},r.validTargetIds);}
 }
}
#endif
