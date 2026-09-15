#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillTargetReservationRuntimeTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void AllTargets_AreFrozenIntoReservationBeforeExecution(){var s=S();var skill=new SkillDefinition{id="S"};var attack=new BattleAttackProposal{reservationId="R",damageType=DamageType.Physical,accuracy=100,baseDamage=1};var built=SkillTargetReservationRuntime.Build(s,skill,attack,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.ALL,candidateIds=new[]{"C","B"}},new Rng());Assert.IsTrue(built.ok,built.reason);var pre=SkillActionTransaction.Precheck(s,built.action);Assert.IsTrue(pre.ok,pre.reason);CollectionAssert.AreEqual(new[]{"B","C"},pre.reservation.fixedTargetIds);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B","C"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
