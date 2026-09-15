#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionGaugeTransactionTests
 {
  sealed class Rng:IRandomSource{public double Next01(string p)=>0;}
  [Test] public void CastReservation_DoesNotConsumeGauge(){var s=Snapshot();var q=Request(2);var r=SkillActionTransaction.Execute(s,q,new Rng(),new Rng(),new Rng());Assert.IsTrue(r.castingStarted);Assert.AreEqual(100,s.actors[0].actionGauge);}
  [Test] public void InstantSuccess_ConsumesFullGauge(){var s=Snapshot();var r=SkillActionTransaction.Execute(s,Request(0),new Rng(),new Rng(),new Rng());Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(0,s.actors[0].actionGauge);}
  [Test] public void CompletionWithDeadFixedTarget_ConsumesHalfGaugeWithoutCostOrCooldown(){var s=Snapshot();var q=Request(1);var started=SkillActionTransaction.Execute(s,q,new Rng(),new Rng(),new Rng());Assert.IsTrue(started.castingStarted);s.actors[1].hp=0;s.actors[1].alive=false;s.actors[0].cast.active=false;s.actors[0].cast.remainingTicks=0;var r=SkillActionTransaction.CompleteCast(s,s.actors[0],q.skill,q.attack,new Rng(),new Rng(),new Rng());Assert.IsTrue(r.executionSkipped);Assert.AreEqual(50,s.actors[0].actionGauge);Assert.AreEqual(10,s.actors[0].mp);Assert.AreEqual(0,s.actors[0].cooldowns.Count);}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord();s.fixedActorOrder.AddRange(new[]{"A","B"});s.actors.Add(new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,mp=10,maxMp=10,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=10,maxHp=10});return s;}
  static SkillActionRequest Request(int cast)=>new SkillActionRequest{skill=new SkillDefinition{id="S",castTicks=cast,resource=SkillResourceKind.MP,resourceCost=2,cooldownTicks=3},attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=1}};
 }
}
#endif
