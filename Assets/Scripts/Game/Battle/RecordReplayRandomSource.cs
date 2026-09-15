using System;
using System.Collections.Generic;
using GuildAdventure.Game.Core;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Game.Battle
{
    public sealed class RecordingRandomSource : IRandomSource
    {
        readonly IRandomSource inner;
        readonly RngStreamSaveRecord stream;

        public RecordingRandomSource(IRandomSource inner,RngStreamSaveRecord stream)
        {
            this.inner=inner??throw new ArgumentNullException(nameof(inner));
            this.stream=stream??throw new ArgumentNullException(nameof(stream));
            if(string.IsNullOrWhiteSpace(stream.purpose))throw new ArgumentException("RNG_PURPOSE_REQUIRED");
            if(stream.recordedRolls==null)stream.recordedRolls=new List<double>();
        }

        public double Next01(string purpose)
        {
            if(string.IsNullOrWhiteSpace(purpose))throw new ArgumentException("RNG_PURPOSE_REQUIRED",nameof(purpose));
            if(!string.Equals(stream.purpose,purpose,StringComparison.Ordinal))throw new InvalidOperationException("RNG_PURPOSE_MISMATCH");
            var value=inner.Next01(purpose);
            if(value<0||value>=1)throw new InvalidOperationException("RNG_VALUE_OUT_OF_RANGE");
            stream.recordedRolls.Add(value);
            stream.cursor=stream.recordedRolls.Count;
            return value;
        }
    }

    public sealed class ReplayRandomAdapter : IRandomSource
    {
        readonly RngStreamSaveRecord stream;
        public ReplayRandomAdapter(RngStreamSaveRecord stream)
        {
            this.stream=stream??throw new ArgumentNullException(nameof(stream));
        }
        public double Next01(string purpose)
        {
            if(string.IsNullOrWhiteSpace(purpose))throw new ArgumentException("RNG_PURPOSE_REQUIRED",nameof(purpose));
            if(!string.Equals(stream.purpose,purpose,StringComparison.Ordinal))throw new InvalidOperationException("RNG_PURPOSE_MISMATCH");
            if(stream.recordedRolls==null||stream.cursor>=stream.recordedRolls.Count)
                throw new InvalidOperationException("RNG_REPLAY_EXHAUSTED");
            return stream.recordedRolls[stream.cursor++];
        }
    }

    public static class BattleRngStreams
    {
        public static RngStreamSaveRecord Require(BattleSnapshotSaveRecord snapshot,string purpose)
        {
            if(snapshot==null||string.IsNullOrWhiteSpace(purpose))throw new ArgumentNullException();
            foreach(var x in snapshot.rngStreams)
                if(x.purpose==purpose)return x;
            var created=new RngStreamSaveRecord{purpose=purpose};
            snapshot.rngStreams.Add(created);
            return created;
        }
    }
}
