using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlantProperties
	{
		public List<PlantBiomeRecord> wildBiomes;

		public float wildCommonalityMaxFraction = 1.25f;

		public IntRange wildClusterSizeRange = IntRange.one;

		public float wildClusterRadius = -1f;

		public List<string> sowTags = new List<string>();

		public float sowWork = 250f;

		public int sowMinSkill;

		public bool blockAdjacentSow;

		public List<ResearchProjectDef> sowResearchPrerequisites;

		public float harvestWork = 150f;

		public float harvestYield;

		public ThingDef harvestedThingDef;

		public string harvestTag;

		public float harvestMinGrowth = 0.65f;

		public float harvestAfterGrowth;

		public bool harvestFailable = true;

		public SoundDef soundHarvesting;

		public SoundDef soundHarvestFinish;

		public float growDays = 2f;

		public float lifespanFraction = 6f;

		public float growMinGlow = 0.51f;

		public float growOptimalGlow = 1f;

		public float fertilityMin = 0.9f;

		public float fertilitySensitivity = 0.5f;

		public bool reproduces = true;

		public float reproduceRadius = 20f;

		public float reproduceMtbDays = 10f;

		public bool dieIfLeafless;

		public bool neverBlightable;

		public bool cavePlant;

		public float topWindExposure = 0.25f;

		public int maxMeshCount = 1;

		public FloatRange visualSizeRange = new FloatRange(0.9f, 1.1f);

		private string leaflessGraphicPath;

		[Unsaved]
		public Graphic leaflessGraphic;

		private string immatureGraphicPath;

		[Unsaved]
		public Graphic immatureGraphic;

		public const int MaxMaxMeshCount = 25;

		public bool Sowable
		{
			get
			{
				return !this.sowTags.NullOrEmpty();
			}
		}

		public bool Harvestable
		{
			get
			{
				return this.harvestYield > 0.0010000000474974513;
			}
		}

		public bool HarvestDestroys
		{
			get
			{
				return this.harvestAfterGrowth <= 0.0;
			}
		}

		public float WildClusterRadiusActual
		{
			get
			{
				if (this.wildClusterRadius > 0.0)
				{
					return this.wildClusterRadius;
				}
				return this.reproduceRadius;
			}
		}

		public bool IsTree
		{
			get
			{
				return this.harvestTag == "Wood";
			}
		}

		public float LifespanDays
		{
			get
			{
				return this.growDays * this.lifespanFraction;
			}
		}

		public int LifespanTicks
		{
			get
			{
				return (int)(this.LifespanDays * 60000.0);
			}
		}

		public bool LimitedLifespan
		{
			get
			{
				return this.lifespanFraction > 0.0;
			}
		}

		public bool Blightable
		{
			get
			{
				return this.Sowable && this.Harvestable && !this.neverBlightable;
			}
		}

		public void PostLoadSpecial(ThingDef parentDef)
		{
			if (!this.leaflessGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.leaflessGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, this.leaflessGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
			if (!this.immatureGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.immatureGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, this.immatureGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (this.maxMeshCount <= 25)
				yield break;
			yield return "maxMeshCount > MaxMaxMeshCount";
			/*Error: Unable to find new state assignment for yield return*/;
		}

		internal IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (this.sowMinSkill > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowingSkillToSow".Translate(), this.sowMinSkill.ToString(), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			string attributes = string.Empty;
			if (this.Harvestable)
			{
				if (!attributes.NullOrEmpty())
				{
					attributes += ", ";
				}
				attributes += "Harvestable".Translate();
			}
			if (this.LimitedLifespan)
			{
				if (!attributes.NullOrEmpty())
				{
					attributes += ", ";
				}
				_003F val = attributes + "LimitedLifespan".Translate();
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "GrowingTime".Translate(), this.growDays.ToString("0.##") + " " + "Days".Translate(), 0, string.Empty)
			{
				overrideReportText = "GrowingTimeDesc".Translate()
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
