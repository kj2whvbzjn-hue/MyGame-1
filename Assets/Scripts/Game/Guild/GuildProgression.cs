using System;

namespace GuildAdventure.Game.Guild
{
    [Serializable]
    public sealed class GuildState
    {
        public int rank=1;
        public int points;
        public int memberCapacity;
        public int warehouseCapacity;
    }

    [Serializable]
    public sealed class GuildRankRule
    {
        public int rank,requiredPoints,memberCapacity,warehouseCapacity;
    }

    public static class GuildProgression
    {
        public static bool CanRankUp(GuildState state,GuildRankRule next)
            => state!=null&&next!=null&&next.rank==state.rank+1&&state.points>=next.requiredPoints;

        public static GuildState RankUp(GuildState state,GuildRankRule next)
        {
            if(!CanRankUp(state,next))throw new InvalidOperationException("GUILD_RANK_UP_INVALID");
            return new GuildState{
                rank=next.rank,
                points=state.points-next.requiredPoints,
                memberCapacity=next.memberCapacity,
                warehouseCapacity=next.warehouseCapacity
            };
        }
    }
}
