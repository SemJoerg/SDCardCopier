using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDCardCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ManagementEventWatcher usbWatcher;

        private bool appHidden = false;
        public bool AppHidden
        {
            get { return appHidden; }
            set
            {
                if (value)
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MainWindow)
                        {
                            Hide();
                        }
                        else
                        {
                            window.Close();
                        }
                    }
                }
                else if (appHidden != value)
                {
                    SdCardManager.SortSdCards();
                    Show();
                    WindowState = WindowState.Normal;
                    Activate();
                }
                appHidden = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            usbWatcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("Win32_VolumeChangeEvent");
            usbWatcher.Query = query;
            usbWatcher.EventArrived += UsbWatcher_EventArrived;
            usbWatcher.Start();

            SdCardManager.SortSdCards();
            sd_card_viewer.ItemsSource = SdCardManager.sdCards;
        }

        private void UsbWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(new Action(() =>
            {
                AppHidden = false;

                foreach (SdCard sdCard in SdCardManager.sdCards)
                {
                    sdCard.UpdateSdCardIsConnected();
                }
                SdCardManager.SortSdCards();
            })));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            AppHidden = true;
        }

        private void ShowSdCardWindow(SdCard sdCard = null)
        {
            this.IsEnabled = false;
            SdCardMenu menu;
            if(sdCard != null)
            {
                menu = new SdCardMenu(sdCard);
            }
            else
            {
                menu = new SdCardMenu();
            }

            menu.Owner = this;
            menu.Show();
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            ShowSdCardWindow();
        }

        private void ItemEditClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SdCard sdCard = button.DataContext as SdCard;

            if (sdCard.fileCopyWorker.IsBusy)
            {
                MessageBox.Show("The SD-Card gets currently copied");
                return;
            }

            
            ShowSdCardWindow(sdCard);
        }

        private void ItemDeleteClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SdCard sdCard = button.DataContext as SdCard;
            
            MessageBoxResult result = MessageBox.Show(this, $"Do you want to delete \"{sdCard.Name}\"", "Delete" , MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                SdCardManager.sdCards.Remove(sdCard);
                SdCardManager.Save();
            }
        }

        private void ItemCopySdCardClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SdCard sdCard = button.DataContext as SdCard;

            sdCard.CopyFiles();
        }
        
        private void BtnSettingsClick(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
