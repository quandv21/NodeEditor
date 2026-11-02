using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class ConnectionLineAdorner : Adorner
    {
        private readonly SolidColorBrush background;
        private Point _arrowP1, _arrowP2;
        private PathGeometry _geometry;
        private Pen _pen;
        private Point endPoint;

        public ConnectionLineAdorner(UIElement adornedElement) : base(adornedElement)
        {
            background = (SolidColorBrush)Application.Current.FindResource("SuccessBrush");
            var alertSuccessColor = (Color)Application.Current.FindResource("AlertSuccessColor");
            _pen = new Pen(background, 1)
            {
                DashStyle = DashStyles.Dash
            };
            IsHitTestVisible = true;
        }

        public void RebuildGeometry(Point start, Point end)
        {
            endPoint = end;
            var bend = CalculateBendPoint(start, end);
            var figure = new PathFigure() { StartPoint = start };
            figure.Segments.Add(new LineSegment(bend, true));
            figure.Segments.Add(new LineSegment(end, true));
            _geometry = new PathGeometry();
            _geometry.Figures.Add(figure);
            if (start.X == end.X)
            {
                BuildArrowHead(bend, end, false);
                InvalidateVisual();
                return;
            }
            BuildArrowHead(bend, end, true);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (_geometry == null)
                return;

            drawingContext.DrawGeometry(Brushes.Transparent, _pen, _geometry);
            StreamGeometry arrow = new StreamGeometry();
            using (var ctx = arrow.Open())
            {
                ctx.BeginFigure(endPoint, true, true);
                ctx.LineTo(_arrowP1, true, true);
                ctx.LineTo(_arrowP2, true, true);
            }

            drawingContext.DrawGeometry(background, null, arrow);
        }

        private void BuildArrowHead(Point bend, Point endPoint, bool isHor)
        {
            Vector dir = endPoint - bend;
            if (dir == default(Vector))
            {
                var x = isHor ? 1 : 0;
                var y = isHor ? 0 : 1;
                dir = new Vector(x, y);
            }
            dir.Normalize();

            Vector normal = new Vector(-dir.Y, dir.X);

            double length = 12;
            double width = 6;

            _arrowP1 = endPoint - dir * length + normal * width;
            _arrowP2 = endPoint - dir * length - normal * width;
        }

        private Point CalculateBendPoint(Point start, Point end)
        {
            double dx = Math.Abs(end.X - start.X);
            double dy = Math.Abs(end.Y - start.Y);

            if (dx > dy)
            {
                // Đi ngang trước
                return new (end.X, start.Y);
            }
            else
            {
                // Đi dọc trước
                return new (start.X, end.Y);
            }
        }
    }
}