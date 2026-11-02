using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NodeEditor.Controls
{
    public class MiniMapControl : Control
    {
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(MiniMapControl), new PropertyMetadata(true));

        public static readonly DependencyProperty ScrollViewerProperty =
                    DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(MiniMapControl),
                new PropertyMetadata(OnScrollViewerChanged));

        private bool _isSyncingFromScroll;

        private Thumb boxView;

        private Border container;

        private Button toggleButton;

        private Path toggleIcon;

        private Canvas view;

        public MiniMapControl()
        { }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public ScrollViewer ScrollViewer
        {
            get => (ScrollViewer)GetValue(ScrollViewerProperty);
            set => SetValue(ScrollViewerProperty, value);
        }

        /// <summary>
        /// Tỉ lệ: 1 pixel trên minimap = bao nhiêu pixel trên ScrollViewer content
        /// </summary>
        private double ScaleX =>
            view.ActualWidth > 0 && ScrollViewer?.ScrollableWidth > 0
                ? (ScrollViewer.ScrollableWidth + ScrollViewer.ViewportWidth) / view.ActualWidth
                : 1.0;

        private double ScaleY =>
            view.ActualHeight > 0 && ScrollViewer?.ScrollableHeight > 0
                ? (ScrollViewer.ScrollableHeight + ScrollViewer.ViewportHeight) / view.ActualHeight
                : 1.0;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            boxView = GetTemplateChild("PART_BoxView") as Thumb;
            view = GetTemplateChild("PART_View") as Canvas;
            container = GetTemplateChild("PART_Container") as Border;
            toggleButton = GetTemplateChild("PART_ToggleMiniMap") as Button;

            if (boxView == null || view == null || container == null || toggleButton == null)
                throw new InvalidOperationException("Required template parts are missing.");
            toggleButton.Click += ToggleButton_Click;
            boxView.DragDelta += OnDragging;
            InitBoxViewPosition();
        }

        private static void OnScrollViewerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var minimap = (MiniMapControl)d;

            if (e.OldValue is ScrollViewer oldSv)
                oldSv.ScrollChanged -= minimap.OnScrollChanged;

            if (e.NewValue is ScrollViewer newSv)
                newSv.ScrollChanged += minimap.OnScrollChanged;
        }

        private void InitBoxViewPosition()
        {
            Canvas.SetLeft(boxView, 0);
            Canvas.SetTop(boxView, 0);
        }

        private void OnDragging(object sender, DragDeltaEventArgs e)
        {
            if (_isSyncingFromScroll) return;

            UpdateBoxViewPosition(e.HorizontalChange, e.VerticalChange);

            var x = Canvas.GetLeft(boxView);
            var y = Canvas.GetTop(boxView);
            ScrollViewer?.ScrollToHorizontalOffset(x * ScaleX);
            ScrollViewer?.ScrollToVerticalOffset(y * ScaleY);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (boxView == null || view == null) return;

            _isSyncingFromScroll = true;

            var x = ScaleX > 0 ? e.HorizontalOffset / ScaleX : 0;
            var y = ScaleY > 0 ? e.VerticalOffset / ScaleY : 0;

            x = Math.Clamp(x, 0, Math.Max(0, view.ActualWidth - boxView.ActualWidth));
            y = Math.Clamp(y, 0, Math.Max(0, view.ActualHeight - boxView.ActualHeight));

            Canvas.SetLeft(boxView, x);
            Canvas.SetTop(boxView, y);

            _isSyncingFromScroll = false;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            container.BeginAnimation(FrameworkElement.WidthProperty,
                new DoubleAnimation(IsExpanded ? 300 : 0, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            container.BeginAnimation(FrameworkElement.HeightProperty,
                new DoubleAnimation(IsExpanded ? 150 : 0, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
        }

        private void UpdateBoxViewPosition(double deltaX, double deltaY)
        {
            var x = Canvas.GetLeft(boxView);
            x = double.IsNaN(x) ? 0 : x + deltaX;

            var y = Canvas.GetTop(boxView);
            y = double.IsNaN(y) ? 0 : y + deltaY;

            x = Math.Clamp(x, 0, Math.Max(0, view.ActualWidth - boxView.ActualWidth));
            y = Math.Clamp(y, 0, Math.Max(0, view.ActualHeight - boxView.ActualHeight));

            Canvas.SetLeft(boxView, x);
            Canvas.SetTop(boxView, y);
        }
    }
}