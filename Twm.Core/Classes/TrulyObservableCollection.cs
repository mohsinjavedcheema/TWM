using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Twm.Core.Classes
{
    public sealed class TrulyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {
        private bool _itemsChanged;

        public bool ItemsChanged
        {
            get => _itemsChanged;
            set
            {
                _itemsChanged = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemsChanged)));
            }
        }

        public TrulyObservableCollection()
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
        }

        public TrulyObservableCollection(IEnumerable<T> pItems) : this()
        {
            foreach (var item in pItems)
            {
                this.Add(item);
            }
        }

        private void FullObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemsChanged)));
            ItemsChanged = true;
            //NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
            //OnCollectionChanged(args);
        }
    }
}
