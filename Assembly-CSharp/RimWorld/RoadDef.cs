using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoadDef : Def
	{
		public class WorldRenderStep
		{
			public RoadWorldLayerDef layer;

			public float width;
		}

		public int priority;

		public bool ancientOnly;

		public float movementCostMultiplier = 1f;

		public int tilesPerSegment = 15;

		public RoadPathingDef pathingMode;

		public List<RoadDefGenStep> roadGenSteps;

		public List<WorldRenderStep> worldRenderSteps;

		public string worldTransitionGroup = string.Empty;

		public float distortionFrequency = 1f;

		public float distortionIntensity;

		[Unsaved]
		private float[] cachedLayerWidth;

		public float GetLayerWidth(RoadWorldLayerDef def)
		{
			if (this.cachedLayerWidth == null)
			{
				this.cachedLayerWidth = new float[DefDatabase<RoadWorldLayerDef>.DefCount];
				for (int i = 0; i < DefDatabase<RoadWorldLayerDef>.DefCount; i++)
				{
					RoadWorldLayerDef roadWorldLayerDef = DefDatabase<RoadWorldLayerDef>.AllDefsListForReading[i];
					if (this.worldRenderSteps != null)
					{
						foreach (WorldRenderStep worldRenderStep in this.worldRenderSteps)
						{
							if (worldRenderStep.layer == roadWorldLayerDef)
							{
								this.cachedLayerWidth[roadWorldLayerDef.index] = worldRenderStep.width;
							}
						}
					}
				}
			}
			return this.cachedLayerWidth[def.index];
		}

		public override void ClearCachedData()
		{
			base.ClearCachedData();
			this.cachedLayerWidth = null;
		}
	}
}
