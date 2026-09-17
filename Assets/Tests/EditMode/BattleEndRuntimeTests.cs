#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleEndRuntimeTests
 {
  sealed class ZeroRng:IRandomSource{public double Next01(string purpose)=>0d;}
  [Test] public void Resolve_DispatchesEndTriggerBeforeCleanup()
  {
   var snapshot=new BattleSnapshotSaveRecord();
   snapshot.fixedActorOrder.Add("A");
   var actor=new BattleActorSaveRecord{actorId="A",alive=true,hp=10,maxHp=10,cast=new CastSaveRecord{active=true,remainingTicks=2}};
   actor.cooldowns.Add(new CooldownSaveRecord{skillId="S",remainingTicks=4});
   actor.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="E",effectId="BUFF",kind=EffectLifecycleKind.BUFF.ToString(),remainingTicks=3});
   snapshot.actors.Add(actor);
   var registrations=new[]{new TriggerRegistration{id="END",ownerId="A",trigger=TriggerEvent.ON_BATTLE_END}};
   var sawStateBeforeCleanup=false;
   var result=BattleEndRuntime.Resolve(snapshot,registrations,new TriggerActionContext{actionId="END:1"},new ZeroRng(),request=>
   {
    sawStateBeforeCleanup=actor.cooldowns.Count==1&&actor.appliedEffects.Count==1&&actor.cast!=null;
   });
   Assert.IsTrue(result.ok,result.reason);
   Assert.IsTrue(sawStateBeforeCleanup);
   Assert.AreEqual(0,actor.cooldowns.Count);
   Assert.AreEqual(0,actor.appliedEffects.Count);
   Assert.IsNull(actor.cast);
  }
 }
}
#endif
