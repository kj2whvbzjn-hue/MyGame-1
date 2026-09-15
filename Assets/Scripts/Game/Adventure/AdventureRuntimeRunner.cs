using System;
using System.Collections.Generic;
using GuildAdventure.Game.Reward;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Adventure
{
    public sealed class AdventureDispatchContext
    {
        public Func<AdventureStep,bool> runEvent;
        public Func<AdventureStep,bool> runEncounterOrBattle;
        public Func<AdventureStep,IEnumerable<RewardItem>> resolveReward;
        public Func<IEnumerable<RewardItem>,bool> commitRewards;
    }

    public sealed class AdventureDispatchResult
    {
        public bool ok;
        public string reason;
        public AdventureStep next;
    }

    public static class AdventureRuntimeRunner
    {
        public static AdventureDispatchResult DispatchCurrent(
            AdventureTimeline timeline,AdventureRunSnapshot run,AdventureDispatchContext context)
        {
            if(timeline==null||run==null||context==null)return Fail("ADVENTURE_INPUT_INVALID");
            var step=AdventureOrchestrator.Current(timeline);
            if(step==null)return Fail("ADVENTURE_STEP_MISSING");
            bool ok;
            switch(step.kind)
            {
                case AdventureStepKind.EVENT:
                    ok=context.runEvent!=null&&context.runEvent(step); break;
                case AdventureStepKind.ENCOUNTER:
                case AdventureStepKind.BATTLE:
                    ok=context.runEncounterOrBattle!=null&&context.runEncounterOrBattle(step); break;
                case AdventureStepKind.REWARD:
                    var rewards=context.resolveReward?.Invoke(step);
                    ok=rewards!=null&&context.commitRewards!=null&&context.commitRewards(rewards); break;
                case AdventureStepKind.RETURN:
                    if(!AdventureOrchestrator.IsReadyForReturn(timeline))return Fail("RETURN_NOT_READY");
                    if(run.state!=AdventureRunState.SUCCESS_PENDING_RETURN)return Fail("QUEST_SUCCESS_NOT_CONFIRMED");
                    AdventureRun.ConfirmReturn(run); ok=true; break;
                default: return Fail("ADVENTURE_STEP_UNSUPPORTED");
            }
            if(!ok)return Fail("ADVENTURE_STEP_FAILED");
            return new AdventureDispatchResult{ok=true,next=AdventureOrchestrator.CompleteCurrent(timeline)};
        }
        static AdventureDispatchResult Fail(string r)=>new AdventureDispatchResult{ok=false,reason=r};
    }
}
