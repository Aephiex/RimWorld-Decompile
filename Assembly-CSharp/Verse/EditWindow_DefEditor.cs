using UnityEngine;

namespace Verse
{
	internal class EditWindow_DefEditor : EditWindow
	{
		public Def def;

		private float viewHeight;

		private Vector2 scrollPosition = default(Vector2);

		private float labelColumnWidth = 140f;

		private const float TopAreaHeight = 16f;

		private const float ExtraScrollHeight = 200f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(400f, 600f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public EditWindow_DefEditor(Def def)
		{
			this.def = def;
			base.optionalTitle = def.ToString();
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
			{
				UI.UnfocusCurrentControl();
			}
			Rect rect = new Rect(0f, 0f, inRect.width, 16f);
			this.labelColumnWidth = Widgets.HorizontalSlider(rect, this.labelColumnWidth, 0f, inRect.width, false, null, null, null, -1f);
			Rect outRect = inRect.AtZero();
			outRect.yMin += 16f;
			Rect rect2 = new Rect(0f, 0f, (float)(outRect.width - 16.0), this.viewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2, true);
			Listing_TreeDefs listing_TreeDefs = new Listing_TreeDefs(this.labelColumnWidth);
			listing_TreeDefs.Begin(rect2);
			TreeNode_Editor node = EditTreeNodeDatabase.RootOf(this.def);
			listing_TreeDefs.ContentLines(node, 0);
			listing_TreeDefs.End();
			if (Event.current.type == EventType.Layout)
			{
				this.viewHeight = (float)(listing_TreeDefs.CurHeight + 200.0);
			}
			Widgets.EndScrollView();
		}
	}
}
