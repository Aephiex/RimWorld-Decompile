using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Apparel : ThingWithComps
	{
		private bool wornByCorpseInt;

		public Pawn Wearer
		{
			get
			{
				Pawn_ApparelTracker pawn_ApparelTracker = base.ParentHolder as Pawn_ApparelTracker;
				return (pawn_ApparelTracker == null) ? null : pawn_ApparelTracker.pawn;
			}
		}

		public bool WornByCorpse
		{
			get
			{
				return this.wornByCorpseInt;
			}
		}

		public void Notify_PawnKilled()
		{
			if (base.def.apparel.careIfWornByCorpse)
			{
				this.wornByCorpseInt = true;
			}
		}

		public void Notify_PawnResurrected()
		{
			this.wornByCorpseInt = false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wornByCorpseInt, "wornByCorpse", false, false);
		}

		public virtual void DrawWornExtras()
		{
		}

		public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			return false;
		}

		public virtual bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ)
		{
			return true;
		}

		public virtual IEnumerable<Gizmo> GetWornGizmos()
		{
			yield break;
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (this.WornByCorpse)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text += "WasWornByCorpse".Translate();
			}
			return text;
		}

		public virtual float GetSpecialApparelScoreOffset()
		{
			return 0f;
		}
	}
}
