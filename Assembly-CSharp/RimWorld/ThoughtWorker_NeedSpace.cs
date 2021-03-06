using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedSpace : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.space == null)
			{
				return ThoughtState.Inactive;
			}
			Room room = p.GetRoom(RegionType.Set_Passable);
			if (room != null && !room.PsychologicallyOutdoors)
			{
				switch (p.needs.space.CurCategory)
				{
				case SpaceCategory.VeryCramped:
					return ThoughtState.ActiveAtStage(0);
				case SpaceCategory.Cramped:
					return ThoughtState.ActiveAtStage(1);
				case SpaceCategory.Normal:
					return ThoughtState.Inactive;
				case SpaceCategory.Spacious:
					return ThoughtState.ActiveAtStage(2);
				default:
					throw new InvalidOperationException("Unknown SpaceCategory");
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
