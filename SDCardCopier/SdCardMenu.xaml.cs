using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
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
    /// Interaktionslogik für SdCardMenu.xaml
    /// </summary>
    public partial class SdCardMenu : Window
    {
        public SdCardMenu()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.IsEnabled = true;
        }
    }
}
