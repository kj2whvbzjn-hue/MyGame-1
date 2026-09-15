#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class ReactiveFamilyOrderTests
 {
  [Test] public void CounterExecutesBeforeFollowUpRegardlessOfPriority(){var action=new TriggerActionContext{actionId="A"};var dispatch=new BattleTriggerDispatch{trigger=TriggerEvent.ON_HIT_RECEIVED,context=new BattleTriggerContext{sourceId="S",targetId="T"},registrations=new List<TriggerRegistration>{new TriggerRegistration{id="F",ownerId="O1",trigger=TriggerEvent.ON_ALLY_ATTACK,reactiveFamily=ReactiveFamily.FOLLOW_UP,priority=999,sequence=1},new TriggerRegistration{id="C",ownerId="O2",trigger=TriggerEvent.ON_HIT_RECEIVED,reactiveFamily=ReactiveFamily.COUNTER,priority=-999,sequence=2}}};TriggerActivationRuntime.Enqueue(action,dispatch);var order=new List<string>();var r=TriggerActivationRuntime.Drain(action,null,x=>order.Add(x.registration.id));Assert.IsTrue(r.ok);CollectionAssert.AreEqual(new[]{"C","F"},order);}
  [Test] public void SameFamilyUsesPriorityThenRegistrationSequence(){var action=new TriggerActionContext{actionId="A"};var dispatch=new BattleTriggerDispatch{registrations=new List<TriggerRegistration>{new TriggerRegistration{id="L",ownerId="O1",reactiveFamily=ReactiveFamily.COUNTER,priority=1,sequence=1},new TriggerRegistration{id="H2",ownerId="O2",reactiveFamily=ReactiveFamily.COUNTER,priority=2,sequence=5},new TriggerRegistration{id="H1",ownerId="O3",reactiveFamily=ReactiveFamily.COUNTER,priority=2,sequence=3}}};TriggerActivationRuntime.Enqueue(action,dispatch);var order=new List<string>();TriggerActivationRuntime.Drain(action,null,x=>order.Add(x.registration.id));CollectionAssert.AreEqual(new[]{"H1","H2","L"},order);}
 }
}
#endif
