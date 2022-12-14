using ConnectFour.Model;

namespace ConnectFour_MAUI.ViewModel
{
    public class GameSizeViewModel : ViewModelBase
    {
        private GameSize _size;

        public GameSize Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeText));
            }
        }

        public string SizeText => _size.ToString();
    }
}