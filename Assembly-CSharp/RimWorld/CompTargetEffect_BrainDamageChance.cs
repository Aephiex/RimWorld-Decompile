using Verse;

namespace RimWorld
{
	public class CompTargetEffect_BrainDamageChance : CompTargetEffect
	{
		private const float Chance = 0.3f;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.Dead && Rand.Value <= 0.30000001192092896)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain != null)
				{
					int num = Rand.RangeInclusive(1, 5);
					Pawn pawn2 = pawn;
					DamageDef flame = DamageDefOf.Flame;
					int amount = num;
					pawn2.TakeDamage(new DamageInfo(flame, amount, -1f, user, brain, base.parent.def, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			}
		}
	}
}
