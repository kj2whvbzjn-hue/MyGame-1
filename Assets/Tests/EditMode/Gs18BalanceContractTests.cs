#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class Gs18BalanceContractTests
 {
  static BattleActorSaveRecord Actor()=>new BattleActorSaveRecord{actorId="A",hp=100,maxHp=100,alive=true};
  static EffectApplyRequest Dot(string id,int maxStacks)=>new EffectApplyRequest{instanceId=id,effectId="FUTURE_DOT",kind=EffectLifecycleKind.DOT,stackRule=EffectStackRule.STACK_SUM,refreshRule="KEEP",snapshotPolicy="SNAPSHOT",dispelCategory="DOT",baseDurationTicks=4,maxStacks=maxStacks};
  static EffectApplyRequest Status(string id)=>new EffectApplyRequest{instanceId=id,effectId="FUTURE_DISABLE",kind=EffectLifecycleKind.STATUS,stackRule=EffectStackRule.UNIQUE_REFRESH,refreshRule="REFRESH",snapshotPolicy="SNAPSHOT",dispelCategory="STATUS",baseDurationTicks=4,actionDisabled=true};
  [Test] public void DotMaxStacks_IsProvidedByLifecycleData(){var actor=Actor();for(var i=0;i<3;i++)Assert.IsTrue(EffectLifecycleRuntime.Apply(actor,Dot("D"+i,3)).ok);var blocked=EffectLifecycleRuntime.Apply(actor,Dot("D3",3));Assert.IsFalse(blocked.ok);Assert.AreEqual("EFFECT_STACK_LIMIT_REACHED",blocked.reason);}
  [Test] public void DotWithoutMaxStacks_IsRejectedInsteadOfUsingImplicitFive(){var result=EffectLifecycleRuntime.Apply(Actor(),Dot("D",0));Assert.IsFalse(result.ok);Assert.AreEqual("EFFECT_MAX_STACKS_MISSING",result.reason);}
  [Test] public void ResistanceCap_IsProvidedByLifecycleData(){Assert.AreEqual(2,EffectLifecycleRuntime.EffectiveStatusDuration(4,80,50));Assert.AreEqual(1,EffectLifecycleRuntime.EffectiveStatusDuration(4,80,80));}
  [Test] public void StatusWithoutResistanceCap_IsRejected(){var result=EffectLifecycleRuntime.Apply(Actor(),Status("S"));Assert.IsFalse(result.ok);Assert.AreEqual("STATUS_RESISTANCE_CAP_MISSING",result.reason);}
  [Test] public void CapabilityFlags_DoNotDependOnStatusId(){var actor=Actor();var req=Status("S");req.statusResistanceCapPercent=60;req.normalCleanseEligible=true;var applied=EffectLifecycleRuntime.Apply(actor,req);Assert.IsTrue(applied.ok);Assert.IsTrue(applied.applied.actionDisabled);var removed=EffectLifecycleRuntime.RemoveStatus(actor,1,false,null,true);Assert.AreEqual(1,removed.removedCount);}
 }
}
#endif
