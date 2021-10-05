using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            SdCardManager.Load();
            SdCardManager.sdCards.CollectionChanged += OnSDCardsChanged;
            sd_card_viewer.ItemsSource = SdCardManager.sdCards;
        }

        private void OnSDCardsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SdCardManager.Save();
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

        private void ItemEditClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SdCard sdCard = button.DataContext as SdCard;
            ShowSdCardWindow(sdCard);
            SdCardManager.sdCards.
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            ShowSdCardWindow();
        }

        private void BtnSettingsClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
