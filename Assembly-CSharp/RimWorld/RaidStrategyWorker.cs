using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class RaidStrategyWorker
	{
		public RaidStrategyDef def;

		public virtual float SelectionChance(Map map)
		{
			return this.def.selectionChance;
		}

		public abstract LordJob MakeLordJob(IncidentParms parms, Map map);

		public virtual bool CanUseWith(IncidentParms parms)
		{
			if ((float)GenDate.DaysPassed < this.def.minDaysPassed)
			{
				return false;
			}
			if (parms.points < this.MinimumPoints(parms.faction))
			{
				return false;
			}
			return true;
		}

		public virtual float MinimumPoints(Faction faction)
		{
			return faction.def.MinPointsToGenerateNormalPawnGroup();
		}

		public virtual float MinMaxAllowedPawnGenOptionCost(Faction faction)
		{
			return 0f;
		}

		public virtual bool CanUsePawnGenOption(PawnGenOption g, List<PawnGenOption> chosenGroups)
		{
			return true;
		}
	}
}
