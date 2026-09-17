using System;

namespace GuildAdventure.Game.Battle
{
    [Serializable]
    public sealed class ActionGaugeState
    {
        public string actorId;
        public double gauge;
        public double agi;
        public bool alive=true;
        public bool casting;
    }

    [Serializable]
    public sealed class ActionGaugeSettings
    {
        // Balance values are supplied by Export/settings. Negative sentinels mean "not supplied".
        public double maxGauge=-1d;
        public double aiReevaluationRatio=-1d;
        public double successfulActionConsumeRatio=-1d;
        public double failedExecutionConsumeRatio=-1d;
        public double agiGaugeCoefficient=-1d;
        public double actionSpeedMultiplier=-1d;

        public string Validate()
        {
            if(maxGauge<=0)return "ACTION_GAUGE_MAX_INVALID";
            if(aiReevaluationRatio<=0||aiReevaluationRatio>1)return "ACTION_GAUGE_AI_INTERVAL_INVALID";
            if(successfulActionConsumeRatio<0)return "ACTION_GAUGE_SUCCESS_CONSUME_INVALID";
            if(failedExecutionConsumeRatio<0)return "ACTION_GAUGE_FAILURE_CONSUME_INVALID";
            if(agiGaugeCoefficient<0)return "ACTION_GAUGE_AGI_COEFFICIENT_INVALID";
            if(actionSpeedMultiplier<0)return "ACTION_GAUGE_SPEED_MULTIPLIER_INVALID";
            return null;
        }

        public double AiReevaluationInterval => maxGauge*aiReevaluationRatio;
        public double SuccessfulActionConsume => maxGauge*successfulActionConsumeRatio;
        public double FailedExecutionConsume => maxGauge*failedExecutionConsumeRatio;
    }

    public static class ActionGauge
    {
        // GS-14 v1.1 defines the base formula itself: (100 + AGI) / 10.
        public static double BaseGainPerTick(double agi)
            => (100d+Math.Max(0d,agi))/10d;

        public static double GainPerTick(double agi,ActionGaugeSettings settings,double actionGaugeGainContribution=0d)
        {
            if(settings==null)throw new ArgumentNullException(nameof(settings));
            var error=settings.Validate();
            if(error!=null)throw new ArgumentException(error,nameof(settings));
            var baseGain=BaseGainPerTick(agi)*settings.agiGaugeCoefficient*settings.actionSpeedMultiplier;
            return Math.Max(0d,baseGain+actionGaugeGainContribution);
        }

        public static void Advance(ActionGaugeState state,ActionGaugeSettings settings,double actionGaugeGainContribution=0d)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(settings==null)throw new ArgumentNullException(nameof(settings));
            var error=settings.Validate();
            if(error!=null)throw new ArgumentException(error,nameof(settings));
            if(!state.alive||state.casting)return;
            state.gauge=Math.Min(settings.maxGauge,Math.Max(0d,state.gauge)+GainPerTick(state.agi,settings,actionGaugeGainContribution));
        }

        public static bool IsReady(ActionGaugeState state,ActionGaugeSettings settings)
            => state!=null&&settings!=null&&settings.Validate()==null&&state.alive&&!state.casting&&state.gauge>=settings.maxGauge;

        public static void ConsumeSuccessful(ActionGaugeState state,ActionGaugeSettings settings)
            => Consume(state,settings,settings.SuccessfulActionConsume);

        public static void ConsumeFailedExecution(ActionGaugeState state,ActionGaugeSettings settings)
            => Consume(state,settings,settings.FailedExecutionConsume);

        static void Consume(ActionGaugeState state,ActionGaugeSettings settings,double amount)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(settings==null)throw new ArgumentNullException(nameof(settings));
            var error=settings.Validate();
            if(error!=null)throw new ArgumentException(error,nameof(settings));
            state.gauge=Math.Max(0d,state.gauge-Math.Max(0d,amount));
        }
    }
}
