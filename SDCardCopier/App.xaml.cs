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
        
        public Forms.NotifyIcon notifyIcon = null;
 
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SdCardManager.Load();

            notifyIcon = new Forms.NotifyIcon();

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
                (Current.MainWindow as MainWindow).AppHidden = false;
            }
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

    }


}
