using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class GenStep_RockChunks : GenStep
	{
		private ModuleBase freqFactorNoise;

		private const float ThreshLooseRock = 0.55f;

		private const float PlaceProbabilityPerCell = 0.006f;

		private const float RubbleProbability = 0.5f;

		public override void Generate(Map map)
		{
			if (!map.TileInfo.WaterCovered)
			{
				this.freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
				this.freqFactorNoise = new ScaleBias(1.0, 1.0, this.freqFactorNoise);
				NoiseDebugUI.StoreNoiseRender(this.freqFactorNoise, "rock_chunks_freq_factor");
				MapGenFloatGrid elevation = MapGenerator.Elevation;
				foreach (IntVec3 allCell in map.AllCells)
				{
					float num = (float)(0.0060000000521540642 * this.freqFactorNoise.GetValue(allCell));
					if (elevation[allCell] < 0.550000011920929 && Rand.Value < num)
					{
						this.GrowLowRockFormationFrom(allCell, map);
					}
				}
				this.freqFactorNoise = null;
			}
		}

		private void GrowLowRockFormationFrom(IntVec3 root, Map map)
		{
			ThingDef rockRubble = ThingDefOf.RockRubble;
			ThingDef mineableThing = Find.World.NaturalRockTypesIn(map.Tile).RandomElement().building.mineableThing;
			Rot4 random = Rot4.Random;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			IntVec3 intVec = root;
			while (true)
			{
				Rot4 random2 = Rot4.Random;
				if (random2 == random)
					continue;
				intVec += random2.FacingCell;
				if (intVec.InBounds(map) && intVec.GetEdifice(map) == null && intVec.GetFirstItem(map) == null && !(elevation[intVec] > 0.550000011920929) && map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordance.Heavy))
				{
					GenSpawn.Spawn(mineableThing, intVec, map);
					IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
					foreach (IntVec3 b in adjacentCellsAndInside)
					{
						if (Rand.Value < 0.5)
						{
							IntVec3 c = intVec + b;
							if (c.InBounds(map))
							{
								bool flag = false;
								List<Thing> thingList = c.GetThingList(map);
								for (int j = 0; j < thingList.Count; j++)
								{
									Thing thing = thingList[j];
									if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									FilthMaker.MakeFilth(c, map, rockRubble, 1);
								}
							}
						}
					}
					continue;
				}
				break;
			}
		}
	}
}
