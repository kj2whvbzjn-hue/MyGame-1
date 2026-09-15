using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Battle
{
    public enum FormationRow { FRONT, BACK }

    [Serializable]
    public sealed class BattleTarget
    {
        public string id;
        public bool alive=true;
        public FormationRow row;
        public int position;
        public int hp;
        public int maxHp;
    }

    public enum TargetRule
    {
        SELF,
        SINGLE_ENEMY,
        SINGLE_ALLY,
        ALL_ENEMIES,
        ALL_ALLIES,
        LOWEST_HP_ALLY
    }

    public static class FormationTargetResolver
    {
        public static List<BattleTarget> Resolve(
            BattleTarget actor,
            IEnumerable<BattleTarget> allies,
            IEnumerable<BattleTarget> enemies,
            TargetRule rule,
            Func<int,int> chooseIndex=null)
        {
            if(actor==null) throw new ArgumentNullException(nameof(actor));
            var a=(allies??Array.Empty<BattleTarget>()).Where(x=>x!=null&&x.alive).ToList();
            var e=(enemies??Array.Empty<BattleTarget>()).Where(x=>x!=null&&x.alive).ToList();

            switch(rule)
            {
                case TargetRule.SELF:return new List<BattleTarget>{actor};
                case TargetRule.ALL_ALLIES:return a;
                case TargetRule.ALL_ENEMIES:return e;
                case TargetRule.LOWEST_HP_ALLY:
                    return a.OrderBy(x=>HpRatio(x)).ThenBy(x=>x.position).Take(1).ToList();
                case TargetRule.SINGLE_ALLY:return Pick(a,chooseIndex);
                case TargetRule.SINGLE_ENEMY:
                    var front=e.Where(x=>x.row==FormationRow.FRONT).ToList();
                    return Pick(front.Count>0?front:e,chooseIndex);
                default:throw new ArgumentOutOfRangeException(nameof(rule));
            }
        }

        public static double HpRatio(BattleTarget x)
            => x.maxHp<=0?0:(double)Math.Max(0,x.hp)/x.maxHp;

        private static List<BattleTarget> Pick(List<BattleTarget> rows,Func<int,int> choose)
        {
            if(rows.Count==0)return new List<BattleTarget>();
            int i=choose==null?0:choose(rows.Count);
            if(i<0||i>=rows.Count)throw new ArgumentOutOfRangeException(nameof(choose));
            return new List<BattleTarget>{rows[i]};
        }
    }
}
