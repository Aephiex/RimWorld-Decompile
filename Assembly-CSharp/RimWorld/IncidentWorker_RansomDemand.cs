using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RansomDemand : IncidentWorker
	{
		private const int TimeoutTicks = 60000;

		private static List<Pawn> candidates = new List<Pawn>();

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			if (!CommsConsoleUtility.PlayerHasPoweredCommsConsole(map))
			{
				return false;
			}
			if (this.RandomKidnappedColonist() == null)
			{
				return false;
			}
			return base.CanFireNowSub(target);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Pawn pawn = this.RandomKidnappedColonist();
			if (pawn == null)
			{
				return false;
			}
			Faction faction = this.FactionWhichKidnapped(pawn);
			int num = this.RandomFee(pawn);
			ChoiceLetter_RansomDemand choiceLetter_RansomDemand = (ChoiceLetter_RansomDemand)LetterMaker.MakeLetter(base.def.letterLabel, "RansomDemand".Translate(pawn.LabelShort, faction.Name, num).AdjustedFor(pawn), base.def.letterDef);
			choiceLetter_RansomDemand.title = "RansomDemandTitle".Translate(map.info.parent.Label);
			choiceLetter_RansomDemand.radioMode = true;
			choiceLetter_RansomDemand.kidnapped = pawn;
			choiceLetter_RansomDemand.faction = faction;
			choiceLetter_RansomDemand.map = map;
			choiceLetter_RansomDemand.fee = num;
			choiceLetter_RansomDemand.StartTimeout(60000);
			Find.LetterStack.ReceiveLetter(choiceLetter_RansomDemand, null);
			return true;
		}

		private Pawn RandomKidnappedColonist()
		{
			IncidentWorker_RansomDemand.candidates.Clear();
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				List<Pawn> kidnappedPawnsListForReading = allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading;
				for (int j = 0; j < kidnappedPawnsListForReading.Count; j++)
				{
					if (kidnappedPawnsListForReading[j].Faction == Faction.OfPlayer)
					{
						IncidentWorker_RansomDemand.candidates.Add(kidnappedPawnsListForReading[j]);
					}
				}
			}
			List<Letter> lettersListForReading = Find.LetterStack.LettersListForReading;
			for (int k = 0; k < lettersListForReading.Count; k++)
			{
				ChoiceLetter_RansomDemand choiceLetter_RansomDemand = lettersListForReading[k] as ChoiceLetter_RansomDemand;
				if (choiceLetter_RansomDemand != null)
				{
					IncidentWorker_RansomDemand.candidates.Remove(choiceLetter_RansomDemand.kidnapped);
				}
			}
			Pawn result = default(Pawn);
			if (!((IEnumerable<Pawn>)IncidentWorker_RansomDemand.candidates).TryRandomElement<Pawn>(out result))
			{
				return null;
			}
			IncidentWorker_RansomDemand.candidates.Clear();
			return result;
		}

		private Faction FactionWhichKidnapped(Pawn pawn)
		{
			return Find.FactionManager.AllFactionsListForReading.Find((Faction x) => x.kidnapped.KidnappedPawnsListForReading.Contains(pawn));
		}

		private int RandomFee(Pawn pawn)
		{
			return (int)(pawn.MarketValue * Rand.Range(1.2f, 3f));
		}
	}
}
