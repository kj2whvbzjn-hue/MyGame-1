#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionDuplicateReservationTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",teamId="E",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",teamId="E",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B","C"});return s;}
  [Test] public void Precheck_PreservesDuplicateFixedTargetsInExactOrder(){var s=Snapshot();var skill=new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=0,castTicks=2};var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="C",skillId="SK"},fixedTargetIds=new List<string>{"C","B","C"}};var r=SkillActionTransaction.Precheck(s,req);Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"C","B","C"},r.reservation.fixedTargetIds);}
 }
}
#endif
