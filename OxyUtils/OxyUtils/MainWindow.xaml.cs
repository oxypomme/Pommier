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

            cbx_adb.IsChecked = Properties.Settings.Default.ADBonStart;
            cbx_shutdown.IsChecked = Properties.Settings.Default.Shutdown;
            tbx_time.Text = Properties.Settings.Default.ShutdownTime.TimeOfDay.ToString();

            if (Properties.Settings.Default.ADBonStart)
            {
                Commander.RegisterNewCommand("\"" + @"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe" + "\" reverse tcp:8500 tcp:8500");
                Commander.RunCommands();
            }

            if (Properties.Settings.Default.Shutdown)
                tbx_time.IsEnabled = true;

            cbx_adb.Checked += cbx_adb_Checked;
            cbx_adb.Unchecked += cbx_adb_Unchecked;
            cbx_shutdown.Checked += cbx_shutdown_Checked;
            cbx_shutdown.Unchecked += cbx_shutdown_Unchecked;
            tbx_time.TextChanged += tbx_time_TextChanged;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();

            if (File.Exists(startupPath))
                cbx_startup.IsChecked = true;

            lbl_version.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

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

            ReloadInterfaces();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay >= Properties.Settings.Default.ShutdownTime.TimeOfDay && Properties.Settings.Default.Shutdown)
            {
                Commander.RegisterNewCommand("shutdown -s -f -t 30");
                Commander.RunCommands();
            }
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
            notify.Visible = true;
            if (!isNotifShownup)
                notify.ShowBalloonTip(5000, "OxyUtils is now reduced !", "At least, it doesn't take many place...", Forms.ToolTipIcon.Info);
            isNotifShownup = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void NotifyMenu_ShowClick(object sender, EventArgs e)
        {
            notify.Visible = false;
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void NotifyMenu_QuitClick(object sender, EventArgs e)
        {
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

        private void cb_network_DropDownOpened(object sender, EventArgs e)
        {
            ReloadInterfaces();
        }

        private void cbx_adb_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ADBonStart = true;
            Properties.Settings.Default.Save();
        }

        private void cbx_adb_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ADBonStart = false;
            Properties.Settings.Default.Save();
        }

        private void cbx_shutdown_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Shutdown = true;
            Properties.Settings.Default.ShutdownTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(tbx_time.Text.Split(':')[0]), int.Parse(tbx_time.Text.Split(':')[1]), 0);
            Properties.Settings.Default.Save();
            tbx_time.IsEnabled = true;
        }

        private void cbx_shutdown_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Shutdown = false;
            Properties.Settings.Default.Save();
            tbx_time.IsEnabled = false;
        }

        private void tbx_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Shutdown = cbx_shutdown.IsEnabled;
            try
            {
                Properties.Settings.Default.ShutdownTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(tbx_time.Text.Split(':')[0]), int.Parse(tbx_time.Text.Split(':')[1]), 0);
            }
            catch (FormatException)
            {
                Console.WriteLine("Error when saving date time");
                return;
            }
            Properties.Settings.Default.Save();
        }

        private void ForceBindIP(string exe, bool is64bits)
        {
            ForceBindIP(exe, "", is64bits);
        }

        private void ForceBindIP(string exe, string arguments = "", bool is64bits = false)
        {
            // Si le programme n'est pas sur le C:, on change de disque
            if (exe[0] != 'C')
                Commander.RegisterNewCommand(exe[0] + ":");

            // On atteint le répertoire du logiciel à ouvrir
            Commander.RegisterNewCommand("cd \"" + Path.GetDirectoryName(exe) + "\"");

            var command = new StringBuilder();
            // On prépare ForceBindIP (64 si nécessaire)
            command.Append("\"" + @"C:\Program Files (x86)\ForceBindIP\ForceBindIP");
            if (is64bits)
                command.Append("64");
            command.Append(".exe\" ");

            // On récupère l'ip à utiliser
            try
            {
                command.Append((cb_network.SelectedItem as ComboBoxItem).Tag);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Select an IP to bind !", "OxyUtils", MessageBoxButton.OK, MessageBoxImage.Error);
                Commander.ClearCommands();
                return;
            }
            // On génère la commande
            command.AppendFormat(" \"{0}\"{1}", exe, (arguments != "" ? " " + arguments : ""));

            // Qu'on enregistre et qu'on exécute
            Commander.RegisterNewCommand(command.ToString());
            Commander.RunCommands();
        }

        private void btn_adb_Click(object sender, RoutedEventArgs e)
        {
            Commander.RegisterNewCommand("\"" + @"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe" + "\" reverse tcp:8500 tcp:8500");
            Commander.RunCommands();
        }

        private void btn_mklink_Click(object sender, RoutedEventArgs e)
        {
            var mklinkDialog = new MklinkDialog();
            mklinkDialog.Owner = Application.Current.MainWindow;
            mklinkDialog.ShowDialog();
        }

        private void btn_eso_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Zenimax Online\Launcher\Bethesda.net_Launcher.exe");
        }

        private void btn_steam_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Steam\steam.exe");
        }

        private void btn_origin_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Origin\Origin.exe");
        }

        private void btn_mega_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Local\MEGAsync\MEGAsync.exe");
        }

        private void btn_skype_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Program Files (x86)\Microsoft\Skype for Desktop\Skype.exe");
        }

        private void btn_teams_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Local\Microsoft\Teams\Update.exe", "--processStart \"Teams.exe\"", true);
        }

        private void btn_bethe_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Program Files (x86)\Bethesda.net Launcher\BethesdaNetUpdater.exe");
        }
    }
}