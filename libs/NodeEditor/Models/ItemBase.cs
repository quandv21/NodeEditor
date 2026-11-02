using NodeEditor.Helpers;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NodeEditor.Models
{
    public enum ResultType
    {
        OK,
        NG,
        NONE
    }

    public class ItemBase : BindableBase, ICloneable, ISerializable
    {
        private double duration;
        private bool isRunning;
        private ResultType result = ResultType.NONE;
        private double x;

        private double y;
        public ItemBase(SerializationInfo info, StreamingContext context)
        {
            X = info.GetDouble(nameof(X));
            Y = info.GetDouble(nameof(Y));
            
        }
        public ItemBase()
        {
        }

        public double Duration
        {
            get { return duration; }
            set { SetProperty(ref duration, value); }
        }

        public bool IsRunning
        {
            get { return isRunning; }
            set { SetProperty(ref isRunning, value); }
        }

        public ResultType Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }

        public double X
        {
            get { return x; }
            set
            {
                SetProperty(ref x, Math.Round(value, 2));
                PositionChanged();
            }
        }

        public double Y
        {
            get { return y; }

            set
            {
                SetProperty(ref y, Math.Round(value, 2));
                PositionChanged();
            }
        }

       
        public virtual object Clone()
        {
            return this.DeepClone();
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(X), X);
            info.AddValue(nameof(Y), Y);
        }
        
        protected virtual void PositionChanged()
        {
        }
    }
}