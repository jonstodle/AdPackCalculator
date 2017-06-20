using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class AdPack : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DateTimeOffset BuyDate { get => _buyDate; set => RaisePropertyChanged(ref _buyDate, value); }
        public int Tickets { get => _tickets; set => RaisePropertyChanged(ref _tickets, value); }

        public void TakeTicket() => Tickets -= 1;

        private void RaisePropertyChanged<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!newValue.Equals(backingField))
            {
                backingField = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private DateTimeOffset _buyDate;
        private int _tickets = 120;
    }
}
