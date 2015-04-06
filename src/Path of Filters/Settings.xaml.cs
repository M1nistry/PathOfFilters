using System;
using System.Windows;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void CheckBoxPasteBin_Checked(object sender, RoutedEventArgs e)
        {
            GroupPasteBin.IsEnabled = true;
        }

        private void CheckBoxPasteBin_Unchecked(object sender, RoutedEventArgs e)
        {
            GroupPasteBin.IsEnabled = false;
        }

        private void ButtonFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fileBrowser = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Locate your filter file",
                DefaultExt = ".txt",
                Filter = "Text Files (*.txt)|*.txt"
            };
            var result = fileBrowser.ShowDialog();
            if (result != true) return;
            TextBoxFilterFile.Text = fileBrowser.FileName;
        }
    }
}
