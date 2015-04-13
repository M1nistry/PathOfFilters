using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for Autocomplete.xaml
    /// </summary>
    public partial class Autocomplete : UserControl
    {
        private readonly CompletionLists _completionLists;

        public string FilterString { get; set; }
        public Autocomplete()
        {
            InitializeComponent();
            _completionLists = new CompletionLists();
            ListViewAutoComplete.ItemsSource = _completionLists.Items;
            // = MainWindow.GetSingleton().DataGridConditions;
        }

        private void ListBoxAutoComplete_OnKeyUp(object sender, KeyEventArgs e)
        {
            
        }

        public void FilterList(string input)
        {
            var tempFilteredList = _completionLists.Items.Where(n => n.Contains(input)).Select(r => r);
            ListViewAutoComplete.ItemsSource = tempFilteredList;
        }

        private void ListViewAutoComplete_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (Visibility == Visibility.Visible)
                {
                    ListViewAutoComplete.Focus();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Visibility = Visibility.Hidden;
                e.Handled = true;
            }
        }

        private void TextValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterList(TextValue.Text);
        }

    }
}
