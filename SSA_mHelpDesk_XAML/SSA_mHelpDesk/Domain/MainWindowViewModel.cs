using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Domain
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private DateTime _lastUpdated = DateTime.Now;
        public DateTime LastUpdated { get => _lastUpdated;
            set {
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
