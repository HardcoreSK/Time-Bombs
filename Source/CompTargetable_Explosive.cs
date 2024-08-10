using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TimeBombs
{
    public class CompTargetable_Explosive : CompUseEffect
    {
        private Thing target;

        public CompProperties_Targetable Props => (CompProperties_Targetable)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref target, "target");
        }

        public override bool SelectedUseOption(Pawn p)
        {
            if (PlayerChoosesTarget)
            {
                Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                {
                    target = t.Thing;
                    parent.GetComp<CompUsable>().TryStartUseJob(p, target);
                }, p);
                return true;
            }
            target = null;
            return false;
        }

        public bool HasTarget
        {
            get
            {
                return target != null;
            }
        }

        public void SetTarget(Thing explosive)
        {
            if (GetTargetingParameters().CanTarget(explosive))
            {
                target = explosive;
            }
        }

        protected bool PlayerChoosesTarget
        {
            get
            {
                return true;
            }
        }

        public override void DoEffect(Pawn usedBy)
        {
            if ((PlayerChoosesTarget && target == null) || (target != null && !GetTargetingParameters().CanTarget(target)))
            {
                return;
            }
            base.DoEffect(usedBy);
            foreach (Thing target2 in GetTargets(target))
            {
                foreach (CompTargetEffect comp in parent.GetComps<CompTargetEffect>())
                {
                    comp.DoEffectOn(usedBy, target2);
                }
                if (Props.moteOnTarget != null)
                {
                    MoteMaker.MakeAttachedOverlay(target2, Props.moteOnTarget, Vector3.zero);
                }
                if (Props.moteConnecting != null)
                {
                    MoteMaker.MakeConnectingLine(usedBy.DrawPos, target2.DrawPos, Props.moteConnecting, usedBy.Map);
                }
            }
            target = null;
        }


        public virtual IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {

            yield return targetChosenByPlayer;
        }

        protected virtual TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetHumans = false,
                canTargetMechs = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                canTargetItems = true,
                canTargetBuildings = false,
                mustBeSelectable = true,
                validator = ((TargetInfo x) => x.Thing.IsValidExplosive() && !x.Thing.HasDetonator())
            };
        }
    }
}