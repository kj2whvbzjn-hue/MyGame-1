#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs18StatusRemoveContractTests
 {
  static AppliedEffectSaveRecord E(string id,string effect,int tick,int seq,bool removable=true,bool protectedEffect=false,string kind="STATUS")=>new AppliedEffectSaveRecord{instanceId=id,effectId=effect,kind=kind,remainingTicks=5,appliedTick=tick,sequence=seq,removable=removable,protectedEffect=protectedEffect};
  [Test] public void CountRemoval_UsesOldestFormalOrderAndSkipsProtected(){var a=new BattleActorSaveRecord();a.appliedEffects.Add(E("C","STUN",2,0));a.appliedEffects.Add(E("B","ACTION_DISABLED",1,2));a.appliedEffects.Add(E("A","STUN",1,1));a.appliedEffects.Add(E("P","STUN",0,0,true,true));var r=EffectLifecycleRuntime.RemoveStatus(a,2);Assert.IsTrue(r.ok);Assert.AreEqual(2,r.removedCount);CollectionAssert.AreEqual(new[]{"A","B"},r.removedInstanceIds);Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="P"));Assert.IsTrue(a.appliedEffects.Exists(x=>x.instanceId=="C"));}
  [Test] public void AllNormalCleanse_DoesNotRemoveDotBuffOrOtherStatus(){var a=new BattleActorSaveRecord();a.appliedEffects.Add(E("S","STUN",0,0));a.appliedEffects.Add(E("X","CUSTOM_STATUS",0,1));a.appliedEffects.Add(E("D","BURN",0,2,true,false,"DOT"));a.appliedEffects.Add(E("U","ATK_UP",0,3,true,false,"BUFF"));var r=EffectLifecycleRuntime.RemoveStatus(a,all:true);Assert.AreEqual(1,r.removedCount);Assert.AreEqual(3,a.appliedEffects.Count);Assert.IsFalse(a.appliedEffects.Exists(x=>x.instanceId=="S"));}
  [Test] public void NoCandidate_IsSuccessfulZeroRemoval(){var a=new BattleActorSaveRecord();a.appliedEffects.Add(E("D","BURN",0,0,true,false,"DOT"));var r=EffectLifecycleRuntime.RemoveStatus(a,all:true);Assert.IsTrue(r.ok);Assert.AreEqual(0,r.removedCount);}
 }
}
#endif
