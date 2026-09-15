#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class PermanentPassiveCooldownTickTests
 {
  [Test] public void Advance_DoesNotTickPermanentPassiveCooldown(){var s=S();var a=s.actors[0];a.cooldowns.Add(new CooldownSaveRecord{skillId="FATAL_PASSIVE",remainingTicks=EffectLifecycleRuntime.PermanentBattleCooldown});a.cooldowns.Add(new CooldownSaveRecord{skillId="NORMAL",remainingTicks=2});var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(EffectLifecycleRuntime.PermanentBattleCooldown,a.cooldowns.Find(x=>x.skillId=="FATAL_PASSIVE").remainingTicks);Assert.AreEqual(1,a.cooldowns.Find(x=>x.skillId=="NORMAL").remainingTicks);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=10,maxMp=10,alive=true});return s;}
 }
}
#endif
