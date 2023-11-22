using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MultiOrdnanceFramework
{
    [StaticConstructorOnStartup]
    public class MOF_Bombardment : Bombardment
    {
		public override void SpawnSetup(Map map, bool respawningAfterReload)
		{
			base.SpawnSetup(map, respawningAfterReload);

			if (!respawningAfterReload)
			{
				GetNextExplosionCell();
			}
		}

        public override void StartStrike()
		{
			Log.Message("shellType: "+this.shellType.ToString());
			Log.Message("explosionThings: "+this.explosionThings.ToString());
			Log.Message("explosionGases: "+this.explosionGases.ToString());
			for(int i = 0; i<shellType.Count; i++)
			{
				this.volleyCount += shellType[i].volleySize;
			}
			
			ShellCheck(shellType, explosionThings, explosionGases);
			base.explosionCount = this.volleyCount;
			base.StartStrike();
		}

		public void ShellCheck(List<MOF_ShellTypes> shells, List<MOF_ExplosionThings> things, List<MOF_ExplosionGases> gases)
		{
			foreach(MOF_ShellTypes shell in shells)
			{
				foreach(MOF_ExplosionThings thing in things)
				{
					if(thing.damage == shell.damage)
					{
						shell.explosionThing = thing.explosionThing;
					}
				}

				foreach(MOF_ExplosionGases gas in gases)
				{
					if(gas.damage == shell.damage)
					{
						shell.explosionGas = gas.explosionGas;
					}
				}
			}
		}

        public override void Tick()
		{
			if (base.Destroyed)
			{
				return;
			}
			if (this.warmupTicks > 0)
			{
				this.warmupTicks--;
				if (this.warmupTicks == 0)
				{
					this.StartStrike();
				}
			}
			else
			{
				var comps = this.AllComps;
				if(comps != null)
				{
					int i = 0;
					int count = this.AllComps.Count;
					while(i < count)
					{
						comps[i].CompTick();
						i++;
					}
				}
				if (TicksPassed >= duration)
				{
					Destroy();
				}
				if (Find.TickManager.TicksGame % 20 == 0 && base.TicksLeft > 0)
				{
					this.StartRandomFire();
				}
			}
			this.EffectTick();
		}

        private void EffectTick()
		{
			if(volleysFired == 0 && shotsFired == 0)
			{
				this.GetNextExplosionCell();
			}
			if (!this.nextExplosionCell.IsValid)
			{
				this.ticksToNextEffect = this.warmupTicks - this.bombIntervalTicks;
				this.GetNextExplosionCell();
			}
			this.ticksToNextEffect--;
			if (this.ticksToNextEffect <= 0 && base.TicksLeft >= this.bombIntervalTicks)
			{
				SoundDefOf.Bombardment_PreImpact.PlayOneShot(new TargetInfo(this.nextExplosionCell, base.Map, false));
				this.projectiles.Add(new Bombardment.BombardmentProjectile(60, this.nextExplosionCell));
				this.ticksToNextEffect = this.bombIntervalTicks;
				this.GetNextExplosionCell();
			}
			for (int i = this.projectiles.Count - 1; i >= 0; i--)
			{
				this.projectiles[i].Tick();
				if (this.projectiles[i].LifeTime <= 0)
				{
					if(shotsFired < shellType[volleysFired].volleySize)
					{
						this.TryDoExplosion(this.projectiles[i], shellType[volleysFired].damage, shellType[volleysFired].explosionThing, shellType[volleysFired].explosionGas);
					}
					else
					{
						shotsFired = 0;
						volleysFired++;
						this.TryDoExplosion(this.projectiles[i], shellType[volleysFired].damage, shellType[volleysFired].explosionThing, shellType[volleysFired].explosionGas);
					}
					this.projectiles.RemoveAt(i);
					shotsFired++;
				}
			}
		}

        private void TryDoExplosion(BombardmentProjectile proj, DamageDef damage, ThingDef postExplosionThing, GasType? postExplosionGas)
		{
			List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
			for (int i = 0; i < list.Count; i++)
			{
					if (list[i].TryGetComp<CompProjectileInterceptor>().CheckBombardmentIntercept(this, proj))
					{
						if(damage == DamageDefOf.EMP)
						{
							bool absorbed = true;
							list[i].TryGetComp<CompProjectileInterceptor>().PostPreApplyDamage(new DamageInfo(damage, damage.defaultDamage), out absorbed);
						}
						else
						{
							return;
						}
						
						return;
					}
			}
			IntVec3 targetCell = proj.targetCell;
			Map map = base.Map;
			float randomInRange = this.explosionRadiusRange.RandomInRange;
			Thing instigator = this.instigator;
			int damAmount = -1;
			float armorPenetration = -1f;
			SoundDef explosionSound = null;
			ThingDef def = this.def;
			GenExplosion.DoExplosion(targetCell, map, randomInRange, damage, instigator, damAmount, armorPenetration, explosionSound, this.weaponDef, def, null, postExplosionThing, 1f, 3, postExplosionGas, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
		}

		public override void Draw()
		{
			base.Draw();
			if (this.projectiles.NullOrEmpty<MOF_Bombardment.BombardmentProjectile>())
			{
				return;
			}
			for (int i = 0; i < this.projectiles.Count; i++)
			{
				this.projectiles[i].Draw(MOF_Bombardment.ProjectileMaterial);
			}
		}

        private void StartRandomFire()
		{
			IntVec3 intVec = (from x in GenRadial.RadialCellsAround(base.Position, randomFireRadius, true)
			where x.InBounds(base.Map)
			select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position)));
			List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].TryGetComp<CompProjectileInterceptor>().BombardmentCanStartFireAt(this, intVec))
				{
					return;
				}
			}
			FireUtility.TryStartFireIn(intVec, base.Map, Rand.Range(0.1f, 0.925f));
		}

        private void GetNextExplosionCell()
		{
			this.nextExplosionCell = (from x in GenRadial.RadialCellsAround(base.Position, this.impactAreaRadius, true)
			where x.InBounds(base.Map)
			select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position) / this.impactAreaRadius));
		}

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToNextEffect, "ticksToNextEffect", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.nextExplosionCell, "nextExplosionCell", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.shotsFired, "shotsFired", 0, false);
			Scribe_Values.Look<int>(ref this.volleysFired, "volleysFired", 0 , false);
			Scribe_Values.Look<int>(ref this.volleyCount, "volleyCount", 0 , false);
			Scribe_Collections.Look<MOF_ShellTypes>(ref this.shellType, "shellType", LookMode.Deep, Array.Empty<object>());
			Scribe_Collections.Look<MOF_ExplosionThings>(ref this.explosionThings, "explosionThings", LookMode.Deep, Array.Empty<object>());
			Scribe_Collections.Look<MOF_ExplosionGases>(ref this.explosionGases, "explosionGases", LookMode.Deep, Array.Empty<object>());
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (!this.nextExplosionCell.IsValid)
				{
					this.GetNextExplosionCell();
				}
				if(this.shellType == null)
				{
					this.shellType = new List<MOF_ShellTypes>();
				}
				this.explosionThings ??= new List<MOF_ExplosionThings>();

				this.explosionGases ??= new List<MOF_ExplosionGases>();
			}
			
		}

		private int shotsFired = 0;

		private int volleysFired = 0;

		public List<MOF_ShellTypes> shellType = new List<MOF_ShellTypes>();

		public List<MOF_ExplosionThings> explosionThings;

		public List<MOF_ExplosionGases> explosionGases;

		private int volleyCount = 0;

		private int ticksToNextEffect;

		private IntVec3 nextExplosionCell = IntVec3.Invalid;

		private List<BombardmentProjectile> projectiles = new List<BombardmentProjectile>();

		private const int StartRandomFireEveryTicks = 20;

		private const int EffectDuration = 60;

		private static readonly Material ProjectileMaterial = MaterialPool.MatFrom("Things/Projectile/Bullet_Big", ShaderDatabase.Transparent, Color.white);
    }
}