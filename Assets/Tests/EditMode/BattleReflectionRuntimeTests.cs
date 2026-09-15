#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleReflectionRuntimeTests
 {
  sealed class R:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void LethalReflection_UsesFatalResolverBeforeSourceHpCommit(){var s=S(5,20);var fatalCalls=0;var p=P();p.baseDamage=10;p.reflectionRate=1;p.fatalResolver=(before,projected)=>{fatalCalls++;return 1;};var r=BattleStepExecutor.ExecuteAttack(s,p,new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(1,fatalCalls);Assert.AreEqual(1,s.actors[0].hp);Assert.IsTrue(s.actors[0].alive);Assert.AreEqual(10,r.reflectedDamage);}
  [Test] public void LethalReflectionWithoutFatal_KillsSource(){var s=S(5,20);var p=P();p.baseDamage=10;p.reflectionRate=1;var r=BattleStepExecutor.ExecuteAttack(s,p,new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(0,s.actors[0].hp);Assert.IsFalse(s.actors[0].alive);}
  [Test] public void LethalReflection_DispatchesReflectionDamageThenDeathWithHitEvents(){var s=S(5,20);var events=new List<TriggerEvent>();var p=P();p.baseDamage=10;p.reflectionRate=1;p.triggerRegistrations=new[]{new TriggerRegistration{id="D",trigger=TriggerEvent.ON_DAMAGE_DEALT,activationChance=1},new TriggerRegistration{id="X",trigger=TriggerEvent.ON_DEATH,activationChance=1}};p.executeReactive=x=>events.Add(x.registration.trigger);var r=BattleStepExecutor.ExecuteAttack(s,p,new R(),new R(),new R());Assert.IsTrue(r.ok,r.reason);CollectionAssert.AreEqual(new[]{TriggerEvent.ON_DAMAGE_DEALT,TriggerEvent.ON_DAMAGE_DEALT,TriggerEvent.ON_DEATH},events);Assert.AreEqual(2,r.triggerDispatches.FindAll(x=>x.trigger==TriggerEvent.ON_DAMAGE_DEALT).Count);Assert.AreEqual(1,r.triggerDispatches.FindAll(x=>x.trigger==TriggerEvent.ON_DEATH).Count);}
  static BattleAttackProposal P()=>new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.PHYSICAL,accuracy=100,baseDamage=1};
  static BattleSnapshotSaveRecord S(int sourceHp,int targetHp){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.fixedActorOrder.Add("B");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=sourceHp,maxHp=sourceHp,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=targetHp,maxHp=targetHp,alive=true});return s;}
 }
}
#endif
