using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanTendUtility
	{
		private static List<Pawn> tmpPawnsNeedingTreatment = new List<Pawn>();

		public static void TryTendToRandomPawn(Caravan caravan)
		{
			CaravanTendUtility.FindPawnsNeedingTend(caravan, CaravanTendUtility.tmpPawnsNeedingTreatment);
			if (CaravanTendUtility.tmpPawnsNeedingTreatment.Any())
			{
				Pawn patient = CaravanTendUtility.tmpPawnsNeedingTreatment.RandomElement();
				Pawn pawn = CaravanTendUtility.FindBestDoctor(caravan, patient);
				if (pawn != null)
				{
					Medicine medicine = null;
					Pawn pawn2 = null;
					CaravanInventoryUtility.TryGetBestMedicine(caravan, patient, out medicine, out pawn2);
					TendUtility.DoTend(pawn, patient, medicine);
					if (medicine != null && medicine.Destroyed && pawn2 != null)
					{
						pawn2.inventory.innerContainer.Remove(medicine);
					}
					CaravanTendUtility.tmpPawnsNeedingTreatment.Clear();
				}
			}
		}

		private static void FindPawnsNeedingTend(Caravan caravan, List<Pawn> outPawnsNeedingTend)
		{
			outPawnsNeedingTend.Clear();
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				if ((pawn.playerSettings == null || (int)pawn.playerSettings.medCare > 0) && pawn.health.HasHediffsNeedingTend(false))
				{
					outPawnsNeedingTend.Add(pawn);
				}
			}
		}

		private static Pawn FindBestDoctor(Caravan caravan, Pawn patient)
		{
			float num = 0f;
			Pawn pawn = null;
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn2 = pawnsListForReading[i];
				if (pawn2 != patient && CaravanUtility.IsOwner(pawn2, caravan.Faction) && !pawn2.Downed && !pawn2.InMentalState && (pawn2.story == null || !pawn2.story.WorkTypeIsDisabled(WorkTypeDefOf.Doctor)))
				{
					float statValue = pawn2.GetStatValue(StatDefOf.MedicalTendQuality, true);
					if (statValue > num || pawn == null)
					{
						num = statValue;
						pawn = pawn2;
					}
				}
			}
			return pawn;
		}
	}
}
