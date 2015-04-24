using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for FilterObject.xaml
    /// </summary>
    public partial class FilterObject : UserControl
    {

        protected bool isDragging;
        private Point clickPosition;
        private DispatcherTimer _animationTimer;
        private bool _isExpaned;
        private MainWindow _main;
        private HitTestResult result;
        public int Order
        {
            get
            {
                int id;
                return int.TryParse(LabelId.Content.ToString(), out id) ? id : -1;
            }
            set { LabelId.Content = value; }
        }

        public int Id { get; set; }

        public string Title
        {
            get { return LabelTitle.Content != null ? LabelTitle.Content.ToString() : ""; }
            set { LabelTitle.Content = value; }
        }

        public string Description { get; set; }

        private ObservableCollection<FilterCondition> _conditions {
            get
            {
                var collection = new ObservableCollection<FilterCondition>();
                foreach (var item in Conditions)
                {
                    collection.Add(item);
                }
                return collection;
            } 
        } 

        public List<FilterCondition> Conditions
        {
            get
            {
                var filterList = new List<FilterCondition>();
                foreach (var item in FilterListView.Items)
                {
                    if (item.GetType() != typeof(FilterCondition)) continue;
                    filterList.Add((FilterCondition)item);
                }
                return filterList;
            }
            set
            {
                foreach (var item in value)
                {
                    if (!FilterListView.Items.Contains(item))FilterListView.Items.Add(item);
                    HandleColor(item);
                }
            }
        }

        public ObservableCollection<FilterCondition> ObservableConditions
        {
            get { return _conditions; }
            set { value = _conditions; }
        }

        public ObservableCollection<string> ObservableClass 
        {
            get
            {
                var collectionString = new[]
                {
                    "ItemLevel", "DropLevel", "Quality", "Rarity", "Class",
                    "BaseType", "Sockets", "LinkedSockets", "SocketGroup",
                    "SetBorderColor", "SetTextColor", "SetBackgroundColor",
                    "PlayAlertSound"
                };

                return new ObservableCollection<string>(collectionString);
            }   
        }

        public bool Show { get; set; }

        public FilterObject()
        {
            InitializeComponent();
            MouseLeftButtonDown += Control_MouseLeftButtonDown;
            MouseLeftButtonUp += Control_MouseLeftButtonUp;
            MouseMove += Control_MouseMove;
            DataContext = this;
            _main = MainWindow.GetSingleton();
        }

        public bool UpdateObject(FilterObject newObj)
        {
            Title = newObj.Title;
            Description = newObj.Description;
            Order = newObj.Order;
            Id = newObj.Id;
            Conditions = newObj.Conditions;

            return true;
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = sender as UserControl;
            clickPosition = e.GetPosition(Parent as UIElement);
            if (draggableControl == null) return;
            draggableControl.CaptureMouse();
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var draggableControl = sender as UserControl;
            if (draggableControl == null) return;
            draggableControl.ReleaseMouseCapture();
            _main.TextBoxTitle.Text = Title;
            _main.TextBoxDescription.Text = Description;
            _main.RadioShow.IsChecked = Show;
            _main.RadioHide.IsChecked = !Show;
            _main.PopulateFilter(this);
            _main.selectedFilter = this;
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as UserControl;

            if (isDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(Parent as UIElement);

                var transform = draggableControl.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    draggableControl.RenderTransform = transform;
                }
                transform.X = currentPosition.X - clickPosition.X;
                transform.Y = currentPosition.Y - clickPosition.Y;
            }
        }

        private void HandleColor(FilterCondition condition)
        {
            if (condition.Name != "SetBorderColor" && condition.Name != "SetTextColor" &&
                condition.Name != "SetBackgroundColor") return;
            var splitValue = condition.Value.Split(' ');
            int r, g, b;
            var color = new Color();
            if (int.TryParse(splitValue[0], out r) && int.TryParse(splitValue[1], out g) &&
                int.TryParse(splitValue[2], out b))
            {
                color = Color.FromRgb((byte) r, (byte) g, (byte) b);
            }
            switch (condition.Name)
            {
                case("SetBorderColor"):
                    UserBorder.BorderBrush = new SolidColorBrush(color);
                    break;
                case("SetTextColor"):
                    LabelTitle.Foreground = new SolidColorBrush(color);
                    break;
                case("SetBackgroundColor"):
                    TitleBorder.Background = new SolidColorBrush(color);
                    break;
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0,0,0,0,300)
            };
            _animationTimer.Tick += delegate
            {
                Animation(true);
                _isExpaned = true;
                _animationTimer.Stop();
            };
            _animationTimer.Start();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            _animationTimer.Stop();
            if (_isExpaned) Animation(false);
        }

        /// <summary>Initiates the fade in of the move/order borders</summary>
        /// <param name="fadeIn">True to fade in, False to fade out</param>
        private void Animation(bool fadeIn)
        {
            BorderMove.Visibility = Visibility.Visible;
            BorderId.Visibility = Visibility.Visible;
            if (fadeIn)
            {
                var a = new DoubleAnimation
                {
                    From = 0,
                    To = 100,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var expand = new DoubleAnimation
                {
                    From = 25,
                    To = 150,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var title = new DoubleAnimation
                {
                    From = 25,
                    To = 20,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var storyboard = new Storyboard();
                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, BorderMove);
                Storyboard.SetTarget(a, BorderId);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));

                storyboard.Children.Add(expand);
                Storyboard.SetTarget(expand, FilterListView);
                Storyboard.SetTargetProperty(expand, new PropertyPath(HeightProperty));

                storyboard.Children.Add(title);
                Storyboard.SetTarget(title, TitleBorder);
                Storyboard.SetTargetProperty(title, new PropertyPath(HeightProperty));

                storyboard.Completed += delegate
                {
                    BorderMove.Visibility = Visibility.Visible;
                    BorderId.Visibility = Visibility.Visible;
                };
                storyboard.Begin(this, true);
            }
            else
            {
                var a = new DoubleAnimation
                {
                    From = 100,
                    To = 0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var collapse = new DoubleAnimation
                {
                    From = 150,
                    To = 25,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var title = new DoubleAnimation
                {
                    From = 20,
                    To = 25,
                    BeginTime = TimeSpan.FromSeconds(0.1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };

                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, BorderMove);
                Storyboard.SetTarget(a, BorderId);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));

                storyboard.Children.Add(collapse);
                Storyboard.SetTarget(collapse, FilterListView);
                Storyboard.SetTargetProperty(collapse, new PropertyPath(HeightProperty));

                storyboard.Children.Add(title);
                Storyboard.SetTarget(title, TitleBorder);
                Storyboard.SetTargetProperty(title, new PropertyPath(HeightProperty));

                storyboard.Completed += delegate
                {
                    BorderId.Visibility = Visibility.Hidden;
                    BorderMove.Visibility = Visibility.Hidden;
                };
                storyboard.Begin(this, true);
                _isExpaned = false;
            }
        }
    }
}
