using Verse;
using RimWorld;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using CombatExtended;

namespace TimeBombs
{
    public class CompProperties_DetonatorSlot : CompProperties
    {
        public CompProperties_DetonatorSlot()
        {
            this.compClass = typeof(Comp_Detonator);
        }
    }

    public class Comp_Detonator : ThingComp
    {
        public bool? unveiled;
        public bool allowRemoteArming;
        public int armingDelay;
        public int disarmingDelay;
        bool isArmed;
        int definedDelayInMinutes;
        readonly int TicksInHour = 2500;
        readonly int AdjustStep = 30;
        int armedTick;
        int ticksUntilExplode;
        public bool wantsToBeArmed;
        public bool wantsToBeDisarmed;

        public bool IsInstalledDetonator
        {
            get
            {
                return this.unveiled.HasValue && this.unveiled.Value;
            }
        }
        
        public bool ValidExplosive
        {
            get
            {
                return this.parent.IsValidExplosive();
            }
        }

        private void DoExplode(Thing thing = null)
        {
            if (ValidExplosive)
                {
                    var explosive = this.parent.TryGetComp<CompExplosive>();
                    if (explosive != null && !explosive.wickStarted)
                        {
                            explosive.StartWick(thing);
                            return;
                        }
                    var explosiveCE = this.parent.TryGetComp<CompExplosiveCE>();
                    if (explosiveCE != null)
                    {
                        explosiveCE.Explode(this.parent, this.parent.Position.ToVector3(), this.parent.Map);
                        return;
                    }
                    if (this.parent.GetType()==typeof(AmmoThing))
                    {
                        DamageInfo damage = new DamageInfo(DamageDefOf.Burn, 10f);
                        this.parent.TakeDamage(damage);
                    }
                }
        }

        private void DecreaseDelay()
        {
            if (!isArmed)
            {
                definedDelayInMinutes -= AdjustStep;
                if (definedDelayInMinutes < 0)
                    ResetDelay();
            }
        }

        private void IncreaseDelay()
        {
            if (!isArmed)
            {
                definedDelayInMinutes += AdjustStep;
            }
        }

        public void DoArm()
        {
            if (isArmed || definedDelayInMinutes == 0)
                DoExplode(this.parent);
            else
            {
                armedTick = Find.TickManager.TicksGame;
                ticksUntilExplode = TicksInHour * definedDelayInMinutes / 60;
                isArmed = true;
                wantsToBeArmed = false;
                wantsToBeDisarmed = false;
            }
        }

        public void DoDisarm()
        {
            isArmed = false;
            wantsToBeArmed = false;
            wantsToBeDisarmed = false;
        }

        public void TryArm()
        {
            if (this.allowRemoteArming)
                DoArm();
            else
                { 
                    wantsToBeDisarmed = false;
                    wantsToBeArmed = true;
                    TB_Utils.UpdateArmingDesignation(this.parent);
                }
        }

        public void TryDisarm()
        {
            if (this.allowRemoteArming)
                DoDisarm();
            else
                {
                    wantsToBeArmed = false;
                    wantsToBeDisarmed = true;
                    TB_Utils.UpdateArmingDesignation(this.parent);
                }
        }

        public void ResetAnyActions()
        {
            wantsToBeArmed = false;
            wantsToBeDisarmed = false;
            this.parent.ResetDesignation(TB_LocalDefOf.OperateBombTimerDes);
        }

        private void ResetDelay()
        {
            definedDelayInMinutes = 0;
        }

        public void FullClearSlot()
        {
            this.wantsToBeArmed = false;
            this.wantsToBeDisarmed = false;
            this.isArmed = false;
            this.definedDelayInMinutes = 0;
            this.unveiled = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref unveiled, "unveiled", null);
            Scribe_Values.Look(ref allowRemoteArming, "allowRemoteArming");
            Scribe_Values.Look(ref armingDelay, "armingDelay");
            Scribe_Values.Look(ref disarmingDelay, "disarmingDelay");
            Scribe_Values.Look(ref isArmed, "isArmed");
            Scribe_Values.Look(ref definedDelayInMinutes, "definedDelayInMinutes");
            Scribe_Values.Look(ref armedTick, "armedTick");
            Scribe_Values.Look(ref ticksUntilExplode, "ticksUntilExplode");
            Scribe_Values.Look(ref wantsToBeArmed, "wantsToBeArmed");
            Scribe_Values.Look(ref wantsToBeDisarmed, "wantsToBeDisarmed");
        }

        public override void CompTick()
        {
            if (IsInstalledDetonator)
            {
                if (Find.TickManager.TicksGame % 30 == 0)
                {
                    if (isArmed)
                    {
                        ticksUntilExplode -= 30;
                        if (ticksUntilExplode < 0)
                            DoExplode(this.parent);
                    }
                }   
            }   
        }

        public override bool AllowStackWith(Thing other)
        {
            var otherDetonator = other.GetDetonator();
            if (otherDetonator == null)
                return false;
            bool sameSlotState = this.IsInstalledDetonator == otherDetonator.IsInstalledDetonator;
            if (!sameSlotState)
                {
                    return false;
                }
            bool sameRemoteAiming = this.allowRemoteArming == otherDetonator.allowRemoteArming;
            if (this.IsInstalledDetonator && !sameRemoteAiming)
                {
                    return false;
                }
            bool sameArmedState = this.isArmed == otherDetonator.isArmed;
            if (this.IsInstalledDetonator && sameRemoteAiming && !sameArmedState)
                {
                    return false;
                }
            return true;
        }

        public override void PostSplitOff(Thing piece)
        {
            if (this.IsInstalledDetonator && piece != this.parent)
            {
                var pieceDetonator = piece.GetDetonator();
                pieceDetonator.armingDelay = this.armingDelay;
                pieceDetonator.disarmingDelay = this.disarmingDelay;
                pieceDetonator.allowRemoteArming = this.allowRemoteArming;
                pieceDetonator.unveiled = this.unveiled;
                this.FullClearSlot();
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        { 
             var otherDetonator = otherStack.GetDetonator();
                if (otherDetonator!= null && !this.IsInstalledDetonator && otherDetonator.IsInstalledDetonator)
                {
                    this.armingDelay = otherDetonator.armingDelay;
                    this.disarmingDelay = otherDetonator.disarmingDelay;
                    this.allowRemoteArming = otherDetonator.allowRemoteArming;
                    this.unveiled = otherDetonator.unveiled;
                }
        }

        public override void PostDraw()
        {
            if (IsInstalledDetonator && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Vector3 vector = this.parent.OccupiedRect().BottomLeft.ToVector3();
                vector.z += 0.3f;
                vector.x += 0.2f;
                Graphics.DrawMesh(MeshPool.plane10, vector, Quaternion.identity, OverlayContainer.DetonatorMat, 0);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (IsInstalledDetonator && ValidExplosive)
            {
                var builder = new StringBuilder();
                var armedStr = isArmed 
                                ? "TB_BombTimerArmed".Translate()
                                : "TB_BombTimerDisarmed".Translate();
                builder.AppendInNewLine(this.allowRemoteArming 
                                            ? "TB_ExplosiveWithRadioTimer".Translate(armedStr)
                                            : "TB_ExplosiveWithTimer".Translate(armedStr));
                builder.AppendInNewLine("TB_TimerSetTo".Translate(definedDelayInMinutes));
                if (isArmed && ticksUntilExplode > 0)
                    builder.AppendInNewLine("TB_TimeToExplode".Translate(60 * ticksUntilExplode / TicksInHour));
                if (this.wantsToBeArmed)
                    builder.AppendInNewLine("TB_wantsToBeArmed".Translate());
                if (this.wantsToBeDisarmed)
                    builder.AppendInNewLine("TB_wantsToBeDisarmed".Translate());
                if (isArmed && this.parent.GetComp<CompExplosive>() != null && this.parent.GetComp<CompExplosive>().wickStarted)
                    builder.AppendInNewLine("TB_Detonating".Translate());
                return builder.ToString();
            } 
            return string.Empty;  
        }

        public override string TransformLabel(string label)
        {
            if (this.IsInstalledDetonator)
            {
                return string.Format("{0} ({1})", label, "TB_Bombed".Translate());
            }
            return label;
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{ 
            if (IsInstalledDetonator && ValidExplosive && (!this.parent.def.CanHaveFaction || this.parent.Faction == Faction.OfPlayer))
            {
                //Arm Action
                var armAction = new Command_Action();
                armAction.action = new Action(this.TryArm);
                armAction.defaultLabel = "TB_ArmAction_Label".Translate();
                armAction.defaultDesc = "TB_ArmAction_Description".Translate();
                armAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/Action_Arm", true);
                if (isArmed)
                    armAction.Disable();
                yield return armAction;

                //Disarm Action
                var disarmAction = new Command_Action();
                disarmAction.action = new Action(this.TryDisarm);
                disarmAction.defaultLabel = "TB_DisarmAction_Label".Translate();
                disarmAction.defaultDesc = "TB_DisarmAction_Description".Translate();
                disarmAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/Action_Disarm", true);
                if (!isArmed)
                    disarmAction.Disable();
                yield return disarmAction;

                //Increase Timer Action
                var IncreaseTimerAction = new Command_Action();
                IncreaseTimerAction.action = new Action(this.IncreaseDelay);
                IncreaseTimerAction.defaultLabel = "TB_IncreaseTimerAction_Label".Translate(AdjustStep);
                IncreaseTimerAction.defaultDesc = "TB_IncreaseTimerAction_Description".Translate();
                IncreaseTimerAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/Action_Increase", true);
                if (isArmed)
                    IncreaseTimerAction.Disable();
                yield return IncreaseTimerAction;

                //Decrease Timer Action
                var DecreaseTimerAction = new Command_Action();
                DecreaseTimerAction.action = new Action(this.DecreaseDelay);
                DecreaseTimerAction.defaultLabel = "TB_DecreaseTimerAction_Label".Translate(AdjustStep);
                DecreaseTimerAction.defaultDesc = "TB_DecreaseTimerAction_Description".Translate();
                DecreaseTimerAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/Action_Decrease", true);
                if (isArmed)
                    DecreaseTimerAction.Disable();
                yield return DecreaseTimerAction;

                if (this.parent.HasActiveDesignation(TB_LocalDefOf.OperateBombTimerDes))
                {
                    //Reset Actions
                    var ResetActions = new Command_Action();
                    ResetActions.action = new Action(this.ResetAnyActions);
                    ResetActions.defaultLabel = "TB_ResetActions_Label".Translate(AdjustStep);
                    ResetActions.defaultDesc = "TB_ResetActions_Description".Translate();
                    ResetActions.icon = ContentFinder<Texture2D>.Get("UI/Commands/ResetActions", true);
                    yield return ResetActions;
                }
            }
		}
    }
}
