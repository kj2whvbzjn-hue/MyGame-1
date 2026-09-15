#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerOwnerReentryTests
 {
  [Test] public void SamePassiveId_DifferentOwners_DoNotBlockEachOther()
  {
   var a=new TriggerActionContext();var first=TriggerActivationRuntime.PrepareActivation(a,"P","ON_HIT_RECEIVED","S","A","A");Assert.IsTrue(first.ok);Assert.IsTrue(TriggerActivationRuntime.CheckChance(first.prepared,1,null).ok);Assert.IsTrue(TriggerActivationRuntime.CommitActivation(first.prepared).ok);
   var second=TriggerActivationRuntime.PrepareActivation(a,"P","ON_HIT_RECEIVED","S","B","B");Assert.IsTrue(second.ok);
  }
  [Test] public void SamePassiveId_SameOwner_IsBlockedWhileExecuting()
  {
   var a=new TriggerActionContext();var first=TriggerActivationRuntime.PrepareActivation(a,"P","ON_HIT_RECEIVED","S","A","A");TriggerActivationRuntime.CheckChance(first.prepared,1,null);TriggerActivationRuntime.CommitActivation(first.prepared);
   var second=TriggerActivationRuntime.PrepareActivation(a,"P","ON_HIT_RECEIVED","S","A","A");Assert.IsFalse(second.ok);Assert.AreEqual("TRIGGER_REENTRY_BLOCKED",second.reason);
  }
 }
}
#endif
