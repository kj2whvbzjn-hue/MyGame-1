using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    [Serializable]
    public sealed class BattleActorOrder
    {
        public string actorId;
        public int fixedOrder;
    }

    public static class BattleTurnOrder
    {
        // GS-14 corrected rule: randomize once at battle start, then keep fixed for that battle.
        public static List<BattleActorOrder> CreateFixedOrder(IEnumerable<string> actorIds,Func<double> draw)
        {
            if(actorIds==null||draw==null)throw new ArgumentNullException();
            var rows=actorIds.Where(x=>!string.IsNullOrWhiteSpace(x)).Distinct().Select(id=>new {
                id, key=draw()
            }).OrderBy(x=>x.key).ThenBy(x=>x.id,StringComparer.Ordinal).ToList();

            var result=new List<BattleActorOrder>();
            for(int i=0;i<rows.Count;i++)result.Add(new BattleActorOrder{actorId=rows[i].id,fixedOrder=i});
            return result;
        }

        public static List<string> LivingActorsInFixedOrder(
            IEnumerable<BattleActorOrder> fixedOrder,ISet<string> livingActorIds)
        {
            if(fixedOrder==null||livingActorIds==null)throw new ArgumentNullException();
            return fixedOrder.OrderBy(x=>x.fixedOrder)
                .Where(x=>livingActorIds.Contains(x.actorId))
                .Select(x=>x.actorId).ToList();
        }
    }
}
