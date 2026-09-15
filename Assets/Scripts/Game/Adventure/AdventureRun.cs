using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Adventure
{
    public enum AdventureRunState { ACTIVE, SUCCESS_PENDING_RETURN, RETURNED, FAILED, ABANDONED }

    [Serializable] public sealed class AdventureRunSnapshot
    {
        public string runId,questId;
        public AdventureRunState state;
        public int elapsedSeconds;
        public int temporaryExperience;
        public List<string> partyCharacterIds=new List<string>();
        public List<string> defeatedMonsterIds=new List<string>();
    }

    public static class AdventureRun
    {
        public static void Advance(AdventureRunSnapshot run,int seconds)
        {
            if(run==null)throw new ArgumentNullException(nameof(run));
            if(run.state!=AdventureRunState.ACTIVE)return;
            run.elapsedSeconds+=Math.Max(0,seconds);
        }

        public static void AddTemporaryExperience(AdventureRunSnapshot run,int exp)
        {
            if(run==null||run.state!=AdventureRunState.ACTIVE)return;
            run.temporaryExperience+=Math.Max(0,exp);
        }

        public static void MarkQuestSuccess(AdventureRunSnapshot run)
        {
            if(run==null)throw new ArgumentNullException(nameof(run));
            if(run.state!=AdventureRunState.ACTIVE)throw new InvalidOperationException("RUN_NOT_ACTIVE");
            run.state=AdventureRunState.SUCCESS_PENDING_RETURN;
        }

        public static int ConfirmReturn(AdventureRunSnapshot run)
        {
            if(run==null||run.state!=AdventureRunState.SUCCESS_PENDING_RETURN)throw new InvalidOperationException("RETURN_NOT_READY");
            int exp=run.temporaryExperience;run.temporaryExperience=0;run.state=AdventureRunState.RETURNED;return exp;
        }

        public static void FailOrAbandon(AdventureRunSnapshot run,bool abandoned)
        {
            if(run==null)throw new ArgumentNullException(nameof(run));
            run.temporaryExperience=0;
            run.state=abandoned?AdventureRunState.ABANDONED:AdventureRunState.FAILED;
        }
    }
}
