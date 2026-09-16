#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CoverAttackPipelineTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="E",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="T",teamId="P",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="P",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","T","C"});return s;}
  static ActionReservationSaveRecord Reservation(string range,List<string> ids)=>new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="SK",fixedTargetIds=ids,hasTargetContract=true,targetCategory="ENEMY",targetRange=range};
  static BattleAttackProposal Attack()=>new BattleAttackProposal{hitCount=1,baseDamage=10,accuracy=999,criticalRatePercent=0};
  [Test] public void SingleCoverRedirectsDamageToProtector(){var s=Snapshot();var rng=new Rng();var cover=new CoverContract{id="CV",protectorId="C",protectedActorId="T",lifetime=CoverLifetimeKind.PERSISTENT,active=true};var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=Reservation("SINGLE",new List<string>{"T"}),buildAttack=_=>Attack(),coverContracts=new[]{cover}},rng,rng,rng);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual("C",r.executedTargetIds[0]);Assert.AreEqual(100,s.actors[1].hp);Assert.AreEqual(90,s.actors[2].hp);}
  [Test] public void RandomDuplicateChecksAndConsumesCoverPerDraw(){var s=Snapshot();var rng=new Rng();var cover=new CoverContract{id="CV",protectorId="C",protectedActorId="T",lifetime=CoverLifetimeKind.USES,active=true,remainingUses=1};var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=Reservation("RANDOM",new List<string>{"T","T"}),buildAttack=_=>Attack(),coverContracts=new[]{cover}},rng,rng,rng);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"C","T"},r.executedTargetIds);Assert.AreEqual(90,s.actors[1].hp);Assert.AreEqual(90,s.actors[2].hp);Assert.AreEqual(0,cover.remainingUses);}
  [Test] public void FrontDoesNotCover(){var s=Snapshot();var rng=new Rng();var cover=new CoverContract{id="CV",protectorId="C",protectedActorId="T",lifetime=CoverLifetimeKind.PERSISTENT,active=true};var r=MultiTargetBattleExecutor.Execute(s,new MultiTargetAttackRequest{reservation=Reservation("FRONT",new List<string>{"T"}),buildAttack=_=>Attack(),coverContracts=new[]{cover}},rng,rng,rng);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual("T",r.executedTargetIds[0]);Assert.AreEqual(90,s.actors[1].hp);Assert.AreEqual(100,s.actors[2].hp);}
 }
}
#endif
