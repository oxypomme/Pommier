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
        private ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
        private Process process;

        public MainWindow()
        {
            InitializeComponent();

            IPAddress add;
            // Enumerate IP addresses
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())
                foreach (var address in interf.GetIPProperties().UnicastAddresses)
                    if (IPAddress.TryParse(address.Address.ToString(), out add))
                        switch (add.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:
                                ipCB.Items.Add((interf.Name, add.ToString()));
                                break;

                            default:
                                break;
                        }
        }

        private void registerNewCommand(string command, string parameter = "")
        {
            if (info.Arguments == "")
                info.Arguments += "/k " + command + " ";
            else
                info.Arguments += "&& " + command + " ";
            if (parameter != "")
                info.Arguments += parameter + " ";
        }

        private void clearCommands()
        {
            info.Arguments = "";
            Console.WriteLine(info.Arguments);
        }

        private void adbBTN_Click(object sender, RoutedEventArgs e)
        {
            registerNewCommand("\"" + @"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe" + "\"", "reverse tcp:8500 tcp:8500");
            process = Process.Start(info);
            System.Threading.Thread.Sleep(1000);
            process.Kill();
            clearCommands();
        }

        private void esoBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registerNewCommand("D:");
                registerNewCommand("cd", "\"" + @"D:\Program Files (x86)\Zenimax Online\Launcher\" + "\"");
                registerNewCommand("ForceBindIP.exe", (ipCB.SelectedItem as (string, string)?).Value.Item2 + " " + "\"" + @"D:\Program Files (x86)\Zenimax Online\Launcher\Bethesda.net_Launcher.exe" + "\"");
                process = Process.Start(info);
                System.Threading.Thread.Sleep(1000);
                process.Kill();
                clearCommands();
            }
            catch (InvalidOperationException)
            {
                clearCommands();
            }
        }

        private void zoomBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registerNewCommand("cd", "\"" + @"C:\Users\Tom SUBLET\AppData\Roaming\Zoom\bin\" + "\"");
                registerNewCommand("ForceBindIP.exe", (ipCB.SelectedItem as (string, string)?).Value.Item2 + " " + "\"" + @"C:\Users\Tom SUBLET\AppData\Roaming\Zoom\bin\Zoom.exe" + "\"");
                process = Process.Start(info);
                System.Threading.Thread.Sleep(1000);
                process.Kill();
                clearCommands();
            }
            catch (InvalidOperationException)
            {
                clearCommands();
            }
        }

        private void steamBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registerNewCommand("D:");
                registerNewCommand("cd", "\"" + @"D:\Program Files (x86)\Steam\" + "\"");
                registerNewCommand("ForceBindIP.exe", (ipCB.SelectedItem as (string, string)?).Value.Item2 + " " + "\"" + @"D:\Program Files (x86)\Steam\steam.exe" + "\"");
                process = Process.Start(info);
                System.Threading.Thread.Sleep(1000);
                process.Kill();
                clearCommands();
            }
            catch (InvalidOperationException)
            {
                clearCommands();
            }
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