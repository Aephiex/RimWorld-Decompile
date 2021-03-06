using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MeteoriteImpact : IncidentWorker
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			IntVec3 intVec = default(IntVec3);
			return this.TryFindCell(out intVec, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec = default(IntVec3);
			if (!this.TryFindCell(out intVec, map))
			{
				return false;
			}
			List<Thing> list = ItemCollectionGeneratorDefOf.Meteorite.Worker.Generate(default(ItemCollectionGeneratorParams));
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, intVec, map);
			LetterDef textLetterDef = (!list[0].def.building.isResourceRock) ? LetterDefOf.NeutralEvent : LetterDefOf.PositiveEvent;
			string text = string.Format(base.def.letterText, list[0].def.label).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(base.def.letterLabel, text, textLetterDef, new TargetInfo(intVec, map, false), null);
			return true;
		}

		private bool TryFindCell(out IntVec3 cell, Map map)
		{
			IntRange mineablesCountRange = ItemCollectionGenerator_Meteorite.MineablesCountRange;
			int maxMineables = mineablesCountRange.max;
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, (Predicate<IntVec3>)delegate(IntVec3 x)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
				CellRect cellRect = CellRect.CenteredOn(x, num, num);
				int num2 = 0;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.InBounds(map) && iterator.Current.Standable(map))
					{
						num2++;
					}
					iterator.MoveNext();
				}
				return num2 >= maxMineables;
			});
		}
	}
}
