using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_SetPlantToGrow : Command
	{
		public IPlantToGrowSettable settable;

		private List<IPlantToGrowSettable> settables;

		private static readonly Texture2D SetPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow", true);

		public Command_SetPlantToGrow()
		{
			base.tutorTag = "GrowingZoneSetPlant";
			ThingDef thingDef = null;
			bool flag = false;
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				IPlantToGrowSettable plantToGrowSettable = selectedObject as IPlantToGrowSettable;
				if (plantToGrowSettable != null)
				{
					if (thingDef != null && thingDef != plantToGrowSettable.GetPlantDefToGrow())
					{
						flag = true;
						break;
					}
					thingDef = plantToGrowSettable.GetPlantDefToGrow();
				}
			}
			if (flag)
			{
				base.icon = Command_SetPlantToGrow.SetPlantToGrowTex;
				base.defaultLabel = "CommandSelectPlantToGrowMulti".Translate();
			}
			else
			{
				base.icon = thingDef.uiIcon;
				base.iconAngle = thingDef.uiIconAngle;
				base.defaultLabel = "CommandSelectPlantToGrow".Translate(thingDef.label);
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (this.settables == null)
			{
				this.settables = new List<IPlantToGrowSettable>();
			}
			if (!this.settables.Contains(this.settable))
			{
				this.settables.Add(this.settable);
			}
			foreach (ThingDef item in GenPlant.ValidPlantTypesForGrowers(this.settables))
			{
				if (this.IsPlantAvailable(item))
				{
					ThingDef localPlantDef = item;
					string text = item.LabelCap;
					if (item.plant.sowMinSkill > 0)
					{
						string text2 = text;
						text = text2 + " (" + "MinSkill".Translate() + ": " + item.plant.sowMinSkill + ")";
					}
					list.Add(new FloatMenuOption(text, delegate
					{
						string s = base.tutorTag + "-" + localPlantDef.defName;
						if (TutorSystem.AllowAction(s))
						{
							for (int i = 0; i < this.settables.Count; i++)
							{
								this.settables[i].SetPlantDefToGrow(localPlantDef);
							}
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.SetGrowingZonePlant, KnowledgeAmount.Total);
							this.WarnAsAppropriate(localPlantDef);
							TutorSystem.Notify_Event(s);
						}
					}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton((float)(rect.x + 5.0), (float)(rect.y + (rect.height - 24.0) / 2.0), localPlantDef), null));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			if (this.settables == null)
			{
				this.settables = new List<IPlantToGrowSettable>();
			}
			this.settables.Add(((Command_SetPlantToGrow)other).settable);
			return false;
		}

		private void WarnAsAppropriate(ThingDef plantDef)
		{
			if (plantDef.plant.sowMinSkill > 0)
			{
				foreach (Pawn item in this.settable.Map.mapPawns.FreeColonistsSpawned)
				{
					if (item.skills.GetSkill(SkillDefOf.Growing).Level >= plantDef.plant.sowMinSkill && !item.Downed && item.workSettings.WorkIsActive(WorkTypeDefOf.Growing))
						return;
				}
				Find.WindowStack.Add(new Dialog_MessageBox("NoGrowerCanPlant".Translate(plantDef.label, plantDef.plant.sowMinSkill).CapitalizeFirst(), null, null, null, null, null, false));
			}
			if (plantDef.plant.cavePlant)
			{
				IntVec3 cell = IntVec3.Invalid;
				int num = 0;
				while (num < this.settables.Count)
				{
					foreach (IntVec3 cell2 in this.settables[num].Cells)
					{
						if (cell2.Roofed(this.settables[num].Map) && !(this.settables[num].Map.glowGrid.GameGlowAt(cell2, true) > 0.0))
						{
							continue;
						}
						cell = cell2;
						break;
					}
					if (!cell.IsValid)
					{
						num++;
						continue;
					}
					break;
				}
				if (cell.IsValid)
				{
					Messages.Message("MessageWarningCavePlantsExposedToLight".Translate(plantDef.label).CapitalizeFirst(), new TargetInfo(cell, this.settable.Map, false), MessageTypeDefOf.RejectInput);
				}
			}
		}

		private bool IsPlantAvailable(ThingDef plantDef)
		{
			List<ResearchProjectDef> sowResearchPrerequisites = plantDef.plant.sowResearchPrerequisites;
			if (sowResearchPrerequisites == null)
			{
				return true;
			}
			for (int i = 0; i < sowResearchPrerequisites.Count; i++)
			{
				if (!sowResearchPrerequisites[i].IsFinished)
				{
					return false;
				}
			}
			return true;
		}
	}
}
