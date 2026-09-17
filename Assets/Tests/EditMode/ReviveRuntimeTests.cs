#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class ReviveRuntimeTests
 {
  [Test] public void ReviveRestoresLifeResourcesClearsEffectsThenRunsReviveEffect()
  {
   var s=Snapshot();var a=s.actors[0];a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="ST",effectId="STUN",kind="STATUS",remainingTicks=3});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="BF",effectId="ATK_UP",kind="BUFF",remainingTicks=3});a.barrierLayers.Add(new BarrierLayerSaveRecord{id="BR",remaining=5});a.cast=new CastSaveRecord{active=true,reservationId="OLD"};bool sawClean=false;
   var r=ReviveRuntime.Execute(s,new ReviveRequest{targetId="A",hpAmount=30,mpAmount=7,reviveEffect=x=>{sawClean=x.alive&&x.hp==30&&x.mp==7&&x.appliedEffects.Count==0&&x.barrierLayers.Count==0&&x.cast==null;}});
   Assert.IsTrue(r.ok,r.reason);Assert.IsTrue(r.revived);Assert.IsTrue(sawClean);Assert.IsTrue(a.alive);Assert.AreEqual(30,a.hp);Assert.AreEqual(7,a.mp);
  }
  [Test] public void LaterReviveFizzlesAfterSuccessfulRevive()
  {
   var s=Snapshot();var first=ReviveRuntime.Execute(s,new ReviveRequest{targetId="A",hpAmount=20,mpAmount=2});var second=ReviveRuntime.Execute(s,new ReviveRequest{targetId="A",hpAmount=80,mpAmount=9});Assert.IsTrue(first.revived);Assert.IsTrue(second.ok);Assert.IsFalse(second.revived);Assert.AreEqual(20,s.actors[0].hp);Assert.AreEqual(2,s.actors[0].mp);
  }
  [Test] public void ReviveClampsResourcesAndNeverReturnsAtZeroHp(){var s=Snapshot();var r=ReviveRuntime.Execute(s,new ReviveRequest{targetId="A",hpAmount=0,mpAmount=99});Assert.IsTrue(r.revived);Assert.AreEqual(1,s.actors[0].hp);Assert.AreEqual(10,s.actors[0].mp);}
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="A",alive=false,hp=0,maxHp=100,mp=0,maxMp=10});s.fixedActorOrder.Add("A");return s;}
 }
}
#endif
