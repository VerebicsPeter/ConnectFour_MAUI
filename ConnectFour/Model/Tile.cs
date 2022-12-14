namespace ConnectFour.Model
{
    public enum TileValue
    {
        Emtpy, X, O
    }

    public class Tile
    {
        public TileValue Value { get; set; }

        public Tile()
        {
            Value = TileValue.Emtpy;
        }
        public Tile(TileValue value)
        {
            Value = value;
        }

        public static Tile MakeTile(char value)
        {
            if (value == 'x')
            {
                return new Tile(TileValue.X);
            }
            if (value == 'o')
            {
                return new Tile(TileValue.O);
            }
            return new Tile(TileValue.Emtpy);
        }

        public bool IsEmpty() { return Value == TileValue.Emtpy; }
        public bool IsX() { return Value == TileValue.X; }
        public bool ISO() { return Value == TileValue.O; }
    }
}