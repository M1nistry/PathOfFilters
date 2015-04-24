using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
        private Settings _settings;
        private Filter _currentFilter;
        public Pastebin Pastebin;
        private static MainWindow _main;
        internal FilterObject selectedFilter;

        private double scaleX = 1, scaleY = 1;

        public MainWindow()
        {
            InitializeComponent();
            _main = this;
            Pastebin = new Pastebin();
            FilterTitleDescription("");
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
        }

        public static MainWindow GetSingleton()
        {
            return _main;
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
            if (_currentFilter.Id == 0) return;
            TextBoxName.Text = _currentFilter.Name;
            TextBoxTag.Text = _currentFilter.Tag;
            AvalonFilter.Text = _currentFilter.FilterValue;
            UpdateFilters();
            ComboBoxFilters.SelectedValue = _currentFilter.Id;

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
            int version;
            var currentFilter = new Filter
            {
                Id = selectedFilter.Id,
                Name = TextBoxName.Text,
                Tag = TextBoxTag.Text,
                FilterValue = AvalonFilter.Text,
                Version = int.TryParse(TextBoxNumber.Text, out version) ? version : 0,
                Pastebin = TextboxPastebin.Text
            };
            if (_sql.UpdateFilter(currentFilter))
            {
                TextStatus.Text = String.Format("Status: Successfully updated filter {0}", TextBoxName.Text);
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
            if (e.Delta > 0) SliderZoom.Value = SliderZoom.Value + 0.1;
            else SliderZoom.Value = SliderZoom.Value - 0.1;
        }

        internal void SnapToGrid()
        {
            var unitWidth = FilterGrid.ActualWidth / 150;
            int columnCount = 0, rowCount = 0;
            var zIndex = DragCanvas.Children.Count;
            for (var i = 1; i <= DragCanvas.Children.Count; i++)
            {
                var i1 = i;
                foreach (FilterObject filterObject in DragCanvas.Children.Cast<object>().Where(filterObject => ((FilterObject)filterObject).Order == i1))
                {
                    if (columnCount == (int)unitWidth)
                    {
                        rowCount++;
                        columnCount = 0;
                    }

                    Canvas.SetLeft(filterObject, (columnCount * 150) + (columnCount * 2));
                    Canvas.SetTop(filterObject, (rowCount * 27) + (rowCount * 2));
                    Canvas.SetZIndex(filterObject, zIndex);
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
            var id = 1;
            var filterList = new List<FilterObject>();
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
                        var plainLine = conditionString.Remove(0, 1);
                        var titleDescription = FilterTitleDescription(plainLine);
                        newFilterObject.Title = titleDescription.ContainsKey("title") ? titleDescription["title"] : plainLine;
                        newFilterObject.Description = titleDescription.ContainsKey("description") ? titleDescription["description"] : plainLine;
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
                newFilterObject.Name = "Filter" + newFilterObject.Order;
                newFilterObject.Id = id;
                filterList.Add(newFilterObject);
                
                id++;
                order++;
            }
            _sql.CreateObjects(filterList);
            PopulateCanvas();
        }

        private void PopulateCanvas()
        {
            var filterObjects = _sql.GetFilterObjects();
            foreach (var filterObject in filterObjects)
            {
                DragCanvas.Children.Add(filterObject);
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

        private static Dictionary<string,string> FilterTitleDescription(string commentLine)
        {
            var objDic = commentLine.Split(';')
                    .Select(p => p.Split('='))
                    .ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : null);
            return objDic;
        }

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((TabItem) TabControlMain.SelectedValue).Header.ToString() == "Creator")
            {
                SliderZoom.Visibility = Visibility.Visible;
                SnapToGrid();
                return;
            }
            SliderZoom.Visibility = Visibility.Hidden;
        }

        private void DragCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SnapToGrid();
        }

        private int _numValue = 0;
        public int NumValue
        {
            get {  return _numValue; }
            set
            {
                _numValue = value;
                TextBoxNumber.Text = value.ToString();
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue--;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxNumber == null)
            {
                return;
            }

            if (!int.TryParse(TextBoxNumber.Text, out _numValue))
                TextBoxNumber.Text = _numValue.ToString();
        }

        #region SuperSecret
        public const string CRYPT_KEY = @"7793F5001D54BCFF0B4BAA7945F3F3F8";
        #endregion

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var scaletransform = new ScaleTransform
            {
                ScaleX = SliderZoom.Value,
                ScaleY = SliderZoom.Value
            };
            DragCanvas.RenderTransform = scaletransform;
        }

        private void DragCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        internal void PopulateFilter(FilterObject filterObject)
        {
            StackPanelConditions.Children.Clear();
            var dynamicGrid = new Grid
            {
                Margin = new Thickness(0,1,0,0),
                Height = 22,
                ColumnDefinitions = { 
                    new ColumnDefinition
                {
                    Name = "ComboBoxColumn", Width = new GridLength(150)
                }, new ColumnDefinition
                {
                    Name = "ConditionValueColumn",
                    Width = new GridLength(180)
                }, new ColumnDefinition
                {
                    Name = "RemoveColumn",
                    Width = new GridLength(20)
                }}
            };

            var comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                ItemsSource = filterObject.ObservableClass
            };

            var textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 22,
                Width = 175,
                Margin = new Thickness(1,0,0,0)
            };

            var removeButton = new Button
            {
                Content = "-",
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(comboBox, 0);
            Grid.SetColumn(textBox, 1);
            Grid.SetColumn(removeButton, 2);

            foreach (var condition in filterObject.Conditions)
            {
                textBox.Text = condition.Value;
                comboBox.Text = condition.Name;
                if (!dynamicGrid.Children.Contains(comboBox)) dynamicGrid.Children.Add(comboBox);
                if (!dynamicGrid.Children.Contains(textBox)) dynamicGrid.Children.Add(textBox);
                if (!dynamicGrid.Children.Contains(removeButton)) dynamicGrid.Children.Add(removeButton);
                if (!StackPanelConditions.Children.Contains(dynamicGrid)) StackPanelConditions.Children.Add(dynamicGrid);
            }
            AddBlankRow();
        }

        private void AddBlankRow()
        {
            var dynamicGrid = new Grid
            {
                Margin = new Thickness(0, 1, 0, 0),
                Height = 22,
                ColumnDefinitions = { 
                    new ColumnDefinition
                {
                    Name = "ComboBoxColumn", Width = new GridLength(150)
                }, new ColumnDefinition
                {
                    Name = "ConditionValueColumn",
                    Width = new GridLength(180)
                }, new ColumnDefinition
                {
                    Name = "RemoveColumn",
                    Width = new GridLength(20)
                }}
            };

            var comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                ItemsSource = _completionList.ObservableClass
            };
            comboBox.DropDownOpened += comboBox_DropDownOpened;

            var textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 22,
                Width = 175,
                Margin = new Thickness(1, 0, 0, 0)
            };

            textBox.TextChanged += TextBoxTextChanged;

            var removeButton = new Button
            {
                Content = "-",
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };

            removeButton.Click += RemoveButtonOnClick;

            Grid.SetColumn(comboBox, 0);
            Grid.SetColumn(textBox, 1);
            Grid.SetColumn(removeButton, 2);
            dynamicGrid.Children.Add(comboBox);
            dynamicGrid.Children.Add(textBox);
            dynamicGrid.Children.Add(removeButton);
            StackPanelConditions.Children.Insert(StackPanelConditions.Children.Count, dynamicGrid);

        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)StackPanelConditions.Children[StackPanelConditions.Children.Count - 1];
            var textBox = ((TextBox)grid.Children[1]).Text;
            if (textBox != string.Empty) AddBlankRow();
        }

        private void RemoveButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            StackPanelConditions.Children.RemoveAt(StackPanelConditions.Children.IndexOf((UIElement)((Button)sender).Parent));
        }

        private void comboBox_DropDownOpened(object sender, EventArgs e)
        {
            var grid = (Grid)StackPanelConditions.Children[StackPanelConditions.Children.Count -1];
            var textBox = ((TextBox) grid.Children[1]).Text;
            if (textBox != string.Empty)AddBlankRow();
        }

        private void TextBoxTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void ButtonUpdateFilterObject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilter == null) return;
            selectedFilter.Title = TextBoxTitle.Text;
            selectedFilter.Description = TextBoxDescription.Text;
            selectedFilter.Show = RadioShow.IsChecked == true;
            selectedFilter.Id = selectedFilter.Id;
            selectedFilter.Order = selectedFilter.Order;
            var conditionList = new List<FilterCondition>();
            foreach (Grid grid in StackPanelConditions.Children)
            {
                var newCondition = new FilterCondition
                {
                    Name = ((ComboBox)grid.Children[0]).Text,
                    Value = ((TextBox)grid.Children[1]).Text
                };
                if (newCondition.Name != string.Empty && newCondition.Value != string.Empty) conditionList.Add(newCondition);
            }
            selectedFilter.Conditions.Clear();
            selectedFilter.Conditions = conditionList;
        }

    }
}