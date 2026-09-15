using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Character
{
    [Serializable]
    public sealed class AdventureEndState
    {
        public bool alive;
        public int hp;
    }

    [Serializable]
    public sealed class ExperienceDistribution
    {
        public int totalExperience;
        public int experiencePerRecipient;
        public int discardedRemainder;
        public Dictionary<string,int> awards = new Dictionary<string,int>();
    }

    [Serializable]
    public sealed class CharacterProgressionState
    {
        public string id;
        public string jobId;
        public int level;
        public int experience;
        public int skillPoints;
        public Dictionary<CharacterStat,int> stats;
    }

    [Serializable]
    public sealed class ProgressionResult
    {
        public bool ok;
        public string reason;
        public int levelsGained;
        public int skillPointsGained;
        public int consumedExperience;
        public int discardedExperience;
        public CharacterProgressionState next;
        public List<LevelGrowthResult> growthResults = new List<LevelGrowthResult>();
    }

    public static class ExperienceProgression
    {
        public const int InitialSkillPoints = 0;
        public const int DefaultSkillPointsPerLevel = 1;

        public static ExperienceDistribution Distribute(
            int totalExperience, IEnumerable<string> partyIds,
            IDictionary<string,AdventureEndState> endStates)
        {
            if (totalExperience < 0) throw new ArgumentOutOfRangeException(nameof(totalExperience));
            if (partyIds == null) throw new ArgumentNullException(nameof(partyIds));
            if (endStates == null) throw new ArgumentNullException(nameof(endStates));

            var eligible = new List<string>();
            var seen = new HashSet<string>();
            foreach (var id in partyIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id)) continue;
                if (endStates.TryGetValue(id, out var state) && state != null && (state.alive || state.hp > 0))
                    eligible.Add(id);
            }

            int per = eligible.Count == 0 ? 0 : totalExperience / eligible.Count;
            var result = new ExperienceDistribution {
                totalExperience=totalExperience,
                experiencePerRecipient=per,
                discardedRemainder=totalExperience - per * eligible.Count
            };
            foreach (var id in eligible) result.awards[id]=per;
            return result;
        }

        public static ProgressionResult Apply(
            CharacterProgressionState source, int awardedExperience,
            IDictionary<int,int> requiredExpByLevel, int maxLevel,
            JobCatalog jobs, GuildAdventure.Game.Core.IRandomSource random,
            int skillPointsPerLevel=DefaultSkillPointsPerLevel)
        {
            if (source == null || source.stats == null) throw new ArgumentNullException(nameof(source));
            if (requiredExpByLevel == null) throw new ArgumentNullException(nameof(requiredExpByLevel));
            if (jobs == null) throw new ArgumentNullException(nameof(jobs));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (awardedExperience < 0 || skillPointsPerLevel < 0) throw new ArgumentOutOfRangeException();

            var next = Clone(source);
            if (next.level >= maxLevel) {
                int discarded = next.experience + awardedExperience;
                next.level=maxLevel; next.experience=0;
                return new ProgressionResult { ok=true, next=next, discardedExperience=discarded };
            }

            var job=jobs.Resolve(next.jobId);
            if (job == null) return Fail("job_unknown", next);

            int pool=next.experience + awardedExperience, consumed=0, gained=0;
            var result=new ProgressionResult { ok=true, next=next };
            while (next.level < maxLevel)
            {
                if (!requiredExpByLevel.TryGetValue(next.level, out int required) || required < 1)
                    return gained == 0 ? Fail("required_exp_missing", next) : Finish(result,pool,consumed,gained,skillPointsPerLevel,maxLevel);
                if (pool < required) break;

                var growth=CharacterGrowth.ResolveLevelUp(next.level,maxLevel,next.jobId,next.stats,job.growth.ToDictionary(),random);
                pool-=required; consumed+=required; next.level++; gained++;
                foreach (var s in growth.stats) next.stats[s.stat]=s.afterValue;
                result.growthResults.Add(growth);
            }
            return Finish(result,pool,consumed,gained,skillPointsPerLevel,maxLevel);
        }

        private static ProgressionResult Finish(ProgressionResult r,int pool,int consumed,int gained,int spPer,int max)
        {
            r.levelsGained=gained; r.skillPointsGained=gained*spPer; r.consumedExperience=consumed;
            r.next.skillPoints += r.skillPointsGained;
            if (r.next.level >= max) { r.discardedExperience=pool; r.next.experience=0; }
            else r.next.experience=pool;
            return r;
        }

        private static ProgressionResult Fail(string reason, CharacterProgressionState next)
            => new ProgressionResult { ok=false, reason=reason, next=next };

        private static CharacterProgressionState Clone(CharacterProgressionState s)
            => new CharacterProgressionState {
                id=s.id, jobId=s.jobId, level=s.level, experience=s.experience, skillPoints=s.skillPoints,
                stats=new Dictionary<CharacterStat,int>(s.stats)
            };
    }
}
