#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickMultiTargetCastTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void CastCompletion_ExecutesOnlySurvivingFrozenTargets(){var s=S();var actor=s.actors[0];actor.cast=new CastSaveRecord{reservationId="R",skillId="S",remainingTicks=1,active=true,fixedTargetIds=new System.Collections.Generic.List<string>{"B","C"}};s.actionReservations.Add(new ActionReservationSaveRecord{reservationId="R",actorId="A",skillId="S",startTick=0,completeTick=1,fixedTargetIds=new System.Collections.Generic.List<string>{"B","C"},usageConditions=new UsageConditionsSaveRecord()});s.actors[1].hp=0;s.actors[1].alive=false;var skill=new SkillDefinition{id="S",castTicks=1};var rng=new Rng();var r=BattleTickRuntime.Advance(s,new BattleTickOptions{resolveSkillById=id=>skill,buildReservedAttack=(a,x,k)=>P(),buildReservedTargetAttack=(a,x,k,id)=>P(),criticalRng=rng,hitRng=rng});Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(10,s.actors[1].hp);Assert.AreEqual(9,s.actors[2].hp);Assert.AreEqual(1,s.resolvedHits.Count);Assert.AreEqual("C",s.resolvedHits[0].targetId);CollectionAssert.Contains(r.castsCompleted,"A:S");}
  static BattleAttackProposal P()=>new BattleAttackProposal{reservationId="R",sourceId="A",skillId="S",damageType=DamageType.Physical,accuracy=100,baseDamage=1};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B","C"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true,actionGauge=100});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="C",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
