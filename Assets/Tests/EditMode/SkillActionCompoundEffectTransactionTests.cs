#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActionCompoundEffectTransactionTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>.99;}
  [Test] public void ActivationCommitsMpAndCooldownBeforeEffects()
  {
   var s=Snapshot();var actor=s.actors[0];var target=s.actors[1];var observedMp=-1;var observedCd=-1;
   var req=Request();req.buildEffects=id=>{observedMp=actor.mp;var cd=actor.cooldowns.Find(x=>x.skillId=="SK");observedCd=cd==null?-1:cd.remainingTicks;return new[]{new CompoundSkillEffectEntry{order=1,effect=new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=10}}};};
   var rng=new Rng();var r=SkillActionTransaction.Execute(s,req,rng,rng,rng);
   Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(15,observedMp);Assert.AreEqual(4,observedCd);Assert.AreEqual(15,actor.mp);Assert.AreEqual(60,target.hp);Assert.AreEqual(0,actor.actionGauge);
  }
  [Test] public void PostActivationEffectFailureDoesNotRollbackMpOrCooldown()
  {
   var s=Snapshot();var actor=s.actors[0];var target=s.actors[1];target.hp=0;target.alive=false;
   var req=Request();req.hasTargetContract=true;req.targetCategory=SkillTargetCategory.CORPSE;req.targetRange=SkillTargetRange.SINGLE;req.buildEffects=id=>new[]{new CompoundSkillEffectEntry{order=1,effect=new SkillEffectRequest{kind=SkillEffectKind.HEAL,power=10}}};
   var rng=new Rng();var r=SkillActionTransaction.Execute(s,req,rng,rng,rng);
   Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_EFFECT_HEAL_TARGET_DEAD",r.reason);Assert.AreEqual(15,actor.mp);Assert.AreEqual(4,actor.cooldowns.Find(x=>x.skillId=="SK").remainingTicks);Assert.AreEqual(50,actor.actionGauge);
  }
  static SkillActionRequest Request()=>new SkillActionRequest{skill=new SkillDefinition{id="SK",resource=SkillResourceKind.MP,resourceCost=5,cooldownTicks=4},attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="T",skillId="SK",accuracy=999,baseDamage=1},fixedTargetIds=new List<string>{"T"}};
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",teamId="P",hp=100,maxHp=100,mp=20,maxMp=20,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="T",teamId="P",hp=50,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"A","T"});return s;}
 }
}
#endif
