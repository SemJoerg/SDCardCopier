using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SDCardCopier
{
    [Serializable]
    public class SdCard : DependencyObject
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
                UpdateSdCardIsConnected();
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

        private List<string> fileExtensions;
        public List<string> FileExtensions
        {
            get
            {
                return fileExtensions;
            }
            set
            {
                fileExtensions = value;
                string output = "";
                foreach (string fileExtension in value)
                {
                    output += fileExtension + "  ";
                }
                FileExtensionsString = output;
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

        public SdCard()
        {
            FileExtensions = new List<string>();
        }

        public SdCard(string name, string sdCardDirectory, string copyDirectory, List<string> _fileExtensions)
        {
            LastTimeOfCopy = DateTime.Now;
            Name = name;
            SdCardDirectory = new DirectoryInfo(sdCardDirectory);
            CopyDirectory = new DirectoryInfo(copyDirectory);
            FileExtensions = _fileExtensions;
            UpdateSdCardIsConnected();
        }

        public void UpdateSdCardIsConnected()
        {
            DriveInfo drive = new DriveInfo(SdCardDirectory.FullName[0].ToString());
            if(drive.IsReady)
            {
                SdCardIsConnected = true;
                MessageBox.Show($"Drive: {drive.Name}\nIs Ready: {drive.IsReady}\nDriveFormat: {drive.DriveFormat}\nDriveType: {drive.DriveType}");
            }
            else
            {
                SdCardIsConnected = false;
            }
            
        }

        public void CopyFiles()
        {

            LastTimeOfCopy = DateTime.Now;
        }

    }
}
