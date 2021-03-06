using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainTabWindow_PawnTable : MainTabWindow
	{
		private PawnTable table;

		protected virtual float ExtraBottomSpace
		{
			get
			{
				return 53f;
			}
		}

		protected virtual float ExtraTopSpace
		{
			get
			{
				return 0f;
			}
		}

		protected abstract PawnTableDef PawnTableDef
		{
			get;
		}

		protected override float Margin
		{
			get
			{
				return 6f;
			}
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				if (this.table == null)
				{
					return Vector2.zero;
				}
				Vector2 size = this.table.Size;
				double x = size.x + this.Margin * 2.0;
				Vector2 size2 = this.table.Size;
				return new Vector2((float)x, (float)(size2.y + this.ExtraBottomSpace + this.ExtraTopSpace + this.Margin * 2.0));
			}
		}

		protected virtual IEnumerable<Pawn> Pawns
		{
			get
			{
				return from x in Find.VisibleMap.mapPawns.FreeColonists
				orderby x.Label
				select x;
			}
		}

		public override void PostOpen()
		{
			if (this.table == null)
			{
				this.table = this.CreateTable();
			}
			this.SetDirty();
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			this.table.PawnTableOnGUI(new Vector2(rect.x, rect.y + this.ExtraTopSpace));
		}

		public void Notify_PawnsChanged()
		{
			this.SetDirty();
		}

		public override void Notify_ResolutionChanged()
		{
			this.table = this.CreateTable();
			base.Notify_ResolutionChanged();
		}

		private PawnTable CreateTable()
		{
			return new PawnTable(this.PawnTableDef, (Func<IEnumerable<Pawn>>)this.get_Pawns, 998, UI.screenWidth - (int)(this.Margin * 2.0), 0, (int)((float)(UI.screenHeight - 35) - this.ExtraBottomSpace - this.ExtraTopSpace - this.Margin * 2.0));
		}

		protected void SetDirty()
		{
			this.table.SetDirty();
			this.SetInitialSizeAndPosition();
		}
	}
}
