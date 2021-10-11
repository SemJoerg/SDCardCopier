using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Management;
using Forms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SDCardCopier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        public readonly Forms.NotifyIcon notifyIcon = null;
        ManagementEventWatcher usbWatcher;

        private volatile bool appHidden = false;
        public bool AppHidden
        {
            get { return appHidden; }
            set
            {
                if(value)
                {
                    foreach (Window window in Current.Windows)
                    {
                        if(window is MainWindow)
                        {
                            (window as MainWindow).Hide();
                        }
                        else
                        {
                            window.Close();
                        }
                    }
                }
                else if(appHidden != value)
                {
                    Current.MainWindow.Show();
                    Current.MainWindow.WindowState = WindowState.Normal;
                    Current.MainWindow.Activate();
                }
                appHidden = value;
            }
        }

        public App()
        {
            notifyIcon = new Forms.NotifyIcon();
            usbWatcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("Win32_VolumeChangeEvent");
            usbWatcher.Query = query;
            usbWatcher.EventArrived += UsbWatcher_EventArrived;
            usbWatcher.Start();
        }

        private void OnSDCardsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SdCardManager.Save();
        }

        private void UsbWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            this.Dispatcher.Invoke(UsbChanged);
            
        }

        private void UsbChanged()
        {
            AppHidden = false;
            foreach(SdCard sdCard in SdCardManager.sdCards)
            {
                sdCard.UpdateSdCardIsConnected();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SdCardManager.Load();
            SdCardManager.sdCards.CollectionChanged += OnSDCardsChanged;

            notifyIcon.Icon = new System.Drawing.Icon("icons/sdcard.ico");
            notifyIcon.Click += NotifyIcon_OpenClick;
            notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Open", null, NotifyIcon_OpenClick);
            notifyIcon.ContextMenuStrip.Items.Add("Close", null, NotifyIcon_CloseClick);
            notifyIcon.Visible = true;

            
        }

        private void NotifyIcon_CloseClick(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void NotifyIcon_OpenClick(object sender, EventArgs e)
        {
            Forms.MouseEventArgs mouseArgs = e as Forms.MouseEventArgs;

            if (mouseArgs == null ||  mouseArgs.Button == Forms.MouseButtons.Left)
            {
                AppHidden = false;
            }
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

    }


}
