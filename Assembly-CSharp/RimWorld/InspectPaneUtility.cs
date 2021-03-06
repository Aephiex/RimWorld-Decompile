using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class InspectPaneUtility
	{
		private static Dictionary<string, string> truncatedLabelsCached = new Dictionary<string, string>();

		public const float TabWidth = 72f;

		public const float TabHeight = 30f;

		private static readonly Texture2D InspectTabButtonFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.07450981f, 0.08627451f, 0.105882354f, 1f));

		public const float CornerButtonsSize = 24f;

		public const float PaneInnerMargin = 12f;

		public const float PaneHeight = 165f;

		private const int TabMinimum = 6;

		private static List<Thing> selectedThings = new List<Thing>();

		public static void Reset()
		{
			InspectPaneUtility.truncatedLabelsCached.Clear();
		}

		public static float PaneWidthFor(IInspectPane pane)
		{
			if (pane == null)
			{
				return 432f;
			}
			int num = 0;
			foreach (InspectTabBase curTab in pane.CurTabs)
			{
				if (curTab.IsVisible)
				{
					num++;
				}
			}
			return (float)(72.0 * (float)Mathf.Max(6, num));
		}

		public static Vector2 PaneSizeFor(IInspectPane pane)
		{
			return new Vector2(InspectPaneUtility.PaneWidthFor(pane), 165f);
		}

		public static bool CanInspectTogether(object A, object B)
		{
			Thing thing = A as Thing;
			Thing thing2 = B as Thing;
			if (thing != null && thing2 != null)
			{
				if (thing.def.category == ThingCategory.Pawn)
				{
					return false;
				}
				return thing.def == thing2.def;
			}
			return false;
		}

		public static string AdjustedLabelFor(IEnumerable<object> selected, Rect rect)
		{
			Zone zone = selected.First() as Zone;
			string str;
			if (zone != null)
			{
				str = zone.label;
			}
			else
			{
				InspectPaneUtility.selectedThings.Clear();
				foreach (object item in selected)
				{
					Thing thing = item as Thing;
					if (thing != null)
					{
						InspectPaneUtility.selectedThings.Add(thing);
					}
				}
				if (InspectPaneUtility.selectedThings.Count == 1)
				{
					str = InspectPaneUtility.selectedThings[0].LabelCap;
				}
				else
				{
					IEnumerable<IGrouping<string, Thing>> source = from th in InspectPaneUtility.selectedThings
					group th by th.LabelCapNoCount into g
					select (g);
					if (source.Count() > 1)
					{
						str = "VariousLabel".Translate();
					}
					else
					{
						int num = 0;
						for (int i = 0; i < InspectPaneUtility.selectedThings.Count; i++)
						{
							num += InspectPaneUtility.selectedThings[i].stackCount;
						}
						str = InspectPaneUtility.selectedThings[0].LabelCapNoCount + " x" + num;
					}
				}
			}
			Text.Font = GameFont.Medium;
			return str.Truncate(rect.width, InspectPaneUtility.truncatedLabelsCached);
		}

		public static void ExtraOnGUI(IInspectPane pane)
		{
			if (pane.AnythingSelected)
			{
				if (KeyBindingDefOf.SelectNextInCell.KeyDownEvent)
				{
					pane.SelectNextInCell();
				}
				pane.DrawInspectGizmos();
				InspectPaneUtility.DoTabs(pane);
			}
		}

		public static void UpdateTabs(IInspectPane pane)
		{
			bool flag = false;
			foreach (InspectTabBase curTab in pane.CurTabs)
			{
				if (curTab.IsVisible && curTab.GetType() == pane.OpenTabType)
				{
					curTab.TabUpdate();
					flag = true;
				}
			}
			if (!flag)
			{
				pane.CloseOpenTab();
			}
		}

		public static void InspectPaneOnGUI(Rect inRect, IInspectPane pane)
		{
			pane.RecentHeight = 165f;
			if (pane.AnythingSelected)
			{
				try
				{
					Rect rect = inRect.ContractedBy(12f);
					rect.yMin -= 4f;
					rect.yMax += 6f;
					GUI.BeginGroup(rect);
					float num = 0f;
					if (pane.ShouldShowSelectNextInCellButton)
					{
						Rect rect2 = new Rect((float)(rect.width - 24.0), 0f, 24f, 24f);
						if (Widgets.ButtonImage(rect2, TexButton.SelectOverlappingNext))
						{
							pane.SelectNextInCell();
						}
						num = (float)(num + 24.0);
						TooltipHandler.TipRegion(rect2, "SelectNextInSquareTip".Translate(KeyBindingDefOf.SelectNextInCell.MainKeyLabel));
					}
					pane.DoInspectPaneButtons(rect, ref num);
					Rect rect3 = new Rect(0f, 0f, rect.width - num, 50f);
					string label = pane.GetLabel(rect3);
					rect3.width += 300f;
					Text.Font = GameFont.Medium;
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect3, label);
					if (pane.ShouldShowPaneContents)
					{
						Rect rect4 = rect.AtZero();
						rect4.yMin += 26f;
						pane.DoPaneContents(rect4);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception doing inspect pane: " + ex.ToString());
				}
				finally
				{
					GUI.EndGroup();
				}
			}
		}

		private static void DoTabs(IInspectPane pane)
		{
			try
			{
				float y = (float)(pane.PaneTopY - 30.0);
				float num = (float)(InspectPaneUtility.PaneWidthFor(pane) - 72.0);
				float width = 0f;
				bool flag = false;
				foreach (InspectTabBase curTab in pane.CurTabs)
				{
					if (curTab.IsVisible)
					{
						Rect rect = new Rect(num, y, 72f, 30f);
						width = num;
						Text.Font = GameFont.Small;
						if (Widgets.ButtonText(rect, curTab.labelKey.Translate(), true, false, true))
						{
							InspectPaneUtility.InterfaceToggleTab(curTab, pane);
						}
						bool flag2 = curTab.GetType() == pane.OpenTabType;
						if (!flag2 && !curTab.TutorHighlightTagClosed.NullOrEmpty())
						{
							UIHighlighter.HighlightOpportunity(rect, curTab.TutorHighlightTagClosed);
						}
						if (flag2)
						{
							curTab.DoTabGUI();
							pane.RecentHeight = 700f;
							flag = true;
						}
						num = (float)(num - 72.0);
					}
				}
				if (flag)
				{
					GUI.DrawTexture(new Rect(0f, y, width, 30f), InspectPaneUtility.InspectTabButtonFillTex);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(ex.ToString(), 742783);
			}
		}

		private static bool IsOpen(InspectTabBase tab, IInspectPane pane)
		{
			return tab.GetType() == pane.OpenTabType;
		}

		private static void ToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (InspectPaneUtility.IsOpen(tab, pane) || (tab == null && pane.OpenTabType == null))
			{
				pane.OpenTabType = null;
				SoundDefOf.TabClose.PlayOneShotOnCamera(null);
			}
			else
			{
				tab.OnOpen();
				pane.OpenTabType = tab.GetType();
				SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
			}
		}

		private static void InterfaceToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (TutorSystem.TutorialMode && !InspectPaneUtility.IsOpen(tab, pane) && !TutorSystem.AllowAction("ITab-" + tab.tutorTag + "-Open"))
				return;
			InspectPaneUtility.ToggleTab(tab, pane);
		}
	}
}
