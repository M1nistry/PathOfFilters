using System.IO;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CompletionWindow _completionWindow;
        private readonly CompletionLists _completionList = new CompletionLists();

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
            LoadFilter();
        }

        private void LoadFilter()
        {
            using (var streamReader = new StreamReader(@"C:\filter.txt"))
            {
                AvalonFilter.Text = streamReader.ReadToEnd();
            }
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
            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(AvalonFilter.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                var stringarray = new[] {"Test1", "Hello World", "Show"};
                foreach (var item in stringarray)
                {
                    data.Add(new CodeCompletion(item));
                }
                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
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
    }
}
