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

namespace BeadArray
{
    /// <summary>
    /// Interaction logic for namePopup.xaml
    /// </summary>
    public partial class NamePopup : Window
    {
        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }
        public NamePopup()
        {
            InitializeComponent();

        }

        private void Palette_Name_Confirm(object sender, RoutedEventArgs e)
        {
            if(ResponseTextBox.Text.Length == 0)
            {
                MessageBox.Show("Must enter a name", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            } else
            {
                DialogResult = true;
            }
        }

        private void ResponseTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (ResponseTextBox.Text.Length == 0)
                {
                    MessageBox.Show("Must enter a name","Invalid Input",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                else
                {
                    DialogResult = true;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResponseTextBox.Focus();
        }
    }
}
