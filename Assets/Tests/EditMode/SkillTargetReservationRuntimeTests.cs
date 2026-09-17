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
  sealed class Rng:IRandomSource{readonly double value;public int calls;public Rng(double value=0){this.value=value;}public double Next01(string purpose){Assert.AreEqual(SkillTargetRuntime.RngPurpose,purpose);calls++;return value;}}
  [Test] public void AllTargets_AreFrozenIntoReservationBeforeExecution(){var s=S();var skill=new SkillDefinition{id="S"};var attack=new BattleAttackProposal{reservationId="R",damageType=DamageType.Physical,accuracy=100,baseDamage=1};var built=SkillTargetReservationRuntime.Build(s,skill,attack,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.ALL,candidateIds=new[]{"C","B"}},new Rng());Assert.IsTrue(built.ok,built.reason);var pre=SkillActionTransaction.Precheck(s,built.action);Assert.IsTrue(pre.ok,pre.reason);CollectionAssert.AreEqual(new[]{"B","C"},pre.reservation.fixedTargetIds);Assert.IsTrue(pre.reservation.hasTargetContract);Assert.AreEqual("ENEMY",pre.reservation.targetCategory);Assert.AreEqual("ALL",pre.reservation.targetRange);}
  [Test] public void RandomTargets_AreExpandedOnceAndDuplicatesAreFrozen(){var s=S();s.actors.Find(x=>x.actorId=="A").teamId="P";s.actors.Find(x=>x.actorId=="B").teamId="E";s.actors.Find(x=>x.actorId=="C").teamId="E";var skill=new SkillDefinition{id="S"};var attack=new BattleAttackProposal{reservationId="R",damageType=DamageType.Physical,accuracy=100,baseDamage=1};var rng=new Rng(.99);var built=SkillTargetReservationRuntime.Build(s,skill,attack,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ENEMY,range=SkillTargetRange.RANDOM,randomCount=2,candidateIds=new[]{"B","C"}},rng);Assert.IsTrue(built.ok,built.reason);Assert.AreEqual(2,rng.calls);CollectionAssert.AreEqual(new[]{"C","C"},built.targets.targetIds);var pre=SkillActionTransaction.Precheck(s,built.action);Assert.IsTrue(pre.ok,pre.reason);CollectionAssert.AreEqual(new[]{"C","C"},pre.reservation.fixedTargetIds);Assert.AreEqual("RANDOM",pre.reservation.targetRange);}
  [Test] public void ExcludeSelf_IsPersistedIntoFormalReservation(){var s=S();s.actors.Find(x=>x.actorId=="A").teamId="P";s.actors.Find(x=>x.actorId=="B").teamId="P";var skill=new SkillDefinition{id="S"};var attack=new BattleAttackProposal{reservationId="R",damageType=DamageType.Physical,accuracy=100,baseDamage=1};var built=SkillTargetReservationRuntime.Build(s,skill,attack,new SkillTargetRequest{sourceId="A",category=SkillTargetCategory.ALLY,range=SkillTargetRange.ALL,excludeSelf=true,candidateIds=new[]{"A","B"}},new Rng());Assert.IsTrue(built.ok,built.reason);var pre=SkillActionTransaction.Precheck(s,built.action);Assert.IsTrue(pre.ok,pre.reason);Assert.IsTrue(pre.reservation.targetExcludeSelf);CollectionAssert.AreEqual(new[]{"B"},pre.reservation.fixedTargetIds);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B","C"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
