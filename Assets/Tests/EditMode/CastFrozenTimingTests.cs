#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CastFrozenTimingTests
 {
  sealed class Rng:IRandomSource{readonly double v;public Rng(double v){this.v=v;}public double Next01(string purpose)=>v;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B"});return s;}
  [Test] public void Precheck_FreezesAllEffectiveTimingValues(){var s=Snapshot();var skill=new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=10,castTicks=5,cooldownTicks=8};var r=SkillActionTransaction.Precheck(s,new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="SK"},timingModifiers=new SkillTimingModifiers{mpCost=-2.4,castTicks=-1.2,cooldownTicks=1.1}});Assert.IsTrue(r.ok);Assert.IsTrue(r.reservation.hasEffectiveTiming);Assert.AreEqual(8,r.reservation.effectiveMpCost);Assert.AreEqual(4,r.reservation.effectiveCastTicks);Assert.AreEqual(10,r.reservation.effectiveCooldownTicks);Assert.AreEqual(4,r.reservation.completeTick-r.reservation.startTick);}
  [Test] public void CompleteCast_ConsumesFrozenMpAndCooldownWithoutModifiers(){var s=Snapshot();var actor=s.actors[0];var skill=new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=10,castTicks=5,cooldownTicks=8};var attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="SK",hitCount=1,baseDamage=1,accuracy=999,evasion=0,criticalRatePercent=0};var started=SkillActionTransaction.Execute(s,new SkillActionRequest{skill=skill,attack=attack,fixedTargetIds=new System.Collections.Generic.List<string>{"B"},timingModifiers=new SkillTimingModifiers{mpCost=-2.4,castTicks=-1.2,cooldownTicks=1.1}},new Rng(.99),new Rng(0),new Rng(.99));Assert.IsTrue(started.castingStarted);Assert.AreEqual(20,actor.mp);Assert.AreEqual(4,actor.cast.remainingTicks);actor.cast.active=false;actor.cast.remainingTicks=0;var completed=SkillActionTransaction.CompleteCast(s,actor,skill,attack,new Rng(.99),new Rng(0),new Rng(.99));Assert.IsTrue(completed.ok,completed.reason);Assert.AreEqual(12,actor.mp);Assert.AreEqual(10,actor.cooldowns.Find(x=>x.skillId=="SK").remainingTicks);Assert.AreEqual(0,actor.actionGauge);Assert.AreEqual(0,s.actionReservations.Count);}
 }
}
#endif
