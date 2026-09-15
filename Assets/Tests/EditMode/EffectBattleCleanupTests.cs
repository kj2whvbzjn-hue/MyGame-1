#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectBattleCleanupTests
 {
  [Test] public void DotDeath_ClearsBattleEffectsAndBarriers(){var s=S();var a=s.actors[0];EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="D",sourceId="X",effectId="BURN",kind=EffectLifecycleKind.DOT,baseDurationTicks=3,value=20});EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="SH",sourceId="X",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=3,value=1},1);a.hp=2;var r=BattleTickRuntime.Advance(s);Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(a.alive);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(0,a.barrierLayers.Count);}
  [Test] public void BattleEnd_ClearsEffectsCooldownCastAndBarrier(){var s=S();var a=s.actors[0];EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="B",sourceId="X",effectId="ATK_UP",kind=EffectLifecycleKind.BUFF,baseDurationTicks=3,value=10});EffectLifecycleRuntime.ApplyBarrier(a,new EffectApplyRequest{instanceId="SH",sourceId="X",effectId="SHIELD",kind=EffectLifecycleKind.BARRIER,baseDurationTicks=3,value=1},5);a.cooldowns.Add(new CooldownSaveRecord{skillId="S",remainingTicks=5});a.cast=new CastSaveRecord{skillId="S",active=true,remainingTicks=2};EffectLifecycleRuntime.CleanupBattleEnd(s);Assert.AreEqual(0,a.appliedEffects.Count);Assert.AreEqual(0,a.barrierLayers.Count);Assert.AreEqual(0,a.cooldowns.Count);Assert.IsNull(a.cast);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
