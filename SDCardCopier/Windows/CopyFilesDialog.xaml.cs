using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            string[] directories = new string[] { sdCard.SdCardDirectoryString, sdCard.CopyDirectoryString };
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
                    logCollection.Add(new LogEntry($"Found {Convert.ToInt32(e.UserState)} new files\n"));
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
        }

        private void BtnCloseWindow(object sender, RoutedEventArgs e)
        {
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
            TimeStamp = DateTime.Now.ToString("[HH:mm:ss]");

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
