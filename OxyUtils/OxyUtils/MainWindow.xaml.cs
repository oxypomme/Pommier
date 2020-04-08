using IWshRuntimeLibrary;
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
using System.Windows.Shapes;

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Forms.NotifyIcon notify = new Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            lbl_version.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var cmNotify = new Forms.ContextMenu();
            cmNotify.MenuItems.Add("&Show");
            cmNotify.MenuItems[0].Click += NotifyMenu_ShowClick;
            cmNotify.MenuItems.Add("&Quit");
            cmNotify.MenuItems[1].Click += NotifyMenu_QuitClick;
            notify.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            notify.ContextMenu = cmNotify;
            notify.Click += NotifyMenu_ShowClick;

            IPAddress add;
            // Enumerate IP addresses
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())
                foreach (var address in interf.GetIPProperties().UnicastAddresses)
                    if (IPAddress.TryParse(address.Address.ToString(), out add))
                        switch (add.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:
                                var item = new ComboBoxItem();
                                item.Tag = add.ToString();
                                item.Content = interf.Name + ", " + add.ToString();
                                ipCB.Items.Add(item);
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
            var command = new StringBuilder();
            var path = exe.Split('\\');

            if (path[0][0] != 'C')
                Commander.RegisterNewCommand(path.First());
            Commander.RegisterNewCommand("cd \"" + string.Join("\\", path.Except(new[] { path.Last() })) + "\"");

            command.Append("ForceBindIP");
            if (is64bits)
                command.Append("64");
            command.Append(".exe ");

            try
            {
                command.Append((ipCB.SelectedItem as ComboBoxItem).Tag);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Select an IP to bind !", "OxyUtils", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            command.AppendFormat(" \"{exe}\"{1}", (arguments != "" ? " " + arguments : ""));

            Commander.RegisterNewCommand(command.ToString());
            //Commander.RunCommands();
        }

        private void adbBTN_Click(object sender, RoutedEventArgs e)
        {
            Commander.RegisterNewCommand("\"" + @"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe" + "\" reverse tcp:8500 tcp:8500");
            Commander.RunCommands();
        }

        private void esoBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Zenimax Online\Launcher\Bethesda.net_Launcher.exe");
        }

        private void uplayBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\Uplay.exe");
        }

        private void steamBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Steam\steam.exe");
        }

        private void originBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"D:\Program Files (x86)\Origin\Origin.exe");
        }

        private void zoomBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Roaming\Zoom\bin\Zoom.exe");
        }

        private void teamsBTN_Click(object sender, RoutedEventArgs e)
        {
            ForceBindIP(@"C:\Users\Tom SUBLET\AppData\Local\Microsoft\Teams\Update.exe", "--processStart \"Teams.exe\"", true);
        }

        private void startupCH_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.Value)
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "oxyutils.exe");
                shortcut.Description = "Shortcut for OxyUtils";
                shortcut.TargetPath = Environment.CurrentDirectory;
                shortcut.Save();
            }
        }
    }
}