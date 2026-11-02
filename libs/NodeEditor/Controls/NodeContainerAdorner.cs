using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class NodeContainerAdorner : Adorner
    {
        private readonly Pen _pen;
        private Point _endPoint;
        private Point _startPoint;
        private Point currentPoint;

        private bool isDragging = false;

        private Point startPoint;

        public NodeContainerAdorner(NodeEditorViewControl adornedElement, Point startPoint, Point endPoint) : base(adornedElement)
        {
            SolidColorBrush primaryBrush = (SolidColorBrush)Application.Current.FindResource("PrimaryBrush");
            Color primaryColor = (Color)Application.Current.FindResource("AlertPrimaryColor");
            _pen = new Pen(primaryBrush, 2)
            {
                DashStyle = DashStyles.Dash
            };

            _startPoint = startPoint;
            _endPoint = endPoint;
            IsHitTestVisible = true;
            Cursor = Cursors.Hand;
        }

        public void UpdateContainerPosition(Point startPoint, Point endPoint)
        {
            if (!isDragging) return;
            _startPoint = startPoint;
            _endPoint = endPoint;
            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            isDragging = true;
            Cursor = Cursors.SizeAll;
            startPoint = e.GetPosition(AdornedElement);
            CaptureMouse();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !isDragging) return;
            currentPoint = e.GetPosition(AdornedElement);
            var deltaX = currentPoint.X - startPoint.X;
            var deltaY = currentPoint.Y - startPoint.Y;
            ((NodeEditorViewControl)AdornedElement).UpdateNodesPosition(deltaX, deltaY);
            startPoint = currentPoint;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            isDragging = false;
            Cursor = Cursors.Hand;
            ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var offset = Contants.CSize.NODE_MARGIN;
            Rect selectionRect;
            if (isDragging)
            {
                selectionRect = new Rect(new Point(_startPoint.X - offset, _startPoint.Y - offset), new Point(_endPoint.X + offset, _endPoint.Y + offset));
                drawingContext.DrawRectangle(Brushes.Transparent, _pen, selectionRect);
                return;
            }
            selectionRect = new Rect(new Point(_startPoint.X - offset, _startPoint.Y - offset), new Point(_endPoint.X + offset, _endPoint.Y + offset));
            drawingContext.DrawRectangle(Brushes.Transparent, _pen, selectionRect);
        }
    }
}