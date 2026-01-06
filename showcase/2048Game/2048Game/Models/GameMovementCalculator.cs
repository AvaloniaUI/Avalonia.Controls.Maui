using _2048Game.Enums;

namespace _2048Game.Models
{
    /// <summary>
    /// Shared movement calculation logic for the 2048 game.
    /// Used by both the main game mode and attract mode.
    /// </summary>
    public static class GameMovementCalculator
    {
        /// <summary>
        /// Calculates the result of moving tiles in the specified direction.
        /// </summary>
        /// <param name="tiles">The current tiles on the board</param>
        /// <param name="direction">The direction to move</param>
        /// <returns>A MoveResult containing all movements, merges, and score changes</returns>
        public static MoveResult CalculateMove(IEnumerable<TileData> tiles, Direction direction)
        {
            var result = new MoveResult();
            var tileList = tiles.ToList();

            // Sort tiles based on direction to process them in correct order
            var sortedTiles = GetSortedTiles(tileList, direction);

            // Track which cells will be occupied and by which tile
            var targetGrid = new TileData?[4, 4];

            // Track which tiles have already merged (can only merge once per move)
            var mergedTileIds = new HashSet<Guid>();

            foreach (var tile in sortedTiles)
            {
                int originalRow = tile.Row;
                int originalCol = tile.Column;

                // Find the farthest empty position in the direction
                var (targetRow, targetCol) = GetFarthestPosition(
                    originalRow, originalCol, direction, targetGrid);

                // Check for merge possibility at the next position
                var nextPos = GetNextPosition(targetRow, targetCol, direction);

                if (IsValidPosition(nextPos.row, nextPos.col))
                {
                    var adjacentTile = targetGrid[nextPos.row, nextPos.col];
                    if (adjacentTile.HasValue &&
                        adjacentTile.Value.Value == tile.Value &&
                        !mergedTileIds.Contains(adjacentTile.Value.Id))
                    {
                        // Merge!
                        result.Movements.Add(new TileMovement
                        {
                            TileData = tile,
                            FromRow = originalRow,
                            FromColumn = originalCol,
                            ToRow = nextPos.row,
                            ToColumn = nextPos.col,
                            WillMerge = true,
                            MergeTargetData = adjacentTile.Value
                        });

                        result.TilesToRemove.Add(tile);
                        result.ScoreAdded += tile.Value * 2;
                        mergedTileIds.Add(adjacentTile.Value.Id);

                        // Update the target tile's value in the grid (for display purposes, the actual
                        // tile object update happens in the caller)
                        targetGrid[nextPos.row, nextPos.col] = new TileData(
                            adjacentTile.Value.Id,
                            nextPos.row,
                            nextPos.col,
                            adjacentTile.Value.Value * 2);

                        continue;
                    }
                }

                // No merge - record movement if position changed
                if (targetRow != originalRow || targetCol != originalCol)
                {
                    result.Movements.Add(new TileMovement
                    {
                        TileData = tile,
                        FromRow = originalRow,
                        FromColumn = originalCol,
                        ToRow = targetRow,
                        ToColumn = targetCol,
                        WillMerge = false
                    });
                }

                // Mark target cell as occupied
                targetGrid[targetRow, targetCol] = new TileData(
                    tile.Id, targetRow, targetCol, tile.Value);
            }

            return result;
        }

        /// <summary>
        /// Sorts tiles based on the movement direction so they are processed in the correct order.
        /// </summary>
        private static List<TileData> GetSortedTiles(List<TileData> tiles, Direction direction)
        {
            return direction switch
            {
                Direction.Left => tiles.OrderBy(t => t.Column).ToList(),
                Direction.Right => tiles.OrderByDescending(t => t.Column).ToList(),
                Direction.Up => tiles.OrderBy(t => t.Row).ToList(),
                Direction.Down => tiles.OrderByDescending(t => t.Row).ToList(),
                _ => tiles
            };
        }

        /// <summary>
        /// Finds the farthest empty position a tile can move to in the given direction.
        /// </summary>
        private static (int row, int col) GetFarthestPosition(
            int row, int col, Direction direction, TileData?[,] grid)
        {
            int dr = direction == Direction.Up ? -1 : (direction == Direction.Down ? 1 : 0);
            int dc = direction == Direction.Left ? -1 : (direction == Direction.Right ? 1 : 0);

            int currentRow = row;
            int currentCol = col;

            while (true)
            {
                int nextRow = currentRow + dr;
                int nextCol = currentCol + dc;

                if (!IsValidPosition(nextRow, nextCol) || grid[nextRow, nextCol].HasValue)
                    break;

                currentRow = nextRow;
                currentCol = nextCol;
            }

            return (currentRow, currentCol);
        }

        /// <summary>
        /// Gets the next position in the given direction.
        /// </summary>
        private static (int row, int col) GetNextPosition(int row, int col, Direction direction)
        {
            return direction switch
            {
                Direction.Up => (row - 1, col),
                Direction.Down => (row + 1, col),
                Direction.Left => (row, col - 1),
                Direction.Right => (row, col + 1),
                _ => (row, col)
            };
        }

        /// <summary>
        /// Checks if a position is within the valid grid bounds (0-3).
        /// </summary>
        private static bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 4 && col >= 0 && col < 4;
        }
    }
}
