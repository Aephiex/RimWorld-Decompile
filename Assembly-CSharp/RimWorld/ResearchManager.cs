using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class ResearchManager : IExposable
	{
		public ResearchProjectDef currentProj;

		private Dictionary<ResearchProjectDef, float> progress = new Dictionary<ResearchProjectDef, float>();

		private float GlobalProgressFactor = 0.007f;

		public bool AnyProjectIsAvailable
		{
			get
			{
				return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find((ResearchProjectDef x) => x.CanStartNow) != null;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<ResearchProjectDef>(ref this.currentProj, "currentProj");
			Scribe_Collections.Look<ResearchProjectDef, float>(ref this.progress, "progress", LookMode.Def, LookMode.Value);
		}

		public float GetProgress(ResearchProjectDef proj)
		{
			float result = default(float);
			if (this.progress.TryGetValue(proj, out result))
			{
				return result;
			}
			this.progress.Add(proj, 0f);
			return 0f;
		}

		public void ResearchPerformed(float amount, Pawn researcher)
		{
			if (this.currentProj == null)
			{
				Log.Error("Researched without having an active project.");
			}
			else
			{
				amount *= this.GlobalProgressFactor;
				if (researcher != null && researcher.Faction != null)
				{
					amount /= this.currentProj.CostFactor(researcher.Faction.def.techLevel);
				}
				if (DebugSettings.fastResearch)
				{
					amount = (float)(amount * 500.0);
				}
				if (researcher != null)
				{
					researcher.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
				}
				float num = this.GetProgress(this.currentProj);
				num += amount;
				this.progress[this.currentProj] = num;
				if (this.currentProj.IsFinished)
				{
					this.ReapplyAllMods();
					this.DoCompletionDialog(this.currentProj, researcher);
					if (researcher != null)
					{
						TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, this.currentProj);
					}
					this.currentProj = null;
				}
			}
		}

		public void ReapplyAllMods()
		{
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (allDef.IsFinished)
				{
					allDef.ReapplyAllMods();
				}
			}
		}

		public void InstantFinish(ResearchProjectDef proj, bool doCompletionDialog = false)
		{
			if (proj.prerequisites != null)
			{
				for (int i = 0; i < proj.prerequisites.Count; i++)
				{
					if (!proj.prerequisites[i].IsFinished)
					{
						this.InstantFinish(proj.prerequisites[i], false);
					}
				}
			}
			this.progress[proj] = proj.baseCost;
			this.ReapplyAllMods();
			if (doCompletionDialog)
			{
				this.DoCompletionDialog(proj, null);
			}
			if (this.currentProj == proj)
			{
				this.currentProj = null;
			}
		}

		private void DoCompletionDialog(ResearchProjectDef proj, Pawn researcher)
		{
			string text = "ResearchFinished".Translate(this.currentProj.LabelCap) + "\n\n" + this.currentProj.DescriptionDiscovered;
			DiaNode diaNode = new DiaNode(text);
			diaNode.options.Add(DiaOption.DefaultOK);
			DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
			diaOption.resolveTree = true;
			diaOption.action = delegate
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, true);
			};
			diaNode.options.Add(diaOption);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, null));
		}

		public void DebugSetAllProjectsFinished()
		{
			this.progress.Clear();
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				this.progress.Add(allDef, allDef.baseCost);
			}
			this.ReapplyAllMods();
		}
	}
}
