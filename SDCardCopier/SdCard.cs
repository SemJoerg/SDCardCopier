using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SDCardCopier
{
    [Serializable]
    public class SdCard
    {
        [XmlIgnore]
        public DateTime LastTimeOfCopy { get; private set; }

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


        [XmlIgnore]
        public DirectoryInfo SdCardDirectory { get; private set; }

        [XmlElement("SdCardDirectory")]
        public string SdCardPathString
        {
            get { return SdCardDirectory.FullName; }
            set { SdCardDirectory = new DirectoryInfo(value); }
        }
        

        [XmlIgnore]
        public DirectoryInfo CopyDirectory { get; private set; }

        [XmlElement("CopyDirectory")]
        public string CopyDirectoryString
        {
            get { return CopyDirectory.FullName; }
            set { CopyDirectory = new DirectoryInfo(value); }
        }

        private SdCard()
        {

        }

        public SdCard(DateTime lastTimeOfCopy, string sdCardDirectory, string copyDirectory)
        {
            LastTimeOfCopy = lastTimeOfCopy;
            SdCardDirectory = new DirectoryInfo(sdCardDirectory);
            CopyDirectory = new DirectoryInfo(copyDirectory);
        }
    }
}
