using IWsh = IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Security.Principal;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using System.Windows.Threading;

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Forms.NotifyIcon notify = new Forms.NotifyIcon();

        private string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "OxyUtils.lnk");
        private bool isNotifShownup = false;

        public MainWindow()
        {
            InitializeComponent();

            App.settings = Properties.Settings.Default;

            cbx_adb.IsChecked = App.settings.ADBonStart;
            cbx_shutdown.IsChecked = App.settings.Shutdown;
            tbx_time.Text = App.settings.ShutdownTime.TimeOfDay.ToString();

            if (App.settings.ADBonStart)
                App.ReloadADB();

            if (App.settings.Shutdown)
                tbx_time.IsEnabled = true;

            cbx_adb.Checked += cbx_adb_Checked;
            cbx_adb.Unchecked += cbx_adb_Unchecked;
            cbx_shutdown.Checked += cbx_shutdown_Checked;
            cbx_shutdown.Unchecked += cbx_shutdown_Unchecked;
            tbx_time.TextChanged += tbx_time_TextChanged;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += timer_Tick;
            timer.Start();

            if (File.Exists(startupPath))
                cbx_startup.IsChecked = true;

            lbl_version.Content = Assembly.GetExecutingAssembly().GetName().Version;

            var cmNotify = new Forms.ContextMenu();
            {
                var item = new Forms.MenuItem();

                item.Text = "OxyUtils - v" + lbl_version.Content;
                item.Enabled = false;
                cmNotify.MenuItems.Add(item);

                item = new Forms.MenuItem();
                item.Text = "&Show";
                item.Click += NotifyMenu_ShowClick;
                cmNotify.MenuItems.Add(item);

                item = new Forms.MenuItem();
                item.Text = "&Quit";
                item.Click += NotifyMenu_QuitClick;
                cmNotify.MenuItems.Add(item);
            }

            notify.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notify.ContextMenu = cmNotify;
            notify.Click += NotifyMenu_ShowClick;
            notify.Visible = true;

            ReloadApps();

            ReloadInterfaces();
        }

        private void ReloadApps()
        {
            wp_fbi.Children.Clear();
            foreach (var app in App.Applications)
            {
                var btn = new Button()
                {
                    Content = app
                };
                btn.Click += App_Click;
                btn.MouseRightButtonDown += AppDelete_Click;
                wp_fbi.Children.Add(btn);
            }
        }

        private void AppDelete_Click(object sender, MouseButtonEventArgs e)
        {
            App.Applications.Remove((Applet)(((Button)sender).Content));
            JSONSerializer.SerializeJSON(App.appsPath, App.Applications);
            ReloadApps();
        }

        private void App_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ForceBindIP((Applet)(((Button)sender).Content), (cb_network.SelectedItem as ComboBoxItem).Tag.ToString());
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Select an IP to bind !", "OxyUtils", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay >= App.settings.ShutdownTime.TimeOfDay && App.settings.Shutdown)
                App.ShutdownPC();

            if (App.calendar.NextEvents.Items[0].Start.DateTime <= DateTime.Now)
                App.rpc.SetEventAsPresence(App.calendar.NextEvents.Items[0]);
            else
                App.rpc.SetEmptyPresence();
        }

        private void ReloadInterfaces()
        {
            cb_network.Items.Clear();
            IPAddress add;
            // Enumerate IP addresses
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())          // pour chaque les carte réseau
                foreach (var address in interf.GetIPProperties().UnicastAddresses)      // pour chaque adresse de la carte
                    if (IPAddress.TryParse(address.Address.ToString(), out add))        // qui est une IP
                        switch (add.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:         // si c'est une IPv4
                                if (add.ToString() != "127.0.0.1")
                                {
                                    var item = new ComboBoxItem();
                                    item.Tag = add.ToString();
                                    item.Content = interf.Name + ", " + add.ToString();
                                    cb_network.Items.Add(item);
                                }
                                break;

                            default:
                                break;
                        }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!isNotifShownup)
                notify.ShowBalloonTip(5000, "OxyUtils is now reduced !", "At least, it doesn't take many place...", Forms.ToolTipIcon.Info);
            isNotifShownup = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void NotifyMenu_ShowClick(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void NotifyMenu_QuitClick(object sender, EventArgs e)
        {
            App.rpc.Close();
            Environment.Exit(1);
        }

        private void cbx_startup_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.Value)
            {
                var shell = new IWsh.WshShell();
                var shortcut = (IWsh.IWshShortcut)shell.CreateShortcut(startupPath);

                shortcut.Description = "Shortcut for OxyUtils";
                shortcut.TargetPath = Path.Combine(Environment.CurrentDirectory, "OxyUtils.exe");
                shortcut.Save();
            }
            else
                File.Delete(startupPath);
        }

        private void cb_network_DropDownOpened(object sender, EventArgs e) => ReloadInterfaces();

        private void cbx_adb_Checked(object sender, RoutedEventArgs e)
        {
            App.settings.ADBonStart = true;
            App.settings.Save();
        }

        private void cbx_adb_Unchecked(object sender, RoutedEventArgs e)
        {
            App.settings.ADBonStart = false;
            App.settings.Save();
        }

        private void cbx_shutdown_Checked(object sender, RoutedEventArgs e)
        {
            App.settings.Shutdown = true;
            App.settings.ShutdownTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(tbx_time.Text.Split(':')[0]), int.Parse(tbx_time.Text.Split(':')[1]), 0);
            App.settings.Save();
            tbx_time.IsEnabled = true;
        }

        private void cbx_shutdown_Unchecked(object sender, RoutedEventArgs e)
        {
            App.settings.Shutdown = false;
            App.settings.Save();
            tbx_time.IsEnabled = false;
        }

        private void tbx_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.settings.Shutdown = cbx_shutdown.IsEnabled;
            try
            {
                App.settings.ShutdownTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(tbx_time.Text.Split(':')[0]), int.Parse(tbx_time.Text.Split(':')[1]), 0);
            }
            catch (FormatException)
            {
                Console.WriteLine("Error when saving date time");
                return;
            }
            App.settings.Save();
        }

        private void btn_adb_Click(object sender, RoutedEventArgs e) => App.ReloadADB();

        private void btn_mklink_Click(object sender, RoutedEventArgs e)
        {
            var mklinkDialog = new MklinkDialog();
            mklinkDialog.Owner = Application.Current.MainWindow;
            mklinkDialog.ShowDialog();
        }

        private void NewApp_Click(object sender, RoutedEventArgs e)
        {
            var newAppDialog = new NewAppDialog();
            newAppDialog.Owner = Application.Current.MainWindow;
            newAppDialog.ShowDialog();
            if (newAppDialog.createdApp != null)
            {
                App.Applications.Add(newAppDialog.createdApp);
                JSONSerializer.SerializeJSON(App.appsPath, App.Applications);
                ReloadApps();
            }
        }
    }
}