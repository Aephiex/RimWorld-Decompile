using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class DamagedMatPool
	{
		private static Dictionary<Material, Material> damagedMats = new Dictionary<Material, Material>();

		private static readonly Color DamagedMatStartingColor = Color.red;

		public static int MatCount
		{
			get
			{
				return DamagedMatPool.damagedMats.Count;
			}
		}

		public static Material GetDamageFlashMat(Material baseMat, float damPct)
		{
			if (damPct < 0.0099999997764825821)
			{
				return baseMat;
			}
			Material material = default(Material);
			if (!DamagedMatPool.damagedMats.TryGetValue(baseMat, out material))
			{
				material = new Material(baseMat);
				DamagedMatPool.damagedMats.Add(baseMat, material);
			}
			Color color2 = material.color = Color.Lerp(baseMat.color, DamagedMatPool.DamagedMatStartingColor, damPct);
			return material;
		}
	}
}
