using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnWoundDrawer
	{
		private class Wound
		{
			private List<Vector2> locsPerSide = new List<Vector2>();

			private Material mat;

			private Quaternion quat;

			private static readonly Vector2 WoundSpan = new Vector2(0.18f, 0.3f);

			public Wound(Pawn pawn)
			{
				this.mat = pawn.RaceProps.FleshType.ChooseWoundOverlay();
				if ((Object)this.mat == (Object)null)
				{
					Log.ErrorOnce(string.Format("No wound graphics data available for flesh type {0}", pawn.RaceProps.FleshType), 76591733);
					this.mat = FleshTypeDefOf.Normal.ChooseWoundOverlay();
				}
				this.quat = Quaternion.AngleAxis((float)Rand.Range(0, 360), Vector3.up);
				for (int i = 0; i < 4; i++)
				{
					this.locsPerSide.Add(new Vector2(Rand.Value, Rand.Value));
				}
			}

			public void DrawWound(Vector3 drawLoc, Quaternion bodyQuat, Rot4 bodyRot, bool forPortrait)
			{
				Vector2 vector = this.locsPerSide[bodyRot.AsInt];
				Vector3 a = drawLoc;
				double num = vector.x - 0.5;
				Vector2 woundSpan = Wound.WoundSpan;
				double x = num * woundSpan.x;
				double num2 = vector.y - 0.5;
				Vector2 woundSpan2 = Wound.WoundSpan;
				drawLoc = a + new Vector3((float)x, 0f, (float)(num2 * woundSpan2.y));
				drawLoc.z -= 0.3f;
				GenDraw.DrawMeshNowOrLater(MeshPool.plane025, drawLoc, this.quat, this.mat, forPortrait);
			}
		}

		protected Pawn pawn;

		private List<Wound> wounds = new List<Wound>();

		private int MaxDisplayWounds = 3;

		public PawnWoundDrawer(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void RenderOverBody(Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, bool forPortrait)
		{
			int num = 0;
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.displayWound)
				{
					Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
					if (hediff_Injury == null || !hediff_Injury.IsOld())
					{
						num++;
					}
				}
			}
			int num2 = Mathf.CeilToInt((float)((float)num / 2.0));
			if (num2 > this.MaxDisplayWounds)
			{
				num2 = this.MaxDisplayWounds;
			}
			while (this.wounds.Count < num2)
			{
				this.wounds.Add(new Wound(this.pawn));
				PortraitsCache.SetDirty(this.pawn);
			}
			while (this.wounds.Count > num2)
			{
				this.wounds.Remove(this.wounds.RandomElement());
				PortraitsCache.SetDirty(this.pawn);
			}
			for (int j = 0; j < this.wounds.Count; j++)
			{
				this.wounds[j].DrawWound(drawLoc, quat, this.pawn.Rotation, forPortrait);
			}
		}
	}
}
