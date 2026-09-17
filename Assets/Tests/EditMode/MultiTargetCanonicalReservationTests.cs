#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class MultiTargetCanonicalReservationTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  [Test] public void FilteredExecutionReservation_UsesCanonicalC02WithoutDuplicateFailure(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B","C"});var canonical=new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=new List<string>{"B","C"}};s.actionReservations.Add(canonical);var filtered=new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=new List<string>{"C"}};var rng=new Rng();var result=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=filtered,buildAttack=id=>new BattleAttackProposal{hitCount=1,baseDamage=10,accuracy=999}},rng,rng,rng);Assert.IsTrue(result.ok,result.reason);CollectionAssert.AreEqual(new[]{"C"},result.executedTargetIds);Assert.AreSame(canonical,result.targetResults[0].reservation);Assert.AreEqual(100,s.actors[1].hp);Assert.AreEqual(90,s.actors[2].hp);}
 }
}
#endif
