using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Caravan_Items : WITab
	{
		private Vector2 scrollPosition;

		private float scrollViewHeight;

		private List<Thing> items = new List<Thing>();

		private const float MassCarriedLineHeight = 22f;

		public WITab_Caravan_Items()
		{
			base.labelKey = "TabCaravanItems";
		}

		protected override void FillTab()
		{
			float num = 0f;
			this.DrawMassUsage(ref num);
			Rect position = new Rect(0f, num, base.size.x, base.size.y - num);
			GUI.BeginGroup(position);
			this.UpdateItemsList();
			Pawn pawn = null;
			CaravanPeopleAndItemsTabUtility.DoRows(position.size, this.items, base.SelCaravan, ref this.scrollPosition, ref this.scrollViewHeight, true, ref pawn, true);
			this.items.Clear();
			GUI.EndGroup();
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();
			this.UpdateItemsList();
			base.size = CaravanPeopleAndItemsTabUtility.GetSize(this.items, this.PaneTopY, true);
			this.items.Clear();
		}

		private void DrawMassUsage(ref float curY)
		{
			curY += 10f;
			Rect rect = new Rect(10f, curY, (float)(base.size.x - 10.0), 100f);
			float massUsage = base.SelCaravan.MassUsage;
			float massCapacity = base.SelCaravan.MassCapacity;
			if (massUsage > massCapacity)
			{
				GUI.color = Color.red;
			}
			Text.Font = GameFont.Small;
			Widgets.Label(rect, "MassCarried".Translate(massUsage.ToString("0.##"), massCapacity.ToString("0.##")));
			GUI.color = Color.white;
			curY += 22f;
		}

		private void UpdateItemsList()
		{
			this.items.Clear();
			this.items.AddRange(CaravanInventoryUtility.AllInventoryItems(base.SelCaravan));
		}
	}
}
