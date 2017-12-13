using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : MetroWindow
    {
        public About()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start((sender as Hyperlink).NavigateUri.ToString());
        }

        private void ViewLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            var cred = ((sender as Button).DataContext as CreditDef);

            using (var stream = typeof(About).Assembly.GetManifestResourceStream(cred.EmbeddedLicenseFilename))
            {
                var aboutLicense = new AboutLicense();

                aboutLicense.Owner = this;

                aboutLicense.CredStream = stream;

                aboutLicense.ShowDialog();
            }
        }
    }
}
