#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillCastIdentityTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void CastCompletionRejectsReservationActorMismatchBeforeCost(){var s=S();var a=s.actors[0];var skill=Skill("S");a.cast=Cast("R","S");s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="OTHER",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>{"B"}});var mp=a.mp;var r=SkillActionTransaction.CompleteCast(s,a,skill,Attack("R","A","S"),new R(),new R(),new R());Assert.IsFalse(r.ok);Assert.AreEqual("CAST_ACTOR_MISMATCH",r.reason);Assert.AreEqual(mp,a.mp);}
  [Test] public void CastCompletionRejectsSkillMismatchBeforeCost(){var s=S();var a=s.actors[0];a.cast=Cast("R","S");s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>{"B"}});var r=SkillActionTransaction.CompleteCast(s,a,Skill("OTHER"),Attack("R","A","OTHER"),new R(),new R(),new R());Assert.IsFalse(r.ok);Assert.AreEqual("CAST_SKILL_MISMATCH",r.reason);}
  [Test] public void CastCompletionRejectsAttackIdentityMismatch(){var s=S();var a=s.actors[0];a.cast=Cast("R","S");s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",fixedTargetIds=new System.Collections.Generic.List<string>{"B"}});var r=SkillActionTransaction.CompleteCast(s,a,Skill("S"),Attack("WRONG","A","S"),new R(),new R(),new R());Assert.IsFalse(r.ok);Assert.AreEqual("CAST_ATTACK_MISMATCH",r.reason);}
  static CastSaveRecord Cast(string r,string skill)=>new CastSaveRecord{reservationId=r,skillId=skill,remainingTicks=0,active=false};
  static SkillDefinition Skill(string id)=>new SkillDefinition{id=id,resource=SkillResourceKind.MP,resourceCost=2};
  static BattleAttackProposal Attack(string r,string source,string skill)=>new BattleAttackProposal{reservationId=r,sourceId=source,targetId="B",skillId=skill,damageType=DamageType.PHYSICAL,accuracy=100,baseDamage=1};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=10,maxMp=10,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
