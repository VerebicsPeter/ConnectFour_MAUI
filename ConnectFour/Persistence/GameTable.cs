namespace ConnectFour.Persistence
{
    public class GameTable
    {
        // Represents the game tabel given a valid game state (loaded from a file)

        public int X, Y;         // size
        public int XTime;        // Sum of X's turns
        public int OTime;        // Sum of O's turns
        public string currPlayer;// current player
        public string[] Table;   // board

        public GameTable(int x, int y, int x_t, int o_t, string curr)
        {
            X = x;
            Y = y;
            XTime = x_t;
            OTime = o_t;
            Table = new string[X];
            currPlayer = curr;
        }
    }
}
