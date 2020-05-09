using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour MklinkDialog.xaml
    /// </summary>
    public partial class MklinkDialog : Window
    {
        public MklinkDialog()
        {
            InitializeComponent();
        }

        private bool IsFile()
        {
            if (tbx_source.Text.Length > 0 && tbx_target.Text.Length > 0
                && ((File.Exists(tbx_source.Text) && File.Exists(tbx_target.Text))
                    || (!File.Exists(tbx_source.Text) && !File.Exists(tbx_target.Text))))
                btn_validate.IsEnabled = true;
            else
                btn_validate.IsEnabled = false;

            if (File.Exists(tbx_source.Text) || File.Exists(tbx_target.Text))
            {
                cb_junction.IsChecked = false;
                cb_junction.IsEnabled = false;

                cb_dir.IsChecked = false;
                cb_dir.IsEnabled = false;

                cb_none.IsEnabled = true;

                return true;
            }
            else
            {
                cb_junction.IsEnabled = true;

                cb_dir.IsEnabled = true;

                cb_none.IsEnabled = false;

                return false;
            }
        }

        private void btn_source_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new WinForms.FolderBrowserDialog();
            folderBrowser.SelectedPath = tbx_source.Text;
            if (folderBrowser.ShowDialog() == WinForms.DialogResult.OK)
            {
                var fileBrowser = new WinForms.OpenFileDialog();
                fileBrowser.InitialDirectory = folderBrowser.SelectedPath;
                if (fileBrowser.ShowDialog() == WinForms.DialogResult.OK)
                    tbx_source.Text = fileBrowser.FileName;
                else
                    tbx_source.Text = folderBrowser.SelectedPath;
            }
            IsFile();
        }

        private void btn_target_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new WinForms.FolderBrowserDialog();
            folderBrowser.SelectedPath = tbx_target.Text;
            if (folderBrowser.ShowDialog() == WinForms.DialogResult.OK)
            {
                var fileBrowser = new WinForms.OpenFileDialog();
                fileBrowser.InitialDirectory = folderBrowser.SelectedPath;
                if (fileBrowser.ShowDialog() == WinForms.DialogResult.OK)
                {
                    tbx_target.Text = fileBrowser.FileName;
                }
                else
                {
                    tbx_target.Text = folderBrowser.SelectedPath;
                }
            }
            IsFile();
        }

        private void btn_validate_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append("mklink ");

            if (cb_hard.IsChecked.Value)
                sb.Append("/h");
            if (!IsFile())
            {
                if (!cb_hard.IsChecked.Value)
                    if (cb_junction.IsChecked.Value)
                        sb.Append("/j");
                    else
                        sb.Append("/d");
            }

            sb.Append($" \"{tbx_target.Text}\" \"{tbx_source.Text}\"");

            Commander.RegisterNewCommand(sb.ToString());
            Commander.RunCommands();
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}