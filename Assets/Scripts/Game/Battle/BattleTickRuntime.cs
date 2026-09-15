using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.AI;
namespace GuildAdventure.Game.Battle
{
 public sealed class BattleTickOptions
 {
  public IEnumerable<TriggerRegistration> triggerRegistrations;public TriggerActionContext actionContext;public IRandomSource passiveTriggerRng;public Func<ReactiveTriggerRequest,int,int,int?> executeFatalReactive;public Func<BattleActorSaveRecord,List<BarrierLayer>> readBarriers;public Action<BattleActorSaveRecord,List<BarrierLayer>> writeBarriers;
  public Func<BattleActorSaveRecord,FormalAiBattleContext> resolveAi;public IRandomSource criticalRng,hitRng,blockRng;
 }
 public sealed class BattleTickResult
 {
  public bool ok=true;public string reason;public int tick;public List<string> expiredEffects=new List<string>(),dotSources=new List<string>(),aiEvaluated=new List<string>(),actionsStarted=new List<string>(),actionsSkipped=new List<string>();public int dotHpLoss,barrierAbsorbed,fatalPrevented;
 }
 public static class BattleTickRuntime
 {
  public static BattleTickResult Advance(BattleSnapshotSaveRecord snapshot)=>Advance(snapshot,null);
  public static BattleTickResult Advance(BattleSnapshotSaveRecord snapshot,BattleTickOptions options)
  {
   if(snapshot==null)return Fail("BATTLE_TICK_SNAPSHOT_MISSING");var validation=BattleSnapshotValidation.Validate(snapshot);if(validation!=null)return Fail(validation);var result=new BattleTickResult{tick=snapshot.tick};var gaugeSettings=new ActionGaugeSettings();var fixedOrder=BattleEffectLifecycle.BuildFixedOrder(snapshot);var context=options?.actionContext??new TriggerActionContext{actionId="TICK:"+snapshot.tick};
   foreach(var id in snapshot.fixedActorOrder){var actor=snapshot.actors.Find(x=>x.actorId==id);if(actor==null)continue;TickPeriodic(actor,result,options,context,fixedOrder);TickCooldowns(actor);ExpireEffects(actor,result);var gauge=new ActionGaugeState{actorId=actor.actorId,gauge=actor.actionGauge,agi=actor.speed,alive=actor.alive&&actor.hp>0,casting=actor.cast?.active??false};ActionGauge.Advance(gauge,gaugeSettings);actor.actionGauge=gauge.gauge;TryAiAction(snapshot,actor,result,options);}
   snapshot.tick++;result.tick=snapshot.tick;return result;
  }
  static void TryAiAction(BattleSnapshotSaveRecord snapshot,BattleActorSaveRecord actor,BattleTickResult result,BattleTickOptions options)
  {
   if(options?.resolveAi==null||!actor.alive||actor.hp<=0||(actor.cast?.active??false)||actor.actionGauge<new ActionGaugeSettings().maxGauge)return;var ai=options.resolveAi(actor);if(ai==null)return;result.aiEvaluated.Add(actor.actorId);var decision=FormalAiBattleBridge.Decide(snapshot,actor,ai);if(!decision.ok){result.actionsSkipped.Add(actor.actorId+":"+decision.reason);return;}var action=SkillActionTransaction.Execute(snapshot,decision.request,options.criticalRng,options.hitRng,options.blockRng);if(!action.ok){result.actionsSkipped.Add(actor.actorId+":"+action.reason);return;}result.actionsStarted.Add(actor.actorId+":"+decision.request.skill.id+(action.castingStarted?":CAST":":INSTANT"));
  }
  static void TickPeriodic(BattleActorSaveRecord actor,BattleTickResult result,BattleTickOptions options,TriggerActionContext context,IReadOnlyDictionary<string,int> fixedOrder){foreach(var e in (actor.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x!=null&&!x.consumed&&x.remainingTicks>0).OrderBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.instanceId,StringComparer.Ordinal)){if(e.kind==EffectLifecycleKind.DOT.ToString()&&actor.alive&&actor.hp>0){var damage=EffectLifecycleRuntime.DotDamage(e.value);var barriers=options?.readBarriers?.Invoke(actor)??new List<BarrierLayer>();var barrier=DamageDefense.ConsumeBarrierFifo(damage,barriers);result.barrierAbsorbed+=barrier.absorbed;options?.writeBarriers?.Invoke(actor,barrier.layers);Func<int,int,int?> fatal=null;if(options?.executeFatalReactive!=null)fatal=(before,projected)=>{var fr=FatalDamageTriggerRuntime.Resolve(before,projected,e.sourceId,e.sourceId,actor.actorId,e.effectId,0,options.triggerRegistrations,fixedOrder,context,options.passiveTriggerRng,options.executeFatalReactive);if(fr.prevented)result.fatalPrevented++;return fr.prevented?(int?)fr.hpAfter:null;};var hp=DamageDefense.CommitHp(actor.hp,barrier.hpDamageCandidate,fatal);actor.hp=hp.hpAfter;actor.alive=actor.hp>0;result.dotHpLoss+=hp.actualHpLoss;result.dotSources.Add(e.instanceId);}e.remainingTicks--;}}
  static void TickCooldowns(BattleActorSaveRecord actor){foreach(var c in actor.cooldowns??new List<CooldownSaveRecord>())if(c!=null&&c.remainingTicks>0)c.remainingTicks--;actor.cooldowns?.RemoveAll(x=>x==null||x.remainingTicks<=0);}
  static void ExpireEffects(BattleActorSaveRecord actor,BattleTickResult result){var expired=(actor.appliedEffects??new List<AppliedEffectSaveRecord>()).Where(x=>x==null||x.consumed||x.remainingTicks<=0).ToList();foreach(var e in expired)if(e!=null)result.expiredEffects.Add(e.instanceId);actor.appliedEffects?.RemoveAll(x=>x==null||x.consumed||x.remainingTicks<=0);}
  static BattleTickResult Fail(string reason)=>new BattleTickResult{ok=false,reason=reason};
 }
}
