using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;

namespace GuildAdventure.Game.Battle
{
    public sealed class TriggerActivationHistory
    {
        public string passiveId,trigger,sourceId,targetId;
        public int ordinal;
    }

    public sealed class ReactiveTriggerRequest
    {
        public TriggerRegistration registration;
        public BattleTriggerContext context;
    }

    // GS-20 shared Action Context. Derived counter/follow-up/passive actions must share this instance.
    public sealed class TriggerActionContext
    {
        public string actionId;
        public int activationCount;
        public int activationLimit=16;
        public readonly HashSet<string> executingKeys=new HashSet<string>(StringComparer.Ordinal);
        public readonly List<TriggerActivationHistory> history=new List<TriggerActivationHistory>();
        public readonly Queue<ReactiveTriggerRequest> reactiveQueue=new Queue<ReactiveTriggerRequest>();
    }

    public sealed class TriggerActivationResult
    {
        public bool ok;
        public string reason;
    }

    public static class TriggerActivationRuntime
    {
        public const string LimitReached="TRIGGER_ACTION_LIMIT_REACHED";
        public const string RngPurpose="PASSIVE_TRIGGER";

        public static TriggerActivationResult CanActivate(TriggerActionContext action,string passiveId)
        {
            if(action==null)return Fail("TRIGGER_ACTION_CONTEXT_MISSING");
            if(string.IsNullOrWhiteSpace(passiveId))return Fail("TRIGGER_PASSIVE_ID_MISSING");
            if(action.activationCount>=action.activationLimit)return Fail(LimitReached);
            if(action.executingKeys.Contains(passiveId))return Fail("TRIGGER_SELF_REENTRY");
            return Ok();
        }

        // chance is [0,1]. 0/1 do not consume RNG. Failure has no Action Context side effects.
        public static TriggerActivationResult TryActivate(
            TriggerActionContext action,string passiveId,double chance,IRandomSource rng,
            string trigger=null,string sourceId=null,string targetId=null)
        {
            var can=CanActivate(action,passiveId);
            if(!can.ok)return can;
            if(double.IsNaN(chance)||double.IsInfinity(chance)||chance<0||chance>1)return Fail("TRIGGER_CHANCE_INVALID");
            if(chance<=0)return Fail("TRIGGER_CHANCE_FAILED");
            if(chance<1)
            {
                if(rng==null)return Fail("TRIGGER_RNG_MISSING");
                if(rng.Next01(RngPurpose)>=chance)return Fail("TRIGGER_CHANCE_FAILED");
            }
            action.activationCount++;
            action.executingKeys.Add(passiveId);
            action.history.Add(new TriggerActivationHistory{passiveId=passiveId,trigger=trigger,sourceId=sourceId,targetId=targetId,ordinal=action.activationCount});
            return Ok();
        }

        public static void Release(TriggerActionContext action,string passiveId)
        {
            if(action!=null&&!string.IsNullOrWhiteSpace(passiveId))action.executingKeys.Remove(passiveId);
        }

        public static void Enqueue(TriggerActionContext action,BattleTriggerDispatch dispatch)
        {
            if(action==null||dispatch==null)return;
            foreach(var registration in dispatch.registrations??new List<TriggerRegistration>())
                action.reactiveQueue.Enqueue(new ReactiveTriggerRequest{registration=registration,context=dispatch.context});
        }

        static TriggerActivationResult Ok()=>new TriggerActivationResult{ok=true};
        static TriggerActivationResult Fail(string reason)=>new TriggerActivationResult{ok=false,reason=reason};
    }
}
