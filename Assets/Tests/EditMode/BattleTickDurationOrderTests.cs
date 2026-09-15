#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class BattleTickDurationOrderTests
 {
  [Test] public void OneTickDot_UpdatesDurationThenAppliesPeriodicThenExpires(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="D",sourceId="S",effectId="BURN",kind=EffectLifecycleKind.DOT.ToString(),remainingTicks=1,value=3,appliedTick=0,sequence=0});s.actors.Add(a);s.fixedActorOrder.Add("A");var r=BattleTickRuntime.Advance(s,new BattleTickOptions());Assert.IsTrue(r.ok);Assert.AreEqual(7,a.hp);CollectionAssert.Contains(r.dotSources,"D");CollectionAssert.Contains(r.expiredEffects,"D");Assert.AreEqual(0,a.appliedEffects.Count);}
 }
}
#endif
