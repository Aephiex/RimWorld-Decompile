using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public static class ToilFailConditions
	{
		public static Toil FailOn(this Toil toil, Func<Toil, bool> condition)
		{
			toil.AddEndCondition(delegate
			{
				if (condition(toil))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return toil;
		}

		public static T FailOn<T>(this T f, Func<bool> condition) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				if (condition())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnDestroyedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				if (f.GetActor().jobs.curJob.GetTarget(ind).Thing.DestroyedOrNull())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnDespawnedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
				Thing thing = target.Thing;
				if (thing == null && target.IsValid)
				{
					return JobCondition.Ongoing;
				}
				if (thing != null && thing.Spawned && thing.Map == f.GetActor().Map)
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Incompletable;
			});
			return f;
		}

		public static T EndOnDespawnedOrNull<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
				Thing thing = target.Thing;
				if (thing == null && target.IsValid)
				{
					return JobCondition.Ongoing;
				}
				if (thing != null && thing.Spawned && thing.Map == f.GetActor().Map)
				{
					return JobCondition.Ongoing;
				}
				return endCondition;
			});
			return f;
		}

		public static T EndOnNoTargetInQueue<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (GenList.NullOrEmpty<LocalTargetInfo>((IList<LocalTargetInfo>)targetQueue))
				{
					return endCondition;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Thing thing = f.GetActor().jobs.curJob.GetTarget(ind).Thing;
				if (((Pawn)thing).Downed)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnNotDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Thing thing = f.GetActor().jobs.curJob.GetTarget(ind).Thing;
				if (!((Pawn)thing).Downed)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnNotAwake<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Thing thing = f.GetActor().jobs.curJob.GetTarget(ind).Thing;
				if (!((Pawn)thing).Awake())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnNotCasualInterruptible<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Thing thing = f.GetActor().jobs.curJob.GetTarget(ind).Thing;
				if (!((Pawn)thing).CanCasuallyInteractNow(false))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				if (pawn != null && pawn.InMentalState)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnAggroMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				if (pawn != null && pawn.InAggroMentalState)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnAggroMentalStateAndHostile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				if (pawn != null && pawn.InAggroMentalState && pawn.HostileTo(f.GetActor()))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnSomeonePhysicallyInteracting<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				if (thing != null && actor.Map.physicalInteractionReservationManager.IsReserved(thing) && !actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				if (actor.Faction != Faction.OfPlayer)
				{
					return JobCondition.Ongoing;
				}
				if (actor.jobs.curJob.ignoreForbidden)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				if (thing == null)
				{
					return JobCondition.Ongoing;
				}
				if (thing.IsForbidden(actor))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnDespawnedNullOrForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.FailOnDespawnedOrNull(ind);
			f.FailOnForbidden(ind);
			return f;
		}

		public static T FailOnDestroyedNullOrForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.FailOnDestroyedOrNull(ind);
			f.FailOnForbidden(ind);
			return f;
		}

		public static T FailOnThingMissingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = curJob.GetTarget(ind).Thing;
				if (thing != null && actor.Map.designationManager.DesignationOn(thing, desDef) != null)
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnCellMissingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				if (actor.Map.designationManager.DesignationAt(curJob.GetTarget(ind).Cell, desDef) == null)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnBurningImmobile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				if (f.GetActor().jobs.curJob.GetTarget(ind).ToTargetInfo(f.GetActor().Map).IsBurning())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static T FailOnCannotTouch<T>(this T f, TargetIndex ind, PathEndMode peMode) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				if (!f.GetActor().CanReachImmediate(f.GetActor().jobs.curJob.GetTarget(ind), peMode))
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static Toil FailOnDespawnedNullOrForbiddenPlacedThings(this Toil toil)
		{
			toil.AddFailCondition(delegate
			{
				if (toil.actor.jobs.curJob.placedThings == null)
				{
					return false;
				}
				int num = 0;
				while (num < toil.actor.jobs.curJob.placedThings.Count)
				{
					ThingStackPartClass thingStackPartClass = toil.actor.jobs.curJob.placedThings[num];
					if (thingStackPartClass.thing != null && thingStackPartClass.thing.Spawned && thingStackPartClass.thing.Map == toil.actor.Map && (toil.actor.CurJob.ignoreForbidden || !thingStackPartClass.thing.IsForbidden(toil.actor)))
					{
						num++;
						continue;
					}
					return true;
				}
				return false;
			});
			return toil;
		}
	}
}
