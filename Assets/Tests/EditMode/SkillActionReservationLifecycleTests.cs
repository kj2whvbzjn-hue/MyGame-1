#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionReservationLifecycleTests
 {
  sealed class Rng:IRandomSource{readonly double v;public Rng(double v){this.v=v;}public double Next01(string purpose)=>v;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="T",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","T"});return s;}
  static SkillDefinition Skill()=>new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=3,castTicks=0,cooldownTicks=2};
  static SkillActionRequest Request()=>new SkillActionRequest{skill=Skill(),attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="SK",hitCount=1,baseDamage=5,accuracy=999,evasion=0,criticalRatePercent=0},fixedTargetIds=new System.Collections.Generic.List<string>{"T"}};
  [Test] public void InstantSuccess_RemovesReservation(){var s=Snapshot();var r=SkillActionTransaction.Execute(s,Request(),new Rng(.99),new Rng(0),new Rng(.99));Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(0,s.actionReservations.Count);Assert.AreEqual(0,s.actors[0].actionGauge);}
  [Test] public void TargetDiesBeforeEffect_FizzlesAndRemovesReservation(){var s=Snapshot();var req=Request();req.skill.castTicks=2;var started=SkillActionTransaction.Execute(s,req,new Rng(.99),new Rng(0),new Rng(.99));Assert.IsTrue(started.castingStarted);var a=s.actors[0];s.actors[1].hp=0;s.actors[1].alive=false;a.cast.active=false;a.cast.remainingTicks=0;var attack=req.attack;var completed=SkillActionTransaction.CompleteCast(s,a,req.skill,attack,new Rng(.99),new Rng(0),new Rng(.99));Assert.IsTrue(completed.ok);Assert.IsTrue(completed.executionSkipped);Assert.AreEqual(0,s.actionReservations.Count);Assert.AreEqual(50,a.actionGauge);Assert.AreEqual(20,a.mp);Assert.AreEqual(0,a.cooldowns.Count);}
 }
}
#endif
