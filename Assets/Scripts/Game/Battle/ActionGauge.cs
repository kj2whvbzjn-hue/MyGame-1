using System;

namespace GuildAdventure.Game.Battle
{
    [Serializable]
    public sealed class ActionGaugeState
    {
        public string actorId;
        public double gauge;
        public double speed;
        public bool alive=true;
        public bool casting;
    }

    public static class ActionGauge
    {
        public const double ReadyThreshold=100d;

        public static void Advance(ActionGaugeState state,double tickScale=1d)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(!state.alive||state.casting)return;
            state.gauge=Math.Max(0,state.gauge)+Math.Max(0,state.speed)*Math.Max(0,tickScale);
        }

        public static bool IsReady(ActionGaugeState state)
            => state!=null&&state.alive&&!state.casting&&state.gauge>=ReadyThreshold;

        public static void Consume(ActionGaugeState state)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            state.gauge=Math.Max(0,state.gauge-ReadyThreshold);
        }
    }
}
