#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillCastCompletionGaugeTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void CastCompletionWithNoValidFrozenTarget_DoesNotConsumeGaugeMpOrCooldown(){var s=S();var a=s.actors[0];var b=s.actors[1];a.actionGauge=100;a.mp=10;a.cast=new CastSaveRecord{reservationId="R",skillId="S",active=false,remainingTicks=0};s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new List<string>{"B"}});b.hp=0;b.alive=false;var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=3,cooldownTicks=9};var attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,accuracy=100,baseDamage=1};var r=SkillActionTransaction.CompleteCast(s,a,skill,attack,new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(r.executionSkipped);Assert.AreEqual("TARGET_INVALID_AT_EFFECT_START",r.reason);Assert.AreEqual(100,a.actionGauge);Assert.AreEqual(10,a.mp);Assert.AreEqual(0,a.cooldowns.Count);}
  [Test] public void CastCompletionConditionFailure_DoesNotConsumeGauge(){var s=S();var a=s.actors[0];a.actionGauge=100;a.mp=1;a.cast=new CastSaveRecord{reservationId="R",skillId="S",active=false,remainingTicks=0};s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new List<string>{"B"}});var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=3};var attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,accuracy=100,baseDamage=1};var r=SkillActionTransaction.CompleteCast(s,a,skill,attack,new R(),new R(),new R());Assert.IsFalse(r.ok);Assert.AreEqual("mp_insufficient",r.reason);Assert.AreEqual(100,a.actionGauge);Assert.AreEqual(1,a.mp);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=10,maxMp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
