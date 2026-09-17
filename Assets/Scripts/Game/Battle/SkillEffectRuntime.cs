using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public enum SkillEffectKind { DAMAGE,HEAL,APPLY,REMOVE,TARGET_CONTROL,REVIVE }
 public sealed class SkillEffectRequest
 {
  public SkillEffectKind kind;
  public string sourceId,effectId,instanceId;
  public EffectLifecycleKind lifecycleKind;
  public EffectStackRule? stackRule;
  public int durationTicks,appliedTick,sequence,maxStacks;
  public double power,statusResistancePercent,statusResistanceCapPercent=-1d,magicIncreaseMultiplier=1d;
  public bool normalCleanse=true,removable=true,protectedEffect,normalCleanseEligible,actionDisabled;
  public TargetControlKind targetControlKind;
  public int targetControlAmount;
  public bool ignoreFrontlineRequirement;
  public double reviveHpPercent;
 }
 public sealed class SkillEffectExecutionResult{public bool ok;public string reason;public int hpDelta,removedCount;public EffectApplyResult applied;public TargetControlResult targetControl;public ReviveResult revive;}
 public static class SkillEffectRuntime
 {
  public static SkillEffectExecutionResult Execute(BattleActorSaveRecord target,SkillEffectRequest request,Func<int,int,int?> fatalResolver=null)
  {
   if(target==null||request==null)return Fail("SKILL_EFFECT_INPUT_INVALID");
   switch(request.kind)
   {
    case SkillEffectKind.DAMAGE:
     {var amount=Math.Max(0,(int)Math.Floor(request.power));var r=BattleDamageRuntime.Commit(target,amount,fatalResolver,true);return new SkillEffectExecutionResult{ok=true,hpDelta=-r.actualHpLoss};}
    case SkillEffectKind.HEAL:
     {if(!target.alive||target.hp<=0)return Fail("HEAL_TARGET_DEAD");var amount=EffectLifecycleRuntime.HealAmount(target.maxHp,request.power,request.magicIncreaseMultiplier);var before=target.hp;target.hp=Math.Min(target.maxHp,target.hp+amount);return new SkillEffectExecutionResult{ok=true,hpDelta=target.hp-before};}
    case SkillEffectKind.APPLY:
     {var a=EffectLifecycleRuntime.Apply(target,new EffectApplyRequest{instanceId=request.instanceId,sourceId=request.sourceId,effectId=request.effectId,kind=request.lifecycleKind,stackRule=request.stackRule,baseDurationTicks=request.durationTicks,value=request.power,statusResistancePercent=request.statusResistancePercent,statusResistanceCapPercent=request.statusResistanceCapPercent,appliedTick=request.appliedTick,sequence=request.sequence,maxStacks=request.maxStacks,removable=request.removable,protectedEffect=request.protectedEffect,normalCleanseEligible=request.normalCleanseEligible,actionDisabled=request.actionDisabled});return new SkillEffectExecutionResult{ok=a.ok,reason=a.reason,applied=a};}
    case SkillEffectKind.REMOVE:
     {var r=EffectLifecycleRuntime.RemoveStatus(target,1,false,request.effectId,request.normalCleanse);return new SkillEffectExecutionResult{ok=r.ok,reason=r.reason,removedCount=r.removedCount};}
    case SkillEffectKind.TARGET_CONTROL:
     {var r=ForcedMovementRuntime.Apply(target,request.targetControlKind,request.targetControlAmount);return new SkillEffectExecutionResult{ok=r.ok,reason=r.reason,targetControl=r};}
    case SkillEffectKind.REVIVE:
     {var r=ReviveRuntime.Revive(target,request.reviveHpPercent,request.ignoreFrontlineRequirement);return new SkillEffectExecutionResult{ok=r.ok,reason=r.reason,revive=r};}
    default:return Fail("SKILL_EFFECT_KIND_UNKNOWN");
   }
  }
  static SkillEffectExecutionResult Fail(string reason)=>new SkillEffectExecutionResult{ok=false,reason=reason};
 }
}
