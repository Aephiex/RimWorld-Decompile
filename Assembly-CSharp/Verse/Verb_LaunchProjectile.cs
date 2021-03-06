using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Verb_LaunchProjectile : Verb
	{
		public virtual ThingDef Projectile
		{
			get
			{
				if (base.ownerEquipment != null)
				{
					CompChangeableProjectile comp = base.ownerEquipment.GetComp<CompChangeableProjectile>();
					if (comp != null && comp.Loaded)
					{
						return comp.Projectile;
					}
				}
				return base.verbProps.defaultProjectile;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(base.caster, (!base.currentTarget.HasThing) ? null : base.currentTarget.Thing, (base.ownerEquipment == null) ? null : base.ownerEquipment.def, this.Projectile, this.ShotsPerBurst > 1));
		}

		protected override bool TryCastShot()
		{
			if (base.currentTarget.HasThing && base.currentTarget.Thing.Map != base.caster.Map)
			{
				return false;
			}
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
				return false;
			}
			ShootLine shootLine = default(ShootLine);
			bool flag = base.TryFindShootLineFromTo(base.caster.Position, base.currentTarget, out shootLine);
			if (base.verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (base.ownerEquipment != null)
			{
				CompChangeableProjectile comp = base.ownerEquipment.GetComp<CompChangeableProjectile>();
				if (comp != null)
				{
					comp.Notify_ProjectileLaunched();
				}
			}
			Thing launcher = base.caster;
			Thing equipment = base.ownerEquipment;
			CompMannable compMannable = base.caster.TryGetComp<CompMannable>();
			if (compMannable != null && compMannable.ManningPawn != null)
			{
				launcher = compMannable.ManningPawn;
				equipment = base.caster;
			}
			Vector3 drawPos = base.caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, base.caster.Map);
			projectile2.FreeIntercept = (base.canFreeInterceptNow && !projectile2.def.projectile.flyOverhead);
			if (base.verbProps.forcedMissRadius > 0.5)
			{
				float num = (float)(base.currentTarget.Cell - base.caster.Position).LengthHorizontalSquared;
				float num2 = (float)((!(num < 9.0)) ? ((!(num < 25.0)) ? ((!(num < 49.0)) ? (base.verbProps.forcedMissRadius * 1.0) : (base.verbProps.forcedMissRadius * 0.800000011920929)) : (base.verbProps.forcedMissRadius * 0.5)) : 0.0);
				if (num2 > 0.5)
				{
					int max = GenRadial.NumCellsInRadius(base.verbProps.forcedMissRadius);
					int num3 = Rand.Range(0, max);
					if (num3 > 0)
					{
						if (DebugViewSettings.drawShooting)
						{
							MoteMaker.ThrowText(base.caster.DrawPos, base.caster.Map, "ToForRad", -1f);
						}
						IntVec3 c = base.currentTarget.Cell + GenRadial.RadialPattern[num3];
						if (base.currentTarget.HasThing)
						{
							projectile2.ThingToNeverIntercept = base.currentTarget.Thing;
						}
						if (!projectile2.def.projectile.flyOverhead)
						{
							projectile2.InterceptWalls = true;
						}
						projectile2.Launch(launcher, drawPos, c, equipment, base.currentTarget.Thing);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(base.caster, this, base.currentTarget);
			if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
			{
				if (DebugViewSettings.drawShooting)
				{
					MoteMaker.ThrowText(base.caster.DrawPos, base.caster.Map, "ToWild", -1f);
				}
				shootLine.ChangeDestToMissWild();
				if (base.currentTarget.HasThing)
				{
					projectile2.ThingToNeverIntercept = base.currentTarget.Thing;
				}
				if (!projectile2.def.projectile.flyOverhead)
				{
					projectile2.InterceptWalls = true;
				}
				projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, base.currentTarget.Thing);
				return true;
			}
			if (Rand.Value > shotReport.ChanceToNotHitCover)
			{
				if (DebugViewSettings.drawShooting)
				{
					MoteMaker.ThrowText(base.caster.DrawPos, base.caster.Map, "ToCover", -1f);
				}
				if (base.currentTarget.Thing != null && base.currentTarget.Thing.def.category == ThingCategory.Pawn)
				{
					Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
					if (!projectile2.def.projectile.flyOverhead)
					{
						projectile2.InterceptWalls = true;
					}
					projectile2.Launch(launcher, drawPos, randomCoverToMissInto, equipment, base.currentTarget.Thing);
					return true;
				}
			}
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(base.caster.DrawPos, base.caster.Map, "ToHit", -1f);
			}
			if (!projectile2.def.projectile.flyOverhead)
			{
				projectile2.InterceptWalls = (!base.currentTarget.HasThing || base.currentTarget.Thing.def.Fillage == FillCategory.Full);
			}
			if (base.currentTarget.Thing != null)
			{
				projectile2.Launch(launcher, drawPos, base.currentTarget, equipment, base.currentTarget.Thing);
			}
			else
			{
				projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, base.currentTarget.Thing);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = true;
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
				return 0f;
			}
			return projectile.projectile.explosionRadius;
		}
	}
}
