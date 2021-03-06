using Verse;

namespace RimWorld
{
	public class Thought_SituationalSocial : Thought_Situational, ISocialThought
	{
		public Pawn otherPawn;

		public override bool VisibleInNeedsTab
		{
			get
			{
				return base.VisibleInNeedsTab && this.MoodOffset() != 0.0;
			}
		}

		public Pawn OtherPawn()
		{
			return this.otherPawn;
		}

		public virtual float OpinionOffset()
		{
			return base.CurStage.baseOpinionOffset;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_SituationalSocial thought_SituationalSocial = other as Thought_SituationalSocial;
			if (thought_SituationalSocial == null)
			{
				return false;
			}
			return base.GroupsWith(other) && this.otherPawn == thought_SituationalSocial.otherPawn;
		}

		protected override ThoughtState CurrentStateInternal()
		{
			return base.def.Worker.CurrentSocialState(base.pawn, this.otherPawn);
		}
	}
}
