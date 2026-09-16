#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionBlockingConditionTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="T",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","T"});return s;}
  static SkillDefinition Skill()=>new SkillDefinition{id="SK",enabled=true,resource=SkillResourceKind.MP,resourceCost=3,castTicks=2,cooldownTicks=2};
  static SkillActionRequest Request(SkillDefinition skill)=>new SkillActionRequest{skill=skill,attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="SK",hitCount=1,baseDamage=1,accuracy=999},fixedTargetIds=new System.Collections.Generic.List<string>{"T"}};
  static void AddStatus(BattleActorSaveRecord a,string id){a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E-"+id,effectId=id,kind="STATUS",remainingTicks=2});}
  [Test] public void Precheck_RejectsStun(){var s=Snapshot();AddStatus(s.actors[0],"STUN");var r=SkillActionTransaction.Precheck(s,Request(Skill()));Assert.IsFalse(r.ok);Assert.AreEqual("actor_incapacitated",r.reason);}
  [Test] public void CastCompletion_RechecksSkillBlockWithoutPayingMpOrCooldown(){var s=Snapshot();var skill=Skill();var rng=new Rng();var started=SkillActionTransaction.Execute(s,Request(skill),rng,rng,rng);Assert.IsTrue(started.castingStarted);var a=s.actors[0];AddStatus(a,"SKILL_BLOCK");a.cast.active=false;a.cast.remainingTicks=0;var attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="SK",hitCount=1,baseDamage=1,accuracy=999};var done=SkillActionTransaction.CompleteCast(s,a,skill,attack,rng,rng,rng);Assert.IsFalse(done.ok);Assert.AreEqual("skill_blocked",done.reason);Assert.AreEqual(20,a.mp);Assert.AreEqual(0,a.cooldowns.Count);Assert.AreEqual(50,a.actionGauge);Assert.AreEqual(0,s.actionReservations.Count);}
 }
}
#endif
