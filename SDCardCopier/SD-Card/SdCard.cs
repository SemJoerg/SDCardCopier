using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

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
            set { SetValue(LastTimeOfCopyProperty, value); }
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
                if(!value.FullName.EndsWith("\\"))
                {
                    SetValue(SdCardDirectoryProperty, new DirectoryInfo(value.FullName + "\\"));
                }
                else
                {
                    SetValue(SdCardDirectoryProperty, value);
                }
                
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
            private set 
            {
                if (!value.FullName.EndsWith("\\"))
                {
                    SetValue(CopyDirectoryProperty, new DirectoryInfo(value.FullName + "\\"));
                }
                else
                {
                    SetValue(CopyDirectoryProperty, value);
                }
            }
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

        //Constructor for XML Serilisation
        private SdCard()
        {
            FileExtensions = new ObservableCollection<string>();
            FileExtensions.CollectionChanged += UpdatedFileExtensions;
            fileCopyWorker = new BackgroundWorker();
            fileCopyWorker.DoWork += FileCopyWorker_DoWork;
            fileCopyWorker.WorkerReportsProgress = true;
            fileCopyWorker.WorkerSupportsCancellation = true;
        }

        public SdCard(string name, string sdCardDirectory, string copyDirectory, List<string> _fileExtensions, DateTime lastTimeOfCopy) : this()
        {
            Name = name;
            SdCardDirectory = new DirectoryInfo(sdCardDirectory);
            CopyDirectory = new DirectoryInfo(copyDirectory);
            FileExtensions = new ObservableCollection<string>(_fileExtensions);
            FileExtensions.CollectionChanged += UpdatedFileExtensions;
            LastTimeOfCopy = lastTimeOfCopy;
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
            DriveInfo drive = new DriveInfo(SdCardDirectory.Root.FullName);
            if (drive.IsReady)
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
                MessageBox.Show("The Copy Directory does not exist!");
                return;
            }
            else if (fileCopyWorker.IsBusy)
            {
                MessageBox.Show("The SD-Card gets currently copied!");
                return;
            }

            CopyFilesDialog copyFilesDialog = new CopyFilesDialog(this);
            copyFilesDialog.Owner = Application.Current.MainWindow;
            copyFilesDialog.Show();
        }

        private void FileCopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<FileInfo> files = new List<FileInfo>();
            object[] args = e.Argument as object[];
            DirectoryInfo wSdDirecotry = new DirectoryInfo(args[0].ToString());
            DirectoryInfo wCopyDirecotry = new DirectoryInfo(args[1].ToString());
            DateTime wLastTimeOfCopy = Convert.ToDateTime(args[2]);

            bool allFileExtensionsAllowed = false;
            int oldFilesCounter = 0;
            int progress = 0;

            int warnigsCounter = 0;
            int succsesfulCopiedFilesCounter = 0;

            void AddLogEntry(object message, LogType logType = LogType.Default)
            {
                if(LogType.Warning == logType)
                {
                    warnigsCounter++;
                }

                fileCopyWorker.ReportProgress(progress, new object[] { message.ToString(), logType });
            }

            bool CheckForCancellation()
            {
                if (fileCopyWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return true;
                }
                return false;
            }

            void AddFile(FileInfo file)
            {
                if(file.CreationTime >= wLastTimeOfCopy)
                {
                    files.Add(file);
                    //AddLogEntry($"Found File: {file.FullName}\nCreationDate: {file.CreationTime.ToString("dd.MM.yyyy HH:mm:ss")}");
                }
                else
                {
                    oldFilesCounter++;
                    //AddLogEntry($"Found old File: {file.FullName}\nCreationDate: {file.CreationTime.ToString("dd.MM.yyyy HH:mm:ss")}");
                }
            }

            void GetAllFiles(DirectoryInfo directory)
            {
                if (CheckForCancellation())
                    return;

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    GetAllFiles(subDirectory);
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    if (CheckForCancellation())
                        return;

                    if (allFileExtensionsAllowed)
                    {
                        AddFile(file);
                    }
                    else
                    {
                        foreach (string fileExtension in FileExtensions)
                        {
                            if (Path.GetExtension(file.FullName) == fileExtension)
                            {
                                AddFile(file);
                                break;
                            }
                        }
                    }
                }
            }

            
            try
            {
                foreach (string fileExtension in FileExtensions)
                {
                    if (fileExtension == ".*")
                    {
                        allFileExtensionsAllowed = true;
                        break;
                    }
                }

                GetAllFiles(wSdDirecotry);
                fileCopyWorker.ReportProgress(progress, files.Count);
                AddLogEntry($"Found {oldFilesCounter} old files");
                AddLogEntry($"Found {files.Count} new files");


                for (progress = 1; progress <= files.Count; progress++)
                {
                    if (CheckForCancellation())
                        return;

                    FileInfo file = files[progress - 1];
                    string newFilePath = file.FullName.Substring(wSdDirecotry.FullName.Length);
                    newFilePath = newFilePath.Insert(0, wCopyDirecotry.FullName);
                    string newDirectory = newFilePath.Substring(0, newFilePath.Length - file.Name.Length);

                    if (!Directory.Exists(newDirectory))
                    {
                        Directory.CreateDirectory(newDirectory);
                    }
                    if (File.Exists(newFilePath))
                    {
                        AddLogEntry($"The File \"{newFilePath}\" does already exist", LogType.Warning);
                    }
                    else
                    {
                        file.CopyTo(newFilePath);
                        succsesfulCopiedFilesCounter++;
                        AddLogEntry($"Copied \"{file.FullName}\"\nto \"{newFilePath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                AddLogEntry(ex, LogType.Error);
            }

            AddLogEntry($"{warnigsCounter} Warnigs");
            AddLogEntry($"Copied {succsesfulCopiedFilesCounter} out of {files.Count} files sucsessfully");

        }
    }
}
