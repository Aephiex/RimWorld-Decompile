using System;
using UnityEngine;

namespace Verse.AI
{
	public static class Toils_Combat
	{
		public static Toil TrySetJobToUseAttackVerb()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				bool allowManualCastWeapons = !actor.IsColonist;
				Verb verb = actor.TryGetAttackVerb(allowManualCastWeapons);
				if (verb == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
					curJob.verbToUse = verb;
				}
			};
			return toil;
		}

		public static Toil GotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				CastPositionRequest newReq = default(CastPositionRequest);
				newReq.caster = toil.actor;
				newReq.target = thing;
				newReq.verb = curJob.verbToUse;
				newReq.maxRangeFromTarget = ((closeIfDowned && pawn != null && pawn.Downed) ? Mathf.Min(curJob.verbToUse.verbProps.range, (float)pawn.RaceProps.executionRange) : curJob.verbToUse.verbProps.range);
				newReq.wantCoverFromTarget = false;
				IntVec3 intVec = default(IntVec3);
				if (!CastPositionFinder.TryFindCastPosition(newReq, out intVec))
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
					toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
					actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
				}
			};
			toil.FailOnDespawnedOrNull(targetInd);
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil CastVerb(TargetIndex targetInd, bool canFreeIntercept = true)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Verb verbToUse = toil.actor.jobs.curJob.verbToUse;
				LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(targetInd);
				bool canFreeIntercept2 = canFreeIntercept;
				verbToUse.TryStartCastOn(target, false, canFreeIntercept2);
			};
			toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
			return toil;
		}

		public static Toil FollowAndMeleeAttack(TargetIndex targetInd, Action hitAction)
		{
			Toil followAndAttack = new Toil();
			followAndAttack.tickAction = delegate
			{
				Pawn actor = followAndAttack.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				if (!thing.Spawned)
				{
					curDriver.ReadyForNextToil();
				}
				else if (thing != actor.pather.Destination.Thing || (!actor.pather.Moving && !actor.CanReachImmediate(thing, PathEndMode.Touch)))
				{
					actor.pather.StartPath(thing, PathEndMode.Touch);
				}
				else if (actor.CanReachImmediate(thing, PathEndMode.Touch))
				{
					if (pawn != null && pawn.Downed && !curJob.killIncappedTarget)
					{
						curDriver.ReadyForNextToil();
					}
					else
					{
						hitAction();
					}
				}
			};
			followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
			return followAndAttack;
		}
	}
}
