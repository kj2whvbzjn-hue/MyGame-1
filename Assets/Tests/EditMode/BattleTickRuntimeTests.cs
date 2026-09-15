#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickRuntimeTests
 {
  [Test] public void Advance_ProcessesDotCooldownExpiryThenGauge()
  {
   var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");
   var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,mp=0,maxMp=0,alive=true,speed=0,actionGauge=0};
   a.cooldowns.Add(new CooldownSaveRecord{skillId="SK",remainingTicks=1});
   a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",effectId="BURN",kind="DOT",remainingTicks=1,value=3,appliedTick=0,sequence=0});s.actors.Add(a);
   var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(7,a.hp);Assert.AreEqual(0,a.cooldowns.Count);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(10,a.actionGauge,1e-9);Assert.AreEqual(1,s.tick);
  }
  [Test] public void DotDeath_PreventsGaugeGain()
  {
   var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");var a=new BattleActorSaveRecord{actorId="A",hp=2,maxHp=10,alive=true,speed=100,actionGauge=20};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",effectId="POISON",kind="DOT",remainingTicks=2,value=3});s.actors.Add(a);var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok);Assert.AreEqual(0,a.hp);Assert.IsFalse(a.alive);Assert.AreEqual(20,a.actionGauge);
  }
 }
}
#endif
