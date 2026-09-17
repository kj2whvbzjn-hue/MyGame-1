using System;
using UnityEngine;
using GuildAdventure.Game.Battle;

namespace GuildAdventure.Game.Data
{
    [Serializable] public sealed class AdventureSettingsFile { public AdventureSettingsRow[] data; }
    [Serializable] public sealed class AdventureSettingsRow { public string id; public string status; public bool enabled; public AdventureSettingsParams @params; }
    [Serializable] public sealed class AdventureSettingsParams { public AdventureGameRuntime game_runtime; }
    [Serializable] public sealed class AdventureGameRuntime { public AdventureBattleFlow battle_flow; }
    [Serializable] public sealed class AdventureBattleFlow { public AdventureActionGauge action_gauge; }
    [Serializable] public sealed class AdventureActionGauge
    {
        public double max;
        public double ai_reevaluation_ratio;
        public double successful_action_consume_ratio;
        public double failed_execution_consume_ratio;
        public double agi_gauge_coefficient=1d;
        public double action_speed_multiplier=1d;
    }

    public static class AdventureBattleSettingsJsonLoader
    {
        public static ActionGaugeSettings LoadActionGaugeSettings(string json)
        {
            if(string.IsNullOrWhiteSpace(json)) throw new ArgumentException("adventure_settings.json is empty.");
            var file=JsonUtility.FromJson<AdventureSettingsFile>(json);
            if(file==null||file.data==null) throw new ArgumentException("adventure_settings.json data is missing.");
            AdventureActionGauge source=null;
            foreach(var row in file.data)
            {
                if(row==null||row.status!="active"||!row.enabled) continue;
                source=row.@params?.game_runtime?.battle_flow?.action_gauge;
                if(source!=null) break;
            }
            if(source==null) throw new ArgumentException("game_runtime.battle_flow.action_gauge is missing.");
            var settings=new ActionGaugeSettings
            {
                maxGauge=source.max,
                aiReevaluationRatio=source.ai_reevaluation_ratio,
                successfulActionConsumeRatio=source.successful_action_consume_ratio,
                failedExecutionConsumeRatio=source.failed_execution_consume_ratio,
                agiGaugeCoefficient=source.agi_gauge_coefficient,
                actionSpeedMultiplier=source.action_speed_multiplier
            };
            var error=settings.Validate();
            if(error!=null) throw new ArgumentException(error);
            return settings;
        }
    }
}
