using NodeEditor.Helpers;
using NodeEditor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class NodeEditorControlContainer : Control
    {
        public static readonly DependencyProperty ArrowsProperty =
            DependencyProperty.Register(nameof(Arrows), typeof(ObservableCollection<ConnectionLineModel>), typeof(NodeEditorControlContainer), new PropertyMetadata(null));

        public static readonly DependencyProperty ConnectionsProperty =
                    DependencyProperty.Register(nameof(Connections), typeof(ObservableCollection<ConnectionInfo>), typeof(NodeEditorControlContainer), new PropertyMetadata(new ObservableCollection<ConnectionInfo>()));

        public static readonly DependencyProperty CurrentMouseXProperty =
            DependencyProperty.Register(nameof(CurrentMouseX), typeof(int), typeof(NodeEditorControlContainer), new PropertyMetadata(0));

        public static readonly DependencyProperty CurrentMouseYProperty =
            DependencyProperty.Register(nameof(CurrentMouseY), typeof(int), typeof(NodeEditorControlContainer), new PropertyMetadata(0));

        public static readonly DependencyProperty NodesProperty =
                                    DependencyProperty.Register(nameof(Nodes), typeof(ObservableCollection<NodeModel>), typeof(NodeEditorControlContainer), new PropertyMetadata(null));

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

        public ObservableCollection<NodeModel> Nodes
        {
            get { return (ObservableCollection<NodeModel>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        #region Brushs

        public static readonly DependencyProperty DangerBrushProperty =
                    DependencyProperty.Register(nameof(DangerBrush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#dc3545")));

        public static readonly DependencyProperty DefaultCornerRadiusProperty =
            DependencyProperty.Register(nameof(DefaultCornerRadius), typeof(CornerRadius), typeof(NodeEditorControlContainer), new PropertyMetadata(new CornerRadius(8)));

        public static readonly DependencyProperty PrimaryBushProperty =
                    DependencyProperty.Register(nameof(PrimaryBush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#007bff")));

        public static readonly DependencyProperty RegionBrushProperty =
            DependencyProperty.Register(nameof(RegionBrush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#424242")));

        public static readonly DependencyProperty SecondaryRegionBrushProperty =
            DependencyProperty.Register(nameof(SecondaryRegionBrush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#2a2a2a")));

        public static readonly DependencyProperty SuccessBrushProperty =
                            DependencyProperty.Register(nameof(SuccessBrush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#28a745")));

        public static readonly DependencyProperty TextBrushProperty =
            DependencyProperty.Register(nameof(TextBrush), typeof(Brush), typeof(NodeEditorControlContainer), new PropertyMetadata(CBrushConverter.Convert("#FFFFFF")));

        public Brush DangerBrush
        {
            get { return (Brush)GetValue(DangerBrushProperty); }
            set { SetValue(DangerBrushProperty, value); }
        }

        public CornerRadius DefaultCornerRadius
        {
            get { return (CornerRadius)GetValue(DefaultCornerRadiusProperty); }
            set { SetValue(DefaultCornerRadiusProperty, value); }
        }

        public Brush PrimaryBush
        {
            get { return (Brush)GetValue(PrimaryBushProperty); }
            set { SetValue(PrimaryBushProperty, value); }
        }

        public Brush RegionBrush
        {
            get { return (Brush)GetValue(RegionBrushProperty); }
            set { SetValue(RegionBrushProperty, value); }
        }

        public Brush SecondaryRegionBrush
        {
            get { return (Brush)GetValue(SecondaryRegionBrushProperty); }
            set { SetValue(SecondaryRegionBrushProperty, value); }
        }

        public Brush SuccessBrush
        {
            get { return (Brush)GetValue(SuccessBrushProperty); }
            set { SetValue(SuccessBrushProperty, value); }
        }

        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            set { SetValue(TextBrushProperty, value); }
        }

        #endregion Brushs
    }
}