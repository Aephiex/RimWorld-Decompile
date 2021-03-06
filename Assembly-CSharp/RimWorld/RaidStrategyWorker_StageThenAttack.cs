using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_StageThenAttack : RaidStrategyWorker
	{
		public override LordJob MakeLordJob(IncidentParms parms, Map map)
		{
			IntVec3 stageLoc = RCellFinder.FindSiegePositionFrom(parms.spawnCenter, map);
			return new LordJob_StageThenAttack(parms.faction, stageLoc);
		}

		public override bool CanUseWith(IncidentParms parms)
		{
			if (!base.CanUseWith(parms))
			{
				return false;
			}
			return parms.faction.def.canStageAttacks;
		}
	}
}
