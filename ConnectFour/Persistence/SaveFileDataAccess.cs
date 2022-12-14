using System;
using System.IO;
using System.Threading.Tasks;

namespace ConnectFour.Persistence
{
    public class SaveFileDataAcess : IGameDataAccess
    {
        private string? _basePath = String.Empty;

        public SaveFileDataAcess(string? basePath)
        {
            _basePath = basePath;
        }

        public async Task<GameTable> LoadAsync(string path)
        {
            if (!String.IsNullOrEmpty(_basePath)) path = Path.Combine(_basePath, path);

            try
            {
                using StreamReader reader = new(path); // using ...

                string line = await reader.ReadLineAsync() ?? String.Empty;

                string[] parts = line.Split(';');

                int x, y, t1, t2;
                string currentPlayer;
                x  = Int32.Parse(parts[0]);
                y  = Int32.Parse(parts[1]);
                t1 = Int32.Parse(parts[2]);
                t2 = Int32.Parse(parts[3]);
                currentPlayer = parts[4];

                GameTable table = new(x, y, t1, t2, currentPlayer);

                for (int i = 0; i < x; i++)
                {
                    line = await reader.ReadLineAsync() ?? String.Empty;

                    table.Table[i] = line;
                }

                return table;
            }
            catch
            {
                throw new SaveFileDataAccessException("Loading failed.");
            }
        }

        public async Task SaveAsync(string path, GameTable game)
        {
            if (!String.IsNullOrEmpty(_basePath)) path = Path.Combine(_basePath, path);

            try
            {
                using StreamWriter writer = new(path); // using ...

                writer.Write($"{game.X};{game.Y};{game.XTime};{game.OTime};{game.currPlayer}");
                writer.WriteLine();
                for (int i = 0; i < game.X; i++)
                {
                    await writer.WriteLineAsync(game.Table[i]);
                }
            }
            catch
            {
                throw new SaveFileDataAccessException("Saving failed.");
            }
        }
    }
}