using RimWorld;
using Verse;

namespace TimeBombs
{
    public class CompTargetEffect_AddDetonator : CompTargetEffect
    {
        public CompProperties_TimerSettings Props
        {
            get
            {
                return (CompProperties_TimerSettings)this.props;
            }
        }

        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (target is ThingWithComps t)
            {
                var detonator = t.GetDetonator();
                detonator.allowRemoteArming = Props.allowRemoteArming;
                detonator.armingDelay = Props.armingDelay;
                detonator.disarmingDelay = Props.disarmingDelay;
                detonator.unveiled = true;
            }
        }
    }
}