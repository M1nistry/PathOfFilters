using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Utils;

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
        private Settings _settings;
        private long _currentFilter;

        private double scaleX = 1, scaleY = 1;

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
            //things();

            //TestFilter.FilterListView.Items.Add(new FilterCondition { Name = "ItemLevel", Value = ">70" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "DropLevel", Tag = "55" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "Quality", Tag = ">= 10" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "Rarity", Tag = "Unique" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "Class", Tag = "'One Hand Axe'" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "BaseType", Tag = "'Siege Axe'" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "Sockets", Tag = ">= 0" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "LinkedSockets", Tag = ">= 0" });
            //TestFilter.FilterListView.Items.Add(new Filter { Name = "SocketGroup", Tag = "6" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "ItemLevel", Tag = ">70" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "DropLevel", Tag = "55" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "Quality", Tag = ">= 10" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "Rarity", Tag = "Unique" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "Class", Tag = "'One Hand Axe'" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "BaseType", Tag = "'Siege Axe'" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "Sockets", Tag = ">= 0" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "LinkedSockets", Tag = ">= 0" });
            //TestFilter2.FilterListView.Items.Add(new Filter { Name = "SocketGroup", Tag = "6" });
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

        private void Settings_MouseDown(object sender, RoutedEventArgs e)
        {
            if (_settings != null && _settings.IsVisible)
            {
                _settings.BringIntoView();
            }
            else
            {
                _settings = new Settings();
                _settings.Closing += (o, ea) => _settings = null;
                _settings.Show();
            }
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
            DragCanvas.Children.Clear();
            PopulateCreator();
            SnapToGrid();
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

        private void FilterGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            FilterGrid.Focus();
        }

        private void FilterGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scaleX > 2 || scaleY > 2)
            {
                scaleX = 1;
                scaleY = 1;
            }

            if (e.Delta > 0)
            {
                if (scaleX > 1.8 || scaleY > 1.8) return;
                scaleX = scaleX + 0.1;
                scaleY = scaleY + 0.1;
            }
            else
            {
                if (scaleX <= 0.6 || scaleY <= 0.6) return;
                scaleX = scaleX - 0.1;
                scaleY = scaleY - 0.1;
            }
            Console.WriteLine("ScaleX " + scaleX + " ScaleY " + scaleY);
            var scaletransform = new ScaleTransform
            {
                ScaleX = scaleX,
                ScaleY = scaleY
            };
            FilterGrid.RenderTransform = scaletransform;
        }

        private void SnapToGrid()
        {
            //TODO: Determine room for horizontal objects, based on width / 150
            //TODO: Based on objects order int, position it respectively

            double unitWidth = FilterGrid.ActualWidth/150;
            int columnCount = 0, rowCount = 0;
            var zIndex = DragCanvas.Children.Count;
            for (var i = 1; i <= DragCanvas.Children.Count; i++)
            {
                foreach (var filterObject in DragCanvas.Children)
                {
                    if (((FilterObject) filterObject).Order != i) continue;
                    if (columnCount == (int)unitWidth)
                    {
                        rowCount ++;
                        columnCount = 0;
                    }
                    Canvas.SetLeft((UIElement)filterObject, (columnCount * 150) + 2);
                    Canvas.SetTop((UIElement) filterObject, (rowCount*27) + 2);
                    Canvas.SetZIndex((UIElement) filterObject, zIndex);
                    zIndex--;
                    columnCount++;
                    break;
                }
            }
        }

        private void PopulateCreator()
        {
            const string strRegex = @"(|#(.+)$[\s\S]?)^(Show|Hide)[\s\S]*?^\s*$";
            var r = new Regex(strRegex, RegexOptions.Multiline);
            var order = 1;

            foreach (var match in r.Matches(AvalonFilter.Text).Cast<Match>().Where(match => match.Success))
            {
                var newFilterObject = new FilterObject { Order = order };
                var splitMatch = match.Value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var conditionList = new List<FilterCondition>();
                foreach (var line in splitMatch)
                {
                    if (line == string.Empty) continue;
                    var conditionString = line.Trim();
                    if (line.StartsWith("#"))
                    {
                        newFilterObject.Description = conditionString.Remove(0, 1);
                        continue;
                    }
                    switch (conditionString)
                    {
                        case "Show":
                            newFilterObject.Show = true;
                            continue;
                        case "Hide":
                            newFilterObject.Show = false;
                            continue;
                    }
                    if (!conditionString.Contains(" ")) continue;
                    var condition = GetCondition(conditionString);
                    if (condition.Name == String.Empty) continue;
                    conditionList.Add(condition);
                }
                newFilterObject.Conditions = conditionList;
                newFilterObject.DataContext = conditionList;
                DragCanvas.Children.Add(newFilterObject);
                order++;
            } 
        }

        public FilterCondition GetCondition(string line)
        {
            var filterCondtion = new FilterCondition();
            var conditionLine = line;
            foreach (var x in FilterCondition.Conditions.Where(line.Contains))
            {
                filterCondtion.Name = x;
                conditionLine = conditionLine.Replace(x + " ", "");
                filterCondtion.Value = conditionLine;
            }
            return filterCondtion;
        }

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((TabItem)TabControlMain.SelectedValue).Header.ToString() == "Creator")
            {
                SnapToGrid();
            }
        }

        private void DragCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SnapToGrid();
        }

    }
}
