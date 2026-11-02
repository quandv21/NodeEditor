using NodeEditor.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeEditor.Controls
{
    public class NodeControl : Control
    {
        public static RoutedCommand DuplicateCommand = new();
        public static RoutedCommand DeleteCommand = new();
        public static RoutedCommand PositionForward = new();
        public static RoutedCommand PositionBackward = new();
        public static RoutedCommand PositionToFront = new();
        public static RoutedCommand PositionToBack = new();
        public static RoutedCommand DisableNode = new();
        public static RoutedCommand EnableNode = new();
        public static readonly DependencyProperty EditorViewProperty =
            DependencyProperty.Register(nameof(EditorView), typeof(NodeEditorViewControl), typeof(NodeControl), new PropertyMetadata(null));


        public static readonly DependencyProperty SettingProperty =
                    DependencyProperty.Register(nameof(Setting), typeof(NodeModel), typeof(NodeControl), new PropertyMetadata(null));

        private bool isDragging = false;

        private Border? PART_Border = null!;

        private Point startPoint;
        public NodeControl()
        {
            CommandBindings.Add(new CommandBinding(DuplicateCommand,OnDuplicate));
            CommandBindings.Add(new CommandBinding(DeleteCommand, OnDelete));
            CommandBindings.Add(new CommandBinding(PositionForward, OnForward));
            CommandBindings.Add(new CommandBinding(PositionBackward, OnBackward));
            CommandBindings.Add(new CommandBinding(PositionToFront, OnPosToFront));
            CommandBindings.Add(new CommandBinding(PositionToBack, OnPosToBack));
            CommandBindings.Add(new CommandBinding(DisableNode, OnDisable));
            CommandBindings.Add(new CommandBinding(EnableNode, OnEnable));
        }

        private void OnPosToBack(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditorView == null) return;
            EditorView.NodePositionToBack(Setting);
        }

        private void OnEnable(object sender, ExecutedRoutedEventArgs e)
        {
            if (Setting == null) return;
            Setting.IsEnabled = true;
        }

        private void OnDisable(object sender, ExecutedRoutedEventArgs e)
        {
            if (Setting == null) return;
            Setting.IsEnabled = false;
        }

        private void OnPosToFront(object sender, ExecutedRoutedEventArgs e)
        {
            if(EditorView == null) return;
            EditorView.NodePositionToFront(Setting);
        }

        private void OnBackward(object sender, ExecutedRoutedEventArgs e)
        {
            if(EditorView == null) return;
            EditorView.NodePositionBackward(Setting);
        }

        private void OnForward(object sender, ExecutedRoutedEventArgs e)
        {

            if (EditorView == null) return;
            EditorView.NodePositionForward(Setting);
        }

        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditorView == null) return;
            EditorView.DeleteNode(Setting);
        }

        private void OnDuplicate(object sender, ExecutedRoutedEventArgs e)
        {
        }

        public NodeEditorViewControl EditorView
        {
            get { return (NodeEditorViewControl)GetValue(EditorViewProperty); }
            set { SetValue(EditorViewProperty, value); }
        }

        public NodeModel Setting
        {
            get { return (NodeModel)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_Border = GetTemplateChild("Part_MainBorder") as Border;
            if (PART_Border == null)
            {
                return;
            }
            PART_Border.MouseMove += OnMouseMove;
            PART_Border.MouseUp += OnMouseUp;
            PART_Border.MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PART_Border == null) return;
            if (EditorView == null) return;
            e.Handled = true;
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                EditorView.AddSelectedNode(Setting);
                 Focus();
                return;
            }
            EditorView.RefreshSelectionNode();
            EditorView.RefreshSelectedConnection();
            EditorView.SelectedSingleNode(Setting);
            isDragging = true;
            startPoint = e.GetPosition(EditorView);
            PART_Border.CaptureMouse();
            Focus();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Setting.IsMouseOver = false;
            base.OnMouseLeave(e);
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Setting.IsMouseOver = true;
            if (e.LeftButton == MouseButtonState.Pressed && isDragging)
            {
                Point currentPosition = e.GetPosition(EditorView);
                var detalPoint = currentPosition - startPoint;
                Setting.X += detalPoint.X;
                Setting.Y += detalPoint.Y;
                startPoint = currentPosition;
            }
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if (sender is Border border)
            {
                border.ReleaseMouseCapture();
                Debug.WriteLine("NodeControl release capture mouse");
            }
        }
    }
}