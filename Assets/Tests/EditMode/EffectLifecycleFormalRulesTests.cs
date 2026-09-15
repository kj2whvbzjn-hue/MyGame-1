#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectLifecycleFormalRulesTests
 {
  [Test] public void Dot_DefaultStackLimit_IsFive(){var a=new BattleActorSaveRecord();for(var i=0;i<5;i++)Assert.IsTrue(EffectLifecycleRuntime.Apply(a,Req("D"+i,"BURN",EffectLifecycleKind.DOT,10+i)).ok);var sixth=EffectLifecycleRuntime.Apply(a,Req("D6","BURN",EffectLifecycleKind.DOT,20));Assert.IsFalse(sixth.ok);Assert.AreEqual("EFFECT_STACK_LIMIT_REACHED",sixth.reason);Assert.AreEqual(5,a.appliedEffects.Count);}
  [Test] public void Buff_KeepHighest_DoesNotAccumulateLowerSnapshot(){var a=new BattleActorSaveRecord();EffectLifecycleRuntime.Apply(a,Req("A","ATK_UP",EffectLifecycleKind.BUFF,20));EffectLifecycleRuntime.Apply(a,Req("B","ATK_UP",EffectLifecycleKind.BUFF,10));Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));}
  [Test] public void NormalCleanse_RemovesOnlyFormalCleanseStatuses(){var a=new BattleActorSaveRecord();EffectLifecycleRuntime.Apply(a,Req("X","OTHER_STATUS",EffectLifecycleKind.STATUS,1));EffectLifecycleRuntime.Apply(a,Req("S","STUN",EffectLifecycleKind.STATUS,1));var removed=EffectLifecycleRuntime.RemoveOldestStatus(a);Assert.NotNull(removed);Assert.AreEqual("STUN",removed.effectId);Assert.AreEqual(1,a.appliedEffects.Count);}
  [Test] public void StatusRefresh_UsesLatestApplicationOrdering(){var a=new BattleActorSaveRecord();var first=Req("A","STUN",EffectLifecycleKind.STATUS,1);first.appliedTick=1;first.sequence=1;EffectLifecycleRuntime.Apply(a,first);var next=Req("B","STUN",EffectLifecycleKind.STATUS,2);next.appliedTick=9;next.sequence=4;var r=EffectLifecycleRuntime.Apply(a,next);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual(9,r.applied.appliedTick);Assert.AreEqual(4,r.applied.sequence);}
  static EffectApplyRequest Req(string instance,string id,EffectLifecycleKind kind,double value)=>new EffectApplyRequest{instanceId=instance,effectId=id,sourceId="SRC",kind=kind,baseDurationTicks=10,value=value};
 }
}
#endif
