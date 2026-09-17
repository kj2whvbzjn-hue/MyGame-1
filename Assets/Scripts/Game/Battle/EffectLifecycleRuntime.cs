using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public enum EffectLifecycleKind { STATUS,DOT,BUFF,DEBUFF,BARRIER }
 public enum EffectStackRule { UNIQUE_REFRESH,STACK_SUM,STACK_HIGHEST,FIFO }
 public sealed class EffectApplyRequest
 {
  public string instanceId,sourceId,effectId;
  public EffectLifecycleKind kind;
  public int baseDurationTicks;
  public double value,statusResistancePercent,statusResistanceCapPercent;
  public int appliedTick,sequence,maxStacks;
  public bool removable=true,protectedEffect,normalCleanseEligible,actionDisabled;
 }
 public sealed class EffectApplyResult{public bool ok;public string reason;public AppliedEffectSaveRecord applied;public int effectiveDurationTicks;}
 public sealed class StatusRemoveResult{public bool ok=true;public string reason;public int removedCount;public List<string> removedInstanceIds=new List<string>();}
 public static class EffectLifecycleRuntime
 {
  public const int PermanentBattleCooldown=999999;
  public static EffectStackRule Rule(EffectLifecycleKind kind,string effectId){if(kind==EffectLifecycleKind.STATUS)return EffectStackRule.UNIQUE_REFRESH;if(kind==EffectLifecycleKind.DOT)return EffectStackRule.STACK_SUM;if(kind==EffectLifecycleKind.BARRIER)return EffectStackRule.FIFO;return EffectStackRule.STACK_HIGHEST;}
  public static int EffectiveStatusDuration(int baseTicks,double resistancePercent,double resistanceCapPercent){var cap=Math.Max(0,resistanceCapPercent);var resistance=Math.Max(0,Math.Min(cap,resistancePercent));return Math.Max(0,(int)Math.Ceiling(Math.Max(0,baseTicks)*(1-resistance/100d)));}
  public static EffectApplyResult Apply(BattleActorSaveRecord actor,EffectApplyRequest request)
  {
   if(actor==null||request==null)return Fail("EFFECT_APPLY_INPUT_INVALID");
   if(string.IsNullOrWhiteSpace(request.instanceId)||string.IsNullOrWhiteSpace(request.effectId))return Fail("EFFECT_ID_MISSING");
   if(request.kind==EffectLifecycleKind.STATUS&&request.statusResistanceCapPercent<=0)return Fail("STATUS_RESISTANCE_CAP_MISSING");
   var rule=Rule(request.kind,request.effectId);
   if((rule==EffectStackRule.STACK_SUM||rule==EffectStackRule.STACK_HIGHEST)&&request.maxStacks<=0)return Fail("EFFECT_MAX_STACKS_MISSING");
   actor.appliedEffects=actor.appliedEffects??new List<AppliedEffectSaveRecord>();
   if(actor.appliedEffects.Any(x=>x!=null&&x.instanceId==request.instanceId))return Fail("EFFECT_INSTANCE_ID_DUPLICATE");
   var duration=request.kind==EffectLifecycleKind.STATUS?EffectiveStatusDuration(request.baseDurationTicks,request.statusResistancePercent,request.statusResistanceCapPercent):Math.Max(0,request.baseDurationTicks);
   var same=actor.appliedEffects.Where(x=>x!=null&&!x.consumed&&x.effectId==request.effectId).OrderBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.instanceId,StringComparer.Ordinal).ToList();
   if(rule==EffectStackRule.UNIQUE_REFRESH&&same.Count>0){var current=same[0];foreach(var duplicate in same.Skip(1))actor.appliedEffects.Remove(duplicate);current.remainingTicks=duration;current.value=request.value;current.sourceId=request.sourceId;current.appliedTick=request.appliedTick;current.sequence=request.sequence;current.removable=request.removable;current.protectedEffect=request.protectedEffect;current.normalCleanseEligible=request.normalCleanseEligible;current.actionDisabled=request.actionDisabled;return new EffectApplyResult{ok=true,applied=current,effectiveDurationTicks=duration};}
   var maxStacks=request.maxStacks>0?request.maxStacks:int.MaxValue;
   if(rule==EffectStackRule.STACK_SUM&&same.Count>=maxStacks)return Fail("EFFECT_STACK_LIMIT_REACHED");
   if(rule==EffectStackRule.STACK_HIGHEST&&same.Count>=maxStacks){var weakest=same.OrderBy(x=>x.value).ThenBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.instanceId,StringComparer.Ordinal).First();if(request.value<=weakest.value)return new EffectApplyResult{ok=true,applied=weakest,effectiveDurationTicks=weakest.remainingTicks};actor.appliedEffects.Remove(weakest);}
   var row=new AppliedEffectSaveRecord{instanceId=request.instanceId,sourceId=request.sourceId,effectId=request.effectId,kind=request.kind.ToString(),remainingTicks=duration,value=request.value,appliedTick=request.appliedTick,sequence=request.sequence,removable=request.removable,protectedEffect=request.protectedEffect,normalCleanseEligible=request.normalCleanseEligible,actionDisabled=request.actionDisabled};actor.appliedEffects.Add(row);return new EffectApplyResult{ok=true,applied=row,effectiveDurationTicks=duration};
  }
  public static EffectApplyResult ApplyBarrier(BattleActorSaveRecord actor,EffectApplyRequest request,int amount){if(actor==null||request==null||request.kind!=EffectLifecycleKind.BARRIER||amount<=0)return Fail("BARRIER_APPLY_INVALID");var applied=Apply(actor,request);if(!applied.ok)return applied;actor.barrierLayers=actor.barrierLayers??new List<BarrierLayerSaveRecord>();actor.barrierLayers.Add(new BarrierLayerSaveRecord{id=request.instanceId,sourceId=request.sourceId,effectId=request.effectId,remaining=amount,appliedTick=request.appliedTick,sequence=request.sequence});return applied;}
  public static void CleanupBarrierLayers(BattleActorSaveRecord actor){if(actor==null||actor.barrierLayers==null)return;var active=new HashSet<string>((actor.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x!=null&&!x.consumed&&x.kind==EffectLifecycleKind.BARRIER.ToString()&&x.remainingTicks>0).Select(x=>x.instanceId),StringComparer.Ordinal);actor.barrierLayers.RemoveAll(x=>x==null||x.remaining<=0||!active.Contains(x.id));}
  public static void CleanupOnDeath(BattleActorSaveRecord actor){if(actor==null)return;actor.appliedEffects?.Clear();actor.barrierLayers?.Clear();}
  public static void CleanupBattleEnd(BattleSnapshotSaveRecord snapshot){if(snapshot==null)return;foreach(var actor in snapshot.actors??new List<BattleActorSaveRecord>()){if(actor==null)continue;actor.appliedEffects?.Clear();actor.barrierLayers?.Clear();actor.cooldowns?.Clear();actor.cast=null;}}
  public static double EffectiveValue(BattleActorSaveRecord actor,string effectId,EffectLifecycleKind kind){var rows=(actor?.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x!=null&&!x.consumed&&x.effectId==effectId).ToList();if(kind==EffectLifecycleKind.BUFF||kind==EffectLifecycleKind.DEBUFF)return rows.Count==0?0:rows.Max(x=>x.value);return rows.Sum(x=>x.value);}
  static IEnumerable<AppliedEffectSaveRecord> RemovableStatuses(BattleActorSaveRecord actor,string effectId,bool normalCleanse)=>(actor?.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x!=null&&!x.consumed&&x.kind==EffectLifecycleKind.STATUS.ToString()&&x.removable&&!x.protectedEffect&&(effectId==null||x.effectId==effectId)&&(!normalCleanse||x.normalCleanseEligible)).OrderBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.instanceId,StringComparer.Ordinal);
  public static StatusRemoveResult RemoveStatus(BattleActorSaveRecord actor,int count=1,bool all=false,string effectId=null,bool normalCleanse=true){if(actor==null)return new StatusRemoveResult{ok=false,reason="STATUS_REMOVE_ACTOR_MISSING"};if(!all&&count<1)return new StatusRemoveResult{ok=false,reason="STATUS_REMOVE_COUNT_INVALID"};var candidates=RemovableStatuses(actor,effectId,normalCleanse).ToList();var take=all?candidates.Count:Math.Min(count,candidates.Count);var result=new StatusRemoveResult();for(var i=0;i<take;i++){var row=candidates[i];row.consumed=true;actor.appliedEffects.Remove(row);result.removedInstanceIds.Add(row.instanceId);}result.removedCount=result.removedInstanceIds.Count;return result;}
  public static AppliedEffectSaveRecord RemoveOldestStatus(BattleActorSaveRecord actor,string effectId=null,bool normalCleanse=true){var row=RemovableStatuses(actor,effectId,normalCleanse).FirstOrDefault();if(row!=null){row.consumed=true;actor.appliedEffects.Remove(row);}return row;}
  public static int DotDamage(double snapshotPower)=>Math.Max(0,(int)Math.Floor(snapshotPower));
  public static int HealAmount(int maxHp,double powerPercent,double magicIncreaseMultiplier=1d)=>Math.Max(0,(int)Math.Ceiling(Math.Floor(Math.Max(0,maxHp)*Math.Max(0,powerPercent)/100d)*Math.Max(0,magicIncreaseMultiplier)));
  public static int BarrierAmount(int maxHp,double powerPercent)=>Math.Max(0,(int)Math.Floor(Math.Max(0,maxHp)*Math.Max(0,powerPercent)/100d));
  static EffectApplyResult Fail(string reason)=>new EffectApplyResult{ok=false,reason=reason};
 }
}
