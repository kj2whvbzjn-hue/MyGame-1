#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerReactiveOrderingTests
 {
  [Test] public void SameEvent_CounterRunsBeforeFollowUpRegardlessOfPriority(){var a=new TriggerActionContext{actionId="A"};var d=new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{R("F",TriggerEvent.ON_ALLY_ATTACK,99,1),R("C2",TriggerEvent.ON_HIT_RECEIVED,1,2),R("C1",TriggerEvent.ON_HIT_RECEIVED,2,3)}};TriggerActivationRuntime.Enqueue(a,d);var seen=new List<string>();TriggerActivationRuntime.Drain(a,null,x=>seen.Add(x.registration.id));CollectionAssert.AreEqual(new[]{"C1","C2","F"},seen);}
  [Test] public void EarlierEventCompletesBeforeLaterEvent(){var a=new TriggerActionContext{actionId="A"};TriggerActivationRuntime.Enqueue(a,new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{R("FIRST",TriggerEvent.ON_ALLY_ATTACK,1,1)}});TriggerActivationRuntime.Enqueue(a,new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{R("SECOND",TriggerEvent.ON_HIT_RECEIVED,999,1)}});var seen=new List<string>();TriggerActivationRuntime.Drain(a,null,x=>seen.Add(x.registration.id));CollectionAssert.AreEqual(new[]{"FIRST","SECOND"},seen);}
  [Test] public void RegistrationsWithinFamily_ArePriorityThenSequence(){var a=new TriggerActionContext{actionId="A"};TriggerActivationRuntime.Enqueue(a,new BattleTriggerDispatch{context=new BattleTriggerContext(),registrations=new List<TriggerRegistration>{R("L",TriggerEvent.ON_HIT_RECEIVED,1,1),R("H2",TriggerEvent.ON_HIT_RECEIVED,2,2),R("H1",TriggerEvent.ON_HIT_RECEIVED,2,1)}});var seen=new List<string>();TriggerActivationRuntime.Drain(a,null,x=>seen.Add(x.registration.id));CollectionAssert.AreEqual(new[]{"H1","H2","L"},seen);}
  static TriggerRegistration R(string id,TriggerEvent e,int p,int s)=>new TriggerRegistration{id=id,trigger=e,priority=p,sequence=s,activationChance=1};
 }
}
#endif
