using ConnectFour.Persistence;

namespace ConnectFour.Model
{
    public class StoredGameBrowserModel
    {
        private IStore store; // persistence

        public List<StoredGameModel> StoredGames { get; private set; }
        
        public event EventHandler? StoreChanged;

        public StoredGameBrowserModel(IStore store)
        {
            this.store = store;

            StoredGames = new();
        }

        public async Task UpdateAsync()
        {
            if (store == null) return;

            StoredGames.Clear();

            // betöltjük a mentéseket
            foreach (String name in await store.GetFilesAsync())
            {
                if (name == "SuspendedGame") continue;

                StoredGames.Add(new StoredGameModel
                {
                    Name = name,
                    Modified = await store.GetModifiedTimeAsync(name)
                });
            }

            // dátum szerint rendezzük az elemeket
            StoredGames = StoredGames.OrderByDescending(item => item.Modified).ToList();

            OnSavesChanged();
        }

        private void OnSavesChanged()
        {
            StoreChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
