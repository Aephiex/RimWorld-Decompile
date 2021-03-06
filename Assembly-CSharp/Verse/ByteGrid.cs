using System;

namespace Verse
{
	public sealed class ByteGrid : IExposable
	{
		private byte[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public byte this[IntVec3 c]
		{
			get
			{
				return this.grid[CellIndicesUtility.CellToIndex(c, this.mapSizeX)];
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, this.mapSizeX);
				this.grid[num] = value;
			}
		}

		public byte this[int index]
		{
			get
			{
				return this.grid[index];
			}
			set
			{
				this.grid[index] = value;
			}
		}

		public byte this[int x, int z]
		{
			get
			{
				return this.grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)];
			}
			set
			{
				this.grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)] = value;
			}
		}

		public int CellsCount
		{
			get
			{
				return this.grid.Length;
			}
		}

		public ByteGrid()
		{
		}

		public ByteGrid(Map map)
		{
			this.ClearAndResizeTo(map);
		}

		public bool MapSizeMatches(Map map)
		{
			int num = this.mapSizeX;
			IntVec3 size = map.Size;
			int result;
			if (num == size.x)
			{
				int num2 = this.mapSizeZ;
				IntVec3 size2 = map.Size;
				result = ((num2 == size2.z) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		public void ClearAndResizeTo(Map map)
		{
			if (this.MapSizeMatches(map) && this.grid != null)
			{
				this.Clear(0);
			}
			else
			{
				IntVec3 size = map.Size;
				this.mapSizeX = size.x;
				IntVec3 size2 = map.Size;
				this.mapSizeZ = size2.z;
				this.grid = new byte[this.mapSizeX * this.mapSizeZ];
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.mapSizeX, "mapSizeX", 0, false);
			Scribe_Values.Look<int>(ref this.mapSizeZ, "mapSizeZ", 0, false);
			DataExposeUtility.ByteArray(ref this.grid, "grid");
		}

		public void Clear(byte value = 0)
		{
			if (value == 0)
			{
				Array.Clear(this.grid, 0, this.grid.Length);
			}
			else
			{
				for (int i = 0; i < this.grid.Length; i++)
				{
					this.grid[i] = value;
				}
			}
		}

		public void DebugDraw()
		{
			for (int i = 0; i < this.grid.Length; i++)
			{
				byte b = this.grid[i];
				if (b > 0)
				{
					IntVec3 c = CellIndicesUtility.IndexToCell(i, this.mapSizeX);
					CellRenderer.RenderCell(c, (float)((float)(int)b / 255.0 * 0.5));
				}
			}
		}
	}
}
