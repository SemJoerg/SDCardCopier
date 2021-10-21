using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SDCardCopier
{
    /// <summary>
    /// Interaktionslogik für CopyFilesDialog.xaml
    /// </summary>
    public partial class CopyFilesDialog : Window
    {

        private SdCard sdCard;
        private ObservableCollection<LogEntry> logCollection;

        public CopyFilesDialog(SdCard _sdCard)
        {
            InitializeComponent();
            Loaded += CopyFilesDialog_Loaded;

            sdCard = _sdCard;
            sdCard.fileCopyWorker.ProgressChanged += FileCopyWorker_ProgressChanged;
            sdCard.fileCopyWorker.RunWorkerCompleted += FileCopyWorker_RunWorkerCompleted;

            logCollection = new ObservableCollection<LogEntry>();
            logViewer.ItemsSource = logCollection;
        }

        private void CopyFilesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            object[] directories = new object[] { sdCard.SdCardDirectoryString, sdCard.CopyDirectoryString, sdCard.LastTimeOfCopy };
            sdCard.fileCopyWorker.RunWorkerAsync(argument: directories);
        }

        private void FileCopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                TBStatus.Text = "cancelled".ToUpper();
            }
            else
            {
                TBStatus.Text = "successful".ToUpper();
                BtnUpdateLastTimeCopied.IsEnabled = true;
            }

            sdCard.fileCopyWorker.ProgressChanged -= FileCopyWorker_ProgressChanged;
            sdCard.fileCopyWorker.RunWorkerCompleted -= FileCopyWorker_RunWorkerCompleted;
        }

        private void FileCopyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
            {
                if (e.UserState is int)
                {
                    PBarCopyProgress.Maximum = Convert.ToInt32(e.UserState);
                }
                else if (e.UserState is object[])
                {
                    object[] args = e.UserState as object[];
                    if(args.Length == 2 && args[0] is string && args[1] is LogType)
                    {
                        logCollection.Add(new LogEntry((string)args[0], (LogType)args[1]));
                    }
                }
                else if(e.UserState is string)
                {
                    logCollection.Add(new LogEntry((string)e.UserState));
                }
            }
            SVLog.ScrollToBottom();
            PBarCopyProgress.Value = e.ProgressPercentage;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(sdCard.fileCopyWorker.IsBusy)
            {
                MessageBoxResult result = MessageBox.Show(this, $"Do you really want to close the Window", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            sdCard.fileCopyWorker.CancelAsync();
            Owner = null;
            logCollection.Clear();
            logCollection = null;
        }

        private void BtnCloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnUpdateLastTimeCopiedClick(object sender, RoutedEventArgs e)
        {
            sdCard.LastTimeOfCopy = DateTime.Now;
            SdCardManager.Save();
            Close();
        }
    }

    public enum LogType
    {
        Default,
        Warning,
        Error
    }

    public struct LogEntry
    {
        public readonly string Message { get;}
        public readonly LogType Type { get;}
        public readonly SolidColorBrush LogColor { get;}
        public readonly string TimeStamp { get; }

        public LogEntry(string message, LogType logType = LogType.Default)
        {
            Message = message;
            Type = logType;
            //TimeStamp = DateTime.Now.ToString("[HH:mm:ss]");
            TimeStamp = "[" + DateTime.Now.ToLongTimeString() + "]";

            if(logType == LogType.Warning)
            {
                LogColor = new SolidColorBrush(Colors.DarkOrange);
            }
            else if(logType == LogType.Error)
            {
                LogColor = new SolidColorBrush(Colors.Red);
            }
            else
            {
                LogColor = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
