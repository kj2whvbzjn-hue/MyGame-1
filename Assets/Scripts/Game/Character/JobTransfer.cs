using System;

namespace GuildAdventure.Game.Character
{
    [Serializable]
    public sealed class JobTransferHistory
    {
        public string fromJobId;
        public string toJobId;
        public int level;
        public int costGold;
        public string changedAt;
    }

    [Serializable]
    public sealed class JobTransferResult
    {
        public bool ok;
        public string reason;
        public string nextJobId;
        public JobTransferHistory history;
    }

    public static class JobTransfer
    {
        public static bool CanEditAtGuild(string phase, string activeQuestRunId)
            => phase == "base" && string.IsNullOrWhiteSpace(activeQuestRunId);

        public static JobTransferResult Prepare(
            string currentJobRef, string nextJobRef, int level,
            string phase, string activeQuestRunId, string changedAt,
            JobCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (!CanEditAtGuild(phase, activeQuestRunId))
                return Fail("guild_edit_unavailable");
            if (level < 1) return Fail("level_invalid");
            if (string.IsNullOrWhiteSpace(changedAt)) return Fail("time_required");

            var current = catalog.Resolve(currentJobRef);
            var next = catalog.Resolve(nextJobRef);
            if (current == null) return Fail("current_job_unknown");
            if (next == null) return Fail("target_job_unknown");
            if (current.id == next.id) return Fail("same_job");

            return new JobTransferResult {
                ok=true, nextJobId=next.id,
                history=new JobTransferHistory {
                    fromJobId=current.id, toJobId=next.id, level=level,
                    costGold=0, changedAt=changedAt
                }
            };
        }

        private static JobTransferResult Fail(string reason)
            => new JobTransferResult { ok=false, reason=reason };
    }
}
