using System;
using System.Collections.Generic;

namespace GuildAdventure.Game.Save
{
    [Serializable] public sealed class ActionReservationSaveRecord
    {
        public string contract="C02";
        public int schemaVersion=1;
        public string reservationId,actorId,skillId;
        public int startTick,completeTick;
        public List<string> fixedTargetIds=new List<string>();
        public UsageConditionsSaveRecord usageConditions=new UsageConditionsSaveRecord();
    }

    [Serializable] public sealed class UsageConditionsSaveRecord
    {
        public string json="{}"; // opaque snapshot: Unity must not invent semantics for Studio usage_conditions.
    }

    [Serializable] public sealed class ResolvedHitSaveRecord
    {
        public string contract="C03";
        public int schemaVersion=1;
        public string actionId,sourceId,targetId,judgement;
        public int hitIndex;
        public double perHitDamage,committedHp,actualHpLoss;
        public OpaqueContractPayload block=new OpaqueContractPayload();
        public OpaqueContractPayload barrier=new OpaqueContractPayload();
        public OpaqueContractPayload triggerContext=new OpaqueContractPayload();

        // Unity diagnostic extension; not part of Studio C03 envelope.
        public List<double> rngRolls=new List<double>();
    }

    [Serializable] public sealed class OpaqueContractPayload { public string json="{}"; }
}
