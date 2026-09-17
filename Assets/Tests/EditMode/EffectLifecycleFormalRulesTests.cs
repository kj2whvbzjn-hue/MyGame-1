#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectLifecycleFormalRulesTests
 {
  [Test] public void Dot_UsesConfiguredStackLimit(){var a=new BattleActorSaveRecord();for(var i=0;i<3;i++)Assert.IsTrue(EffectLifecycleRuntime.Apply(a,Req("D"+i,"CUSTOM_DOT",EffectLifecycleKind.DOT,EffectStackRule.STACK_SUM,10+i,3)).ok);var fourth=EffectLifecycleRuntime.Apply(a,Req("D3","CUSTOM_DOT",EffectLifecycleKind.DOT,EffectStackRule.STACK_SUM,20,3));Assert.IsFalse(fourth.ok);Assert.AreEqual("EFFECT_STACK_LIMIT_REACHED",fourth.reason);Assert.AreEqual(3,a.appliedEffects.Count);}
  [Test] public void Status_Resistance_UsesConfiguredCap(){Assert.AreEqual(50,EffectLifecycleRuntime.EffectiveStatusDuration(100,80,50));Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveStatusDuration(100,80,80));}
  [Test] public void Buff_KeepHighest_PreservesIndependentStacks(){var a=new BattleActorSaveRecord();var high=Req("A","ATK_UP",EffectLifecycleKind.BUFF,EffectStackRule.STACK_HIGHEST,20,2);high.baseDurationTicks=1;var low=Req("B","ATK_UP",EffectLifecycleKind.BUFF,EffectStackRule.STACK_HIGHEST,10,2);low.baseDurationTicks=5;EffectLifecycleRuntime.Apply(a,high);EffectLifecycleRuntime.Apply(a,low);Assert.AreEqual(2,a.appliedEffects.Count);Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));a.appliedEffects[0].remainingTicks=0;a.appliedEffects.RemoveAll(x=>x.remainingTicks<=0);Assert.AreEqual(10,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));}
  [Test] public void NormalCleanse_UsesLifecycleCapability_NotRuntimeIdBranch(){var a=new BattleActorSaveRecord();var status=Req("S","CUSTOM_DISABLED",EffectLifecycleKind.STATUS,EffectStackRule.UNIQUE_REFRESH,1);status.statusResistanceCapPercent=50;status.normalCleanseEligible=true;EffectLifecycleRuntime.Apply(a,status);var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("CUSTOM_DISABLED",removed.effectId);}
  [Test] public void StatusRefresh_UsesLatestApplicationOrdering(){var a=new BattleActorSaveRecord();var first=Req("A","CUSTOM_STATUS",EffectLifecycleKind.STATUS,EffectStackRule.UNIQUE_REFRESH,1);first.statusResistanceCapPercent=50;first.appliedTick=1;first.sequence=1;EffectLifecycleRuntime.Apply(a,first);var next=Req("B","CUSTOM_STATUS",EffectLifecycleKind.STATUS,EffectStackRule.UNIQUE_REFRESH,2);next.statusResistanceCapPercent=50;next.appliedTick=9;next.sequence=4;var r=EffectLifecycleRuntime.Apply(a,next);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(9,r.applied.appliedTick);Assert.AreEqual(4,r.applied.sequence);}
  static EffectApplyRequest Req(string instance,string id,EffectLifecycleKind kind,EffectStackRule rule,double value,int maxStacks=0)=>new EffectApplyRequest{instanceId=instance,effectId=id,sourceId="SRC",kind=kind,stackRule=rule,refreshRule=rule==EffectStackRule.UNIQUE_REFRESH?"REFRESH":"KEEP",snapshotPolicy="SNAPSHOT",dispelCategory=kind.ToString(),baseDurationTicks=10,value=value,maxStacks=maxStacks};
 }
}
#endif
