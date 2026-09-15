#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class TriggerEventOwnerScopeTests
 {
  static readonly IReadOnlyDictionary<string,int> Order=new Dictionary<string,int>{{"A",0},{"B",1},{"C",2}};
  static List<TriggerRegistration> Registrations(TriggerEvent e)=>new List<TriggerRegistration>{new TriggerRegistration{id="A",ownerId="A",trigger=e},new TriggerRegistration{id="B",ownerId="B",trigger=e},new TriggerRegistration{id="C",ownerId="C",trigger=e}};
  static ResolvedHitSaveRecord Hit()=>new ResolvedHitSaveRecord{actionId="X",sourceId="A",targetId="B",judgement="HIT",actualHpLoss=10};
  [TestCase(TriggerEvent.ON_HIT_DEALT,"A")][TestCase(TriggerEvent.ON_DAMAGE_DEALT,"A")][TestCase(TriggerEvent.ON_CRITICAL,"A")]
  public void SourceOwnedEvents_OnlyResolveSourceRegistration(TriggerEvent e,string owner){var h=Hit();if(e==TriggerEvent.ON_CRITICAL)h.judgement="CRITICAL";var d=BattleEffectLifecycle.DispatchResolvedHitEvents(h,Registrations(e),Order).Find(x=>x.trigger==e);Assert.NotNull(d);Assert.AreEqual(1,d.registrations.Count);Assert.AreEqual(owner,d.registrations[0].ownerId);}
  [TestCase(TriggerEvent.ON_HIT_RECEIVED,"B")][TestCase(TriggerEvent.ON_BLOCK,"B")][TestCase(TriggerEvent.ON_DEATH,"B")]
  public void TargetOwnedEvents_OnlyResolveTargetRegistration(TriggerEvent e,string owner){var d=BattleEffectLifecycle.DispatchResolvedHitEvents(Hit(),Registrations(e),Order,blocked:e==TriggerEvent.ON_BLOCK,targetDied:e==TriggerEvent.ON_DEATH).Find(x=>x.trigger==e);Assert.NotNull(d);Assert.AreEqual(1,d.registrations.Count);Assert.AreEqual(owner,d.registrations[0].ownerId);}
  [Test] public void Evade_IsOwnedByDefendingTarget(){var h=Hit();h.judgement="MISS";var d=BattleEffectLifecycle.DispatchResolvedHitEvents(h,Registrations(TriggerEvent.ON_EVADE),Order)[0];Assert.AreEqual(1,d.registrations.Count);Assert.AreEqual("B",d.registrations[0].ownerId);}
 }
}
#endif
