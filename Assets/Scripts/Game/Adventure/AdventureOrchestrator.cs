using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Adventure
{
    public enum AdventureStepKind { EVENT, ENCOUNTER, BATTLE, REWARD, RETURN }

    [Serializable]
    public sealed class AdventureStep
    {
        public AdventureStepKind kind;
        public string refId;
        public bool completed;
    }

    [Serializable]
    public sealed class AdventureTimeline
    {
        public string runId,questId;
        public int cursor;
        public List<AdventureStep> steps=new List<AdventureStep>();
    }

    public static class AdventureOrchestrator
    {
        public static AdventureStep Current(AdventureTimeline timeline)
        {
            if(timeline==null)throw new ArgumentNullException(nameof(timeline));
            return timeline.cursor>=0&&timeline.cursor<timeline.steps.Count?timeline.steps[timeline.cursor]:null;
        }

        public static AdventureStep CompleteCurrent(AdventureTimeline timeline)
        {
            var current=Current(timeline);
            if(current==null)return null;
            current.completed=true;
            timeline.cursor++;
            return Current(timeline);
        }

        public static bool IsReadyForReturn(AdventureTimeline timeline)
        {
            if(timeline==null)return false;
            for(int i=0;i<timeline.steps.Count;i++)
                if(timeline.steps[i].kind!=AdventureStepKind.RETURN&&!timeline.steps[i].completed)return false;
            return true;
        }
    }
}
