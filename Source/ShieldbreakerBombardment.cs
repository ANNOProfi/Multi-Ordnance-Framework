using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ShieldbreakerPermits
{
    [StaticConstructorOnStartup]
    public class ShieldbreakerBombardment : Bombardment
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
			for(int i = 0; i<shellType.Count; i++)
			{
				this.volleyCount += shellType[i].volleySize;
			}
			ShellCheck(shellType);
			//this.duration = this.bombIntervalTicks * this.volleyCount;
			base.explosionCount = this.volleyCount;
			base.StartStrike();
		}

		/*public ThingDef ShellThingCheck(DamageDef damage)
		{
			Log.Message("ShellThingCheck called");
			Log.Message("ShellDamage: "+damage);
			
			
			if(damage == DamageDefOf.Extinguish)
			{
				return ThingDefOf.Filth_FireFoam;
			}
			else
			{
				Log.Message("Returning Null");
				return null;
			}
			
			//Log.Message("Projectile "+shell.damage+" "+shell.explosionThing+""+shell.explosionGas);
		}

		public GasType? ShellGasCheck(DamageDef damage)
		{
			Log.Message("ShellGasCheck called");
			Log.Message("ShellDamage: "+damage);
			if(damage == DamageDefOf.Smoke)
			{
				return GasType.BlindSmoke;
			}
			if(damage == DamageDefOf.ToxGas)
			{
				return GasType.ToxGas;
			}
			else
			{
				Log.Message("Returning Null");
				return null;
			}

			
		}*/

		public void ShellCheck(List<SP_ShellTypes> shells)
		{
			foreach(SP_ShellTypes shell in shells)
			if(shell.damage == DamageDefOf.Extinguish)
			{
				shell.explosionThing = ThingDefOf.Filth_FireFoam;
			}
			else if(shell.damage == DamageDefOf.Smoke)
			{
				shell.explosionGas = GasType.BlindSmoke;
			}
			else if(shell.damage == DamageDefOf.ToxGas)
			{
				shell.explosionGas = GasType.ToxGas;
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
				//base.explosionCount = 0;
				//base.duration = 0;
				//base.Tick();
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
				//Log.Message("projectiles.Count: "+projectiles.Count);
				//Log.Message("projectile damage: "+projectiles[i].DamageDef);
				this.projectiles[i].Tick();
				if (this.projectiles[i].LifeTime <= 0)
				{
					/*Log.Message("projectile #"+i);
					if(i>(this.explosionCount-this.empCount))
					{
						Log.Message("Doing EMP projectile");
						this.TryDoExplosion(this.projectiles[i], DamageDefOf.EMP);
					}
					else
					{
						Log.Message("Doing regular projectile");
						this.TryDoExplosion(this.projectiles[i], DamageDefOf.Bomb);
					}*/
					//this.TryDoExplosion(this.projectiles[i], DamageDefOf.EMP);
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
				//if(shellType[volleysFired].damage != DamageDefOf.EMP)
				//{
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
				/*}
				else
				{
					if (list[i].TryGetComp<CompProjectileInterceptor>().CheckBombardmentIntercept(this, proj))
					{
						bool absorbed = false;
						list[i].TryGetComp<CompProjectileInterceptor>().PostPreApplyDamage(new DamageInfo(shellType[volleysFired].damage, shellType[volleysFired].damage.defaultDamage), out absorbed);
					}
				}*/
				
			}
			IntVec3 targetCell = proj.targetCell;
			Map map = base.Map;
			float randomInRange = this.explosionRadiusRange.RandomInRange;
			Thing instigator = this.instigator;
			int damAmount = -1;
			float armorPenetration = -1f;
			SoundDef explosionSound = null;
			ThingDef def = this.def;
			Log.Message("Projectile "+damage+" "+postExplosionThing+""+postExplosionGas);
			GenExplosion.DoExplosion(targetCell, map, randomInRange, damage, instigator, damAmount, armorPenetration, explosionSound, this.weaponDef, def, null, postExplosionThing, 1f, 3, postExplosionGas, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
		}

		public override void Draw()
		{
			base.Draw();
			if (this.projectiles.NullOrEmpty<ShieldbreakerBombardment.BombardmentProjectile>())
			{
				return;
			}
			for (int i = 0; i < this.projectiles.Count; i++)
			{
				this.projectiles[i].Draw(ShieldbreakerBombardment.ProjectileMaterial);
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
			Scribe_Values.Look<float>(ref this.impactAreaRadius, "impactAreaRadius", 15f, false);
			Scribe_Values.Look<FloatRange>(ref this.explosionRadiusRange, "explosionRadiusRange", new FloatRange(6f, 8f), false);
			Scribe_Values.Look<int>(ref this.randomFireRadius, "randomFireRadius", 25, false);
			Scribe_Values.Look<int>(ref this.bombIntervalTicks, "bombIntervalTicks", 18, false);
			Scribe_Values.Look<int>(ref this.warmupTicks, "warmupTicks", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToNextEffect, "ticksToNextEffect", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.nextExplosionCell, "nextExplosionCell", default(IntVec3), false);
			//Scribe_Values.Look<int>(ref this.empCount, "empCount", 1, false);
			Scribe_Collections.Look<BombardmentProjectile>(ref this.projectiles, "projectiles", LookMode.Deep, Array.Empty<object>());
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (!this.nextExplosionCell.IsValid)
				{
					this.GetNextExplosionCell();
				}
				if (this.projectiles == null)
				{
					this.projectiles = new List<BombardmentProjectile>();
				}
			}
		}

		private int shotsFired = 0;

		private int volleysFired = 0;

		public List<SP_ShellTypes> shellType = new List<SP_ShellTypes>();

		//public int empCount = 1;

		//public float impactAreaRadius = 15f;

		//public FloatRange explosionRadiusRange = new FloatRange(6f, 8f);

		//public int randomFireRadius = 25;

		//public int bombIntervalTicks = 18;

		private int volleyCount = 0;

		//public int warmupTicks = 60;

		private int ticksToNextEffect;

		private IntVec3 nextExplosionCell = IntVec3.Invalid;

		private List<BombardmentProjectile> projectiles = new List<BombardmentProjectile>();

		//public const int EffectiveAreaRadius = 23;

		private const int StartRandomFireEveryTicks = 20;

		private const int EffectDuration = 60;

		private static readonly Material ProjectileMaterial = MaterialPool.MatFrom("Things/Projectile/Bullet_Big", ShaderDatabase.Transparent, Color.white);

		/*public static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(1f, 0.1f),
				true
			}
		};*/
    }
}