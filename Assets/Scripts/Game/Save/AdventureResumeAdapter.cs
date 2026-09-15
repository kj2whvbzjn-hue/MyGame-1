using System;
using GuildAdventure.Game.Adventure;

namespace GuildAdventure.Game.Save
{
    public static class AdventureResumeAdapter
    {
        public static AdventureTimelineSaveRecord ToSave(AdventureTimeline t)
        {
            if(t==null)return null;
            var r=new AdventureTimelineSaveRecord{runId=t.runId,questId=t.questId,cursor=t.cursor};
            foreach(var s in t.steps)
                r.steps.Add(new AdventureTimelineStepSaveRecord{kind=(int)s.kind,refId=s.refId,completed=s.completed});
            return r;
        }

        public static AdventureTimeline FromSave(AdventureTimelineSaveRecord s)
        {
            if(s==null)return null;
            var t=new AdventureTimeline{runId=s.runId,questId=s.questId,cursor=s.cursor};
            foreach(var x in s.steps)
            {
                if(!Enum.IsDefined(typeof(AdventureStepKind),x.kind))throw new InvalidOperationException("ADVENTURE_STEP_KIND_INVALID");
                t.steps.Add(new AdventureStep{kind=(AdventureStepKind)x.kind,refId=x.refId,completed=x.completed});
            }
            if(t.cursor<0||t.cursor>t.steps.Count)throw new InvalidOperationException("ADVENTURE_CURSOR_INVALID");
            return t;
        }
    }
}
