using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace ShieldbreakerPermits
{
    public class SP_RoyalTitlePermitWorker_ShieldbreakerOrbitalStrike : RoyalTitlePermitWorker_Targeted
    {
        private Faction faction;

		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!base.CanHitTarget(target))
			{
				if (target.IsValid && showMessages)
				{
					Messages.Message(this.def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput, true);
				}
				return false;
			}
			return true;
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			GenDraw.DrawRadiusRing(this.caller.Position, this.def.royalAid.targetingRange, Color.white, null);
			GenDraw.DrawRadiusRing(target.Cell, this.def.royalAid.radius + this.def.royalAid.explosionRadiusRange.max, Color.white, null);
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            this.CallBombardment(target.Cell);
        }

		public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			if (faction.HostileTo(Faction.OfPlayer))
			{
				yield return new FloatMenuOption(this.def.LabelCap + ": " + "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
				yield break;
			}
			string label = this.def.LabelCap + ": ";
			Action action = null;
			bool free;
			if (base.FillAidOption(pawn, faction, ref label, out free))
			{
				action = delegate()
				{
					this.BeginCallBombardment(pawn, faction, map, free);
				};
			}
			yield return new FloatMenuOption(label, action, faction.def.FactionIcon, faction.Color, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0, HorizontalJustification.Left, false);
			yield break;
		}

		private void BeginCallBombardment(Pawn caller, Faction faction, Map map, bool free)
		{
			this.targetingParameters = new TargetingParameters();
			this.targetingParameters.canTargetLocations = true;
			this.targetingParameters.canTargetSelf = true;
			this.targetingParameters.canTargetFires = true;
			this.targetingParameters.canTargetItems = true;
			this.caller = caller;
			this.map = map;
			this.faction = faction;
			this.free = free;
			this.targetingParameters.validator = ((TargetInfo target) => (this.def.royalAid.targetingRange <= 0f || target.Cell.DistanceTo(caller.Position) <= this.def.royalAid.targetingRange) && !target.Cell.Fogged(map));
			Find.Targeter.BeginTargeting((ITargetingSource)this, null, false, null, null);
		}

        private void CallBombardment(IntVec3 targetCell)
		{
			ShieldbreakerBombardment bombardment = (ShieldbreakerBombardment)GenSpawn.Spawn(SP_DefOf.ShieldbreakerBombardment, targetCell, this.map, WipeMode.Vanish);
			bombardment.impactAreaRadius = this.def.royalAid.radius;
			bombardment.explosionRadiusRange = this.def.royalAid.explosionRadiusRange;
			bombardment.bombIntervalTicks = this.def.royalAid.intervalTicks;
			bombardment.randomFireRadius = 1;
			bombardment.explosionCount = -1;
			bombardment.shellType = this.def.GetModExtension<SP_RoyalAid>().shellType;
			bombardment.explosionThings = this.def.GetModExtension<SP_RoyalOrdinance>().explosionThings;
			bombardment.explosionGases = this.def.GetModExtension<SP_RoyalOrdinance>().explosionGases;
			bombardment.warmupTicks = this.def.royalAid.warmupTicks;
			bombardment.instigator = this.caller;
			SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera(null);
			this.caller.royalty.GetPermit(this.def, this.faction).Notify_Used();
			if (!this.free)
			{
				this.caller.royalty.TryRemoveFavor(this.faction, this.def.royalAid.favorCost);
			}
		}
    }
}