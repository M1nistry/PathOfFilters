using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Mono.CSharp.Linq;

namespace PathOfFilters
{
    /// <summary>
    /// Interaction logic for FilterObject.xaml
    /// </summary>
    public partial class FilterObject : UserControl
    {

        protected bool isDragging;
        private Point clickPosition;

        public int Order
        {
            get
            {
                int id;
                return int.TryParse(LabelId.Content.ToString(), out id) ? id : -1;
            }
            set { LabelId.Content = value; }
        }

        public string Description
        {
            get { return LabelTitle.Content.ToString(); }
            set { LabelTitle.Content = value; }
        }

        public List<FilterCondition> Conditions
        {
            get { return new List<FilterCondition>(); }
            set
            {
                foreach (var condition in value)
                {
                    FilterListView.Items.Add(condition);
                    HandleColor(condition);
                }
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
            ShowPopups(true);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            ShowPopups(false);
        }

        /// <summary>Initiates the fade in of the move/order borders</summary>
        /// <param name="fadeIn">True to fade in, False to fade out</param>
        private void ShowPopups(bool fadeIn)
        {
            BorderMove.Visibility = Visibility.Visible;
            BorderId.Visibility = Visibility.Visible;
            if (fadeIn)
            {
                var a = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, BorderMove);
                Storyboard.SetTarget(a, BorderId);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate
                {
                    BorderMove.Visibility = Visibility.Visible;
                    BorderId.Visibility = Visibility.Visible;
                };
                storyboard.Begin();
            }
            else
            {
                var a = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, BorderMove);
                Storyboard.SetTarget(a, BorderId);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate
                {
                    BorderId.Visibility = Visibility.Hidden;
                    BorderMove.Visibility = Visibility.Hidden;
                };
                storyboard.Begin();
            }
        }
    }
}
