#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class EffectBattleCleanupTests
 {
  [Test] public void DeathCleanup_RemovesOnlyEffectsMarkedForDeathCleanup(){var s=S();var a=s.actors[0];Apply(a,"DROP","DOT",EffectLifecycleKind.DOT,true,true);Apply(a,"KEEP","BUFF",EffectLifecycleKind.BUFF,false,true);EffectLifecycleRuntime.CleanupOnDeath(a);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual("KEEP",a.appliedEffects[0].instanceId);}
  [Test] public void BattleEnd_RemovesOnlyEffectsMarkedForBattleEndCleanup(){var s=S();var a=s.actors[0];Apply(a,"DROP","BUFF",EffectLifecycleKind.BUFF,true,true);Apply(a,"KEEP","BUFF",EffectLifecycleKind.BUFF,true,false);a.cooldowns.Add(new CooldownSaveRecord{skillId="S",remainingTicks=5});a.cast=new CastSaveRecord{skillId="S",active=true,remainingTicks=2};EffectLifecycleRuntime.CleanupBattleEnd(s);Assert.AreEqual(1,a.appliedEffects.Count);Assert.AreEqual("KEEP",a.appliedEffects[0].instanceId);Assert.AreEqual(0,a.cooldowns.Count);Assert.IsNull(a.cast);}
  static void Apply(BattleActorSaveRecord a,string id,string effectId,EffectLifecycleKind kind,bool removeOnDeath,bool removeOnBattleEnd){var r=EffectLifecycleRuntime.Apply(a,new EffectApplyRequest{instanceId=id,sourceId="X",effectId=effectId,kind=kind,stackRule=EffectStackRule.FIFO,baseDurationTicks=3,value=10,refreshRule="KEEP",snapshotPolicy="SNAPSHOT",dispelCategory=kind.ToString(),removeOnDeath=removeOnDeath,removeOnBattleEnd=removeOnBattleEnd});Assert.IsTrue(r.ok,r.reason);}
  static BattleSnapshotSaveRecord S(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="1",seed="S"};s.fixedActorOrder.Add("A");s.actors.Add(new BattleActorSaveRecord{actorId="A",hp=10,maxHp=10,alive=true});return s;}
 }
}
#endif
