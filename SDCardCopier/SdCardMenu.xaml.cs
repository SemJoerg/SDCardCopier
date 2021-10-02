using System;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Dialogs;

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
            CheckTextBoxes();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.IsEnabled = true;
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            SdCardManager.sdCards.Add(new SdCard(DateTime.Now, TbCopyFrom.Text, TbCopyTo.Text));
            Close();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnCopyFromClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog(TbCopyFrom, "Copy From");
        }

        private void BtnCopyToClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog(TbCopyTo, "Copy To");
        }

        private void OpenFolderDialog(TextBox textBox, string name = "")
        {
            IsEnabled = false;
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog(name);
            fileDialog.IsFolderPicker = true;
            if(Directory.Exists(textBox.Text))
            {
                fileDialog.InitialDirectory = textBox.Text;
            }
            else
            {
                fileDialog.InitialDirectory = AppContext.BaseDirectory;
            }
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBox.Text = fileDialog.FileName;
            }
            fileDialog.Dispose();
            IsEnabled = true;
        }

        private void TbFolderTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if(Directory.Exists(textBox.Text))
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                textBox.Foreground = new SolidColorBrush(Colors.Red);
            }
            CheckTextBoxes();
        }

        private void TbNameTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckTextBoxes();
        }

        private void CheckTextBoxes()
        {
            if(Directory.Exists(TbCopyFrom.Text) && Directory.Exists(TbCopyTo.Text) && !String.IsNullOrEmpty(TbName.Text))
            {
                BtnSave.IsEnabled = true;
            }
            else
            {
                BtnSave.IsEnabled = false;
            }
        }

    }
}
