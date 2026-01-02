namespace _2048Game.Models
{
    public class TileMovement
    {
        public required NumberTile Tile { get; set; }
        public int FromRow { get; set; }
        public int FromColumn { get; set; }
        public int ToRow { get; set; }
        public int ToColumn { get; set; }
        public bool WillMerge { get; set; }
        public NumberTile? MergeTarget { get; set; }
    }

    public class MoveResult
    {
        public List<TileMovement> Movements { get; set; } = new();
        public List<NumberTile> TilesToRemove { get; set; } = new();
        public int ScoreAdded { get; set; }
        public bool HasMoved => Movements.Count > 0;
    }
}
