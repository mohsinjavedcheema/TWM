using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Twm.Core.Annotations;
using Twm.Core.Classes;

namespace Twm.Core.Controllers
{
    public class LogController : INotifyPropertyChanged
    {
        public ObservableCollection<LogMessage> LogList { get; set; }

        private readonly object _logLock = new object();
        public OperationCommand ClearCommand { get; set; }

        private static LogController _mInstance;

        public static LogController Instance
        {
            get { return _mInstance ?? (_mInstance = new LogController()); }
        }

        private string _filePath;

        protected LogController()
        {
            LogList = new ObservableCollection<LogMessage>();
            BindingOperations.EnableCollectionSynchronization(LogList, _logLock);

            ClearCommand = new OperationCommand(Clear);
        }

        private void Clear(object param)
        {
            lock (_logLock)
            {
                LogList.Clear();
            }
        }


        public static void Clear()
        {
            Instance.Clear(null);
        }

        public static void Print(string message, bool saveToFile = true)
        {
            lock (Instance._logLock)
            {
                var logMessage = new LogMessage() {DateTime = DateTime.Now, Message = message};
                Instance.LogList.Insert(0, logMessage);
                if (saveToFile && SystemOptions.Instance.LogInFile)
                {
                    Instance.CheckFileLog();
                    File.AppendAllText(Instance._filePath, logMessage.ToString() + Environment.NewLine);
                }
            }
        }


        public static void Print(string[] messages)
        {
            var fileMessages = new List<string>();
            foreach (var message in messages)
            {
                Print(message, false);
                var logMessage = new LogMessage() {DateTime = DateTime.Now, Message = message};
                fileMessages.Add(logMessage.ToString());
            }

            if (SystemOptions.Instance.LogInFile)
            {
                Instance.CheckFileLog();
                File.AppendAllLines(Instance._filePath, fileMessages);
            }
        }

        public static void Init()
        {
            if (SystemOptions.Instance.LogInFile)
            {
                Instance.CheckFileLog();
            }
        }

        private void CheckFileLog()
        {
            if (!string.IsNullOrEmpty(_filePath))
                return;

            var pathToLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm\\Log";
            Directory.CreateDirectory(pathToLog);
            var fileName = "log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            _filePath = Path.Combine(pathToLog, fileName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}