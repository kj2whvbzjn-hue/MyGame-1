#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectHighestStackCapTests
 {
  static EffectApplyRequest Req(string id,double value,int tick,int seq)=>new EffectApplyRequest{instanceId=id,sourceId="S",effectId="ATK_UP",kind=EffectLifecycleKind.BUFF,baseDurationTicks=10,value=value,appliedTick=tick,sequence=seq,maxStacks=2};
  [Test] public void HighestStack_WhenFull_IgnoresWeakerIncomingValue()
  {
   var a=new BattleActorSaveRecord();EffectLifecycleRuntime.Apply(a,Req("A",10,1,1));EffectLifecycleRuntime.Apply(a,Req("B",20,2,2));var r=EffectLifecycleRuntime.Apply(a,Req("C",5,3,3));Assert.IsTrue(r.ok);Assert.AreEqual(2,a.appliedEffects.Count);Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="A"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="B"));Assert.AreEqual(20,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));
  }
  [Test] public void HighestStack_WhenFull_ReplacesDeterministicWeakestWithStrongerValue()
  {
   var a=new BattleActorSaveRecord();EffectLifecycleRuntime.Apply(a,Req("A",10,1,1));EffectLifecycleRuntime.Apply(a,Req("B",20,2,2));var r=EffectLifecycleRuntime.Apply(a,Req("C",30,3,3));Assert.IsTrue(r.ok);Assert.AreEqual("C",r.applied.instanceId);Assert.AreEqual(2,a.appliedEffects.Count);Assert.IsFalse(a.appliedEffects.Exists(x=>x.instanceId=="A"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="B"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="C"));Assert.AreEqual(30,EffectLifecycleRuntime.EffectiveValue(a,"ATK_UP",EffectLifecycleKind.BUFF));
  }
 }
}
#endif
