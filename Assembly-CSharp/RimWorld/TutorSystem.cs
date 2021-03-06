using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TutorSystem
	{
		public static bool TutorialMode
		{
			get
			{
				return Find.Storyteller != null && Find.Storyteller.def != null && Find.Storyteller.def.tutorialMode;
			}
		}

		public static bool AdaptiveTrainingEnabled
		{
			get
			{
				if (!Prefs.AdaptiveTrainingEnabled)
				{
					return false;
				}
				if (Find.Storyteller != null && Find.Storyteller.def != null && Find.Storyteller.def.disableAdaptiveTraining)
				{
					return false;
				}
				return true;
			}
		}

		public static void Notify_Event(string eventTag, IntVec3 cell)
		{
			TutorSystem.Notify_Event(new EventPack(eventTag, cell));
		}

		public static void Notify_Event(EventPack ep)
		{
			if (TutorSystem.TutorialMode)
			{
				if (DebugViewSettings.logTutor)
				{
					Log.Message("Notify_Event: " + ep);
				}
				if (Current.Game != null)
				{
					Lesson current = Find.ActiveLesson.Current;
					if (Find.ActiveLesson.Current != null)
					{
						Find.ActiveLesson.Current.Notify_Event(ep);
					}
					foreach (InstructionDef allDef in DefDatabase<InstructionDef>.AllDefs)
					{
						if (allDef.eventTagInitiate == ep.Tag && (allDef.eventTagInitiateSource == null || (current != null && allDef.eventTagInitiateSource == current.Instruction)) && (TutorSystem.TutorialMode || !allDef.tutorialModeOnly))
						{
							Find.ActiveLesson.Activate(allDef);
							break;
						}
					}
				}
			}
		}

		public static bool AllowAction(EventPack ep)
		{
			if (!TutorSystem.TutorialMode)
			{
				return true;
			}
			if (DebugViewSettings.logTutor)
			{
				Log.Message("AllowAction: " + ep);
			}
			if (ep.Cells != null && ep.Cells.Count() == 1)
			{
				return TutorSystem.AllowAction(new EventPack(ep.Tag, ep.Cells.First()));
			}
			if (Find.ActiveLesson.Current != null)
			{
				AcceptanceReport acceptanceReport = Find.ActiveLesson.Current.AllowAction(ep);
				if (!acceptanceReport.Accepted)
				{
					string text = acceptanceReport.Reason.NullOrEmpty() ? Find.ActiveLesson.Current.DefaultRejectInputMessage : acceptanceReport.Reason;
					Messages.Message(text, MessageTypeDefOf.RejectInput);
					return false;
				}
			}
			return true;
		}
	}
}
