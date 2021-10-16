using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Management;

namespace SDCardCopier
{
    static public class SdCardManager
    {
        static public string LoadPath = AppContext.BaseDirectory + "sdcards.xml";
        static public ObservableCollection<SdCard> sdCards = new ObservableCollection<SdCard>();
        
        static private XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SdCard>));
        
        static public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        static public bool Save()
        {
            try
            {
                using (StreamWriter stream = new StreamWriter(LoadPath))
                {
                    serializer.Serialize(stream, sdCards);
                    Debug.WriteLine($"\nSaved SdCards to {LoadPath}\n");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            return false;
        }

        static public bool Load()
        {
            if (!File.Exists(LoadPath))
                return false;

            try
            {
                using (StreamReader stream = new StreamReader(LoadPath))
                {
                    sdCards = serializer.Deserialize(stream) as ObservableCollection<SdCard>;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            return false;
        }

        static public void SortSdCards()
        {
            List<SdCard> sortedList = new List<SdCard>(sdCards);
            sortedList.Sort();

            for (int i = 0; i < sortedList.Count; i++)
            {
                sdCards.Move(sdCards.IndexOf(sortedList[i]), i);
            }

            SdCardManager.Save();
        }

    }
}
