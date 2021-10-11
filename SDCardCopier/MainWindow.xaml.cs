using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        
        public MainWindow()
        {
            InitializeComponent();
            sd_card_viewer.ItemsSource = SdCardManager.sdCards;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            (Application.Current as App).AppHidden = true;
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
            }
        }

        

        private void BtnSettingsClick(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
