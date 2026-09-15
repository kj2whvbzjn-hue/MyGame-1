#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class StatusRemovalProtectionTests
 {
  [Test] public void NormalCleanse_SkipsProtectedAndNonRemovableStatuses(){var a=new BattleActorSaveRecord{actorId="A"};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="P",effectId="STUN",kind="STATUS",remainingTicks=5,appliedTick=0,sequence=0,removable=true,protectedEffect=true});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="N",effectId="ACTION_DISABLED",kind="STATUS",remainingTicks=5,appliedTick=1,sequence=0,removable=false});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="R",effectId="STUN",kind="STATUS",remainingTicks=5,appliedTick=2,sequence=0,removable=true});var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("R",removed.instanceId);Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="P"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="N"));}
  [Test] public void Refresh_UsesLatestRemovalFlags(){var a=new BattleActorSaveRecord{actorId="A"};EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="S1",effectId="STUN",kind=EffectLifecycleKind.STATUS,baseDurationTicks=3,removable=false,protectedEffect=true});EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="S2",effectId="STUN",kind=EffectLifecycleKind.STATUS,baseDurationTicks=4,removable=true,protectedEffect=false,appliedTick=1});Assert.AreEqual(1,a.appliedEffects.Count);Assert.IsTrue(a.appliedEffects[0].removable);Assert.IsFalse(a.appliedEffects[0].protectedEffect);Assert.AreEqual(4,a.appliedEffects[0].remainingTicks);}
 }
}
#endif
