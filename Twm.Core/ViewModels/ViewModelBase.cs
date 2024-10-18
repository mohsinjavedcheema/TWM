using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Core.Classes;
using Newtonsoft.Json;

namespace Twm.Core.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        [JsonIgnore]
        public ViewModelBase ParentViewModel { get; set; }

        [JsonIgnore] public List<ViewModelBase> ChildViewModels { get; set; }


        [JsonIgnore]
        public Session Session
        {
            get { return Session.Instance; }
        }


        private bool _isBusy;

        [JsonIgnore]
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _message;

        [JsonIgnore]
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _subMessage;

        [JsonIgnore]
        public string SubMessage
        {
            get { return _subMessage; }
            set
            {
                if (_subMessage != value)
                {
                    _subMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        [JsonIgnore] public bool IsDeleted { get; set; }

        [JsonIgnore] public bool IsModified { get; set; }

        [JsonIgnore] public bool IsLoaded { get; set; }


/*

        [JsonIgnore]
        public virtual NavigationViewModel Navigation
        {
            get { return App.Navigation; }
        }*/

        [JsonIgnore]
        public Action CloseAction { get; set; }


        protected ViewModelBase()
        {
            IsModified = false;
            IsLoaded = false;
            IsDeleted = false;
            _subscribedLostFocus = false;
            ChildViewModels = new List<ViewModelBase>();
        }


        public virtual void Delete()
        {
            IsDeleted = true;
            foreach (var vm in ChildViewModels)
            {
                vm.Delete();
            }
        }

        public void UpdateProperty(string name)
        {
            OnPropertyChanged(name);
        }

        public event EventHandler LostFocus;

        public void OnLostFocus()
        {
            LostFocus?.Invoke(this, new EventArgs());
        }

        private bool _subscribedLostFocus;


        public void SubScribeLostFocus(bool enabled, EventHandler eventHandler)
        {
            if (!enabled)
                LostFocus -= eventHandler;
            else if (!_subscribedLostFocus)
                LostFocus += eventHandler;

            _subscribedLostFocus = enabled;
        }

        public virtual void Save()
        {
        }

        public virtual void FetchData()
        {
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public event EventHandler DataPropertyChanged;

        protected virtual void OnDataPropertyChanged([CallerMemberName] string propertyName = null)
        {
            IsModified = true;
            DataPropertyChanged?.Invoke(this, new EventArgs());
        }

    }
}