#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs14TickPhaseOrderTests
 {
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true,formationRow=0});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=100,maxHp=100,alive=true,formationRow=0});s.fixedActorOrder.AddRange(new[]{"A","B"});return s;}
  [Test] public void DurationUpdateOccursBeforePeriodicAndExpiry(){var s=Snapshot();s.actors[0].appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",sourceId="B",effectId="BURN",kind="DOT",remainingTicks=1,value=7,appliedTick=0,sequence=0});var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(93,s.actors[0].hp);Assert.AreEqual(7,r.dotHpLoss);Assert.Contains("D",r.expiredEffects);Assert.AreEqual(0,s.actors[0].appliedEffects.Count);}
  [Test] public void CooldownPhaseRunsBeforeEffectExpiryAcrossActors(){var s=Snapshot();s.actors[0].cooldowns.Add(new CooldownSaveRecord{skillId="SK",remainingTicks=1});s.actors[1].appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="STUN",kind="STATUS",remainingTicks=1,appliedTick=0,sequence=0});var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(0,s.actors[0].cooldowns.Count);Assert.AreEqual(0,s.actors[1].appliedEffects.Count);Assert.Contains("E",r.expiredEffects);}
 }
}
#endif
