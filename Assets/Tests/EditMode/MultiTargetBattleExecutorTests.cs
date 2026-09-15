#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class MultiTargetBattleExecutorTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void FixedTargets_ExecuteInBattleOrder_NotReservationOrder(){var s=S();var reservation=R();reservation.fixedTargetIds.AddRange(new[]{"C","B"});s.actionReservations.Add(reservation);var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=reservation,buildAttack=id=>P()},new Rng(),new Rng(),null);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"B","C"},r.executedTargetIds);Assert.AreEqual(2,s.resolvedHits.Count);Assert.AreEqual("B",s.resolvedHits[0].targetId);Assert.AreEqual("C",s.resolvedHits[1].targetId);}
  [Test] public void DeadFixedTarget_IsRemovedWithoutReacquire(){var s=S();s.actors[1].hp=0;s.actors[1].alive=false;var reservation=R();reservation.fixedTargetIds.AddRange(new[]{"B","C"});s.actionReservations.Add(reservation);var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=reservation,buildAttack=id=>P()},new Rng(),new Rng(),null);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"B"},r.skippedTargetIds);CollectionAssert.AreEqual(new[]{"C"},r.executedTargetIds);}
  [Test] public void MultiHit_IsResolvedPerTarget(){var s=S();var reservation=R();reservation.fixedTargetIds.AddRange(new[]{"B","C"});s.actionReservations.Add(reservation);var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=reservation,buildAttack=id=>{var p=P();p.hitCount=2;return p;}},new Rng(),new Rng(),null);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(4,s.resolvedHits.Count);Assert.AreEqual(8,s.actors[1].hp);Assert.AreEqual(8,s.actors[2].hp);}
  static BattleAttackProposal P()=>new BattleAttackProposal{damageType=DamageType.Physical,accuracy=100,baseDamage=1};
  static ActionReservationSaveRecord R()=>new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>()};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="X",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B","C"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
