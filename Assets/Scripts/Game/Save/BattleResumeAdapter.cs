using System;
using System.Collections.Generic;
using System.Linq;
using GuildAdventure.Game.Battle;
using GuildAdventure.Game.Skills;
namespace GuildAdventure.Game.Save
{
 public sealed class RestoredBattleActor{public ActionGaugeState gauge;public SkillActorState resources;public CastState cast;public List<CooldownSaveRecord> cooldowns;public List<AppliedEffect> effects;public string aiProgramId;}
 public static class BattleResumeAdapter
 {
  public static RestoredBattleActor RestoreActor(BattleActorSaveRecord a){if(a==null)throw new ArgumentNullException(nameof(a));return new RestoredBattleActor{gauge=new ActionGaugeState{actorId=a.actorId,gauge=a.actionGauge,agi=a.speed,alive=a.alive,casting=a.cast!=null&&a.cast.active},resources=new SkillActorState{actorId=a.actorId,hp=a.hp,maxHp=a.maxHp,mp=a.mp,maxMp=a.maxMp,alive=a.alive},cast=a.cast==null?null:new CastState{actorId=a.actorId,skillId=a.cast.skillId,targetId=a.cast.targetId,remainingTicks=a.cast.remainingTicks,active=a.cast.active},cooldowns=(a.cooldowns??new List<CooldownSaveRecord>()).Select(x=>new CooldownSaveRecord{skillId=x.skillId,remainingTicks=x.remainingTicks}).ToList(),effects=(a.appliedEffects??new List<AppliedEffectSaveRecord>()).Select(x=>new AppliedEffect{instanceId=x.instanceId,sourceId=x.sourceId,effectId=x.effectId,kind=ParseKind(x.kind),remainingTicks=x.remainingTicks,appliedTick=x.appliedTick,sequence=x.sequence,value=x.value,consumed=x.consumed,refreshRule=x.refreshRule,snapshotPolicy=x.snapshotPolicy,dispelCategory=x.dispelCategory,removeOnDeath=x.removeOnDeath,removeOnBattleEnd=x.removeOnBattleEnd,removable=x.removable,protectedEffect=x.protectedEffect,normalCleanseEligible=x.normalCleanseEligible,actionDisabled=x.actionDisabled}).ToList(),aiProgramId=a.aiProgramId};}
  static ApplyKind ParseKind(string kind){if(!Enum.TryParse(kind,true,out ApplyKind value))throw new InvalidOperationException("BATTLE_EFFECT_KIND_INVALID");return value;}
 }
 public sealed class ReplayRandomSource
 {
  readonly RngStreamSaveRecord state;public ReplayRandomSource(RngStreamSaveRecord state){this.state=state??throw new ArgumentNullException(nameof(state));if(state.recordedRolls==null)state.recordedRolls=new List<double>();}
  public double Next(){if(state.cursor>=state.recordedRolls.Count)throw new InvalidOperationException("RNG_REPLAY_EXHAUSTED");return state.recordedRolls[state.cursor++];}
 }
}
