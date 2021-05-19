﻿using Verse;
using System.Linq;
using RimWorld;
using CombatExtended;

namespace TimeBombs
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Log.Message("Time bombs loaded successfully!");
            var explosiveDefs = DefDatabase<ThingDef>.AllDefs.Where(def =>  !def.comps.NullOrEmpty()
                                                                            && (def.HasComp(typeof(CompExplosive))
                                                                            || def.HasComp(typeof(CompExplosiveCE)))).ToList();
            Log.Message($"Found {explosiveDefs.Count} explosive defs to add detonator slot.");
            if (!explosiveDefs.NullOrEmpty())
            {
                var newProps = new CompProperties_DetonatorSlot();
                for (var i = 0; i < explosiveDefs.Count; i++)
                {
                    explosiveDefs[i].comps.Add(newProps);
                }
            }
        }
    }
}