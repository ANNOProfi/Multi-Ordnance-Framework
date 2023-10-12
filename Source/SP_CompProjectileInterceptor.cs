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
			if (!Active || !Props.interceptAirProjectiles)
            {
                return false;
            }

            if (!projectile.targetCell.InHorDistOf(parent.Position, Props.radius))
            {
                return false;
            }

            if ((bombardment.instigator == null || !bombardment.instigator.HostileTo(parent)) && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
            {
                return false;
            }

            lastInterceptTicks = Find.TickManager.TicksGame;
            drawInterceptCone = false;
            TriggerEffecter(projectile.targetCell);
            return true;
		}

        private void BreakShieldEmp(DamageInfo dinfo)
        {
            float fTheta;
            Vector3 center;
            if (Active)
            {
                EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, Props.radius);
                int num = Mathf.CeilToInt(Props.radius * 2f);
                fTheta = (float)Math.PI * 2f / (float)num;
                center = parent.TrueCenter();
                for (int i = 0; i < num; i++)
                {
                    FleckMaker.ConnectingLine(PosAtIndex(i), PosAtIndex((i + 1) % num), FleckDefOf.LineEMP, parent.Map, 1.5f);
                }
            }

            dinfo.SetAmount((float)Props.disarmedByEmpForTicks / 30f);
            stunner.Notify_DamageApplied(dinfo);
            Vector3 PosAtIndex(int index)
            {
                return new Vector3(Props.radius * Mathf.Cos(fTheta * (float)index) + center.x, 0f, Props.radius * Mathf.Sin(fTheta * (float)index) + center.z);
            }
        }

        private void TriggerEffecter(IntVec3 pos)
		{
			Effecter effecter = new Effecter(this.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
			effecter.Trigger(new TargetInfo(pos, this.parent.Map, false), TargetInfo.Invalid, -1);
			effecter.Cleanup();
		}
        
        public  bool BombardmentCanStartFireAt(ShieldbreakerBombardment bombardment, IntVec3 cell)
		{
			if (!Active || !Props.interceptAirProjectiles)
            {
                return true;
            }

            if ((bombardment.instigator == null || !bombardment.instigator.HostileTo(parent)) && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
            {
                return true;
            }

            return !cell.InHorDistOf(parent.Position, Props.radius);
		}

        private int lastInterceptTicks = -999999;

        private bool drawInterceptCone;

        private StunHandler stunner;
    }
}