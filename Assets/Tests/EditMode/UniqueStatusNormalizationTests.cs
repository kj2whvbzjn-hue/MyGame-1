#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class UniqueStatusNormalizationTests
 {
  [Test] public void Refresh_RemovesLegacyDuplicateStatusRows(){var a=new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10};a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="OLD1",effectId="STUN",kind="STATUS",dispelCategory="STATUS",remainingTicks=1,appliedTick=1,sequence=1});a.appliedEffects.Add(new AppliedEffectSaveRecord{instanceId="OLD2",effectId="STUN",kind="STATUS",dispelCategory="STATUS",remainingTicks=2,appliedTick=2,sequence=2});var r=EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId="NEW",sourceId="S",effectId="STUN",kind=EffectLifecycleKind.STATUS,stackRule=EffectStackRule.UNIQUE_REFRESH,refreshRule="REFRESH",snapshotPolicy="SNAPSHOT",dispelCategory="STATUS",baseDurationTicks=8,statusResistancePercent=25,statusResistanceCapPercent=75,appliedTick=3,sequence=3});Assert.IsTrue(r.ok);Assert.AreEqual(1,a.appliedEffects.FindAll(x=>x.effectId=="STUN").Count);Assert.AreEqual("OLD1",r.applied.instanceId);Assert.AreEqual(6,r.applied.remainingTicks);Assert.AreEqual("S",r.applied.sourceId);}
 }
}
#endif
