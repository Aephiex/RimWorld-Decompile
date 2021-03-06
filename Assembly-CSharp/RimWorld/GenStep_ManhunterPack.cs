using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ManhunterPack : GenStep
	{
		public FloatRange pointsRange = new FloatRange(300f, 500f);

		private int MinRoomCells = 225;

		public override void Generate(Map map)
		{
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false);
			IntVec3 root = default(IntVec3);
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams) && x.GetRoom(map, RegionType.Set_Passable).CellCount >= this.MinRoomCells), map, out root))
			{
				float randomInRange = this.pointsRange.RandomInRange;
				PawnKindDef animalKind = default(PawnKindDef);
				if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(randomInRange, map.Tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(randomInRange, -1, out animalKind))
					return;
				List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, map.Tile, randomInRange);
				for (int i = 0; i < list.Count; i++)
				{
					IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 10);
					GenSpawn.Spawn(list[i], loc, map, Rot4.Random, false);
					list[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null);
				}
			}
		}
	}
}
