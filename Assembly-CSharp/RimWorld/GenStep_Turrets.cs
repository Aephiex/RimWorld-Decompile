using RimWorld.BaseGen;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_Turrets : GenStep
	{
		public IntRange widthRange = new IntRange(3, 4);

		public IntRange turretsCountRange = new IntRange(4, 5);

		public IntRange mortarsCountRange = new IntRange(0, 1);

		public IntRange guardsCountRange = IntRange.one;

		private const int Padding = 7;

		public override void Generate(Map map)
		{
			CellRect cellRect = default(CellRect);
			if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out cellRect))
			{
				cellRect = this.FindRandomRectToDefend(map);
			}
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : (from x in Find.FactionManager.AllFactions
			where !x.defeated && x.HostileTo(Faction.OfPlayer) && !x.def.hidden && (int)x.def.techLevel >= 4
			select x).RandomElementWithFallback(Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined));
			int randomInRange = this.widthRange.RandomInRange;
			CellRect rect = cellRect.ExpandedBy(7 + randomInRange).ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = randomInRange;
			resolveParams.edgeDefenseTurretsCount = this.turretsCountRange.RandomInRange;
			resolveParams.edgeDefenseMortarsCount = this.mortarsCountRange.RandomInRange;
			resolveParams.edgeDefenseGuardsCount = this.guardsCountRange.RandomInRange;
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("edgeDefense", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
			ResolveParams resolveParams2 = default(ResolveParams);
			resolveParams2.rect = rect;
			resolveParams2.faction = faction;
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("outdoorLighting", resolveParams2);
			RimWorld.BaseGen.BaseGen.Generate();
		}

		private CellRect FindRandomRectToDefend(Map map)
		{
			IntVec3 size = map.Size;
			int x2 = size.x;
			IntVec3 size2 = map.Size;
			int rectRadius = Mathf.Max(Mathf.RoundToInt((float)((float)Mathf.Min(x2, size2.z) * 0.070000000298023224)), 1);
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			IntVec3 center = default(IntVec3);
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>)delegate(IntVec3 x)
			{
				if (!map.reachability.CanReachMapEdge(x, traverseParams))
				{
					return false;
				}
				CellRect cellRect = CellRect.CenteredOn(x, rectRadius);
				int num = 0;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (!iterator.Current.InBounds(map))
					{
						return false;
					}
					if (iterator.Current.Standable(map) || iterator.Current.GetPlant(map) != null)
					{
						num++;
					}
					iterator.MoveNext();
				}
				return (float)num / (float)cellRect.Area >= 0.60000002384185791;
			}, map, out center))
			{
				return CellRect.CenteredOn(center, rectRadius);
			}
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(map)), map, out center))
			{
				return CellRect.CenteredOn(center, rectRadius);
			}
			return CellRect.CenteredOn(CellFinder.RandomCell(map), rectRadius).ClipInsideMap(map);
		}
	}
}
