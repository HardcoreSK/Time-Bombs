using Verse;

namespace TimeBombs
{
    public class CompProperties_TimerSettings : CompProperties
    {
        public CompProperties_TimerSettings()
        {
            this.compClass = typeof(Comp_BombTimer);
        }

        public bool allowRemoteArming;
        public int armingDelay;
        public int disarmingDelay;
    }

    public class Comp_BombTimer : ThingComp
    {
        public CompProperties_TimerSettings Props
        {
            get
            {
                return (CompProperties_TimerSettings)this.props;
            }
        }
    }
}