using System;

namespace GuildAdventure.Game.Skills
{
    [Serializable]
    public sealed class CastState
    {
        public string actorId,skillId,targetId;
        public int remainingTicks;
        public bool active;
    }

    public static class CastRuntime
    {
        public static CastState Begin(string actorId,SkillDefinition skill,string targetId)
        {
            if(skill==null)throw new ArgumentNullException(nameof(skill));
            return new CastState{
                actorId=actorId,skillId=skill.id,targetId=targetId,
                remainingTicks=Math.Max(0,skill.castTicks),active=skill.castTicks>0
            };
        }

        public static bool Advance(CastState state)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(!state.active)return true;
            state.remainingTicks=Math.Max(0,state.remainingTicks-1);
            if(state.remainingTicks==0){state.active=false;return true;}
            return false;
        }

        public static void Cancel(CastState state)
        {
            if(state==null)return;
            state.active=false;state.remainingTicks=0;
        }
    }
}
