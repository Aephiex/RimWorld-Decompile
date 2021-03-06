using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TendPatient : JobDriver
	{
		private bool usesMedicine;

		private const int BaseTendDuration = 600;

		protected Thing MedicineUsed
		{
			get
			{
				return base.job.targetB.Thing;
			}
		}

		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)base.job.targetA.Thing;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usesMedicine, "usesMedicine", false, false);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usesMedicine = (this.MedicineUsed != null);
		}

		public override bool TryMakePreToilReservations()
		{
			if (!base.pawn.Reserve(this.Deliveree, base.job, 1, -1, null))
			{
				return false;
			}
			if (this.usesMedicine && !base.pawn.Reserve(this.MedicineUsed, base.job, 1, -1, null))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0052: stateMachine*/;
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (!WorkGiver_Tend.GoodLayingStatusForTend(_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree, _003CMakeNewToils_003Ec__Iterator._0024this.pawn))
				{
					return true;
				}
				if (_003CMakeNewToils_003Ec__Iterator._0024this.MedicineUsed != null)
				{
					if (_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree.playerSettings == null)
					{
						return true;
					}
					if (!_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree.playerSettings.medCare.AllowsMedicine(_003CMakeNewToils_003Ec__Iterator._0024this.MedicineUsed.def))
					{
						return true;
					}
				}
				if (_003CMakeNewToils_003Ec__Iterator._0024this.pawn == _003CMakeNewToils_003Ec__Iterator._0024this.Deliveree && (_003CMakeNewToils_003Ec__Iterator._0024this.pawn.playerSettings == null || !_003CMakeNewToils_003Ec__Iterator._0024this.pawn.playerSettings.selfTend))
				{
					return true;
				}
				return false;
			});
			base.AddEndCondition(delegate
			{
				if (HealthAIUtility.ShouldBeTendedNow(_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree))
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil reserveMedicine = null;
			if (this.usesMedicine)
			{
				reserveMedicine = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return reserveMedicine;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			PathEndMode interactionCell = (PathEndMode)((this.Deliveree == base.pawn) ? 1 : 4);
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
			yield return gotoToil;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
