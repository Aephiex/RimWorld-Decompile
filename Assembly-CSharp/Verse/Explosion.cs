using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Explosion : Thing
	{
		public float radius;

		public DamageDef damType;

		public int damAmount;

		public Thing instigator;

		public ThingDef weapon;

		public ThingDef projectile;

		public bool applyDamageToExplosionCellsNeighbors;

		public ThingDef preExplosionSpawnThingDef;

		public float preExplosionSpawnChance;

		public int preExplosionSpawnThingCount = 1;

		public ThingDef postExplosionSpawnThingDef;

		public float postExplosionSpawnChance;

		public int postExplosionSpawnThingCount = 1;

		public float chanceToStartFire;

		public bool dealMoreDamageAtCenter;

		private int startTick;

		private List<IntVec3> cellsToAffect;

		private List<Thing> damagedThings;

		private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;

		private const float DamageFactorAtEdge = 0.2f;

		private static HashSet<IntVec3> tmpCells = new HashSet<IntVec3>();

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.cellsToAffect = SimplePool<List<IntVec3>>.Get();
				this.cellsToAffect.Clear();
				this.damagedThings = SimplePool<List<Thing>>.Get();
				this.damagedThings.Clear();
				this.addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
				this.addedCellsAffectedOnlyByDamage.Clear();
			}
		}

		public override void DeSpawn()
		{
			base.DeSpawn();
			this.cellsToAffect.Clear();
			SimplePool<List<IntVec3>>.Return(this.cellsToAffect);
			this.cellsToAffect = null;
			this.damagedThings.Clear();
			SimplePool<List<Thing>>.Return(this.damagedThings);
			this.damagedThings = null;
			this.addedCellsAffectedOnlyByDamage.Clear();
			SimplePool<HashSet<IntVec3>>.Return(this.addedCellsAffectedOnlyByDamage);
			this.addedCellsAffectedOnlyByDamage = null;
		}

		public virtual void StartExplosion(SoundDef explosionSound)
		{
			if (!base.Spawned)
			{
				Log.Error("Called StartExplosion() on unspawned thing.");
			}
			else
			{
				this.startTick = Find.TickManager.TicksGame;
				this.cellsToAffect.Clear();
				this.damagedThings.Clear();
				this.addedCellsAffectedOnlyByDamage.Clear();
				this.cellsToAffect.AddRange(this.damType.Worker.ExplosionCellsToHit(this));
				if (this.applyDamageToExplosionCellsNeighbors)
				{
					this.AddCellsNeighbors(this.cellsToAffect);
				}
				this.damType.Worker.ExplosionStart(this, this.cellsToAffect);
				this.PlayExplosionSound(explosionSound);
				MoteMaker.MakeWaterSplash(base.Position.ToVector3Shifted(), base.Map, (float)(this.radius * 6.0), 20f);
				this.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
				RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate(Region x)
				{
					List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
					for (int num = list.Count - 1; num >= 0; num--)
					{
						((Pawn)list[num]).mindState.Notify_Explosion(this);
					}
					return false;
				}, 25, RegionType.Set_Passable);
			}
		}

		public override void Tick()
		{
			int ticksGame = Find.TickManager.TicksGame;
			int count = this.cellsToAffect.Count;
			int num = count - 1;
			while (num >= 0 && ticksGame >= this.GetCellAffectTick(this.cellsToAffect[num]))
			{
				try
				{
					this.AffectCell(this.cellsToAffect[num]);
				}
				catch (Exception ex)
				{
					Log.Error("Explosion could not affect cell " + this.cellsToAffect[num] + ": " + ex);
				}
				this.cellsToAffect.RemoveAt(num);
				num--;
			}
			if (!this.cellsToAffect.Any())
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}

		public int GetDamageAmountAt(IntVec3 c)
		{
			if (!this.dealMoreDamageAtCenter)
			{
				return this.damAmount;
			}
			float t = c.DistanceTo(base.Position) / this.radius;
			int a = GenMath.RoundRandom(Mathf.Lerp((float)this.damAmount, (float)((float)this.damAmount * 0.20000000298023224), t));
			return Mathf.Max(a, 1);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.radius, "radius", 0f, false);
			Scribe_Defs.Look<DamageDef>(ref this.damType, "damType");
			Scribe_Values.Look<int>(ref this.damAmount, "damAmount", 0, false);
			Scribe_References.Look<Thing>(ref this.instigator, "instigator", false);
			Scribe_Defs.Look<ThingDef>(ref this.weapon, "weapon");
			Scribe_Defs.Look<ThingDef>(ref this.projectile, "projectile");
			Scribe_Values.Look<bool>(ref this.applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors", false, false);
			Scribe_Defs.Look<ThingDef>(ref this.preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
			Scribe_Values.Look<float>(ref this.preExplosionSpawnChance, "preExplosionSpawnChance", 0f, false);
			Scribe_Values.Look<int>(ref this.preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1, false);
			Scribe_Defs.Look<ThingDef>(ref this.postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
			Scribe_Values.Look<float>(ref this.postExplosionSpawnChance, "postExplosionSpawnChance", 0f, false);
			Scribe_Values.Look<int>(ref this.postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1, false);
			Scribe_Values.Look<float>(ref this.chanceToStartFire, "chanceToStartFire", 0f, false);
			Scribe_Values.Look<bool>(ref this.dealMoreDamageAtCenter, "dealMoreDamageAtCenter", false, false);
			Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
			Scribe_Collections.Look<IntVec3>(ref this.cellsToAffect, "cellsToAffect", LookMode.Value, new object[0]);
			Scribe_Collections.Look<Thing>(ref this.damagedThings, "damagedThings", LookMode.Reference, new object[0]);
			Scribe_Collections.Look<IntVec3>(ref this.addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.damagedThings.RemoveAll((Thing x) => x == null);
			}
		}

		private int GetCellAffectTick(IntVec3 cell)
		{
			return this.startTick + (int)((cell - base.Position).LengthHorizontal * 1.5);
		}

		private void AffectCell(IntVec3 c)
		{
			bool flag = this.ShouldCellBeAffectedOnlyByDamage(c);
			if (!flag && Rand.Chance(this.preExplosionSpawnChance) && c.Walkable(base.Map))
			{
				this.TrySpawnExplosionThing(this.preExplosionSpawnThingDef, c, this.preExplosionSpawnThingCount);
			}
			this.damType.Worker.ExplosionAffectCell(this, c, this.damagedThings, !flag);
			if (!flag && Rand.Chance(this.postExplosionSpawnChance) && c.Walkable(base.Map))
			{
				this.TrySpawnExplosionThing(this.postExplosionSpawnThingDef, c, this.postExplosionSpawnThingCount);
			}
			float num = this.chanceToStartFire;
			if (this.dealMoreDamageAtCenter)
			{
				num *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(base.Position) / this.radius);
			}
			if (Rand.Chance(num))
			{
				FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
			}
		}

		private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
		{
			if (thingDef != null)
			{
				if (thingDef.IsFilth)
				{
					FilthMaker.MakeFilth(c, base.Map, thingDef, count);
				}
				else
				{
					Thing thing = ThingMaker.MakeThing(thingDef, null);
					thing.stackCount = count;
					GenSpawn.Spawn(thing, c, base.Map);
				}
			}
		}

		private void PlayExplosionSound(SoundDef explosionSound)
		{
			if ((!Prefs.DevMode) ? (!explosionSound.NullOrUndefined()) : (explosionSound != null))
			{
				explosionSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
			else
			{
				this.damType.soundExplosion.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
		}

		private void AddCellsNeighbors(List<IntVec3> cells)
		{
			Explosion.tmpCells.Clear();
			this.addedCellsAffectedOnlyByDamage.Clear();
			for (int i = 0; i < cells.Count; i++)
			{
				Explosion.tmpCells.Add(cells[i]);
			}
			for (int j = 0; j < cells.Count; j++)
			{
				if (cells[j].Walkable(base.Map))
				{
					for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
					{
						IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
						if (intVec.InBounds(base.Map) && Explosion.tmpCells.Add(intVec))
						{
							this.addedCellsAffectedOnlyByDamage.Add(intVec);
						}
					}
				}
			}
			cells.Clear();
			foreach (IntVec3 tmpCell in Explosion.tmpCells)
			{
				cells.Add(tmpCell);
			}
			Explosion.tmpCells.Clear();
		}

		private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
		{
			if (!this.applyDamageToExplosionCellsNeighbors)
			{
				return false;
			}
			return this.addedCellsAffectedOnlyByDamage.Contains(c);
		}
	}
}
