using NodeEditor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeEditor.Controls
{
    public class WidgetControl : Control
    {
        public static readonly DependencyProperty NodesProperty =
            DependencyProperty.Register(nameof(Nodes), typeof(ObservableCollection<NodeModel>), typeof(WidgetControl), new PropertyMetadata(new ObservableCollection<NodeModel>()));

        private ListBox listBox;

        private Point startPoint;

        public ObservableCollection<NodeModel> Nodes
        {
            get { return (ObservableCollection<NodeModel>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            listBox = GetTemplateChild("PART_ListBox") as ListBox;
            if (listBox == null) return;
            listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            listBox.MouseMove += ListBox_MouseMove;
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(listBox);
                Vector diff = currentPoint - startPoint;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Start the drag-and-drop operation
                    DataObject data = new DataObject(nameof(NodeModel), listBox.SelectedItem);
                    DragDrop.DoDragDrop(listBox, data, DragDropEffects.Move);
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(listBox);
        }
    }
}