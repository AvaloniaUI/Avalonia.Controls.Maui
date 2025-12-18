using _2048Game.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048Game.Data
{
    public class GameRepository
    {
        private readonly IList<NumberTile> numbers;

        public GameRepository()
        {
            numbers = new List<NumberTile>();

            numbers.Add(new NumberTile { Index = 1, Number = string.Empty});
            numbers.Add(new NumberTile { Index = 2, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 3, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 4, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 5, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 6, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 7, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 8, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 9, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 10, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 11, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 12, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 13, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 14, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 15, Number = string.Empty });
            numbers.Add(new NumberTile { Index = 16, Number = string.Empty });
        }

        public Task<IList<NumberTile>> ListAsync() => Task.FromResult(numbers);
    }
}
