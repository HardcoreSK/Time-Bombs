using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TimeBombs
{
    public class WorkGiver_OperateBombTimer : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Designation> desList = pawn.Map.designationManager.AllDesignations;
            for (int i = 0; i < desList.Count; i++)
            {
                if (desList[i].def == TB_LocalDefOf.OperateBombTimerDes)
                {
                    yield return desList[i].target.Thing;
                }
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(TB_LocalDefOf.OperateBombTimerDes);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return pawn.Map.designationManager.DesignationOn(t, TB_LocalDefOf.OperateBombTimerDes) != null
                        && pawn.CanReserve(t, 1, -1, null, forced)
                        && TB_Utils.NeedToOperate(t);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (TB_Utils.NeedToOperate(t))
            {
                var detonator = TB_Utils.GetDetonator(t);
                if (detonator.wantsToBeArmed)
                    return JobMaker.MakeJob(TB_LocalDefOf.ArmDetonatorJob, t);
                if (detonator.wantsToBeDisarmed)
                    return JobMaker.MakeJob(TB_LocalDefOf.DisarmDetonatorJob, t);
            }
            return JobMaker.MakeJob(JobDefOf.Wait_Wander, t);
        }
    }
}