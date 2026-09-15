using System;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public enum SkillEffectKind{HEAL,APPLY,REMOVE,RESOURCE_CHANGE,REVIVE,TARGET_CONTROL}
 public sealed class SkillEffectRequest
 {
  public SkillEffectKind kind;public string sourceId,effectId,instanceId;public EffectLifecycleKind lifecycleKind;public int durationTicks,sequence,maxStacks;public double power,statusResistancePercent,magicIncreaseMultiplier=1d;public int hpDelta,mpDelta;public bool normalCleanse=true;
 }
 public sealed class SkillEffectResult{public bool ok;public string reason;public int hpBefore,hpAfter,mpBefore,mpAfter;public AppliedEffectSaveRecord applied,removed;public BarrierLayerSaveRecord barrier;}
 public static class SkillEffectRuntime
 {
  public static SkillEffectResult Execute(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord target,SkillEffectRequest r)
  {
   if(snapshot==null||target==null||r==null)return Fail("SKILL_EFFECT_INPUT_INVALID");var result=new SkillEffectResult{ok=true,hpBefore=target.hp,mpBefore=target.mp};
   switch(r.kind)
   {
    case SkillEffectKind.HEAL:if(!target.alive||target.hp<=0)return Fail("SKILL_EFFECT_HEAL_TARGET_DEAD");target.hp=Math.Min(target.maxHp,target.hp+EffectLifecycleRuntime.HealAmount(target.maxHp,r.power,r.magicIncreaseMultiplier));break;
    case SkillEffectKind.APPLY:
     if(r.lifecycleKind==EffectLifecycleKind.BARRIER){var amount=EffectLifecycleRuntime.BarrierAmount(target.maxHp,r.power);if(string.IsNullOrWhiteSpace(r.instanceId)||amount<=0)return Fail("SKILL_EFFECT_BARRIER_INVALID");var b=new BarrierLayerSaveRecord{id=r.instanceId,sourceId=r.sourceId,effectId=r.effectId,remaining=amount,appliedTick=snapshot.tick,sequence=r.sequence};target.barrierLayers.Add(b);result.barrier=b;break;}
     var applied=EffectLifecycleRuntime.Apply(target,new EffectApplyRequest{instanceId=r.instanceId,sourceId=r.sourceId,effectId=r.effectId,kind=r.lifecycleKind,baseDurationTicks=r.durationTicks,value=r.power,statusResistancePercent=r.statusResistancePercent,appliedTick=snapshot.tick,sequence=r.sequence,maxStacks=r.maxStacks});if(!applied.ok)return Fail(applied.reason);result.applied=applied.applied;break;
    case SkillEffectKind.REMOVE:result.removed=EffectLifecycleRuntime.RemoveOldestStatus(target,r.effectId,r.normalCleanse);if(result.removed==null)return Fail("SKILL_EFFECT_REMOVE_NOT_FOUND");break;
    case SkillEffectKind.RESOURCE_CHANGE:target.hp=Math.Max(0,Math.Min(target.maxHp,target.hp+r.hpDelta));target.mp=Math.Max(0,Math.Min(target.maxMp,target.mp+r.mpDelta));target.alive=target.hp>0;break;
    case SkillEffectKind.REVIVE:if(target.alive&&target.hp>0)return Fail("SKILL_EFFECT_REVIVE_TARGET_ALIVE");target.hp=Math.Max(1,Math.Min(target.maxHp,EffectLifecycleRuntime.HealAmount(target.maxHp,r.power)));target.alive=true;break;
    case SkillEffectKind.TARGET_CONTROL:return Fail("SKILL_EFFECT_TARGET_CONTROL_NOT_IMPLEMENTED");
    default:return Fail("SKILL_EFFECT_KIND_UNSUPPORTED");
   }
   result.hpAfter=target.hp;result.mpAfter=target.mp;return result;
  }
  static SkillEffectResult Fail(string reason)=>new SkillEffectResult{ok=false,reason=reason};
 }
}
