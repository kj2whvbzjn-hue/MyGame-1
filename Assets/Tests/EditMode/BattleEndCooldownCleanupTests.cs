#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleEndCooldownCleanupTests
 {
  [Test] public void CleanupBattleEnd_ClearsNormalAndPermanentBattleCooldowns(){var s=new BattleSnapshotSaveRecord();var a=new BattleActorSaveRecord{actorId="A"};a.cooldowns.Add(new CooldownSaveRecord{skillId="ACTIVE",remainingTicks=12});a.cooldowns.Add(new CooldownSaveRecord{skillId="FATAL_PASSIVE",remainingTicks=EffectLifecycleRuntime.PermanentBattleCooldown});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="BUFF",kind=EffectLifecycleKind.BUFF.ToString(),remainingTicks=5});a.cast=new CastSaveRecord{active=true,remainingTicks=2};s.actors.Add(a);EffectLifecycleRuntime.CleanupBattleEnd(s);Assert.AreEqual(0,a.cooldowns.Count);Assert.AreEqual(0,a.appliedEffects.Count);Assert.IsNull(a.cast);}
 }
}
#endif
