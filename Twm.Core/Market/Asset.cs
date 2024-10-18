using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;
using Twm.Core.Enums;
using Twm.Model.Model;

namespace Twm.Core.Market
{
    public class Asset : INotifyPropertyChanged
    {

        private string _assetName;
        public string AssetName
        {
            get { return _assetName; }

            set
            {
                if (_assetName != value)
                {
                    _assetName = value;
                    OnPropertyChanged();
                }
            }
        }




        private double _balance;
        public double Balance
        {
            get { return _balance; }

            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged();
                }
            }
        }

        public Asset()
        {
           
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}