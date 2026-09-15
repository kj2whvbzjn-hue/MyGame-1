#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionTimingIntegrationTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");return s;}
  [Test] public void EffectiveCastTicks_AreFrozenIntoReservation(){var s=Snapshot();var skill=new SkillDefinition{id="SK",resource=SkillResourceKind.MP,resourceCost=10,castTicks=5,cooldownTicks=8};var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="SK"},timingModifiers=new SkillTimingModifiers{castTicks=-2.2}};var r=SkillActionTransaction.Execute(s,req,null,null,null);Assert.IsTrue(r.ok);Assert.IsTrue(r.castingStarted);Assert.AreEqual(3,r.reservation.completeTick-r.reservation.startTick);Assert.AreEqual(3,s.actors[0].cast.remainingTicks);Assert.AreEqual(20,s.actors[0].mp);Assert.AreEqual(0,s.actors[0].cooldowns.Count);}
  [Test] public void EffectiveMpCost_IsValidatedBeforeReservation(){var s=Snapshot();s.actors[0].mp=7;var skill=new SkillDefinition{id="SK",resource=SkillResourceKind.MP,resourceCost=10};var req=new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="SK"},timingModifiers=new SkillTimingModifiers{mpCost=-3.1}};var r=SkillActionTransaction.Precheck(s,req);Assert.IsTrue(r.ok);}
 }
}
#endif
