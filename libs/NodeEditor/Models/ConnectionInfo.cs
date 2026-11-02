using System.Windows;

namespace NodeEditor.Models
{
    public enum ConnectionType
    {
        Input,
        Output
    }

    public class ConnectionInfo : BindableBase
    {
        private ConnectionPointModel fromPoint;
        private bool isSelected = false;
        private ConnectionPointModel toPoint;
        private ConnectionType type;

        public ConnectionInfo(ConnectionPointModel fromPoint, ConnectionPointModel toPoint)
        {
            FromPoint = fromPoint;
            ToPoint = toPoint;
        }
        private List<Point> manualPath;

        /// <summary>
        /// Hình dạng do người dùng chỉnh: [điểm neo đầu, ...các khúc cua, điểm neo cuối].
        /// null = tự động định tuyến.
        /// </summary>
        public List<Point> ManualPath { get => manualPath; set => SetProperty(ref manualPath, value); }
        public ConnectionPointModel FromPoint { get => fromPoint; set => SetProperty(ref fromPoint, value); }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); ToPoint.IsSelected = value; FromPoint.IsSelected = value; }
        }

        public ConnectionPointModel ToPoint { get => toPoint; set => SetProperty(ref toPoint, value); }
        public ConnectionType Type { get => type; set => SetProperty(ref type, value); }
        public void Destroy()
        {
            FromPoint.IsConnected = false;
            ToPoint.IsConnected = false;
            FromPoint.Parent.RemoveChild(ToPoint.Parent);
            ToPoint.Parent.RemoveFronts(FromPoint.Parent);
        }
    }
}