#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillEffectTargetControlTests
 {
  [Test] public void CoverTargetControlCreatesFormalCoverContract()
  {
   var s=Snapshot();var covers=new List<CoverContract>();var target=s.actors[1];
   var r=SkillEffectRuntime.Execute(s,target,new SkillEffectRequest{kind=SkillEffectKind.TARGET_CONTROL,targetControlKind=TargetControlKind.COVER,sourceId="P",instanceId="CV",coverLifetime=CoverLifetimeKind.USES,coverUses=2,coverContracts=covers});
   Assert.IsTrue(r.ok,r.reason);Assert.AreEqual(1,covers.Count);Assert.AreEqual("P",covers[0].protectorId);Assert.AreEqual("T",covers[0].protectedActorId);Assert.AreEqual(2,covers[0].remainingUses);Assert.AreSame(covers[0],r.cover);
  }
  [Test] public void CoverTargetControlRejectsEnemyProtector()
  {
   var s=Snapshot();s.actors[0].teamId="E";var covers=new List<CoverContract>();
   var r=SkillEffectRuntime.Execute(s,s.actors[1],new SkillEffectRequest{kind=SkillEffectKind.TARGET_CONTROL,targetControlKind=TargetControlKind.COVER,sourceId="P",coverContracts=covers});
   Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_EFFECT_COVER_TEAM_MISMATCH",r.reason);Assert.AreEqual(0,covers.Count);
  }
  [Test] public void DurationCoverRequiresPositiveDuration()
  {
   var s=Snapshot();var covers=new List<CoverContract>();
   var r=SkillEffectRuntime.Execute(s,s.actors[1],new SkillEffectRequest{kind=SkillEffectKind.TARGET_CONTROL,targetControlKind=TargetControlKind.COVER,sourceId="P",coverLifetime=CoverLifetimeKind.DURATION,coverDurationTicks=0,coverContracts=covers});
   Assert.IsFalse(r.ok);Assert.AreEqual("SKILL_EFFECT_COVER_DURATION_INVALID",r.reason);
  }
  static BattleSnapshotSaveRecord Snapshot(){var s=new BattleSnapshotSaveRecord{battleId="B",settingsVersion="S",seed="X"};s.actors.Add(new BattleActorSaveRecord{actorId="P",teamId="P",hp=100,maxHp=100,alive=true});s.actors.Add(new BattleActorSaveRecord{actorId="T",teamId="P",hp=100,maxHp=100,alive=true});s.fixedActorOrder.AddRange(new[]{"P","T"});return s;}
 }
}
#endif
