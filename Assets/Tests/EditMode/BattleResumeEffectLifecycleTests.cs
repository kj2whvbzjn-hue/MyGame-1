#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleResumeEffectLifecycleTests
 {
  [Test] public void RestoreActor_PreservesLifecycleOrderingAndCapabilities()
  {
   var actor=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};
   actor.appliedEffects.Add(new AppliedEffectSaveRecord{
    instanceId="E1",sourceId="SRC",effectId="FUTURE_DISABLE",kind="STATUS",remainingTicks=4,
    appliedTick=7,sequence=3,value=2.5,removable=false,protectedEffect=true,
    normalCleanseEligible=true,actionDisabled=true
   });
   var restored=BattleResumeAdapter.RestoreActor(actor);
   Assert.AreEqual(1,restored.effects.Count);
   var effect=restored.effects[0];
   Assert.AreEqual(7,effect.appliedTick);
   Assert.AreEqual(3,effect.sequence);
   Assert.IsFalse(effect.removable);
   Assert.IsTrue(effect.protectedEffect);
   Assert.IsTrue(effect.normalCleanseEligible);
   Assert.IsTrue(effect.actionDisabled);
   Assert.AreEqual(2.5,effect.value,1e-9);
  }
 }
}
#endif
