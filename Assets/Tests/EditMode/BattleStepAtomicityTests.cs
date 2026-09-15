#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleStepAtomicityTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void MissingBlockRng_FailsBeforeReservationOrHitMutation(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});var p=new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",hitCount=2,damageType=DamageType.Physical,accuracy=100,baseDamage=3,blockEligible=true,blockRate=1,blockCutRate=.5};var r=BattleStepExecutor.ExecuteAttack(s,p,new Rng(),new Rng(),null);Assert.IsFalse(r.ok);Assert.AreEqual("BATTLE_STEP_BLOCK_RNG_MISSING",r.reason);Assert.AreEqual(10,s.actors[1].hp);Assert.AreEqual(0,s.actionReservations.Count);Assert.AreEqual(0,s.resolvedHits.Count);}
 }
}
#endif
