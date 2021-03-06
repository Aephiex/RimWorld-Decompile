using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Storyteller : IExposable
	{
		public StorytellerDef def;

		public DifficultyDef difficulty;

		public List<StorytellerComp> storytellerComps;

		public IncidentQueue incidentQueue = new IncidentQueue();

		public StoryIntender_Population intenderPopulation;

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		private static List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

		public List<IIncidentTarget> AllIncidentTargets
		{
			get
			{
				Storyteller.tmpAllIncidentTargets.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Storyteller.tmpAllIncidentTargets.Add(maps[i]);
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled)
					{
						Storyteller.tmpAllIncidentTargets.Add(caravans[j]);
					}
				}
				Storyteller.tmpAllIncidentTargets.Add(Find.World);
				return Storyteller.tmpAllIncidentTargets;
			}
		}

		public Storyteller()
		{
		}

		public Storyteller(StorytellerDef def, DifficultyDef difficulty)
		{
			this.def = def;
			this.difficulty = difficulty;
			this.intenderPopulation = new StoryIntender_Population(this);
			this.InitializeStorytellerComps();
		}

		private void InitializeStorytellerComps()
		{
			this.storytellerComps = new List<StorytellerComp>();
			for (int i = 0; i < this.def.comps.Count; i++)
			{
				StorytellerComp storytellerComp = (StorytellerComp)Activator.CreateInstance(this.def.comps[i].compClass);
				storytellerComp.props = this.def.comps[i];
				this.storytellerComps.Add(storytellerComp);
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<StorytellerDef>(ref this.def, "def");
			Scribe_Defs.Look<DifficultyDef>(ref this.difficulty, "difficulty");
			Scribe_Deep.Look<IncidentQueue>(ref this.incidentQueue, "incidentQueue", new object[0]);
			Scribe_Deep.Look<StoryIntender_Population>(ref this.intenderPopulation, "intenderPopulation", new object[1]
			{
				this
			});
			if (this.difficulty == null)
			{
				Log.Error("Loaded storyteller without difficulty");
				this.difficulty = DefDatabase<DifficultyDef>.AllDefsListForReading[3];
			}
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				this.InitializeStorytellerComps();
			}
		}

		public void StorytellerTick()
		{
			this.incidentQueue.IncidentQueueTick();
			if (Find.TickManager.TicksGame % 1000 == 0 && DebugSettings.enableStoryteller)
			{
				foreach (FiringIncident item in this.MakeIncidentsForInterval())
				{
					this.TryFire(item);
				}
			}
		}

		public void TryFire(FiringIncident fi)
		{
			if (!fi.parms.forced && !fi.def.Worker.CanFireNow(fi.parms.target))
				return;
			if (fi.def.Worker.TryExecute(fi.parms))
			{
				fi.parms.target.StoryState.Notify_IncidentFired(fi);
			}
		}

		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			List<IIncidentTarget> targets = this.AllIncidentTargets;
			for (int j = 0; j < this.storytellerComps.Count; j++)
			{
				StorytellerComp c = this.storytellerComps[j];
				if (!(GenDate.DaysPassedFloat <= c.props.minDaysPassed))
				{
					for (int i = 0; i < targets.Count; i++)
					{
						IIncidentTarget targ = targets[i];
						if (c.props.allowedTargetTypes == null || c.props.allowedTargetTypes.Count == 0 || c.props.allowedTargetTypes.Intersect(targ.AcceptedTypes()).Any())
						{
							foreach (FiringIncident item in c.MakeIntervalIncidents(targ))
							{
								if (!Find.Storyteller.difficulty.allowBigThreats && (item.def.category == IncidentCategory.ThreatBig || item.def.category == IncidentCategory.RaidBeacon))
								{
									continue;
								}
								yield return item;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
			}
			yield break;
			IL_0226:
			/*Error near IL_0227: Unexpected return in MoveNext()*/;
		}

		public void Notify_DefChanged()
		{
			this.InitializeStorytellerComps();
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Storyteller : " + this.def.label);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(this.intenderPopulation.DebugReadout);
			stringBuilder.AppendLine("Global stats:");
			stringBuilder.AppendLine("   numRaidsEnemy: " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
			stringBuilder.AppendLine("   TotalThreatFactor: " + Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor.ToString("F5"));
			stringBuilder.AppendLine("      ShortFactor: " + Find.StoryWatcher.watcherRampUp.ShortTermFactor.ToString("F5"));
			stringBuilder.AppendLine("      LongFactor: " + Find.StoryWatcher.watcherRampUp.LongTermFactor.ToString("F5"));
			stringBuilder.AppendLine("   Current default ThreatBig parms points:");
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				IncidentParms incidentParms = this.storytellerComps[i].GenerateParms(IncidentCategory.ThreatBig, Find.VisibleMap);
				stringBuilder.AppendLine("      " + this.storytellerComps[i].GetType() + ": " + incidentParms.points);
			}
			if (Find.VisibleMap != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("VisibleMap stats:");
				stringBuilder.AppendLine("   Wealth: " + Find.VisibleMap.wealthWatcher.WealthTotal);
				stringBuilder.AppendLine("   DaysSinceSeriousDamage: " + Find.VisibleMap.damageWatcher.DaysSinceSeriousDamage.ToString("F1"));
				stringBuilder.AppendLine("   LastThreatBigQueueTick: " + Find.VisibleMap.storyState.LastThreatBigTick.ToStringTicksToPeriod(true, false, true));
				stringBuilder.AppendLine("   FireDanger: " + Find.VisibleMap.fireWatcher.FireDanger.ToString("F2"));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident targets:");
			for (int j = 0; j < this.AllIncidentTargets.Count; j++)
			{
				stringBuilder.AppendLine("   " + this.AllIncidentTargets[j].ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
