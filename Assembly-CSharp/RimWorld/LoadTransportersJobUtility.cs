using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class LoadTransportersJobUtility
	{
		private static HashSet<Thing> neededThings = new HashSet<Thing>();

		public static bool HasJobOnTransporter(Pawn pawn, CompTransporter transporter)
		{
			if (transporter.parent.IsForbidden(pawn))
			{
				return false;
			}
			if (!transporter.AnythingLeftToLoad)
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return false;
			}
			if (!pawn.CanReserveAndReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, false))
			{
				return false;
			}
			Thing thing = LoadTransportersJobUtility.FindThingToLoad(pawn, transporter);
			if (thing == null)
			{
				return false;
			}
			return true;
		}

		public static Job JobOnTransporter(Pawn p, CompTransporter transporter)
		{
			Thing thing = LoadTransportersJobUtility.FindThingToLoad(p, transporter);
			Job job = new Job(JobDefOf.HaulToContainer, thing, transporter.parent);
			int countToTransfer = TransferableUtility.TransferableMatchingDesperate(thing, transporter.leftToLoad).CountToTransfer;
			job.count = Mathf.Min(countToTransfer, thing.stackCount);
			job.ignoreForbidden = true;
			return job;
		}

		private static Thing FindThingToLoad(Pawn p, CompTransporter transporter)
		{
			LoadTransportersJobUtility.neededThings.Clear();
			List<TransferableOneWay> leftToLoad = transporter.leftToLoad;
			if (leftToLoad != null)
			{
				for (int i = 0; i < leftToLoad.Count; i++)
				{
					TransferableOneWay transferableOneWay = leftToLoad[i];
					if (transferableOneWay.CountToTransfer > 0)
					{
						for (int j = 0; j < transferableOneWay.things.Count; j++)
						{
							LoadTransportersJobUtility.neededThings.Add(transferableOneWay.things[j]);
						}
					}
				}
			}
			if (!LoadTransportersJobUtility.neededThings.Any())
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Thing x) => LoadTransportersJobUtility.neededThings.Contains(x) && p.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
			if (thing == null)
			{
				foreach (Thing neededThing in LoadTransportersJobUtility.neededThings)
				{
					Pawn pawn = neededThing as Pawn;
					if (pawn != null && (!pawn.IsColonist || pawn.Downed) && !pawn.inventory.UnloadEverything && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
					{
						return pawn;
					}
				}
			}
			LoadTransportersJobUtility.neededThings.Clear();
			return thing;
		}
	}
}
