#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class CastFrozenTimingTests
 {
  [Test] public void Precheck_FreezesAllEffectiveTimingValues(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","B"});var skill=new SkillDefinition{id="SK",resource=SkillResourceKind.MP,resourceCost=10,castTicks=5,cooldownTicks=8};var r=SkillActionTransaction.Precheck(s,new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="SK"},timingModifiers=new SkillTimingModifiers{mpCost=-2.4,castTicks=-1.2,cooldownTicks=1.1}});Assert.IsTrue(r.ok);Assert.IsTrue(r.reservation.hasEffectiveTiming);Assert.AreEqual(8,r.reservation.effectiveMpCost);Assert.AreEqual(4,r.reservation.effectiveCastTicks);Assert.AreEqual(10,r.reservation.effectiveCooldownTicks);Assert.AreEqual(4,r.reservation.completeTick-r.reservation.startTick);}
 }
}
#endif
