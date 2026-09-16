#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class FormationDamagePipelineTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="F",teamId="P",formationRow=0,hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="R",teamId="P",formationRow=1,hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="E",teamId="E",formationRow=0,hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"F","R","E"});return s;}
  static BattleAttackProposal Attack(string id)=>new BattleAttackProposal{reservationId=id,sourceId="R",targetId="E",skillId="SK",hitCount=1,baseDamage=20,accuracy=999,criticalRatePercent=0};
  static ActionReservationSaveRecord Reservation(string id,string range)=>new ActionReservationSaveRecord{reservationId=id,actorId="R",skillId="SK",fixedTargetIds=new System.Collections.Generic.List<string>{"E"},hasTargetContract=true,targetCategory="ENEMY",targetRange=range};
  [Test] public void RearSingle_IsHalfDamageInExecutor(){var s=Snapshot();var rng=new Rng();var p=Attack("R1");var r=BattleStepExecutor.ExecuteAttack(s,p,Reservation("R1","SINGLE"),rng,rng,rng);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(10d,r.resolvedHit.perHitDamage);Assert.AreEqual(90,s.actors[2].hp);}
  [Test] public void RearBack_IsFullDamageInExecutor(){var s=Snapshot();var rng=new Rng();var p=Attack("R2");var r=BattleStepExecutor.ExecuteAttack(s,p,Reservation("R2","BACK"),rng,rng,rng);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(20d,r.resolvedHit.perHitDamage);Assert.AreEqual(80,s.actors[2].hp);}
  [Test] public void RearPromotedToFront_IsFullDamageAtExecution(){var s=Snapshot();s.actors[0].hp=0;s.actors[0].alive=false;var rng=new Rng();var p=Attack("R3");var r=BattleStepExecutor.ExecuteAttack(s,p,Reservation("R3","SINGLE"),rng,rng,rng);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(20d,r.resolvedHit.perHitDamage);Assert.AreEqual(80,s.actors[2].hp);}
 }
}
#endif
