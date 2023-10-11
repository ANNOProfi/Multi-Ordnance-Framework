using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace ShieldbreakerPermits
{
    [StaticConstructorOnStartup]
    public class SP_CompProjectileInterceptor : CompProjectileInterceptor
    {
        private bool debugInterceptNonHostileProjectiles;

        public bool CheckBombardmentIntercept(ShieldbreakerBombardment bombardment, SP_BombardmentProjectile projectile)
		{
			if (!this.Active || !this.Props.interceptAirProjectiles)
			{
				return false;
			}
			if (!projectile.targetCell.InHorDistOf(this.parent.Position, this.Props.radius))
			{
				return false;
			}
			if ((bombardment.instigator == null || !bombardment.instigator.HostileTo(this.parent)) && !this.debugInterceptNonHostileProjectiles && !this.Props.interceptNonHostileProjectiles)
			{
				return false;
			}
			this.lastInterceptTicks = Find.TickManager.TicksGame;
			this.drawInterceptCone = false;
			this.TriggerEffecter(projectile.targetCell);
			return true;
		}

        private void TriggerEffecter(IntVec3 pos)
		{
			Effecter effecter = new Effecter(this.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
			effecter.Trigger(new TargetInfo(pos, this.parent.Map, false), TargetInfo.Invalid, -1);
			effecter.Cleanup();
		}
        
        public  bool BombardmentCanStartFireAt(ShieldbreakerBombardment bombardment, IntVec3 cell)
		{
			return !this.Active || !this.Props.interceptAirProjectiles || ((bombardment.instigator == null || !bombardment.instigator.HostileTo(this.parent)) && !this.debugInterceptNonHostileProjectiles && !this.Props.interceptNonHostileProjectiles) || !cell.InHorDistOf(this.parent.Position, this.Props.radius);
		}

        private int lastInterceptTicks = -999999;

        private bool drawInterceptCone;
    }
}