using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PlaceWorker_FuelingPort : PlaceWorker
	{
		private static readonly Material FuelingPortCellMaterial = MaterialPool.MatFrom("UI/Overlays/FuelingPort", ShaderDatabase.Transparent);

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			Map visibleMap = Find.VisibleMap;
			if (def.building != null && def.building.hasFuelingPort && FuelingPortUtility.GetFuelingPortCell(center, rot).Standable(visibleMap))
			{
				PlaceWorker_FuelingPort.DrawFuelingPortCell(center, rot);
			}
		}

		public static void DrawFuelingPortCell(IntVec3 center, Rot4 rot)
		{
			Vector3 position = FuelingPortUtility.GetFuelingPortCell(center, rot).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, PlaceWorker_FuelingPort.FuelingPortCellMaterial, 0);
		}
	}
}
