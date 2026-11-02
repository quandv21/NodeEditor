using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class RectangleAdorner : Adorner
    {
        private readonly Pen _pen;
        private Point currentPoint;
        private readonly Point _startPoint;
        private readonly Color _alertPrimaryColor;

        public RectangleAdorner(NodeEditorViewControl adornedElement, Point startPoint) : base(adornedElement)
        {
            var primaryBrush = (SolidColorBrush)Application.Current.FindResource("PrimaryBrush");
            _alertPrimaryColor = (Color)Application.Current.FindResource("AlertPrimaryColor");
            _pen = new Pen(primaryBrush, 2);
            _pen.DashStyle = DashStyles.Dash;
            _startPoint = startPoint;
            currentPoint = startPoint;
            IsHitTestVisible = true;
            isDragging = true;
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = _alertPrimaryColor,
                Direction = 0,
                ShadowDepth = 0,
                Opacity = 1,
                BlurRadius = 15
            };
            CaptureMouse();
        }

        private bool isDragging = false;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            isDragging = false;
            ReleaseMouseCapture();
            var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }
            var nodeEditor = AdornedElement as NodeEditorViewControl;
            nodeEditor?.RefreshSelectionNode();
            nodeEditor?.RefreshSelectedConnection();
            nodeEditor?.AddSelectionNode(_startPoint, currentPoint);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !isDragging) return;
            currentPoint = e.GetPosition(this);
            InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!isDragging)
            {
                base.OnRender(drawingContext);
                return;
            }
            Rect rect = new Rect(RenderSize);
            drawingContext.DrawRectangle(Brushes.Transparent, null, rect);
            Rect selectionRect = new Rect(_startPoint, currentPoint);
            drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(10, _alertPrimaryColor.R, _alertPrimaryColor.G, _alertPrimaryColor.B)), _pen, selectionRect);
        }
    }
}