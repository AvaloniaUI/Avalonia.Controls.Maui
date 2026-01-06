namespace _2048Game.Models
{
    /// <summary>
    /// Lightweight representation of a tile for movement calculations.
    /// Can be used by both game mode (with NumberTile) and attract mode (with Border elements).
    /// </summary>
    public readonly struct TileData
    {
        public Guid Id { get; init; }
        public int Row { get; init; }
        public int Column { get; init; }
        public int Value { get; init; }

        public TileData(Guid id, int row, int column, int value)
        {
            Id = id;
            Row = row;
            Column = column;
            Value = value;
        }

        public static TileData FromNumberTile(NumberTile tile) =>
            new(tile.Id, tile.Row, tile.Column, tile.Value);
    }

    public class TileMovement
    {
        // For game mode compatibility
        public NumberTile? Tile { get; set; }
        public NumberTile? MergeTarget { get; set; }

        // Generic tile data for shared logic
        public TileData TileData { get; set; }
        public TileData? MergeTargetData { get; set; }

        public int FromRow { get; set; }
        public int FromColumn { get; set; }
        public int ToRow { get; set; }
        public int ToColumn { get; set; }
        public bool WillMerge { get; set; }
    }

    public class MoveResult
    {
        public List<TileMovement> Movements { get; set; } = new();
        public List<TileData> TilesToRemove { get; set; } = new();
        public int ScoreAdded { get; set; }
        public bool HasMoved => Movements.Count > 0;
    }
}
