using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TextVessel.Credits;

namespace TextVessel
{
    /// <summary>
    /// Interaction logic for AboutLicense.xaml
    /// </summary>
    public partial class AboutLicense : MetroWindow
    {
        public Stream CredStream { get; set; } = null;

        public AboutLicense()
        {
            InitializeComponent();
        }

        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            HtmlView.NavigateToStream(CredStream);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
