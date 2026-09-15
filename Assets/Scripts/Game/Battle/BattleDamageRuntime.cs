using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Save;
namespace GuildAdventure.Game.Battle
{
 public sealed class BattleDamageResult{public int requestedDamage,barrierAbsorbed,hpDamageCandidate,hpBefore,hpAfter,actualHpLoss;public bool fatalCandidate,fatalPrevented,died;public List<BarrierLayer> remainingBarriers=new List<BarrierLayer>();}
 public static class BattleDamageRuntime
 {
  public static BattleDamageResult Commit(BattleActorSaveRecord target,int damage,Func<int,int,int?> fatalResolver=null,bool useBarrier=true){if(target==null)throw new ArgumentNullException(nameof(target));var requested=Math.Max(0,damage);var ordered=(target.barrierLayers??new List<BarrierLayerSaveRecord>()).Where(x=>x!=null).OrderBy(x=>x.appliedTick).ThenBy(x=>x.sequence).ThenBy(x=>x.id,StringComparer.Ordinal).ToList();var runtime=ordered.Select(x=>new BarrierLayer{id=x.id,remaining=x.remaining}).ToList();var barrier=useBarrier?DamageDefense.ConsumeBarrierFifo(requested,runtime):new BarrierResult{hpDamageCandidate=requested,layers=runtime};Persist(target,ordered,barrier.layers);var hp=DamageDefense.CommitHp(target.hp,barrier.hpDamageCandidate,fatalResolver);target.hp=hp.hpAfter;target.alive=target.hp>0;return new BattleDamageResult{requestedDamage=requested,barrierAbsorbed=barrier.absorbed,hpDamageCandidate=barrier.hpDamageCandidate,hpBefore=hp.hpBefore,hpAfter=hp.hpAfter,actualHpLoss=hp.actualHpLoss,fatalCandidate=hp.fatalCandidate,fatalPrevented=hp.fatalPrevented,died=hp.hpBefore>0&&hp.hpAfter<=0,remainingBarriers=barrier.layers};}
  static void Persist(BattleActorSaveRecord target,List<BarrierLayerSaveRecord> ordered,List<BarrierLayer> remaining){if(target.barrierLayers==null)return;var map=remaining.ToDictionary(x=>x.id,x=>x.remaining,StringComparer.Ordinal);target.barrierLayers=ordered.Where(x=>map.ContainsKey(x.id)&&map[x.id]>0).Select(x=>{x.remaining=map[x.id];return x;}).ToList();if(target.appliedEffects==null)return;var active=new HashSet<string>(target.barrierLayers.Select(x=>x.id),StringComparer.Ordinal);target.appliedEffects.RemoveAll(x=>x!=null&&x.kind==EffectLifecycleKind.BARRIER.ToString()&&!active.Contains(x.instanceId));}
 }
}
