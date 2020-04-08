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

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Forms.NotifyIcon notify = new Forms.NotifyIcon();

        private string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "OxyUtils.lnk");

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(startupPath))
                cbx_startup.IsChecked = true;

            lbl_version.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var cmNotify = new Forms.ContextMenu();
            {
                var item = new Forms.MenuItem();

                item.Text = "&Show";
                item.Click += NotifyMenu_ShowClick;
                cmNotify.MenuItems.Add(item);

                item = new Forms.MenuItem();
                item.Text = "&Quit";
                item.Click += NotifyMenu_QuitClick;
                cmNotify.MenuItems.Add(item);
            }

            notify.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            notify.ContextMenu = cmNotify;
            notify.Click += NotifyMenu_ShowClick;

            IPAddress add;
            // Enumerate IP addresses
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())          // pour chaque les carte réseau
                foreach (var address in interf.GetIPProperties().UnicastAddresses)      // pour chaque adresse de la carte
                    if (IPAddress.TryParse(address.Address.ToString(), out add))        // qui est une IP
                        switch (add.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:         // si c'est une IPv4
                                var item = new ComboBoxItem();
                                item.Tag = add.ToString();
                                item.Content = interf.Name + ", " + add.ToString();
                                cb_network.Items.Add(item);
                                break;

                            default:
                                break;
                        }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notify.Visible = true;
            notify.ShowBalloonTip(5000, "OxyUtils is now reduced !", "At least, it doesn't take many place...", Forms.ToolTipIcon.Info);
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

        private void btn_eso_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Zenimax Online\Launcher\Bethesda.net_Launcher.exe");
        }

        private void btn_uplay_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\Uplay.exe");
        }

        private void btn_steam_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Steam\steam.exe");
        }

        private void btn_origin_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Origin\Origin.exe");
        }

        private void btn_zoom_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Roaming\Zoom\bin\Zoom.exe");
        }

        private void btn_teams_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Local\Microsoft\Teams\Update.exe", "--processStart \"Teams.exe\"", true);
        }

        private void cbx_startup_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.Value)
            {
                var shell = new IWsh.WshShell();
                var shortcut = (IWsh.IWshShortcut)shell.CreateShortcut(startupPath);

                shortcut.Description = "Shortcut for OxyUtils";
                shortcut.TargetPath = Path.Combine(Environment.CurrentDirectory, "OxyUtils.exe");
                //shortcut.IconLocation = Path.Combine(Environment.CurrentDirectory, "icon.ico");
                shortcut.Save();
            }
            else
                File.Delete(startupPath);
        }
    }
}