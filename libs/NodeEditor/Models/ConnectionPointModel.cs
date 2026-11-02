using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace NodeEditor.Models
{
    public enum ConnectionPointPosition
    {
        [Description("Left Center")]
        LC,

        [Description("TOP CENTER")]
        TC,

        [Description("BOTTOM CENTER")]
        BC,

        [Description("RIGHT CENTER")]
        RC,
    }
    [Serializable]
    public class ConnectionPointModel : BindableBase, ISerializable
    {
        private const int MARGIN = 20;
        private bool isConnected = false;
        private bool isMouseOver;
        private bool isSelected = false;
        private ConnectionPointPosition position;
        private int size;
        private double x;

        private double y;

        public ConnectionPointModel()
        {
        }

        public ConnectionPointModel(int width, int height, int xP, int yP, ConnectionPointPosition pos)
        {
            Size = 10;
            Position = pos;
            Calculate(width, height, xP, yP);
        }
        public ConnectionPointModel(SerializationInfo info, StreamingContext context)
        {
            X = info.GetDouble(nameof(X));
            Y = info.GetDouble(nameof(Y));
            Size = info.GetInt32(nameof(Size));
            Position = (ConnectionPointPosition)info.GetValue(nameof(Position), typeof(ConnectionPointPosition));
            Parent = info.GetValue(nameof(Parent), typeof(NodeModel)) as NodeModel;
        }
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }

        public bool IsMouseOver
        {
            get { return isMouseOver; }
            set { SetProperty(ref isMouseOver, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public NodeModel Parent { get; set; }
        public ConnectionPointPosition Position { get => position; set => SetProperty(ref position, value); }

        public int Size
        {
            get { return size; }
            set { SetProperty(ref size, value); }
        }

        public double X
        {
            get { return x; }
            set { SetProperty(ref x, value); }
        }

        public double Y
        {
            get { return y; }
            set { SetProperty(ref y, value); }
        }

        public void Calculate(int width, int height, int xP, int yP)
        {
            switch (position)
            {
                case ConnectionPointPosition.LC:
                    X = xP - MARGIN + Size / 2;
                    Y = yP + height / 2;
                    return;

                case ConnectionPointPosition.TC:
                    X = xP + width / 2;
                    Y = yP - MARGIN + Size / 2;
                    return;

                case ConnectionPointPosition.BC:
                    X = xP + width / 2;
                    Y = height + yP + MARGIN - Size / 2;
                    return;

                case ConnectionPointPosition.RC:
                    X = xP + width + MARGIN - Size / 2;
                    Y = yP + height / 2;
                    return;

                default:
                    return;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Size), Size);
            info.AddValue(nameof(X), X);
            info.AddValue(nameof(Y), Y);
            info.AddValue(nameof(Position), Position);
            info.AddValue(nameof(Parent), Parent);
        }

        public bool MouseDropOnConnectionPoint(Point currentPos)
        {
            IsMouseOver = false;
            var startPoint = new Point((int)X - Size / 2, (int)Y - Size / 2);
            var endPoint = new Point((int)(X + Size / 2), (int)(Y + Size / 2));
            if (currentPos.X >= startPoint.X && currentPos.X <= endPoint.X &&
                currentPos.Y >= startPoint.Y && currentPos.Y <= endPoint.Y)
            {
                IsConnected = true;
                return true;
            }
            return false;
        }

        public void MouseOnConnectionPoint(Point currentPos)
        {
            var startPoint = new Point((int)X - Size / 2, (int)Y - Size / 2);
            var endPoint = new Point((int)(X + Size / 2), (int)(Y + Size / 2));
            if (currentPos.X >= startPoint.X && currentPos.X <= endPoint.X &&
                currentPos.Y >= startPoint.Y && currentPos.Y <= endPoint.Y)
            {
                Debug.WriteLine($"Mouse on connection point {position}");
                IsMouseOver = true;
                return;
            }
            IsMouseOver = false;
        }

        public void RefreshMouseOver()
        {
            IsMouseOver = false;
        }
    }
}