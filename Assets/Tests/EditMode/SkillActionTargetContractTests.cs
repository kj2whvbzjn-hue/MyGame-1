#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionTargetContractTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",formationRow=0,hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="F",teamId="E",formationRow=0,hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="R",teamId="E",formationRow=1,hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","F","R"});return s;}
  static SkillDefinition Skill()=>new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=3,castTicks=2,cooldownTicks=2};
  static BattleAttackProposal Attack(string target)=>new BattleAttackProposal{reservationId="Q",sourceId="A",targetId=target,skillId="SK",hitCount=1,baseDamage=5,accuracy=999};
  [Test] public void Precheck_RejectsRearEnemyForSingle(){var s=Snapshot();var req=new SkillActionRequest{skill=Skill(),attack=Attack("R"),fixedTargetIds=new List<string>{"R"},hasTargetContract=true,targetCategory=SkillTargetCategory.ENEMY,targetRange=SkillTargetRange.SINGLE};var r=SkillActionTransaction.Precheck(s,req);Assert.IsFalse(r.ok);Assert.AreEqual("TARGET_INVALID_AT_PRECHECK",r.reason);}
  [Test] public void Completion_DropsTargetThatMovedOutOfSingleRange_NoReacquire(){var s=Snapshot();var skill=Skill();var req=new SkillActionRequest{skill=skill,attack=Attack("F"),fixedTargetIds=new List<string>{"F"},hasTargetContract=true,targetCategory=SkillTargetCategory.ENEMY,targetRange=SkillTargetRange.SINGLE};var rng=new Rng();var started=SkillActionTransaction.Execute(s,req,rng,rng,rng);Assert.IsTrue(started.castingStarted);Assert.IsTrue(started.reservation.hasTargetContract);Assert.AreEqual("ENEMY",started.reservation.targetCategory);Assert.AreEqual("SINGLE",started.reservation.targetRange);s.actors[1].formationRow=1;s.actors[2].formationRow=0;var a=s.actors[0];a.cast.active=false;a.cast.remainingTicks=0;var done=SkillActionTransaction.CompleteCast(s,a,skill,Attack("F"),rng,rng,rng);Assert.IsTrue(done.ok,done.reason);Assert.IsTrue(done.executionSkipped);Assert.AreEqual(100,s.actors[2].hp);Assert.AreEqual(20,a.mp);Assert.AreEqual(0,a.cooldowns.Count);Assert.AreEqual(50,a.actionGauge);Assert.AreEqual(0,s.actionReservations.Count);}
 }
}
#endif
