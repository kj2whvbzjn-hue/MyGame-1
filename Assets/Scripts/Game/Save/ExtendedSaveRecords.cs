using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Save
{
    [Serializable] public sealed class CharacterStatsSaveRecord
    {
        public int STR=10,VIT=10,AGI=10,DEX=10,INT=10,MND=10,LUK=10;
    }

    [Serializable] public sealed class GrowthHistorySaveRecord
    {
        public int level;
        public string jobId;
        public int strGain,vitGain,agiGain,dexGain,intGain,mndGain,lukGain;
        public List<double> rngRolls=new List<double>();
    }

    [Serializable] public sealed class AdventureTimelineSaveRecord
    {
        public string runId,questId;
        public int cursor;
        public List<AdventureTimelineStepSaveRecord> steps=new List<AdventureTimelineStepSaveRecord>();
    }

    [Serializable] public sealed class AdventureTimelineStepSaveRecord
    {
        public int kind;
        public string refId;
        public bool completed;
    }
}
