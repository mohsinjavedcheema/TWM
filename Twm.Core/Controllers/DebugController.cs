using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using Twm.Core.Classes;

namespace Twm.Core.Controllers
{
    public class DebugController : INotifyPropertyChanged
    {
        public ObservableCollection<string> DebugList { get; set; }

        private readonly object _debugLock = new object();
        public OperationCommand ClearCommand { get; set; }

        private static DebugController _mInstance;

        public static DebugController Instance
        {
            get { return _mInstance ?? (_mInstance = new DebugController()); }
        }

        private readonly SynchronizationContext _uiContext;



        protected DebugController()
        {
            _uiContext = SynchronizationContext.Current;
            DebugList = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(DebugList, _debugLock);
            ClearCommand = new OperationCommand(Clear);
        }

        public void Init()
        {

        }

        private static void Clear(object param)
        {
            Instance._uiContext.Post(x =>
            {
                Instance.DebugList.Clear();
                Instance.OnPropertyChanged("DebugList");



            }, null);
        }


        public static void Clear()
        {
            Clear(null);
        }

        public static void Print(string message)
        {
            Instance._uiContext.Post(x =>
            {
                Instance.DebugList.Add(message);
                Instance.OnPropertyChanged("DebugList");
            }, null);
        }


        public static void Print(string[] messages)
        {
            foreach (var message in messages)
            {
                Print(message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}