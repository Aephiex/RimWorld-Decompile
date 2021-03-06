using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AdvancedGameConfig : Window
	{
		private int selTile = -1;

		private const float ColumnWidth = 200f;

		private static readonly int[] MapSizes = new int[8]
		{
			200,
			225,
			250,
			275,
			300,
			325,
			350,
			400
		};

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(700f, 500f);
			}
		}

		public Dialog_AdvancedGameConfig(int selTile)
		{
			base.doCloseButton = true;
			base.closeOnEscapeKey = true;
			base.forcePause = true;
			base.absorbInputAroundWindow = true;
			this.selTile = selTile;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = 200f;
			listing_Standard.Begin(inRect.AtZero());
			listing_Standard.Label("MapSize".Translate(), -1f);
			int[] mapSizes = Dialog_AdvancedGameConfig.MapSizes;
			foreach (int num in mapSizes)
			{
				switch (num)
				{
				case 200:
					listing_Standard.Label("MapSizeSmall".Translate(), -1f);
					break;
				case 250:
					listing_Standard.Label("MapSizeMedium".Translate(), -1f);
					break;
				case 300:
					listing_Standard.Label("MapSizeLarge".Translate(), -1f);
					break;
				case 350:
					listing_Standard.Label("MapSizeExtreme".Translate(), -1f);
					break;
				}
				string label = "MapSizeDesc".Translate(num, num * num);
				if (listing_Standard.RadioButton(label, Find.GameInitData.mapSize == num, 0f))
				{
					Find.GameInitData.mapSize = num;
				}
			}
			listing_Standard.NewColumn();
			GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
			listing_Standard.Label("MapStartSeason".Translate(), -1f);
			string label2 = (Find.GameInitData.startingSeason != 0) ? Find.GameInitData.startingSeason.LabelCap() : "MapStartSeasonDefault".Translate();
			Rect rect = listing_Standard.GetRect(32f);
			GridLayout gridLayout = new GridLayout(rect, 5, 1, 0f, 4f);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(0, 1, 1), "-", true, false, true))
			{
				Season startingSeason = Find.GameInitData.startingSeason;
				startingSeason = ((startingSeason != 0) ? (startingSeason - 1) : Season.Winter);
				Find.GameInitData.startingSeason = startingSeason;
			}
			Widgets.Label(gridLayout.GetCellRectByIndex(1, 3, 1), label2);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(4, 1, 1), "+", true, false, true))
			{
				Season startingSeason2 = Find.GameInitData.startingSeason;
				startingSeason2 = ((startingSeason2 != Season.Winter) ? (startingSeason2 + 1) : Season.Undefined);
				Find.GameInitData.startingSeason = startingSeason2;
			}
			GenUI.ResetLabelAlign();
			if (this.selTile >= 0 && Find.GameInitData.startingSeason != 0)
			{
				Vector2 vector = Find.WorldGrid.LongLatOf(this.selTile);
				float y = vector.y;
				if (GenTemperature.AverageTemperatureAtTileForTwelfth(this.selTile, Find.GameInitData.startingSeason.GetFirstTwelfth(y)) < 3.0)
				{
					listing_Standard.Label("MapTemperatureDangerWarning".Translate(), -1f);
				}
			}
			if (Find.GameInitData.mapSize > 250)
			{
				listing_Standard.Label("MapSizePerformanceWarning".Translate(), -1f);
			}
			listing_Standard.End();
		}
	}
}
