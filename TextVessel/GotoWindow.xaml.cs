using System;
using System.Collections.Generic;
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

namespace TextVessel
{
    /// <summary>
    /// Interaction logic for GotoWindow.xaml
    /// </summary>
    public partial class GotoWindow : Window
    {
        public static readonly DependencyProperty GotoProperty = DependencyProperty.Register(nameof(Goto), typeof(int), typeof(GotoWindow));

        public int Goto
        {
            get => (int)GetValue(GotoProperty);
            set => SetValue(GotoProperty, value);
        }

        public GotoWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(EntryIDTextBox))
                return;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(EntryIDTextBox))
                return;

            Goto = -1;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EntryIDTextBox.Focus();
            EntryIDTextBox.SelectAll();
        }

        private void EntryIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Validation.GetHasError(EntryIDTextBox))
                    return;

                Close();
            }
        }
    }
}
