#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectLifecycleFormalRulesTests
 {
  [Test] public void Dot_StackLimit_IsSuppliedByLifecycleData(){var a=new BattleActorSaveRecord();for(var i=0;i<3;i++)Assert.IsTrue(EffectLifecycleRuntime.Apply(a,Req("D"+i,"BURN",EffectLifecycleKind.DOT,10+i,3)).ok);var fourth=EffectLifecycleRuntime.Apply(a,Req("D4","BURN",EffectLifecycleKind.DOT,20,3));Assert.IsFalse(fourth.ok);Assert.AreEqual("EFFECT_STACK_LIMIT_REACHED",fourth.reason);Assert.AreEqual(3,a.appliedEffects.Count);}
  [Test] public void Dot_MissingStackLimit_IsRejected(){var a=new BattleActorSaveRecord();var r=EffectLifecycleRuntime.Apply(a,Req("D","BURN",EffectLifecycleKind.DOT,10,0));Assert.IsFalse(r.ok);Assert.AreEqual("EFFECT_MAX_STACKS_MISSING",r.reason);}
  [Test] public void Status_ResistanceCap_IsSuppliedByBalanceData(){Assert.AreEqual(50,EffectLifecycleRuntime.EffectiveStatusDuration(100,80,50));Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveStatusDuration(100,80,80));}
  [Test] public void Buff_KeepHighest_PreservesIndependentStacks(){var a=new BattleActorSaveRecord();var high=Req("A","ATK_UP",EffectLifecycleKind.BUFF,20,2);high.baseDurationTicks=1;var low=Req("B","ATK_UP",EffectLifecycleKind.BUFF,10,2);low.baseDurationTicks=5;EffectLifecycleRuntime.Apply(a,high);EffectLifecycleRuntime.Apply(a,low);Assert.AreEqual(2,a.appliedEffects.Count);Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));a.appliedEffects[0].remainingTicks=0;a.appliedEffects.RemoveAll(x=>x.remainingTicks<=0);Assert.AreEqual(10,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));}
  [Test] public void NormalCleanse_UsesLifecycleCapability_NotEffectId(){var a=new BattleActorSaveRecord();var other=Req("X","CUSTOM_STATUS",EffectLifecycleKind.STATUS,1,0);other.normalCleanseEligible=true;EffectLifecycleRuntime.Apply(a,other);var stun=Req("S","STUN",EffectLifecycleKind.STATUS,1,0);EffectLifecycleRuntime.Apply(a,stun);var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("CUSTOM_STATUS",removed.effectId);Assert.AreEqual(1,a.appliedEffects.Count);}
  [Test] public void StatusRefresh_UsesLatestApplicationOrdering(){var a=new BattleActorSaveRecord();var first=Req("A","STUN",EffectLifecycleKind.STATUS,1,0);first.appliedTick=1;first.sequence=1;EffectLifecycleRuntime.Apply(a,first);var next=Req("B","STUN",EffectLifecycleKind.STATUS,2,0);next.appliedTick=9;next.sequence=4;var r=EffectLifecycleRuntime.Apply(a,next);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(9,r.applied.appliedTick);Assert.AreEqual(4,r.applied.sequence);}
  static EffectApplyRequest Req(string instance,string id,EffectLifecycleKind kind,double value,int maxStacks)=>new EffectApplyRequest{instanceId=instance,effectId=id,sourceId="SRC",kind=kind,baseDurationTicks=10,value=value,maxStacks=maxStacks,statusResistanceCapPercent=75};
 }
}
#endif
