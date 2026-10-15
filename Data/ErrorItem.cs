
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Data
{
    public class ErrorItem : INotifyPropertyChanged
    {
        public int Code { get; set; }
        public int Tier { get; set; }
        public string Message { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged();
            }
        }

        private bool _ignore;
        public bool Ignore
        {
            get => _ignore;
            set
            {
                if (_ignore == value) return;
                _ignore = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}