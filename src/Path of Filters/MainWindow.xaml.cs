using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        CompletionWindow _completionWindow;
        private readonly CompletionLists _completionList = new CompletionLists();
        private readonly SqlWrapper _sql;
        private long _currentFilter;

        public MainWindow()
        {
            InitializeComponent();


            //Initializes the custom syntax highlighting within poefilter.xshd
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PathOfFilters.Resources.poefilter.xshd"))
            {
                if (stream == null) return;
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    AvalonFilter.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    AvalonFilter.TextArea.IndentationStrategy = new CustomIndentationStrategy();
                    AvalonFilter.TextArea.TextView.LineTransformers.Add(new ColorizeRgb());
                    AvalonFilter.ShowLineNumbers = true;
                    
                    AvalonFilter.TextArea.TextEntering += AvalonFilter_TextEntering;
                    AvalonFilter.TextArea.TextEntered += AvalonFilter_TextEntered;
                    AvalonFilter.TextArea.KeyDown += AvalonFilter_KeyDown;
                }
            }
            _sql = new SqlWrapper();
            
            UpdateFilters();
            TestFilter.FilterListView.Items.Add(new Filter {Name = "ItemLevel", Tag = ">70"});
            TestFilter.FilterListView.Items.Add(new Filter { Name = "DropLevel", Tag = "55" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "Quality", Tag = ">= 10" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "Rarity", Tag = "Unique" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "Class", Tag = "'One Hand Axe'" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "BaseType", Tag = "'Siege Axe'" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "Sockets", Tag = ">= 0" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "LinkedSockets", Tag = ">= 0" });
            TestFilter.FilterListView.Items.Add(new Filter { Name = "SocketGroup", Tag = "6" });
        }

        private void UpdateFilters()
        {
            ComboBoxFilters.ItemsSource = null;
            ComboBoxFilters.ItemsSource = _sql.GetFilters();
            ComboBoxFilters.DisplayMemberPath = "Name";
            ComboBoxFilters.SelectedValuePath = "Id"; 
        }

        private void AvalonFilter_KeyDown(object sender, KeyEventArgs e)
        {
            //Show autocomplete on CTRL+Enter
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.Space)
            {
                e.Handled = true;
                _completionWindow = new CompletionWindow(AvalonFilter.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                foreach (var item in _completionList.Conditions)
                {
                    data.Add(new CodeCompletion(item));
                }
                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        private void AvalonFilter_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\"")
            {
                _completionWindow = new CompletionWindow(AvalonFilter.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                foreach (var item in _completionList.Items)
                {
                    data.Add(new CodeCompletion(item));
                }
                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }

        }

        private void AvalonFilter_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length <= 0 || _completionWindow == null) return;
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        private void NewFilter_MouseDown(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sa");
        }

        private void ButtonAddFilter_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = _sql.CreateFilter();
            if (_currentFilter <= 0) return;
            TextBoxName.Text = "New Filter";

        }

        private void ComboBoxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: Check for unsaved filter changes
            var selectedFilter = ((Filter) ComboBoxFilters.SelectedItem);
            if (selectedFilter == null) return;
            TextBoxName.Text = selectedFilter.Name;
            TextBoxTag.Text = selectedFilter.Tag;
            AvalonFilter.Text = selectedFilter.FilterValue;
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            var selectedFilter = ((Filter) ComboBoxFilters.SelectedItem);
            if (selectedFilter == null) return;
            if (_sql.UpdateFilter(selectedFilter.Id, TextBoxName.Text, TextBoxTag.Text, AvalonFilter.Text))
            {
                LabelStatus.Content = String.Format("Status: Successfully updated filter {0}", TextBoxName.Text);
            }
            UpdateFilters();
            ComboBoxFilters.SelectedValue = selectedFilter.Id;
        }

        private void TestFilter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        
        }

        private void TestFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("oj");
        }
    }
}
