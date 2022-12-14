using System;

namespace ConnectFour.Model
{
    public class TimerAdvancedEventArgs : EventArgs
    {
        public int TurnTime { get; private set; }
        public int XTime { get; private set; }
        public int OTime { get; private set; }

        public TimerAdvancedEventArgs(int turn, int x, int o)
        {
            TurnTime = turn; XTime = x; OTime = o;
        }
    }
}
