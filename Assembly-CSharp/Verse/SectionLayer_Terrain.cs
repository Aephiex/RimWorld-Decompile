using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal class SectionLayer_Terrain : SectionLayer
	{
		private static readonly Color32 ColorWhite = new Color32(255, 255, 255, 255);

		private static readonly Color32 ColorClear = new Color32(255, 255, 255, 0);

		public override bool Visible
		{
			get
			{
				return DebugViewSettings.drawTerrain;
			}
		}

		public SectionLayer_Terrain(Section section)
			: base(section)
		{
			base.relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public virtual Material GetMaterialFor(TerrainDef terrain)
		{
			return terrain.DrawMatSingle;
		}

		public bool AllowRenderingFor(TerrainDef terrain)
		{
			return DebugViewSettings.drawTerrainWater || !terrain.HasTag("Water");
		}

		public override void Regenerate()
		{
			base.ClearSubMeshes(MeshParts.All);
			TerrainGrid terrainGrid = base.Map.terrainGrid;
			CellRect cellRect = base.section.CellRect;
			TerrainDef[] array = new TerrainDef[8];
			HashSet<TerrainDef> hashSet = new HashSet<TerrainDef>();
			bool[] array2 = new bool[8];
			foreach (IntVec3 item in cellRect)
			{
				IntVec3 current = item;
				hashSet.Clear();
				TerrainDef terrainDef = terrainGrid.TerrainAt(current);
				LayerSubMesh subMesh = base.GetSubMesh(this.GetMaterialFor(terrainDef));
				if (subMesh != null && this.AllowRenderingFor(terrainDef))
				{
					int count = subMesh.verts.Count;
					subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
					subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
					subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
					subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
					subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
					subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
					subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
					subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 1);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count + 3);
				}
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = current + GenAdj.AdjacentCellsAroundBottom[i];
					if (!c.InBounds(base.Map))
					{
						array[i] = terrainDef;
					}
					else
					{
						TerrainDef terrainDef2 = terrainGrid.TerrainAt(c);
						Thing edifice = c.GetEdifice(base.Map);
						if (edifice != null && edifice.def.coversFloor)
						{
							terrainDef2 = TerrainDefOf.Underwall;
						}
						array[i] = terrainDef2;
						if (terrainDef2 != terrainDef && terrainDef2.edgeType != 0 && terrainDef2.renderPrecedence >= terrainDef.renderPrecedence && !hashSet.Contains(terrainDef2))
						{
							hashSet.Add(terrainDef2);
						}
					}
				}
				foreach (TerrainDef item2 in hashSet)
				{
					LayerSubMesh subMesh2 = base.GetSubMesh(this.GetMaterialFor(item2));
					if (subMesh2 != null && this.AllowRenderingFor(item2))
					{
						int count = subMesh2.verts.Count;
						subMesh2.verts.Add(new Vector3((float)((float)current.x + 0.5), 0f, (float)current.z));
						subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
						subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)((float)current.z + 0.5)));
						subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
						subMesh2.verts.Add(new Vector3((float)((float)current.x + 0.5), 0f, (float)(current.z + 1)));
						subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
						subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)((float)current.z + 0.5)));
						subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
						subMesh2.verts.Add(new Vector3((float)((float)current.x + 0.5), 0f, (float)((float)current.z + 0.5)));
						for (int j = 0; j < 8; j++)
						{
							array2[j] = false;
						}
						for (int k = 0; k < 8; k++)
						{
							if (k % 2 == 0)
							{
								if (array[k] == item2)
								{
									array2[(k - 1 + 8) % 8] = true;
									array2[k] = true;
									array2[(k + 1) % 8] = true;
								}
							}
							else if (array[k] == item2)
							{
								array2[k] = true;
							}
						}
						for (int l = 0; l < 8; l++)
						{
							if (array2[l])
							{
								subMesh2.colors.Add(SectionLayer_Terrain.ColorWhite);
							}
							else
							{
								subMesh2.colors.Add(SectionLayer_Terrain.ColorClear);
							}
						}
						subMesh2.colors.Add(SectionLayer_Terrain.ColorClear);
						for (int m = 0; m < 8; m++)
						{
							subMesh2.tris.Add(count + m);
							subMesh2.tris.Add(count + (m + 1) % 8);
							subMesh2.tris.Add(count + 8);
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All);
		}
	}
}
