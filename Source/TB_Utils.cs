using Verse;
using RimWorld;
using CombatExtended;

namespace TimeBombs
{
    public static class TB_Utils
    {
        public static Comp_Detonator GetDetonator(this Thing thing)
		{
			if (thing != null && thing is ThingWithComps t)
			{
				return t.TryGetComp<Comp_Detonator>();
			}
			return null;
		}

		public static bool NeedToOperate(Thing thing)
		{
			var detonator = TB_Utils.GetDetonator(thing);
			if (detonator != null)
			{
				return detonator.IsInstalledDetonator && (detonator.wantsToBeArmed || detonator.wantsToBeDisarmed);
			}
			else
				return false;
		}

		public static bool HasDetonator(this Thing thing)
		{
			var detonator = thing.GetDetonator();
			return detonator != null && detonator.IsInstalledDetonator;
		}

		public static bool IsValidExplosive(this Thing thing)
		{
			if (thing is ThingWithComps t)
			{
				bool isExplosive = t.TryGetComp<CompExplosive>() != null;
				bool isCEExplosive = t.TryGetComp<CompExplosiveCE>() != null;
				return isExplosive || isCEExplosive;
			}
			return false;
		}

		public static bool HasActiveDesignation(this Thing thing, DesignationDef def)
		{
			return thing.Map.designationManager.DesignationOn(thing, def) != null;
		}

		public static void ResetDesignation(this Thing thing, DesignationDef def)
		{
			if (thing.HasActiveDesignation(def))
			{
				Designation designation = thing.Map.designationManager.DesignationOn(thing, def);
				designation.Delete();
			}
		}

		public static void UpdateArmingDesignation(Thing thing)
		{
			bool needs = NeedToOperate(thing);
			bool withTimerDesignation = thing.HasActiveDesignation(TB_LocalDefOf.OperateBombTimerDes);
			if (needs && !withTimerDesignation)
			{
				thing.Map.designationManager.AddDesignation(new Designation(thing, TB_LocalDefOf.OperateBombTimerDes));
			}
			else if (!needs && withTimerDesignation)
			{
				thing.ResetDesignation(TB_LocalDefOf.OperateBombTimerDes);
			}
		}
	}
}
