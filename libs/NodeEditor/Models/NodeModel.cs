using NodeEditor.Contants;
using System.Runtime.Serialization;
using System.Windows;

namespace NodeEditor.Models
{
    [Serializable]
    public class NodeModel : ItemBase
    {
        private string group;
        private bool isEnabled = true;
        private bool isMouseOver = false;
        private bool isSelected;
        private string title = "#NODE 1";

        private string tooltip = "#None";
        private int zIndex = 100;

        public NodeModel(double x, double y) : this()
        {
            Width = CSize.DEFAULT_NODE_WIDTH;
            Height = CSize.DEFAULT_NODE_HEIGHT;
            X = x;
            Y = y;
            InitConnectionPoint();
        }

        public NodeModel()
        {
            Childs = new();
            Fronts = new();
        }

        public NodeModel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Width = info.GetDouble(nameof(Width));
            Height = info.GetDouble(nameof(Height));
            IsEnabled = info.GetBoolean(nameof(IsEnabled));
            Title = info.GetString(nameof(Title));
            ToolTip = info.GetString(nameof(ToolTip));
            ZIndex = info.GetInt32(nameof(ZIndex));
            Fronts = (List<NodeModel>)info.GetValue(nameof(Fronts), typeof(List<NodeModel>));
            Childs = (List<NodeModel>)info.GetValue(nameof(Childs), typeof(List<NodeModel>));
            Group = info.GetString(nameof(Group));
            LC = info.GetValue(nameof(LC),typeof(ConnectionPointModel)) as ConnectionPointModel;
            CT = info.GetValue(nameof(CT),typeof(ConnectionPointModel)) as ConnectionPointModel;
            CB = info.GetValue(nameof(CB),typeof(ConnectionPointModel)) as ConnectionPointModel;
            RC = info.GetValue(nameof(RC),typeof(ConnectionPointModel)) as ConnectionPointModel;
        }

        public List<NodeModel> Childs { get; set; }

        public List<NodeModel> Fronts { get; set; }

        public string Group
        {
            get { return group; }
            set { SetProperty(ref group, value); }
        }

        public double Height { get; set; }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        public bool IsMouseOver { get => isMouseOver; set => SetProperty(ref isMouseOver, value); }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public Point StartPoint => new(X, Y);

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); ToolTip = GetTooltip(); }
        }

        public string ToolTip
        {
            get { return tooltip; }
            set { SetProperty(ref tooltip, value); }
        }

        public double Width { get; set; }
        public int ZIndex { get => zIndex; set => SetProperty(ref zIndex, value); }

        public void AddChild(NodeModel child)
        {
            Childs.Add(child);
        }

        public void AddFront(NodeModel front)
        {
            Fronts.AddRange(front.Fronts);
            Fronts.Add(front);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Width), Width);
            info.AddValue(nameof(Height), Height);
            info.AddValue(nameof(IsEnabled), IsEnabled);
            info.AddValue(nameof(Title), Title);
            info.AddValue(nameof(ToolTip), ToolTip);
            info.AddValue(nameof(Fronts), Fronts);
            info.AddValue(nameof(Childs), Childs);
            info.AddValue(nameof(ZIndex), ZIndex);
            info.AddValue(nameof(Group), Group);
            info.AddValue(nameof(LC), LC);
            info.AddValue(nameof(CT), CT);
            info.AddValue(nameof(RC), RC);
            info.AddValue(nameof(CB), CB);
        }

        public void RemoveChild(NodeModel child)
        {
            Childs.Remove(child);
        }

        public void RemoveFronts(NodeModel front)
        {
            Fronts.Remove(front);
            foreach (var item in front.Fronts)
            {
                Fronts.Remove(item);
            }
        }

        internal bool IsMouseOnNode(Point currentPos)
        {
            var startPoint = new Point((int)X - Contants.CSize.NODE_MARGIN, (int)Y - Contants.CSize.NODE_MARGIN);
            var endPoint = new Point((int)(X + Width + Contants.CSize.NODE_MARGIN), (int)(Y + Height + Contants.CSize.NODE_MARGIN));
            if (currentPos.X >= startPoint.X && currentPos.X <= endPoint.X &&
                currentPos.Y >= startPoint.Y && currentPos.Y <= endPoint.Y)
            {
                LC.MouseOnConnectionPoint(currentPos);
                CT.MouseOnConnectionPoint(currentPos);
                RC.MouseOnConnectionPoint(currentPos);
                CB.MouseOnConnectionPoint(currentPos);
                return true;
            }
            LC.RefreshMouseOver();
            CT.RefreshMouseOver();
            RC.RefreshMouseOver();
            CB.RefreshMouseOver();
            return false;
        }

        internal ConnectionPointModel SetConnected(Point currentPos)
        {
            if (LC.MouseDropOnConnectionPoint(currentPos)) return LC;
            if (CT.MouseDropOnConnectionPoint(currentPos)) return CT;
            if (RC.MouseDropOnConnectionPoint(currentPos)) return RC;
            if (CB.MouseDropOnConnectionPoint(currentPos)) return CB;
            return null;
        }

        protected override void PositionChanged()
        {
            UpdateConnectionPoint();
        }

        private string GetTooltip()
        {
            return $"Node: {Title}\nResult: {Result}";
        }

        private void InitConnectionPoint()
        {
            LC = new((int)Width, (int)Height, (int)X, (int)Y, ConnectionPointPosition.LC)
            {
                Parent = this,
            };
            CT = new((int)Width, (int)Height, (int)X, (int)Y, ConnectionPointPosition.TC)
            {
                Parent = this,
            };
            RC = new((int)Width, (int)Height, (int)X, (int)Y, ConnectionPointPosition.RC)
            {
                Parent = this,
            };
            CB = new((int)Width, (int)Height, (int)X, (int)Y, ConnectionPointPosition.BC)
            {
                Parent = this,
            };
        }

        private void UpdateConnectionPoint()
        {
            LC?.Calculate((int)Width, (int)Height, (int)X, (int)Y);
            CT?.Calculate((int)Width, (int)Height, (int)X, (int)Y);
            RC?.Calculate((int)Width, (int)Height, (int)X, (int)Y);
            CB?.Calculate((int)Width, (int)Height, (int)X, (int)Y);
        }

        #region ConnectionPoints

        /// <summary>
        /// Hori: Center - C
        /// Vert: Bottom - B
        /// </summary>
        public ConnectionPointModel CB { get; private set; }

        /// <summary>
        /// Hori: Center - C
        /// Vert: Top - T
        /// </summary>
        public ConnectionPointModel CT { get; private set; }

        /// <summary>
        /// Hori: Left - L
        /// Vert: Center - C
        /// </summary>
        public ConnectionPointModel LC { get; private set; }

        /// <summary>
        /// Hori: Right - R
        /// Vert: Center - C
        /// </summary>
        public ConnectionPointModel RC { get; private set; }

        #endregion ConnectionPoints
    }
}