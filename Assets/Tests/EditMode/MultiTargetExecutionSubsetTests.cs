#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class MultiTargetExecutionSubsetTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  [Test] public void ExecutesOnlyRevalidatedSubsetAgainstCanonicalC02()
  {
   var s=Snapshot();var r=Reservation();s.actionReservations.Add(r);s.actors.Find(x=>x.actorId=="B").alive=false;s.actors.Find(x=>x.actorId=="B").hp=0;var rng=new Rng();
   var result=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=r,executionTargetIds=new[]{"C"},buildAttack=id=>Attack(id)},rng,rng,rng);
   Assert.IsTrue(result.ok,result.reason);CollectionAssert.AreEqual(new[]{"C"},result.executedTargetIds);Assert.AreEqual(90,s.actors.Find(x=>x.actorId=="C").hp);CollectionAssert.AreEqual(new[]{"B","C"},r.fixedTargetIds);Assert.AreSame(r,s.actionReservations[0]);
  }
  [Test] public void RejectsExecutionTargetOutsideFrozenC02()
  {
   var s=Snapshot();var r=Reservation();s.actionReservations.Add(r);var rng=new Rng();
   var result=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=r,executionTargetIds=new[]{"X"},buildAttack=id=>Attack(id)},rng,rng,rng);
   Assert.IsFalse(result.ok);Assert.AreEqual("MULTI_TARGET_TARGET_NOT_RESERVED",result.reason);
  }
  static ActionReservationSaveRecord Reservation()=>new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=new List<string>{"B","C"}};
  static BattleAttackProposal Attack(string id)=>new BattleAttackProposal{reservationId="R",sourceId="A",targetId=id,skillId="SK",accuracy=999,baseDamage=10};
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="BT",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",teamId="E",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="E",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B","C"});return s;}
 }
}
#endif
