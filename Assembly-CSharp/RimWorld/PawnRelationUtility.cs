using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnRelationUtility
	{
		public static IEnumerable<PawnRelationDef> GetRelations(this Pawn me, Pawn other)
		{
			if (me != other && me.RaceProps.IsFlesh && other.RaceProps.IsFlesh && me.relations.RelatedToAnyoneOrAnyoneRelatedToMe && other.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				try
				{
					bool anyNonKinFamilyByBloodRelation = false;
					List<PawnRelationDef> defs = DefDatabase<PawnRelationDef>.AllDefsListForReading;
					int i = 0;
					int count = defs.Count;
					for (; i < count; i++)
					{
						PawnRelationDef def = defs[i];
						if (def != PawnRelationDefOf.Kin && def.Worker.InRelation(me, other))
						{
							if (!def.familyByBloodRelation)
								;
							yield return def;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
					if (!anyNonKinFamilyByBloodRelation && PawnRelationDefOf.Kin.Worker.InRelation(me, other))
					{
						yield return PawnRelationDefOf.Kin;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				finally
				{
					((_003CGetRelations_003Ec__Iterator0)/*Error near IL_01d7: stateMachine*/)._003C_003E__Finally0();
				}
			}
			yield break;
			IL_01e7:
			/*Error near IL_01e8: Unexpected return in MoveNext()*/;
		}

		public static PawnRelationDef GetMostImportantRelation(this Pawn me, Pawn other)
		{
			PawnRelationDef pawnRelationDef = null;
			foreach (PawnRelationDef relation in me.GetRelations(other))
			{
				if (pawnRelationDef == null || relation.importance > pawnRelationDef.importance)
				{
					pawnRelationDef = relation;
				}
			}
			return pawnRelationDef;
		}

		public static void Notify_PawnsSeenByPlayer(IEnumerable<Pawn> seenPawns, out string pawnRelationsInfo, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonistsAndPrisoners
			where x.relations.everSeenByPlayer
			select x;
			bool flag = false;
			foreach (Pawn seenPawn in seenPawns)
			{
				if (seenPawn.RaceProps.IsFlesh && (informEvenIfSeenBefore || !seenPawn.relations.everSeenByPlayer))
				{
					seenPawn.relations.everSeenByPlayer = true;
					bool flag2 = false;
					foreach (Pawn item in enumerable)
					{
						if (seenPawn != item)
						{
							PawnRelationDef mostImportantRelation = item.GetMostImportantRelation(seenPawn);
							if (mostImportantRelation != null)
							{
								if (!flag2)
								{
									flag2 = true;
									if (flag)
									{
										stringBuilder.AppendLine();
									}
									if (writeSeenPawnsNames)
									{
										stringBuilder.AppendLine(seenPawn.KindLabel.CapitalizeFirst() + " " + seenPawn.LabelShort + ":");
									}
								}
								flag = true;
								stringBuilder.AppendLine("  " + "Relationship".Translate(mostImportantRelation.GetGenderSpecificLabelCap(seenPawn), item.KindLabel + " " + item.LabelShort));
							}
						}
					}
				}
			}
			if (flag)
			{
				pawnRelationsInfo = stringBuilder.ToString().TrimEndNewlines();
			}
			else
			{
				pawnRelationsInfo = null;
			}
		}

		public static void Notify_PawnsSeenByPlayer_Letter(IEnumerable<Pawn> seenPawns, ref string letterLabel, ref string letterText, string relationsInfoHeader, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
		{
			string text = default(string);
			PawnRelationUtility.Notify_PawnsSeenByPlayer(seenPawns, out text, informEvenIfSeenBefore, writeSeenPawnsNames);
			if (!text.NullOrEmpty())
			{
				if (letterLabel.NullOrEmpty())
				{
					letterLabel = "LetterLabelNoticedRelatedPawns".Translate();
				}
				else
				{
					letterLabel = letterLabel + " " + "RelationshipAppendedLetterSuffix".Translate();
				}
				if (!letterText.NullOrEmpty())
				{
					letterText += "\n\n";
				}
				letterText = letterText + relationsInfoHeader + "\n\n" + text;
			}
		}

		public static bool TryAppendRelationsWithColonistsInfo(ref string text, Pawn pawn)
		{
			string text2 = null;
			return PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref text2, pawn);
		}

		public static bool TryAppendRelationsWithColonistsInfo(ref string text, ref string title, Pawn pawn)
		{
			Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			if (mostImportantColonyRelative == null)
			{
				return false;
			}
			if (title != null)
			{
				title = title + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			string genderSpecificLabel = mostImportantColonyRelative.GetMostImportantRelation(pawn).GetGenderSpecificLabel(pawn);
			string str = "\n\n";
			str = ((!mostImportantColonyRelative.IsColonist) ? (str + "RelationshipAppendedLetterTextPrisoner".Translate(mostImportantColonyRelative.LabelShort, genderSpecificLabel)) : (str + "RelationshipAppendedLetterTextColonist".Translate(mostImportantColonyRelative.LabelShort, genderSpecificLabel)));
			text += str.AdjustedFor(pawn);
			return true;
		}

		public static Pawn GetMostImportantColonyRelative(Pawn pawn)
		{
			IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonistsAndPrisoners
			where x.relations.everSeenByPlayer
			select x;
			float num = 0f;
			Pawn pawn2 = null;
			foreach (Pawn item in enumerable)
			{
				PawnRelationDef mostImportantRelation = pawn.GetMostImportantRelation(item);
				if (mostImportantRelation != null && (pawn2 == null || mostImportantRelation.importance > num))
				{
					num = mostImportantRelation.importance;
					pawn2 = item;
				}
			}
			return pawn2;
		}

		public static float MaxPossibleBioAgeAt(float myBiologicalAge, float myChronologicalAge, float atChronologicalAge)
		{
			float num = Mathf.Min(myBiologicalAge, myChronologicalAge - atChronologicalAge);
			if (num < 0.0)
			{
				return -1f;
			}
			return num;
		}

		public static float MinPossibleBioAgeAt(float myBiologicalAge, float atChronologicalAge)
		{
			return Mathf.Max(myBiologicalAge - atChronologicalAge, 0f);
		}
	}
}
