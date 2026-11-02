namespace NodeEditor.Models
{
    public class ConnectionLineModel : ItemBase
    {
        private double endX;
        private double endY;

        public double EndX
        {
            get { return endX; }
            set { SetProperty(ref endX, value); }
        }

        public double EndY
        {
            get { return endY; }
            set { SetProperty(ref endY, value); }
        }

        public ItemBase? FromItem { get; set; }
        public ItemBase? ToItem { get; set; }
    }
}