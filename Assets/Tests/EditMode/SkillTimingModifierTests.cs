#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Tests.EditMode
{
 public sealed class SkillTimingModifierTests
 {
  [Test] public void FinalValues_AddContributionsThenCeil(){var s=new SkillDefinition{resource=SkillResourceKind.MP,resourceCost=10,castTicks=4,cooldownTicks=8};var r=SkillTimingModifierRuntime.Resolve(s,new SkillTimingModifiers{mpCost=-2.4,castTicks=-1.2,cooldownTicks=1.1});Assert.AreEqual(8,r.mpCost);Assert.AreEqual(3,r.castTicks);Assert.AreEqual(10,r.cooldownTicks);}
  [Test] public void Reductions_CannotMakeFinalValuesNegative(){var s=new SkillDefinition{resource=SkillResourceKind.MP,resourceCost=5,castTicks=2,cooldownTicks=3};var r=SkillTimingModifierRuntime.Resolve(s,new SkillTimingModifiers{mpCost=-99,castTicks=-99,cooldownTicks=-99});Assert.AreEqual(0,r.mpCost);Assert.AreEqual(0,r.castTicks);Assert.AreEqual(0,r.cooldownTicks);}
 }
}
#endif
