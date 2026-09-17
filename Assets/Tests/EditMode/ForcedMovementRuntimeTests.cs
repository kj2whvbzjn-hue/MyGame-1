#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class ForcedMovementRuntimeTests
 {
  [Test] public void EstablishedMovementInterruptsCastBeforeMoveThenUpdatesAndRequestsAi()
  {
   var s=Snapshot();var a=s.actors[0];var order=new List<string>();
   var r=ForcedMovementRuntime.Execute(s,new ForcedMovementRequest{targetId="A",destinationFormationIndex=2,canEstablish=(snap,actor,to)=>{order.Add("establish");Assert.IsTrue(actor.cast.active);Assert.AreEqual(0,actor.formationRow);return true;},updateRangeAndTargets=(snap,actor)=>{order.Add("update");Assert.IsFalse(actor.cast.active);Assert.AreEqual(2,actor.formationRow);},requestAiReevaluation=(snap,actor)=>{order.Add("ai");Assert.AreEqual(2,actor.formationRow);}});
   Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(r.established);Assert.IsTrue(r.castInterrupted);Assert.IsTrue(r.moved);CollectionAssert.AreEqual(new[]{"establish","update","ai"},order);Assert.AreEqual(2,a.formationRow);Assert.IsFalse(a.cast.active);Assert.AreEqual(0,s.actionReservations.Count);
  }
  [Test] public void FailedEstablishmentDoesNotInterruptOrMove()
  {
   var s=Snapshot();var a=s.actors[0];var r=ForcedMovementRuntime.Execute(s,new ForcedMovementRequest{targetId="A",destinationFormationIndex=3,canEstablish=(snap,actor,to)=>false});
   Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(r.established);Assert.IsFalse(r.castInterrupted);Assert.IsFalse(r.moved);Assert.AreEqual(0,a.formationRow);Assert.IsTrue(a.cast.active);Assert.AreEqual(1,s.actionReservations.Count);
  }
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",hp=100,maxHp=100,alive=true,formationRow=0,cast=new CastSaveRecord{reservationId="R",skillId="SK",active=true,remainingTicks=3}});s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK"});s.fixedActorOrder.Add("A");return s;}
 }
}
#endif
