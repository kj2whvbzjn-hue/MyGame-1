using System;
namespace GuildAdventure.Game.Skills
{
 public sealed class SkillTimingModifiers
 {
  public double mpCost,castTicks,cooldownTicks;
 }
 public sealed class EffectiveSkillTiming
 {
  public int mpCost,castTicks,cooldownTicks;
 }
 public static class SkillTimingModifierRuntime
 {
  public static EffectiveSkillTiming Resolve(SkillDefinition skill,SkillTimingModifiers modifiers)
  {
   if(skill==null)throw new ArgumentNullException(nameof(skill));
   var m=modifiers??new SkillTimingModifiers();
   return new EffectiveSkillTiming{
    mpCost=skill.resource==SkillResourceKind.MP?Final(skill.resourceCost,m.mpCost):skill.resourceCost,
    castTicks=Final(skill.castTicks,m.castTicks),
    cooldownTicks=Final(skill.cooldownTicks,m.cooldownTicks)
   };
  }
  static int Final(int baseValue,double contribution)
  {
   if(double.IsNaN(contribution)||double.IsInfinity(contribution))throw new ArgumentOutOfRangeException(nameof(contribution));
   return Math.Max(0,(int)Math.Ceiling(baseValue+contribution));
  }
 }
}
