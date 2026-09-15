#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerDirectOwnerReentryTests
 {
  [Test] public void DirectActivation_SamePassiveDifferentOwnersDoNotBlock(){var a=new TriggerActionContext{actionId="A"};var first=TriggerActivationRuntime.TryActivate(a,"P",1,null,ownerId:"O1");Assert.IsTrue(first.ok);var second=TriggerActivationRuntime.TryActivate(a,"P",1,null,ownerId:"O2");Assert.IsTrue(second.ok);TriggerActivationRuntime.Release(a,"P","O1");TriggerActivationRuntime.Release(a,"P","O2");}
  [Test] public void DirectActivation_SamePassiveSameOwnerBlocksUntilRelease(){var a=new TriggerActionContext{actionId="A"};Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P",1,null,ownerId:"O1").ok);var blocked=TriggerActivationRuntime.TryActivate(a,"P",1,null,ownerId:"O1");Assert.IsFalse(blocked.ok);Assert.AreEqual("TRIGGER_REENTRY_BLOCKED",blocked.reason);TriggerActivationRuntime.Release(a,"P","O1");Assert.IsTrue(TriggerActivationRuntime.TryActivate(a,"P",1,null,ownerId:"O1").ok);}
 }
}
#endif
