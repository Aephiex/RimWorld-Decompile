using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldDebugMatsSpectrum
	{
		private static readonly Material[] spectrumMats;

		public const int MaterialCount = 100;

		private const float Opacity = 0.25f;

		private static readonly Color[] DebugSpectrum;

		static WorldDebugMatsSpectrum()
		{
			WorldDebugMatsSpectrum.spectrumMats = new Material[100];
			WorldDebugMatsSpectrum.DebugSpectrum = DebugMatsSpectrum.DebugSpectrum;
			for (int i = 0; i < 100; i++)
			{
				WorldDebugMatsSpectrum.spectrumMats[i] = MatsFromSpectrum.Get(WorldDebugMatsSpectrum.DebugSpectrum, (float)((float)i / 100.0), ShaderDatabase.WorldOverlayTransparent);
				WorldDebugMatsSpectrum.spectrumMats[i].renderQueue = WorldMaterials.DebugTileRenderQueue;
			}
		}

		public static Material Mat(int ind)
		{
			ind = Mathf.Clamp(ind, 0, 99);
			return WorldDebugMatsSpectrum.spectrumMats[ind];
		}
	}
}
