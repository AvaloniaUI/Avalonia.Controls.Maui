namespace _2048Game.Models
{
    /// <summary>
    /// Represents a snapshot of the game state for undo functionality.
    /// </summary>
    public class GameState
    {
        public required int[][] Board { get; set; }
        public required List<NumberTile> Tiles { get; set; }
        public int Score { get; set; }
        public int TotalMoves { get; set; }
    }
}
