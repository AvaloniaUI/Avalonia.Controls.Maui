using CommunityToolkit.Mvvm.ComponentModel;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace _2048Game.Models
{
    public partial class NumberTile : ObservableObject
    {
        // Unique identifier for this tile entity
        public Guid Id { get; set; } = Guid.NewGuid();

        // Grid position (0-3)
        [ObservableProperty]
        private int row;

        [ObservableProperty]
        private int column;

        [ObservableProperty]
        private bool isNewNumberGenerated;

        [ObservableProperty]
        private bool isNumberMultiplied;

        [ObservableProperty]
        private string number = string.Empty;

        // Value as integer for calculations
        public int Value => string.IsNullOrEmpty(Number) ? 0 : int.Parse(Number);
    }
}