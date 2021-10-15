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

        public CopyFilesDialog(SdCard _sdCard)
        {
            InitializeComponent();
            sdCard = _sdCard;
            sdCard.fileCopyWorker.ProgressChanged += FileCopyWorker_ProgressChanged;
            sdCard.fileCopyWorker.RunWorkerCompleted += FileCopyWorker_RunWorkerCompleted;
            string[] directories = new string[] { sdCard.SdCardDirectoryString, sdCard.CopyDirectoryString };
            sdCard.fileCopyWorker.RunWorkerAsync(argument: directories);
        }

        private void FileCopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                MessageBox.Show("Cancelled");
            }
            else
            {
                MessageBox.Show("Completed!!!");
            }

            sdCard.fileCopyWorker.ProgressChanged -= FileCopyWorker_ProgressChanged;
            sdCard.fileCopyWorker.RunWorkerCompleted -= FileCopyWorker_RunWorkerCompleted;
        }

        private void FileCopyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
            {
                if (e.UserState.GetType() == typeof(int))
                {
                    PBarCopyProgress.Maximum = Convert.ToInt32(e.UserState);
                    TBLog.Text += $"Found {Convert.ToInt32(e.UserState)} files\n";
                }
                else if (e.UserState.GetType() == typeof(string))
                {
                    TBLog.Text += (e.UserState as string) + "\n";
                }
                return;
            }
            
            TBLog.Text += $"Progress: {e.ProgressPercentage}\n";
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
}
