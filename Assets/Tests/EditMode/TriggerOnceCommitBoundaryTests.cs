#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerOnceCommitBoundaryTests
 {
  sealed class Rng:IRandomSource{public double value;public double Next01(string purpose)=>value;}
  [Test] public void OnceTrigger_ChanceFailureDoesNotConsumeRegistration(){var reg=new TriggerRegistration{id="P",trigger=TriggerEvent.ON_HIT_RECEIVED,once=true,activationChance=.5};var a=new TriggerActionContext{actionId="A"};TriggerActivationRuntime.Enqueue(a,new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{reg}});var r=TriggerActivationRuntime.Drain(a,new Rng{value=.9},null);Assert.IsTrue(r.ok);Assert.AreEqual(0,r.executed);Assert.IsFalse(reg.consumed);Assert.AreEqual(0,a.activationCount);}
  [Test] public void OnceTrigger_IsConsumedOnlyAfterSuccessfulCommit(){var reg=new TriggerRegistration{id="P",trigger=TriggerEvent.ON_HIT_RECEIVED,once=true,activationChance=1};var a=new TriggerActionContext{actionId="A"};TriggerActivationRuntime.Enqueue(a,new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{reg}});var r=TriggerActivationRuntime.Drain(a,null,null);Assert.IsTrue(r.ok);Assert.AreEqual(1,r.executed);Assert.IsTrue(reg.consumed);Assert.AreEqual(1,a.activationCount);Assert.AreEqual(1,a.history.Count);}
 }
}
#endif
