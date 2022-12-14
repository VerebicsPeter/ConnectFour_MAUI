namespace ConnectFour_MAUI.ViewModel
{
    /// <summary>
    /// Játékmező típusa.
    /// </summary>
    public class ConnectFourField : ViewModelBase
    {
        private String _text = String.Empty;

        private bool _isWinning = false;

        /// <summary>
        /// Felirat lekérdezése, vagy beállítása.
        /// </summary>
        public String Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWinning
        {
            get { return _isWinning; }
            set
            {
                if (_isWinning != value)
                {
                    _isWinning = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Row { get; set; }

        public int Col { get; set; }

        public int Number { get; set; }

        public DelegateCommand? StepCommand { get; set; }

    }
}