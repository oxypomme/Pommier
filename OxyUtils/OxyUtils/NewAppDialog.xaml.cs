using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour NewAppDialog.xaml
    /// </summary>
    public partial class NewAppDialog : Window
    {
        internal Applet createdApp;

        public NewAppDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            createdApp = new Applet(tbx_name.Text, tbx_path.Text, tbx_args.Text, cbx_bits.IsChecked.Value);
            Close();
        }

        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Exe File (*.exe)|*.exe"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                tbx_path.Text = openFileDialog.FileName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}