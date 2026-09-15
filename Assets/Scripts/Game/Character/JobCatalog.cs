using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAdventure.Game.Character
{
    public sealed class JobCatalog
    {
        public const int RequiredJobCount = 7;
        public const int RequiredGrowthTotal = 60;
        private readonly Dictionary<string, JobDefinition> byId;
        private readonly Dictionary<string, JobDefinition> byName;

        public JobCatalog(IEnumerable<JobDefinition> jobs)
        {
            if (jobs == null) throw new ArgumentNullException(nameof(jobs));
            var rows = jobs.ToList();
            if (rows.Count != RequiredJobCount) throw new ArgumentException("GS-03 requires exactly 7 jobs.");

            byId = new Dictionary<string, JobDefinition>();
            byName = new Dictionary<string, JobDefinition>();
            foreach (var job in rows)
            {
                if (job == null || string.IsNullOrWhiteSpace(job.id) || string.IsNullOrWhiteSpace(job.name) || job.growth == null)
                    throw new ArgumentException("Invalid job definition.");
                if (job.growth.Total != RequiredGrowthTotal)
                    throw new ArgumentException($"Job {job.id} growth total must be 60.");
                if (byId.ContainsKey(job.id) || byName.ContainsKey(job.name))
                    throw new ArgumentException("Duplicated job id/name.");
                byId.Add(job.id, job); byName.Add(job.name, job);
            }
        }

        public JobDefinition Resolve(string idOrName)
        {
            if (string.IsNullOrWhiteSpace(idOrName)) return null;
            if (byId.TryGetValue(idOrName, out var id)) return id;
            byName.TryGetValue(idOrName, out var name);
            return name;
        }
    }
}
