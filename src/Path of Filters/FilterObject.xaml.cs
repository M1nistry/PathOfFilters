using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for FilterObject.xaml
    /// </summary>
    public partial class FilterObject : UserControl
    {
        public FilterObject()
        {
            InitializeComponent();
            
        }



        private void FilterListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("LEFT DOWN");
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                
            }
        }
    }
}
