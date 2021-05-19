using Verse;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace TimeBombs
{
   	public class JobDriver_InstallDetonator : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.useDuration = this.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.job.count = 1;
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
			yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			Toil toil = Toils_General.Wait(this.useDuration, TargetIndex.None);
				toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
				toil.FailOnDespawnedNullOrForbidden(TargetIndex.B);
				toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
				if (this.job.targetB.IsValid)
				{
					toil.FailOnDespawnedOrNull(TargetIndex.B);
				}
			yield return toil;
			Toil use = new Toil();
				use.initAction = delegate()
				{	
					Pawn actor = use.actor;
					var detonator = actor.carryTracker.CarriedThing;
					var targetExplosive = detonator.TryGetComp<CompTargetable_Explosive>();
					if (targetExplosive != null && !targetExplosive.HasTarget)
					{
						targetExplosive.SetTarget(actor.CurJob.targetB.Thing);
					}
					actor.carryTracker.CarriedThing.TryGetComp<CompUsable>().UsedBy(actor);
				};
				use.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return use;
		}

		private int useDuration;
	}
}
