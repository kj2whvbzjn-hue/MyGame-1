#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickCastCompletionTests
 {
  sealed class Rng:IRandomSource{public double Next01(string p)=>0;}
  [Test] public void CompletedCast_ReusesReservationAndExecutesFixedTarget()
  {
   var s=Snapshot();var a=s.actors[0];a.cast=new CastSaveRecord{reservationId="R",skillId="S",targetId="B",remainingTicks=1,active=true,fixedTargetIds=new System.Collections.Generic.List<string>{"B"}};s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>{"B"}});
   var o=Options();var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(a.cast.active);Assert.AreEqual(1,r.castsCompleted.Count);Assert.AreEqual(9,s.actors[1].hp);Assert.AreEqual(0,a.actionGauge);Assert.AreEqual(1,s.actionReservations.Count);
  }
  [Test] public void CompletedCast_WithDeadFixedTarget_FizzlesAndConsumesHalfGauge()
  {
   var s=Snapshot();var a=s.actors[0];s.actors[1].alive=false;s.actors[1].hp=0;a.cast=new CastSaveRecord{reservationId="R",skillId="S",targetId="B",remainingTicks=1,active=true,fixedTargetIds=new System.Collections.Generic.List<string>{"B"}};s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>{"B"}});var r=BattleTickRuntime.Advance(s,Options());Assert.IsTrue(r.ok);Assert.AreEqual(50,a.actionGauge);Assert.AreEqual(0,r.castsCompleted.Count);StringAssert.Contains("TARGET_INVALID_AT_EFFECT_START",r.actionsSkipped[0]);
  }
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B"});s.actors.Add(new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=10,maxHp=10});return s;}
  static BattleTickOptions Options()=>new BattleTickOptions{criticalRng=new Rng(),hitRng=new Rng(),blockRng=new Rng(),resolveSkillById=id=>new SkillDefinition{id=id},buildReservedAttack=(a,r,s)=>new BattleAttackProposal{reservationId=r.reservationId,sourceId=a.actorId,targetId=r.fixedTargetIds[0],skillId=s.id,hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=1}};
 }
}
#endif
