#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillActivationRollbackTests
 {
  sealed class Rng:IRandomSource{public double Next01(string p)=>0;}
  [Test] public void BattleStepPrecommitFailure_RestoresMpAndCooldown()
  {
   var s=new BattleSnapshotSaveRecord();s.fixedActorOrder.AddRange(new[]{"A","B"});var a=new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,mp=10,maxMp=10,actionGauge=100};s.actors.Add(a);s.actors.Add(new BattleActorSaveRecord{actorId="B",alive=true,hp=10,maxHp=10});
   var skill=new SkillDefinition{id="S",resource=SkillResourceKind.MP,resourceCost=4,cooldownTicks=3};var attack=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",hitCount=1,damageType=DamageType.Physical,accuracy=100,baseDamage=2,blockEligible=true,blockRate=100};
   var r=SkillActionTransaction.Execute(s,new SkillActionRequest{skill=skill,attack=attack},new Rng(),new Rng(),null);
   Assert.IsFalse(r.ok);Assert.AreEqual("BATTLE_STEP_BLOCK_RNG_MISSING",r.reason);Assert.AreEqual(10,a.mp);Assert.AreEqual(0,a.cooldowns.Count);Assert.AreEqual(100,a.actionGauge);
  }
 }
}
#endif
