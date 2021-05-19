using Verse;
using System.Collections.Generic;
using Verse.AI;

namespace TimeBombs
{
   	public class JobDriver_ArmDetonator : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => this.Map.designationManager.DesignationOn(this.TargetThingA, TB_LocalDefOf.OperateBombTimerDes) == null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(15, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			Toil operating = new Toil();
			operating.initAction = delegate()
			{
				Pawn actor = operating.actor;
				ThingWithComps thing = (ThingWithComps)actor.CurJob.targetA.Thing;
				var detonator = TB_Utils.GetDetonator(thing);
				if (detonator != null && detonator.wantsToBeArmed)
				{
					this.TicksUntilJodDone = detonator.armingDelay;
					this.TotalJobTicks = this.TicksUntilJodDone;
				}
			};
			operating.WithProgressBar(TargetIndex.A, () => (float)(this.TotalJobTicks - this.TicksUntilJodDone)/(float)(this.TotalJobTicks + 1));
			operating.tickAction = delegate ()
			{
				this.TicksUntilJodDone--;
				if (this.TicksUntilJodDone <= 0)
				{
					Pawn actor = operating.actor;
					ThingWithComps thing = (ThingWithComps)actor.CurJob.targetA.Thing;
					var detonator = TB_Utils.GetDetonator(thing);
					if (detonator != null && detonator.wantsToBeArmed)
					{
						detonator.DoArm();
					}
					Designation designation = this.Map.designationManager.DesignationOn(thing, TB_LocalDefOf.OperateBombTimerDes);
					if (designation != null)
					{
						designation.Delete();
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
				}
			};
			operating.defaultCompleteMode = ToilCompleteMode.Never;
			yield return operating;
		}

		private int TicksUntilJodDone;
		private int TotalJobTicks;
	}
}
