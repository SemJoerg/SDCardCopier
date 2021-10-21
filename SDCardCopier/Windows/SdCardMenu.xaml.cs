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
using System.Collections.ObjectModel;

namespace SDCardCopier
{
    /// <summary>
    /// Interaktionslogik für SdCardMenu.xaml
    /// </summary>
    public partial class SdCardMenu : Window
    {
        private SdCard sdCard;
        private bool createNewSdCard;
        private bool fileExtinsionsCorrect = false;
        List<string> fileExtensions = new List<string>();
        private bool dateTimeCorrect;
        private DateTime newLastTimeOfCopy;

        public SdCardMenu()
        {
            InitializeComponent();
            createNewSdCard = true;
            TbFileExtension.Text = ".*";
            TbLastTimeOfCopy.Text = DateTime.MinValue.ToString();
            CheckTextBoxes();
        }

        public SdCardMenu(SdCard _sdCard)
        {
            InitializeComponent();
            createNewSdCard = false;
            sdCard = _sdCard;
            TbName.Text = sdCard.Name;
            TbSdCardDirectory.Text = sdCard.SdCardDirectoryString;
            TbCopyDirectory.Text = sdCard.CopyDirectoryString;
            string tbFileExtensionOutput = "";
            for(int i = 0; i < sdCard.FileExtensions.Count; i++)
            {
                tbFileExtensionOutput += sdCard.FileExtensions[i];

                if(i+1 < sdCard.FileExtensions.Count)
                {
                    tbFileExtensionOutput += "; ";
                }
            }
            TbFileExtension.Text = tbFileExtensionOutput;
            TbLastTimeOfCopy.Text = sdCard.LastTimeOfCopy.ToString();
            CheckTextBoxes();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.IsEnabled = true;
            Owner = null;
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            
            if(createNewSdCard)
            {
                sdCard = new SdCard(TbName.Text, TbSdCardDirectory.Text, TbCopyDirectory.Text, fileExtensions, newLastTimeOfCopy);
                SdCardManager.sdCards.Add(sdCard);
                SdCardManager.Save();
                Close();
            }
            else
            {
                sdCard.Name = TbName.Text;
                sdCard.SdCardDirectoryString = TbSdCardDirectory.Text;
                sdCard.CopyDirectoryString = TbCopyDirectory.Text;
                sdCard.FileExtensions = new ObservableCollection<string>(fileExtensions);
                sdCard.LastTimeOfCopy = newLastTimeOfCopy;
                SdCardManager.Save();
                Close();
            }
            
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnCopyFromClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog(TbSdCardDirectory, "Copy From");
        }

        private void BtnCopyToClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog(TbCopyDirectory, "Copy To");
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
            if(Directory.Exists(TbSdCardDirectory.Text) && Directory.Exists(TbCopyDirectory.Text) && !String.IsNullOrEmpty(TbName.Text) && fileExtinsionsCorrect && dateTimeCorrect)
            {
                BtnSave.IsEnabled = true;
            }
            else
            {
                BtnSave.IsEnabled = false;
            }
        }

        private void TbFileExtensionTextChanged(object sender, TextChangedEventArgs e = null)
        {           
            TextBox textBox = sender as TextBox;
            string fileExtension = "";
            bool fileExtensionHasDot = false;
            bool foundSemiColon = false;
            fileExtensions.Clear();

            bool CheckFileExtension()
            {
                if (!fileExtensionHasDot)
                {
                    textBox.Foreground = new SolidColorBrush(Colors.Red);
                    fileExtinsionsCorrect = false;
                    CheckTextBoxes();
                    return false;
                }

                fileExtensions.Add(fileExtension);
                fileExtension = "";

                return true;
            }

            foreach (char letter in textBox.Text)
            {
                if (letter == ' ')
                    continue;
                
                if(letter == ';')
                {
                    foundSemiColon = true;
                    if (!CheckFileExtension())
                        return;
                    fileExtensionHasDot = false;
                }
                else if(letter == '.')
                {
                    if(!fileExtensionHasDot && fileExtension.Length > 0)
                    {
                        textBox.Foreground = new SolidColorBrush(Colors.Red);
                        fileExtinsionsCorrect = false;
                        CheckTextBoxes();
                        return;
                    }
                    fileExtension += letter;
                    fileExtensionHasDot = true;
                }
                else
                {
                    fileExtension += letter;
                }
            }
            if(CheckFileExtension())
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
                fileExtinsionsCorrect = true;
                CheckTextBoxes();
            }
        }

        private void TbLastTimeOfCopyTextChanged(object sender, TextChangedEventArgs e = null)
        {
            TextBox textBox = sender as TextBox;


            if (DateTime.TryParse(textBox.Text, out newLastTimeOfCopy))
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
                dateTimeCorrect = true;
            }
            else
            {
                textBox.Foreground = new SolidColorBrush(Colors.Red);
                dateTimeCorrect = false;
            }
            CheckTextBoxes();
        }
    }
}
