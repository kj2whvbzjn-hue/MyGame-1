#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectLifecycleFormalRulesTests
 {
  [Test] public void Dot_DefaultStackLimit_IsFormalFive(){var a=new BattleActorSaveRecord();for(var i=0;i<5;i++)Assert.IsTrue(EffectLifecycleRuntime.Apply(a,Req("D"+i,"BURN",EffectLifecycleKind.DOT,10+i)).ok);var sixth=EffectLifecycleRuntime.Apply(a,Req("D6","BURN",EffectLifecycleKind.DOT,20));Assert.IsFalse(sixth.ok);Assert.AreEqual("EFFECT_STACK_LIMIT_REACHED",sixth.reason);Assert.AreEqual(5,a.appliedEffects.Count);}
  [Test] public void Status_Resistance_IsClampedToFormalSeventyFivePercent(){Assert.AreEqual(25,EffectLifecycleRuntime.EffectiveStatusDuration(100,80));Assert.AreEqual(75,EffectLifecycleRuntime.FormalStatusResistanceCapPercent);}
  [Test] public void Buff_KeepHighest_PreservesIndependentStacks(){var a=new BattleActorSaveRecord();var high=Req("A","ATK_UP",EffectLifecycleKind.BUFF,20,2);high.baseDurationTicks=1;var low=Req("B","ATK_UP",EffectLifecycleKind.BUFF,10,2);low.baseDurationTicks=5;EffectLifecycleRuntime.Apply(a,high);EffectLifecycleRuntime.Apply(a,low);Assert.AreEqual(2,a.appliedEffects.Count);Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));a.appliedEffects[0].remainingTicks=0;a.appliedEffects.RemoveAll(x=>x.remainingTicks<=0);Assert.AreEqual(10,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));}
  [Test] public void NormalCleanse_UsesLifecycleCapability_NotRuntimeIdBranch(){var a=new BattleActorSaveRecord();var status=Req("S","STUN",EffectLifecycleKind.STATUS,1);status.normalCleanseEligible=true;EffectLifecycleRuntime.Apply(a,status);var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("STUN",removed.effectId);}
  [Test] public void StatusRefresh_UsesLatestApplicationOrdering(){var a=new BattleActorSaveRecord();var first=Req("A","STUN",EffectLifecycleKind.STATUS,1);first.appliedTick=1;first.sequence=1;EffectLifecycleRuntime.Apply(a,first);var next=Req("B","STUN",EffectLifecycleKind.STATUS,2);next.appliedTick=9;next.sequence=4;var r=EffectLifecycleRuntime.Apply(a,next);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(9,r.applied.appliedTick);Assert.AreEqual(4,r.applied.sequence);}
  static EffectApplyRequest Req(string instance,string id,EffectLifecycleKind kind,double value,int maxStacks=0)=>new EffectApplyRequest{instanceId=instance,effectId=id,sourceId="SRC",kind=kind,baseDurationTicks=10,value=value,maxStacks=maxStacks};
 }
}
#endif
