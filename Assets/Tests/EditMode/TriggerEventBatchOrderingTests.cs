#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerEventBatchOrderingTests
 {
  [Test] public void SeparateDispatchesInSameHit_CounterRunsBeforeFollowUp(){var action=new TriggerActionContext{actionId="A"};var counter=new TriggerRegistration{id="COUNTER",trigger=TriggerEvent.ON_HIT_RECEIVED,priority=0,sequence=9};var follow=new TriggerRegistration{id="FOLLOW",trigger=TriggerEvent.ON_ALLY_ATTACK,priority=999,sequence=1};var dealt=new TriggerRegistration{id="DEALT",trigger=TriggerEvent.ON_HIT_DEALT,priority=9999,sequence=0};var dispatches=new[]{D(TriggerEvent.ON_ALLY_ATTACK,follow),D(TriggerEvent.ON_HIT_DEALT,dealt),D(TriggerEvent.ON_HIT_RECEIVED,counter)};TriggerActivationRuntime.EnqueueBatch(action,dispatches);var order=new List<string>();var r=TriggerActivationRuntime.Drain(action,null,x=>order.Add(x.registration.id));Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{"COUNTER","FOLLOW","DEALT"},order);Assert.AreEqual(1,action.reactiveEventSequence);}
  [Test] public void SeparateEnqueueCalls_RemainSeparateEvents(){var action=new TriggerActionContext{actionId="A"};TriggerActivationRuntime.Enqueue(action,D(TriggerEvent.ON_ALLY_ATTACK,new TriggerRegistration{id="FIRST",trigger=TriggerEvent.ON_ALLY_ATTACK,priority=0}));TriggerActivationRuntime.Enqueue(action,D(TriggerEvent.ON_HIT_RECEIVED,new TriggerRegistration{id="SECOND",trigger=TriggerEvent.ON_HIT_RECEIVED,priority=999}));var order=new List<string>();TriggerActivationRuntime.Drain(action,null,x=>order.Add(x.registration.id));CollectionAssert.AreEqual(new[]{"FIRST","SECOND"},order);Assert.AreEqual(2,action.reactiveEventSequence);}
  static BattleTriggerDispatch D(TriggerEvent e,TriggerRegistration r)=>new BattleTriggerDispatch{trigger=e,context=new BattleTriggerContext{sourceId="S",targetId="T"},registrations=new List<TriggerRegistration>{r}};
 }
}
#endif
