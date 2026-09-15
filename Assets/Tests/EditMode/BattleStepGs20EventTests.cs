#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleStepGs20EventTests
 {
  sealed class Rng:IRandomSource{public double Next01(string purpose)=>0;}
  [Test] public void BlockHit_DispatchesOnBlock(){var s=S();var p=P();p.blockEligible=true;p.blockRate=1;p.blockCutRate=.5;p.triggerRegistrations=new[]{R("B",TriggerEvent.ON_BLOCK)};var r=BattleStepExecutor.ExecuteAttack(s,p,new Rng(),new Rng(),new Rng());Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(r.triggerDispatches.Exists(x=>x.trigger==TriggerEvent.ON_BLOCK));}
  [Test] public void LethalHit_DispatchesDeath(){var s=S();s.actors[1].hp=2;var p=P();p.triggerRegistrations=new[]{R("D",TriggerEvent.ON_DEATH)};var r=BattleStepExecutor.ExecuteAttack(s,p,new Rng(),new Rng(),null);Assert.IsTrue(r.ok,r.reason);Assert.IsFalse(s.actors[1].alive);Assert.IsTrue(r.triggerDispatches.Exists(x=>x.trigger==TriggerEvent.ON_DEATH));}
  [Test] public void NormalHit_ConsumesPersistedBarrierFifo(){var s=S();s.actors[1].barrierLayers.Add(new BarrierLayerSaveRecord{id="X",remaining=2,appliedTick=1,sequence=1});s.actors[1].barrierLayers.Add(new BarrierLayerSaveRecord{id="Y",remaining=5,appliedTick=2,sequence=1});var r=BattleStepExecutor.ExecuteAttack(s,P(),new Rng(),new Rng(),null);Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(10,s.actors[1].hp);Assert.AreEqual(1,s.actors[1].barrierLayers.Count);Assert.AreEqual("Y",s.actors[1].barrierLayers[0].id);Assert.AreEqual(3,s.actors[1].barrierLayers[0].remaining);}
  static TriggerRegistration R(string id,TriggerEvent e)=>new TriggerRegistration{id=id,trigger=e,activationChance=1};
  static BattleAttackProposal P()=>new BattleAttackProposal{reservationId="R",sourceId="A",targetId="B",skillId="S",damageType=DamageType.Physical,accuracy=100,baseDamage=4};
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.AddRange(new[]{"A","B"});s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="B",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
