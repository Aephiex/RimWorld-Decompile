using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class TileFinder
	{
		private static List<Pair<int, int>> tmpTiles = new List<Pair<int, int>>();

		private static List<int> tmpPlayerTiles = new List<int>();

		public static int RandomStartingTile()
		{
			return TileFinder.RandomFactionBaseTileFor(Faction.OfPlayer, true, null);
		}

		public static int RandomFactionBaseTileFor(Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
		{
			for (int i = 0; i < 500; i++)
			{
				int num = default(int);
				if ((from _ in Enumerable.Range(0, 100)
				select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight<int>((Func<int, float>)delegate(int x)
				{
					Tile tile = Find.WorldGrid[x];
					if (tile.biome.canBuildBase && tile.biome.implemented && tile.hilliness != Hilliness.Impassable)
					{
						if (mustBeAutoChoosable && !tile.biome.canAutoChoose)
						{
							return 0f;
						}
						if (extraValidator != null && !extraValidator(x))
						{
							return 0f;
						}
						return tile.biome.factionBaseSelectionWeight;
					}
					return 0f;
				}, out num) && TileFinder.IsValidTileForNewSettlement(num, null))
				{
					return num;
				}
			}
			Log.Error("Failed to find faction base tile for " + faction);
			return 0;
		}

		public static bool IsValidTileForNewSettlement(int tile, StringBuilder reason = null)
		{
			Tile tile2 = Find.WorldGrid[tile];
			if (!tile2.biome.canBuildBase)
			{
				if (reason != null)
				{
					reason.Append("CannotLandBiome".Translate(tile2.biome.label));
				}
				return false;
			}
			if (!tile2.biome.implemented)
			{
				if (reason != null)
				{
					reason.Append("BiomeNotImplemented".Translate() + ": " + tile2.biome.label);
				}
				return false;
			}
			if (tile2.hilliness == Hilliness.Impassable)
			{
				if (reason != null)
				{
					reason.Append("CannotLandImpassableMountains".Translate());
				}
				return false;
			}
			Settlement settlement = Find.WorldObjects.SettlementAt(tile);
			if (settlement != null)
			{
				if (reason != null)
				{
					if (settlement.Faction == null)
					{
						reason.Append("TileOccupied".Translate());
					}
					else if (settlement.Faction == Faction.OfPlayer)
					{
						reason.Append("YourBaseAlreadyThere".Translate());
					}
					else
					{
						reason.Append("BaseAlreadyThere".Translate(settlement.Faction.Name));
					}
				}
				return false;
			}
			if (Find.WorldObjects.AnySettlementAtOrAdjacent(tile))
			{
				if (reason != null)
				{
					reason.Append("FactionBaseAdjacent".Translate());
				}
				return false;
			}
			if (!Find.WorldObjects.AnyMapParentAt(tile) && Current.Game.FindMap(tile) == null && !Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedFactionBase, tile))
			{
				return true;
			}
			if (reason != null)
			{
				reason.Append("TileOccupied".Translate());
			}
			return false;
		}

		public static bool TryFindPassableTileWithTraversalDistance(int rootTile, int minDist, int maxDist, out int result, Predicate<int> validator = null, bool ignoreFirstTilePassability = false, bool preferCloserTiles = false)
		{
			TileFinder.tmpTiles.Clear();
			Find.WorldFloodFiller.FloodFill(rootTile, (int x) => !Find.World.Impassable(x) || (x == rootTile && ignoreFirstTilePassability), delegate(int tile, int traversalDistance)
			{
				if (traversalDistance > maxDist)
				{
					return true;
				}
				if (traversalDistance >= minDist && (validator == null || validator(tile)))
				{
					TileFinder.tmpTiles.Add(new Pair<int, int>(tile, traversalDistance));
				}
				return false;
			}, 2147483647, null);
			Pair<int, int> pair = default(Pair<int, int>);
			if (preferCloserTiles)
			{
				if (((IEnumerable<Pair<int, int>>)TileFinder.tmpTiles).TryRandomElementByWeight<Pair<int, int>>((Func<Pair<int, int>, float>)((Pair<int, int> x) => (float)(1.0 - (float)(x.Second - minDist) / (float)((float)(maxDist - minDist) + 0.0099999997764825821))), out pair))
				{
					result = pair.First;
					return true;
				}
				result = -1;
				return false;
			}
			if (((IEnumerable<Pair<int, int>>)TileFinder.tmpTiles).TryRandomElement<Pair<int, int>>(out pair))
			{
				result = pair.First;
				return true;
			}
			result = -1;
			return false;
		}

		public static bool TryFindRandomPlayerTile(out int tile, bool allowCaravans, Predicate<int> validator = null)
		{
			TileFinder.tmpPlayerTiles.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && maps[i].mapPawns.FreeColonistsSpawnedCount != 0 && (validator == null || validator(maps[i].Tile)))
				{
					TileFinder.tmpPlayerTiles.Add(maps[i].Tile);
				}
			}
			if (allowCaravans)
			{
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled && (validator == null || validator(caravans[j].Tile)))
					{
						TileFinder.tmpPlayerTiles.Add(caravans[j].Tile);
					}
				}
			}
			if (((IEnumerable<int>)TileFinder.tmpPlayerTiles).TryRandomElement<int>(out tile))
			{
				return true;
			}
			Map map = default(Map);
			if ((from x in Find.Maps
			where x.IsPlayerHome && (validator == null || validator(x.Tile))
			select x).TryRandomElement<Map>(out map))
			{
				tile = map.Tile;
				return true;
			}
			Map map2 = default(Map);
			if ((from x in Find.Maps
			where x.mapPawns.FreeColonistsSpawnedCount != 0 && (validator == null || validator(x.Tile))
			select x).TryRandomElement<Map>(out map2))
			{
				tile = map2.Tile;
				return true;
			}
			Caravan caravan = default(Caravan);
			if (!allowCaravans && (from x in Find.WorldObjects.Caravans
			where x.IsPlayerControlled && (validator == null || validator(x.Tile))
			select x).TryRandomElement<Caravan>(out caravan))
			{
				tile = caravan.Tile;
				return true;
			}
			tile = -1;
			return false;
		}

		public static bool TryFindNewSiteTile(out int tile, int minDist = 8, int maxDist = 30, bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
		{
			Func<int, int> findTile = delegate(int root)
			{
				int minDist2 = minDist;
				int maxDist2 = maxDist;
				Predicate<int> validator = (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && TileFinder.IsValidTileForNewSettlement(x, null);
				bool preferCloserTiles2 = preferCloserTiles;
				int result = default(int);
				if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator, false, preferCloserTiles2))
				{
					return result;
				}
				return -1;
			};
			int arg = default(int);
			if (nearThisTile != -1)
			{
				arg = nearThisTile;
			}
			else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, (Predicate<int>)((int x) => findTile(x) != -1)))
			{
				tile = -1;
				return false;
			}
			tile = findTile(arg);
			return tile != -1;
		}
	}
}
