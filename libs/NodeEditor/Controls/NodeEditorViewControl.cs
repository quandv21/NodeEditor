using NodeEditor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class NodeEditorViewControl : Control
    {
        public static readonly DependencyProperty ArrowsProperty =
            DependencyProperty.Register(nameof(Arrows), typeof(ObservableCollection<ConnectionLineModel>), typeof(NodeEditorViewControl), new PropertyMetadata(new ObservableCollection<ConnectionLineModel>()));

        public static readonly DependencyProperty ConnectionsProperty =
            DependencyProperty.Register(nameof(Connections), typeof(ObservableCollection<ConnectionInfo>), typeof(NodeEditorViewControl), new PropertyMetadata(new ObservableCollection<ConnectionInfo>()));

        public static readonly DependencyProperty CurrentMouseXProperty =
                    DependencyProperty.Register(nameof(CurrentMouseX), typeof(int), typeof(NodeEditorViewControl), new PropertyMetadata(0));

        public static readonly DependencyProperty CurrentMouseYProperty =
            DependencyProperty.Register(nameof(CurrentMouseY), typeof(int), typeof(NodeEditorViewControl), new PropertyMetadata(0));

        public static readonly DependencyProperty NodeHeightProperty =
            DependencyProperty.Register(nameof(NodeHeight), typeof(int), typeof(NodeEditorViewControl), new PropertyMetadata(100));

        public static readonly DependencyProperty NodesProperty =
                                            DependencyProperty.Register(nameof(Nodes), typeof(ObservableCollection<NodeModel>), typeof(NodeEditorViewControl), new PropertyMetadata(new ObservableCollection<NodeModel>()));

        public static readonly DependencyProperty NodeWidthProperty =
            DependencyProperty.Register(nameof(NodeWidth), typeof(int), typeof(NodeEditorViewControl), new PropertyMetadata(200));

        public static readonly DependencyProperty ScrollViewerProperty =
                    DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(NodeEditorViewControl), new PropertyMetadata(null));

        public static RoutedCommand DeleteSelectedItems = new();
        public static RoutedCommand SelectAll = new();
        private readonly double scale = 1.1;
        private Canvas canvas;
        private RectangleAdorner currentAdorner;
        private NodeModel? currentNode;
        private Point dragStartPoint;
        private bool isDragging = false;
        private NodeContainerAdorner nodeContainerAdorner;
        private ScaleTransform? scaleTransform = new();
        private Point dragNodeStartPoint;

        public NodeEditorViewControl()
        {
            Focusable = true;
            CommandBindings.Add(new(SelectAll, OnSelectAll));
            CommandBindings.Add(new(DeleteSelectedItems, OnDeleteItems));
            SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            DeleteSelectedItems.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public ObservableCollection<ConnectionLineModel> Arrows
        {
            get { return (ObservableCollection<ConnectionLineModel>)GetValue(ArrowsProperty); }
            set { SetValue(ArrowsProperty, value); }
        }

        public ObservableCollection<ConnectionInfo> Connections
        {
            get { return (ObservableCollection<ConnectionInfo>)GetValue(ConnectionsProperty); }
            set { SetValue(ConnectionsProperty, value); }
        }

        public int CurrentMouseX
        {
            get { return (int)GetValue(CurrentMouseXProperty); }
            set { SetValue(CurrentMouseXProperty, value); }
        }

        public int CurrentMouseY
        {
            get { return (int)GetValue(CurrentMouseYProperty); }
            set { SetValue(CurrentMouseYProperty, value); }
        }

        public int NodeHeight
        {
            get { return (int)GetValue(NodeHeightProperty); }
            set { SetValue(NodeHeightProperty, value); }
        }

        public ObservableCollection<NodeModel> Nodes
        {
            get { return (ObservableCollection<NodeModel>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        public int NodeWidth
        {
            get { return (int)GetValue(NodeWidthProperty); }
            set { SetValue(NodeWidthProperty, value); }
        }

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public void AddSelectedNode(NodeModel selectedNode)
        {
            if (selectedNode == null) return;
            selectedNode.IsSelected = true;
            var selectedNodes = Nodes.Where(n => n.IsSelected);
            AddNodeContainerAdorner(selectedNodes.ToList());
        }

        public void AddSelectionNode(Point startPoint, Point endPoint)
        {
            var deltaX = endPoint.X - startPoint.X;
            var deltaY = endPoint.Y - startPoint.Y;
            var childNodes = Nodes.Where(n => (n.X - startPoint.X) > 0 && (n.X - startPoint.X) <= deltaX && (n.Y - startPoint.Y) > 0 && (n.Y - startPoint.Y) <= deltaY);
            foreach (var node in childNodes)
            {
                node.IsSelected = true;
            }
            AddNodeContainerAdorner(childNodes.ToList());
        }

        public void DeleteConnectionLine(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null) return;
            connectionInfo.IsSelected = false;
            connectionInfo.FromPoint.IsConnected = false;
            connectionInfo.ToPoint.IsConnected = false;
            Connections.Remove(connectionInfo);
            connectionInfo.Destroy();
        }

        public void HandleArrowHeadDroped(ConnectionPointModel from, Point position)
        {
            ConnectionPointModel toPoint = null;
            foreach (var node in Nodes)
            {
                toPoint = node.SetConnected(position);
                if (toPoint != null) break;
            }
            if (toPoint == null)
            {
                return;
            }
            from.IsConnected = true;
            from.Parent.AddChild(toPoint.Parent);
            toPoint.Parent.AddFront(from.Parent);
            var connection = new ConnectionInfo(from, toPoint);
            Connections.Add(connection);
        }

        public void HandleConnectionDragging(Point position)
        {
            foreach (var node in Nodes)
            {
                if (node.IsMouseOnNode(position))
                {
                    node.IsMouseOver = true;
                    continue;
                }
                node.IsMouseOver = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            canvas = GetTemplateChild("PART_Canvas") as Canvas;
            if (canvas == null) return;
            canvas.DragOver += Canvas_DragOver;
            canvas.PreviewDragOver += Canvas_PreviewDragOver;
            canvas.Drop += Canvas_Drop;
        }

        private void Canvas_PreviewDragOver(object sender, DragEventArgs e)
        {
            dragNodeStartPoint = e.GetPosition(this);
            CurrentMouseX = (int)dragNodeStartPoint.X;
            CurrentMouseY = (int)dragNodeStartPoint.Y;
            if (currentNode != null)
            {
                return;
            }
            currentNode = (e.Data.GetData(nameof(NodeModel)) as NodeModel)?.Clone() as NodeModel;
            if (currentNode == null)
            {
                return;
            }
            currentNode.X = dragNodeStartPoint.X;
            currentNode.Y = dragNodeStartPoint.Y;
            Nodes.Add(currentNode);
        }

        public void RefreshSelectedConnection()
        {
            foreach (var item in Connections)
            {
                item.IsSelected = false;
            }
        }

        public void RefreshSelectionNode()
        {
            foreach (var node in Nodes)
            {
                if (!node.IsSelected)
                {
                    continue;
                }
                node.IsSelected = false;
                node.ZIndex--;
            }
        }

        public void SelectedSingleNode(NodeModel node)
        {
            if (node == null) return;
            node.IsSelected = true;
            node.ZIndex++;
        }

        public void UpdateNodesPosition(double deltaX, double deltaY)
        {
            var selectedNodes = Nodes.Where(n => n.IsSelected);
            foreach (var node in selectedNodes)
            {
                node.X += deltaX;
                node.Y += deltaY;
            }
            var nodeMinX = selectedNodes.Min(n => n.X);
            var nodeMinY = selectedNodes.Min(n => n.Y);
            var nodeMaxX = selectedNodes.Max(n => n.X + n.Width);
            var nodeMaxY = selectedNodes.Max(n => n.Y + n.Height);
            var newStartPoint = new Point(nodeMinX, nodeMinY);
            var newEndPoint = new Point(nodeMaxX, nodeMaxY);
            nodeContainerAdorner?.UpdateContainerPosition(newStartPoint, newEndPoint);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                currentAdorner = new RectangleAdorner(this, e.GetPosition(this));
                var adonerLayer = AdornerLayer.GetAdornerLayer(this);
                RemoveNodeContainerAdorner(adonerLayer);
                adonerLayer.Add(currentAdorner);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            CurrentMouseX = (int)mousePos.X;
            CurrentMouseY = (int)mousePos.Y;
            if (isDragging)
            {
                SetScrollOffset(e);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            ScrollViewer.Cursor = Cursors.Hand;
            isDragging = true;
            dragStartPoint = e.GetPosition(ScrollViewer);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            ScrollViewer.Cursor = Cursors.Arrow;
            isDragging = false;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (currentAdorner != null)
            {
                var adonerLayer = AdornerLayer.GetAdornerLayer(this);
                adonerLayer.Remove(currentAdorner);
                currentAdorner = null;
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }
            var mousePos = e.GetPosition(ScrollViewer);

            var imagePointX = (ScrollViewer.HorizontalOffset + mousePos.X) / scaleTransform.ScaleX;
            var imagePointY = (ScrollViewer.VerticalOffset + mousePos.Y) / scaleTransform.ScaleY;

            var newScaleX = e.Delta > 0 ? (scaleTransform.ScaleX * scale) : (scaleTransform.ScaleX / scale);
            var newScaleY = e.Delta > 0 ? (scaleTransform.ScaleY * scale) : (scaleTransform.ScaleY / scale);

            scaleTransform.ScaleX = newScaleX;
            scaleTransform.ScaleY = newScaleY;

            var newHorizontalOffset = imagePointX * newScaleX - mousePos.X;
            var newVerticalOffset = imagePointY * newScaleY - mousePos.Y;

            ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            this.LayoutTransform = scaleTransform;
        }

        private static T? FindParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T t)
                    return t;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        private void AddNodeContainerAdorner(List<NodeModel> nodes)
        {
            if (nodes.Count == 0) return;
            var nodeMinX = nodes.Min(n => n.X);
            var nodeMinY = nodes.Min(n => n.Y);
            var nodeMaxX = nodes.Max(n => n.X + n.Width);
            var nodeMaxY = nodes.Max(n => n.Y + n.Height);
            var newStartPoint = new Point(nodeMinX, nodeMinY);
            var newEndPoint = new Point(nodeMaxX, nodeMaxY);
            var adonerLayer = AdornerLayer.GetAdornerLayer(this);
            RemoveNodeContainerAdorner(adonerLayer);
            nodeContainerAdorner = new NodeContainerAdorner(this, newStartPoint, newEndPoint);
            adonerLayer.Add(nodeContainerAdorner);
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            var position = e.GetPosition(canvas);
            if (currentNode == null)
            {
                return;
              
            }
            currentNode.X = position.X;
            currentNode.Y = position.Y;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            currentNode = null;
        }

        private void OnDeleteItems(object sender, ExecutedRoutedEventArgs e)
        {
            var nodes = Nodes.Where(n => n.IsSelected).ToList();
            foreach (var node in nodes)
            {
                Nodes.Remove(node);
            }
            var adonerLayer = AdornerLayer.GetAdornerLayer(this);
            RemoveNodeContainerAdorner(adonerLayer);
        }

        private void OnSelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var item in Nodes)
            {
                item.IsSelected = true;
            }
            AddNodeContainerAdorner(Nodes.ToList());
        }

        private void RemoveNodeContainerAdorner(AdornerLayer adonerLayer)
        {
            if (nodeContainerAdorner != null && adonerLayer != null)
            {
                adonerLayer.Remove(nodeContainerAdorner);
                nodeContainerAdorner = null;
            }
        }

        private void SetScrollOffset(MouseEventArgs mouse)
        {
            var hOff = ScrollViewer.HorizontalOffset;
            var vOff = ScrollViewer.VerticalOffset;
            var currentMousePosition = mouse.GetPosition(ScrollViewer);
            ScrollViewer.ScrollToHorizontalOffset(hOff + dragStartPoint.X - currentMousePosition.X);
            ScrollViewer.ScrollToVerticalOffset(vOff + dragStartPoint.Y - currentMousePosition.Y);
            dragStartPoint = currentMousePosition;
        }

        #region Z-Index Actions

        internal void NodePositionBackward(NodeModel node)
        {
            if (node == null) return;
            var smallerThanLast = Nodes.FirstOrDefault(n => n.ZIndex < node.ZIndex);
            if (smallerThanLast == null)
            {
                node.ZIndex--;
                return;
            }
            node.ZIndex = smallerThanLast.ZIndex - 1;
        }

        internal void NodePositionForward(NodeModel node)
        {
            if (node == null) return;
            var largerThanFirst = Nodes.FirstOrDefault(n => n.ZIndex > node.ZIndex);
            if (largerThanFirst == null)
            {
                node.ZIndex++;
                return;
            }
            node.ZIndex = largerThanFirst.ZIndex + 1;
        }

        internal void NodePositionToBack(NodeModel node)
        {
            if (node == null) return;
            var max = Nodes.Min(n => n.ZIndex);
            node.ZIndex = max - 1;
        }

        internal void NodePositionToFront(NodeModel node)
        {
            if (node == null) return;
            var max = Nodes.Max(n => n.ZIndex);
            node.ZIndex = max + 1;
        }

        internal void DeleteNode(NodeModel setting)
        {
            if(setting == null) return;
            Nodes.Remove(setting);
            var connections = Connections.Where(c => c.FromPoint.Parent == setting || c.ToPoint.Parent == setting).ToList();
            foreach (var connection in connections)
            {
                connection.FromPoint.IsSelected = false;
                connection.ToPoint.IsSelected = false;
                Connections.Remove(connection);
            }
        }

        #endregion Z-Index Actions
    }
}