using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnDef : Def
	{
		public Type workerClass = typeof(PawnColumnWorker);

		public bool sortable;

		public bool ignoreWhenCalculatingOptimalTableSize;

		public string headerIcon;

		public Vector2 headerIconSize;

		public string headerTip;

		public bool headerAlwaysInteractable;

		public TrainableDef trainable;

		public int gap;

		public WorkTypeDef workType;

		public bool moveWorkTypeLabelDown;

		public int widthPriority;

		[Unsaved]
		private PawnColumnWorker workerInt;

		[Unsaved]
		private Texture2D headerIconTex;

		public PawnColumnWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (PawnColumnWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public Texture2D HeaderIcon
		{
			get
			{
				if ((UnityEngine.Object)this.headerIconTex == (UnityEngine.Object)null && !this.headerIcon.NullOrEmpty())
				{
					this.headerIconTex = ContentFinder<Texture2D>.Get(this.headerIcon, true);
				}
				return this.headerIconTex;
			}
		}

		public Vector2 HeaderIconSize
		{
			get
			{
				if (this.headerIconSize != default(Vector2))
				{
					return this.headerIconSize;
				}
				Texture2D texture2D = this.HeaderIcon;
				if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
				{
					return new Vector2((float)texture2D.width, (float)texture2D.height);
				}
				return Vector2.zero;
			}
		}

		public bool HeaderInteractable
		{
			get
			{
				return this.sortable || !this.headerTip.NullOrEmpty() || this.headerAlwaysInteractable;
			}
		}
	}
}
