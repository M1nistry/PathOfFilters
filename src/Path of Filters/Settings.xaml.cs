using System;
using System.Windows;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private MainWindow _main;
        public Settings()
        {
            InitializeComponent();
            _main = MainWindow.GetSingleton();
            TextBoxUsername.Text = Properties.Settings.Default.PastebinUsername;
            PasswordPasteBin.Password = Properties.Settings.Default.PastebinPassword != string.Empty
                ? Crypto.DecryptStringAES(Properties.Settings.Default.PastebinPassword, MainWindow.CRYPT_KEY) : "";
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

        private void ButtonVerify_Click(object sender, RoutedEventArgs e)
        {
            var pastebin = _main.Pastebin;
            pastebin.Username = TextBoxUsername.Text;
            pastebin.Password = PasswordPasteBin.Password;
            var user = _main.Pastebin.PastebinUser;
            if (user == null) return;
            Properties.Settings.Default.PastebinUsername = pastebin.Username;
            Properties.Settings.Default.PastebinPassword = Crypto.EncryptStringAES(pastebin.Password, MainWindow.CRYPT_KEY);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
