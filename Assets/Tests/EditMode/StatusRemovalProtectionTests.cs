#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class StatusRemovalProtectionTests
 {
  [Test] public void NormalCleanse_SkipsProtectedAndNonRemovableStatuses(){var a=new BattleActorSaveRecord{actorId="A"};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="P",effectId="STUN",kind="STATUS",dispelCategory="STATUS",remainingTicks=5,appliedTick=0,sequence=0,removable=true,protectedEffect=true,normalCleanseEligible=true});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="N",effectId="ACTION_DISABLED",kind="STATUS",dispelCategory="STATUS",remainingTicks=5,appliedTick=1,sequence=0,removable=false,normalCleanseEligible=true});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="R",effectId="STUN",kind="STATUS",dispelCategory="STATUS",remainingTicks=5,appliedTick=2,sequence=0,removable=true,normalCleanseEligible=true});var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("R",removed.instanceId);Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="P"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="N"));}
  [Test] public void Refresh_UsesLatestRemovalFlags(){var a=new BattleActorSaveRecord{actorId="A"};EffectLifecycleRuntime.Apply(a,Req("S1",3,false,true,0));EffectLifecycleRuntime.Apply(a,Req("S2",4,true,false,1));Assert.AreEqual(1,a.appliedEffects.Count);Assert.IsTrue(a.appliedEffects[0].removable);Assert.IsFalse(a.appliedEffects[0].protectedEffect);Assert.AreEqual(4,a.appliedEffects[0].remainingTicks);}
  static EffectApplyRequest Req(string id,int duration,bool removable,bool protectedEffect,int tick)=>new EffectApplyRequest{instanceId=id,effectId="STUN",kind=EffectLifecycleKind.STATUS,stackRule=EffectStackRule.UNIQUE_REFRESH,refreshRule="REFRESH",snapshotPolicy="SNAPSHOT",dispelCategory="STATUS",baseDurationTicks=duration,statusResistanceCapPercent=50,removable=removable,protectedEffect=protectedEffect,appliedTick=tick};
 }
}
#endif
