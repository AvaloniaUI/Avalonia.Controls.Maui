using CommunityToolkit.Mvvm.ComponentModel;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace _2048Game.Models
{
    public partial class NumberTile : ObservableObject
    {
        public int Index { get; set; }

        [ObservableProperty]
        private bool isNewMatchGenerated;

        [ObservableProperty]
        private bool isNewNumberGenerated;

        [ObservableProperty]
        private bool isNumberMultiplied;

        [ObservableProperty]
        private string number;
    }
}