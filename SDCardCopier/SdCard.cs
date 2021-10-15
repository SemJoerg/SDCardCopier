using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace SDCardCopier
{
    [Serializable]
    public class SdCard : DependencyObject, IComparable
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(SdCard));

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty LastTimeOfCopyProperty = DependencyProperty.Register("LastTimeOfCopy", typeof(DateTime), typeof(SdCard));

        [XmlIgnore]
        public DateTime LastTimeOfCopy 
        {   
            get { return (DateTime)GetValue(LastTimeOfCopyProperty); }
            private set { SetValue(LastTimeOfCopyProperty, value); }
        }

        static private string timeFormat = "dd.MM.yyyy HH:mm:ss";
        [XmlElement("lastTimeOfCopy")]
        public string lastTimeOfCopyString
        {
            get { return LastTimeOfCopy.ToString(timeFormat); }
            set 
            {
                try
                {
                    LastTimeOfCopy = DateTime.ParseExact(value, timeFormat, null);
                }
                catch (Exception ex)
                {
                    SdCardManager.ShowError(ex.Message);
                }
            }
        }


        public static readonly DependencyProperty SdCardDirectoryProperty = DependencyProperty.Register("SdCardDirectory", typeof(DirectoryInfo), typeof(SdCard));

        [XmlIgnore]
        public DirectoryInfo SdCardDirectory
        {
            get { return (DirectoryInfo)GetValue(SdCardDirectoryProperty); }
            private set
            { 
                SetValue(SdCardDirectoryProperty, value);
                UpdateSdCardIsConnected();
            }
        }

        [XmlElement("SdCardDirectory")]
        public string SdCardDirectoryString
        {
            get { return SdCardDirectory.FullName; }
            set 
            { 
                SdCardDirectory = new DirectoryInfo(value); 
            }
        }


        public static readonly DependencyProperty CopyDirectoryProperty = DependencyProperty.Register("CopyDirectory", typeof(DirectoryInfo), typeof(SdCard));

        [XmlIgnore]
        public DirectoryInfo CopyDirectory
        {
            get { return (DirectoryInfo)GetValue(CopyDirectoryProperty); }
            private set { SetValue(CopyDirectoryProperty, value); }
        }

        [XmlElement("CopyDirectory")]
        public string CopyDirectoryString
        {
            get { return CopyDirectory.FullName; }
            set { CopyDirectory = new DirectoryInfo(value); }
        }

        private ObservableCollection<string> fileExtensions;
        public ObservableCollection<string> FileExtensions
        {
            get
            {
                return fileExtensions;
            }
            set
            {
                fileExtensions = value;
                UpdatedFileExtensions(null, null);
            }
        }

        public static readonly DependencyProperty FileExtensionsStringProperty = DependencyProperty.Register("FileExtensionsString", typeof(string), typeof(SdCard));

        [XmlIgnore]
        public string FileExtensionsString
        {
            get { return (string)GetValue(FileExtensionsStringProperty); }
            set { SetValue(FileExtensionsStringProperty, value); }
        }

        public static readonly DependencyProperty SdCardIsConnectedProperty = DependencyProperty.Register("SdCardIsConnected", typeof(bool), typeof(SdCard));

        [XmlIgnore]
        public bool SdCardIsConnected 
        {
            get { return (bool)GetValue(SdCardIsConnectedProperty); }
            private set { SetValue(SdCardIsConnectedProperty, value); }
        }

        [XmlIgnore]
        public readonly BackgroundWorker fileCopyWorker;

        private SdCard()
        {
            FileExtensions = new ObservableCollection<string>();
            fileCopyWorker = new BackgroundWorker();
            fileCopyWorker.DoWork += FileCopyWorker_DoWork;
            fileCopyWorker.WorkerReportsProgress = true;
            fileCopyWorker.WorkerSupportsCancellation = true;
            FileExtensions.CollectionChanged += UpdatedFileExtensions;
        }

        public SdCard(string name, string sdCardDirectory, string copyDirectory, List<string> _fileExtensions)
        {
            LastTimeOfCopy = DateTime.Now;
            Name = name;
            SdCardDirectory = new DirectoryInfo(sdCardDirectory);
            CopyDirectory = new DirectoryInfo(copyDirectory);
            FileExtensions = new ObservableCollection<string>(_fileExtensions);
            FileExtensions.CollectionChanged += UpdatedFileExtensions;
            UpdatedFileExtensions(null, null);
            UpdateSdCardIsConnected();
        }

        public void UpdatedFileExtensions(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string output = "";
            foreach (string fileExtension in FileExtensions)
            {
                output += fileExtension + "  ";
            }
            FileExtensionsString = output;
        }
        
        public void UpdateSdCardIsConnected()
        {
            DriveInfo drive = new DriveInfo(SdCardDirectory.FullName[0].ToString());
            if(drive.IsReady)
            {
                SdCardIsConnected = true;
                //MessageBox.Show($"Drive: {drive.Name}\nIs Ready: {drive.IsReady}\nDriveFormat: {drive.DriveFormat}\nDriveType: {drive.DriveType}");
            }
            else
            {
                SdCardIsConnected = false;
            }
            
        }

        // Used to Sort the List of SdCards properly
        public int CompareTo(object obj)
        {
            SdCard CompareSdCard = obj as SdCard;

            if(CompareSdCard.SdCardIsConnected == false && SdCardIsConnected == true)
            {
                return -1;
            }
            if(CompareSdCard.SdCardIsConnected == true && SdCardIsConnected == false)
            {
                return 1;
            }

            return 0;
        }

        public void CopyFiles()
        {

            if (!SdCardDirectory.Exists)
            {
                MessageBox.Show("The SD-Card Directory does not exist!");
                return;
            }
            else if (!CopyDirectory.Exists)
            {
                MessageBox.Show("The Copy-To Directory does not exist!");
                return;
            }
            else if (fileCopyWorker.IsBusy)
            {
                MessageBox.Show("The SD-Card gets currently copied");
                return;
            }

            CopyFilesDialog copyFilesDialog = new CopyFilesDialog(this);
            copyFilesDialog.Owner = Application.Current.MainWindow;
            copyFilesDialog.Show();

        }

        private void FileCopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<FileInfo> files = new List<FileInfo>();
            string[] directories = e.Argument as string[];
            DirectoryInfo wSdDirecotry = new DirectoryInfo(directories[0]);
            DirectoryInfo wCopyDirecotry = new DirectoryInfo(directories[1]);
            bool allFileExtensionsAllowed = false;
            foreach (string fileExtension in FileExtensions)
            {
                if (fileExtension == ".*")
                {
                    allFileExtensionsAllowed = true;
                    break;
                }
            }

            void GetAllFiles(DirectoryInfo directory)
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    GetAllFiles(subDirectory);
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    if(allFileExtensionsAllowed)
                    {
                        files.Add(file);
                        fileCopyWorker.ReportProgress(0, $"Found File: {file.FullName}");
                    }
                    else
                    {
                        foreach (string fileExtension in FileExtensions)
                        {
                            if (Path.GetExtension(file.FullName) == fileExtension)
                            {
                                files.Add(file);
                                fileCopyWorker.ReportProgress(0, $"Found File: {file.FullName}");
                                break;
                            }
                        }
                    }
                }
            }

            fileCopyWorker.ReportProgress(0, $"SD-Directory: {wSdDirecotry.FullName}\n");
            GetAllFiles(wSdDirecotry);
            fileCopyWorker.ReportProgress(0, "\n");
            fileCopyWorker.ReportProgress(0, files.Count);

            for (int i = 0; i < files.Count; i++)
            {
                fileCopyWorker.ReportProgress(i + 1);
                if (fileCopyWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
