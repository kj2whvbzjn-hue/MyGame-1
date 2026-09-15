#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
 public sealed class DotDamagePipelineTests
 {
  static BattleSnapshotSaveRecord Snapshot(int hp,double dot){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");var a=new BattleActorSaveRecord{actorId="A",hp=hp,maxHp=10,alive=true};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",sourceId="E",effectId="BURN",kind="DOT",remainingTicks=2,value=dot});s.actors.Add(a);return s;}
  [Test] public void Dot_ConsumesBarrierBeforeHp()
  {
   var s=Snapshot(10,5);var layers=new List<BarrierLayer>{new BarrierLayer{instanceId="X",amount=3}};var o=new BattleTickOptions{readBarriers=a=>layers,writeBarriers=(a,x)=>layers=x};var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok);Assert.AreEqual(8,s.actors[0].hp);Assert.AreEqual(3,r.barrierAbsorbed);Assert.AreEqual(2,r.dotHpLoss);
  }
  [Test] public void FatalDot_CanBePreventedAtHpCommitBoundary()
  {
   var s=Snapshot(2,5);var reg=new TriggerRegistration{id="ENDURE",trigger=TriggerEvent.ON_FATAL_DAMAGE,activationChance=1};var o=new BattleTickOptions{triggerRegistrations=new[]{reg},actionContext=new TriggerActionContext{actionId="T"},executeFatalReactive=(q,b,p)=>1};var r=BattleTickRuntime.Advance(s,o);Assert.IsTrue(r.ok);Assert.AreEqual(1,s.actors[0].hp);Assert.IsTrue(s.actors[0].alive);Assert.AreEqual(1,r.fatalPrevented);
  }
 }
}
#endif
