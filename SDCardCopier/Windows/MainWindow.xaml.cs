using System;
using System.ComponentModel;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace SDCardCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ManagementEventWatcher usbInsertedWatcher;
        ManagementEventWatcher usbChangedWatcher;

        private bool appHidden = false;
        public bool AppHidden
        {
            get { return appHidden; }
            set
            {
                if (value)
                {
                    for (int i = 0; i <Application.Current.Windows.Count; i++)
                    {
                        Window window = Application.Current.Windows[i];
                        if (window is MainWindow)
                        {
                            Hide();
                        }
                        else
                        {
                            window.Close();
                        }
                    }
                    SdCardManager.Save();
                    //SdCardManager.sdCards = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                else if (appHidden != value)
                {
                    //SdCardManager.Load();
                    //SdCardManager.SortSdCards();
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

            usbInsertedWatcher = new ManagementEventWatcher();
            WqlEventQuery insertedQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            usbInsertedWatcher.Query = insertedQuery;
            usbInsertedWatcher.EventArrived += UsbInsertedWatcher_EventArrived;
            
            usbChangedWatcher = new ManagementEventWatcher();
            WqlEventQuery changedQuery = new WqlEventQuery("Win32_VolumeChangeEvent");
            usbChangedWatcher.Query = changedQuery;
            usbChangedWatcher.EventArrived += UsbChangedWatcher_EventArrived;

            usbInsertedWatcher.Start();
            usbChangedWatcher.Start();

            SdCardManager.SortSdCards();
            sd_card_viewer.ItemsSource = SdCardManager.sdCards;
        }

        private void UpdateAndSortSdCards()
        {
            foreach (SdCard sdCard in SdCardManager.sdCards)
            {
                sdCard.UpdateSdCardIsConnected();
            }
            SdCardManager.SortSdCards();
        }

        private void UsbInsertedWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                AppHidden = false;
            }));
        }
        
        private void UsbChangedWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            
            Dispatcher.Invoke(new Action(() =>
            {
                UpdateAndSortSdCards();
            }));
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

            MessageBoxResult result = MessageBox.Show(this, $"Do you want to Copy new files from \"{sdCard.SdCardDirectory.FullName}\" " +
                $"to \"{sdCard.CopyDirectory.FullName}\"", $"Copy SD-Card: {sdCard.Name}", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                sdCard.CopyFiles();
            }
        }
        
        private void BtnSettingsClick(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
