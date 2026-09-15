#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PermanentCooldownLegacyTickTests
 {
  [Test] public void LegacyTick_DoesNotDecrementPermanentBattleCooldown(){var actor=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10};actor.cooldowns.Add(new CooldownSaveRecord{skillId="PASSIVE",remainingTicks=EffectLifecycleRuntime.PermanentBattleCooldown});actor.cooldowns.Add(new CooldownSaveRecord{skillId="NORMAL",remainingTicks=2});SkillActionTransaction.AdvanceCastAndCooldowns(actor);Assert.AreEqual(EffectLifecycleRuntime.PermanentBattleCooldown,actor.cooldowns.Find(x=>x.skillId=="PASSIVE").remainingTicks);Assert.AreEqual(1,actor.cooldowns.Find(x=>x.skillId=="NORMAL").remainingTicks);}
 }
}
#endif
