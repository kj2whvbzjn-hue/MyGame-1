using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Game.Battle
{
    public sealed class FatalDamageTriggerResult
    {
        public bool prevented;
        public int hpAfter;
        public int executed,skipped;
        public string stopReason;
    }

    public static class FatalDamageTriggerRuntime
    {
        // Synchronous GS-20 interrupt: after Block/Barrier, immediately before HP commit.
        // A reactive handler may return a replacement HP. Null means this passive did not prevent death.
        public static FatalDamageTriggerResult Resolve(
            int hpBefore,int projectedHp,string sourceId,string actionSourceId,string targetId,string skillId,int hitIndex,
            IEnumerable<TriggerRegistration> registrations,IReadOnlyDictionary<string,int> fixedOrder,
            TriggerActionContext actionContext,IRandomSource rng,Func<ReactiveTriggerRequest,int,int,int?> execute)
        {
            var result=new FatalDamageTriggerResult{hpAfter=Math.Max(0,projectedHp)};
            if(projectedHp>0)return result;
            var context=new BattleTriggerContext{actionId=actionContext?.actionId,sourceId=sourceId,actionSourceId=actionSourceId,
                targetId=targetId,skillId=skillId,hitIndex=hitIndex,actualHpLoss=Math.Max(0,hpBefore)};
            var dispatch=new BattleTriggerDispatch{trigger=TriggerEvent.ON_FATAL_DAMAGE,context=context,
                registrations=TriggerRuntime.Resolve(registrations,TriggerEvent.ON_FATAL_DAMAGE,fixedOrder)};
            foreach(var registration in dispatch.registrations)
            {
                var activated=TriggerActivationRuntime.TryActivate(actionContext,registration.id,registration.activationChance,rng,
                    TriggerEvent.ON_FATAL_DAMAGE.ToString(),sourceId,targetId);
                if(!activated.ok)
                {
                    if(activated.reason==TriggerActivationRuntime.LimitReached){result.stopReason=activated.reason;break;}
                    result.skipped++;continue;
                }
                try
                {
                    TriggerRuntime.MarkConsumed(registration);
                    var replacement=execute?.Invoke(new ReactiveTriggerRequest{registration=registration,context=context},hpBefore,projectedHp);
                    result.executed++;
                    if(replacement.HasValue&&replacement.Value>0){result.prevented=true;result.hpAfter=replacement.Value;break;}
                }
                finally { TriggerActivationRuntime.Release(actionContext,registration.id); }
            }
            return result;
        }
    }
}
