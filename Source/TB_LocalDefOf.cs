using Verse;
using RimWorld;
using UnityEngine;

namespace TimeBombs
{
    [DefOf]
    public static class TB_LocalDefOf
    {
        static TB_LocalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TB_LocalDefOf));
		}

        public static DesignationDef OperateBombTimerDes;
        public static WorkGiverDef OperateBombTimer;
        public static JobDef ArmDetonatorJob;
        public static JobDef DisarmDetonatorJob;
    }

    [StaticConstructorOnStartup]
    public static class OverlayContainer
    {
        public static readonly Material DetonatorMat = MaterialPool.MatFrom("Things/Overlay/DetonatorOverlay", ShaderDatabase.MetaOverlay);
    }
}