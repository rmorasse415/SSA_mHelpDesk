using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Domain
{
    public class LoadingPageViewModel : INotifyPropertyChanged
    {
        private string _displayMessage;
        public string DisplayMessage
        {
            get => _displayMessage;
            set
            {
                _displayMessage = value;
                OnPropertyChanged("DisplayMessage");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
