#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class HpCostPrecheckTests
 {
  [Test] public void HpCostSkill_IsRejectedBeforeReservationOrCast(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B"});var skill=new SkillDefinition{id="HP_SK",resource=SkillResourceKind.HP,resourceCost=10,castTicks=5};var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="HP_SK"}};var pre=SkillActionTransaction.Precheck(s,req);Assert.IsFalse(pre.ok);Assert.AreEqual("HP_COST_NOT_SUPPORTED_BY_FORMAL_RUNTIME",pre.reason);var exec=SkillActionTransaction.Execute(s,req,null,null,null);Assert.IsFalse(exec.ok);Assert.AreEqual(0,s.actionReservations.Count);Assert.IsNull(s.actors[0].cast);Assert.AreEqual(100,s.actors[0].hp);}
 }
}
#endif
