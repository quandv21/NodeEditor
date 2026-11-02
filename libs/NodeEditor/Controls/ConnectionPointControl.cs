using NodeEditor.Helpers;
using NodeEditor.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class ConnectionPointControl : Control
    {
        public static readonly DependencyProperty ConnectionPointProperty =
            DependencyProperty.Register(nameof(ConnectionPoint), typeof(ConnectionPointModel), typeof(ConnectionPointControl), new PropertyMetadata(null));

        public static readonly DependencyProperty EditorViewProperty =
            DependencyProperty.Register(nameof(EditorView), typeof(NodeEditorViewControl), typeof(ConnectionPointControl), new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ConnectionPointControl), new PropertyMetadata(false, OnSelectedChanged));

        public static readonly DependencyProperty PrimaryBrushProperty =
            DependencyProperty.Register(nameof(PrimaryBrush), typeof(Brush), typeof(ConnectionPointControl), new PropertyMetadata(CBrushConverter.Convert("#007bff")));

        private ConnectionLineAdorner connectionLineAdorner = null!;
        private Point currentPoint;
        private bool isMoving = false;

        private Point startPoint;

        public ConnectionPointControl()
        {
        }

        public ConnectionPointModel ConnectionPoint
        {
            get { return (ConnectionPointModel)GetValue(ConnectionPointProperty); }
            set { SetValue(ConnectionPointProperty, value); }
        }

        public NodeEditorViewControl EditorView
        {
            get { return (NodeEditorViewControl)GetValue(EditorViewProperty); }
            set { SetValue(EditorViewProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Brush PrimaryBrush
        {
            get { return (Brush)GetValue(PrimaryBrushProperty); }
            set { SetValue(PrimaryBrushProperty, value); }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                base.OnMouseDown(e);
                return;
            }
            if (EditorView == null)
            {
                base.OnMouseDown(e);
                return;
            }
            isMoving = true;
            Cursor = Cursors.Cross;
            startPoint = new Point(ConnectionPoint.X, ConnectionPoint.Y);
            InitConnectionLineAdorner();
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !isMoving || connectionLineAdorner == null)
            {
                base.OnMouseMove(e);
                return;
            }
            Cursor = Cursors.Pen;
            currentPoint = e.GetPosition(EditorView);
            connectionLineAdorner.RebuildGeometry(startPoint, currentPoint);
            EditorView.HandleConnectionDragging(currentPoint);
            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            isMoving = false;
            Cursor = Cursors.Arrow;
            EditorView.HandleArrowHeadDroped(ConnectionPoint, currentPoint);
            var adonerLayer = AdornerLayer.GetAdornerLayer(EditorView);
            if (adonerLayer == null) return;
            if (connectionLineAdorner == null) return;
            adonerLayer.Remove(connectionLineAdorner);
            connectionLineAdorner = null;
            ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ConnectionPointControl)d;
            control?.InvalidateVisual();
        }

        private void InitConnectionLineAdorner()
        {
            connectionLineAdorner = new ConnectionLineAdorner(EditorView);
            var adonerLayer = AdornerLayer.GetAdornerLayer(EditorView);
            if (adonerLayer == null) return;
            adonerLayer.Add(connectionLineAdorner);
            var start = new Point(ConnectionPoint.X, ConnectionPoint.Y);
            var end = new Point(ConnectionPoint.X, ConnectionPoint.Y);
            connectionLineAdorner.RebuildGeometry(start, end);
        }
    }
}