using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarmClothes : Alert
	{
		private static List<Thing> jackets = new List<Thing>();

		private static List<Thing> shirts = new List<Thing>();

		private static List<Thing> pants = new List<Thing>();

		private const float MedicinePerColonistThreshold = 2f;

		private const int CheckNextTwelfthsCount = 3;

		private const float CanShowAlertOnlyIfTempBelow = 5f;

		public Alert_NeedWarmClothes()
		{
			base.defaultLabel = "NeedWarmClothes".Translate();
			base.defaultPriority = AlertPriority.High;
		}

		private int NeededWarmClothesCount(Map map)
		{
			return map.mapPawns.FreeColonistsSpawnedCount;
		}

		private int ColonistsWithWarmClothesCount(Map map)
		{
			float num = this.LowestTemperatureComing(map);
			int num2 = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.GetStatValue(StatDefOf.ComfyTemperatureMin, true) <= num)
				{
					num2++;
				}
			}
			return num2;
		}

		private int FreeWarmClothesSetsCount(Map map)
		{
			Alert_NeedWarmClothes.jackets.Clear();
			Alert_NeedWarmClothes.shirts.Clear();
			Alert_NeedWarmClothes.pants.Clear();
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsInAnyStorage() && !(list[i].GetStatValue(StatDefOf.Insulation_Cold, true) >= 0.0))
				{
					if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						if (list[i].def.apparel.layers.Contains(ApparelLayer.OnSkin))
						{
							Alert_NeedWarmClothes.shirts.Add(list[i]);
						}
						else
						{
							Alert_NeedWarmClothes.jackets.Add(list[i]);
						}
					}
					if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
					{
						Alert_NeedWarmClothes.pants.Add(list[i]);
					}
				}
			}
			Alert_NeedWarmClothes.jackets.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			Alert_NeedWarmClothes.shirts.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			Alert_NeedWarmClothes.pants.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			float num = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - this.LowestTemperatureComing(map);
			if (num <= 0.0)
			{
				return GenMath.Max(Alert_NeedWarmClothes.jackets.Count, Alert_NeedWarmClothes.shirts.Count, Alert_NeedWarmClothes.pants.Count);
			}
			int num2 = 0;
			while (true)
			{
				if (!Alert_NeedWarmClothes.jackets.Any() && !Alert_NeedWarmClothes.shirts.Any() && !Alert_NeedWarmClothes.pants.Any())
					break;
				float num3 = 0f;
				if (Alert_NeedWarmClothes.jackets.Any())
				{
					Thing thing = Alert_NeedWarmClothes.jackets[Alert_NeedWarmClothes.jackets.Count - 1];
					Alert_NeedWarmClothes.jackets.RemoveLast();
					float num4 = (float)(0.0 - thing.GetStatValue(StatDefOf.Insulation_Cold, true));
					num3 += num4;
				}
				if (num3 < num && Alert_NeedWarmClothes.shirts.Any())
				{
					Thing thing2 = Alert_NeedWarmClothes.shirts[Alert_NeedWarmClothes.shirts.Count - 1];
					Alert_NeedWarmClothes.shirts.RemoveLast();
					float num5 = (float)(0.0 - thing2.GetStatValue(StatDefOf.Insulation_Cold, true));
					num3 += num5;
				}
				if (num3 < num && Alert_NeedWarmClothes.pants.Any())
				{
					for (int j = 0; j < Alert_NeedWarmClothes.pants.Count; j++)
					{
						float num6 = (float)(0.0 - Alert_NeedWarmClothes.pants[j].GetStatValue(StatDefOf.Insulation_Cold, true));
						if (num6 + num3 >= num)
						{
							num3 += num6;
							Alert_NeedWarmClothes.pants.RemoveAt(j);
							break;
						}
					}
				}
				if (!(num3 >= num))
					break;
				num2++;
			}
			return num2;
		}

		private int MissingWarmClothesCount(Map map)
		{
			if (this.LowestTemperatureComing(map) >= ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
			{
				return 0;
			}
			return Mathf.Max(this.NeededWarmClothesCount(map) - this.ColonistsWithWarmClothesCount(map) - this.FreeWarmClothesSetsCount(map), 0);
		}

		private float LowestTemperatureComing(Map map)
		{
			Twelfth twelfth = GenLocalDate.Twelfth(map);
			float a = this.GetTemperature(twelfth, map);
			for (int i = 0; i < 3; i++)
			{
				twelfth = twelfth.NextTwelfth();
				a = Mathf.Min(a, this.GetTemperature(twelfth, map));
			}
			return Mathf.Min(a, map.mapTemperature.OutdoorTemp);
		}

		public override string GetExplanation()
		{
			Map map = this.MapWithMissingWarmClothes();
			if (map == null)
			{
				return string.Empty;
			}
			int num = this.MissingWarmClothesCount(map);
			if (num == this.NeededWarmClothesCount(map))
			{
				return "NeedWarmClothesDesc1All".Translate() + "\n\n" + "NeedWarmClothesDesc2".Translate(this.LowestTemperatureComing(map).ToStringTemperature("F0"));
			}
			return "NeedWarmClothesDesc1".Translate(num) + "\n\n" + "NeedWarmClothesDesc2".Translate(this.LowestTemperatureComing(map).ToStringTemperature("F0"));
		}

		public override AlertReport GetReport()
		{
			Map map = this.MapWithMissingWarmClothes();
			if (map == null)
			{
				return false;
			}
			float num = this.LowestTemperatureComing(map);
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.GetStatValue(StatDefOf.ComfyTemperatureMin, true) > num)
				{
					return item;
				}
			}
			return true;
		}

		private Map MapWithMissingWarmClothes()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome && !(this.LowestTemperatureComing(map) >= 5.0) && this.MissingWarmClothesCount(map) > 0)
				{
					return map;
				}
			}
			return null;
		}

		private float GetTemperature(Twelfth twelfth, Map map)
		{
			return GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth);
		}
	}
}
