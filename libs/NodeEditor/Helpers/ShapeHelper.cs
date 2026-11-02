using System.Windows;

namespace NodeEditor.Helpers
{
    internal class ShapeHelper
    {
        public static bool IsLineNotIntersectingRect(Point p1, Point p2, Rect rect)
        {
            // Kiểm tra xem đoạn thẳng p1-p2 có cắt rectangle không
            // Trả về true nếu KHÔNG cắt, false nếu CÓ cắt

            // Nếu cả 2 điểm nằm trong rect => cắt
            if (rect.Contains(p1) && rect.Contains(p2))
                return false;

            // 4 cạnh của rect
            var topLeft = new Point(rect.Left, rect.Top);
            var topRight = new Point(rect.Right, rect.Top);
            var bottomLeft = new Point(rect.Left, rect.Bottom);
            var bottomRight = new Point(rect.Right, rect.Bottom);

            // Kiểm tra đoạn p1-p2 có giao với bất kỳ cạnh nào không
            if (SegmentsIntersect(p1, p2, topLeft, topRight)) return false; // top
            if (SegmentsIntersect(p1, p2, bottomLeft, bottomRight)) return false; // bottom
            if (SegmentsIntersect(p1, p2, topLeft, bottomLeft)) return false; // left
            if (SegmentsIntersect(p1, p2, topRight, bottomRight)) return false; // right

            return true;
        }

        public static Rect GetRect(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p1.X - p2.X);
            double height = Math.Abs(p1.Y - p2.Y);
            return new Rect(x, y, width, height);
        }

        public static bool IsCrossNode(Point p1, Point p2, Rect node)
        {
            var rect = GetRect(p1, p2);
            if (node.X >= rect.X && node.X <= rect.X + rect.Width && node.Y >= rect.Y && node.Y <= rect.Y + rect.Height)
            {
                return true;
            }
            if (node.X + node.Width >= rect.X && node.X + node.Width <= rect.X + rect.Width || node.Y + node.Height>= rect.Y && node.Y <= rect.Y + rect.Height)
            {
                return true;
            }
            return false;
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Kiểm tra 2 đoạn thẳng AB và CD có giao nhau không (bao gồm đầu mút).
        /// </summary>
        private static bool SegmentsIntersect(Point a, Point b, Point c, Point d)
        {
            double d1 = Cross(c, d, a);
            double d2 = Cross(c, d, b);
            double d3 = Cross(a, b, c);
            double d4 = Cross(a, b, d);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            // Kiểm tra trường hợp collinear (điểm nằm trên đoạn kia)
            if (d1 == 0 && OnSegment(c, d, a)) return true;
            if (d2 == 0 && OnSegment(c, d, b)) return true;
            if (d3 == 0 && OnSegment(a, b, c)) return true;
            if (d4 == 0 && OnSegment(a, b, d)) return true;

            return false;
        }

        /// <summary>
        /// Cross product của vector (p1→p2) x (p1→p).
        /// </summary>
        private static double Cross(Point p1, Point p2, Point p)
        {
            return (p2.X - p1.X) * (p.Y - p1.Y)
                 - (p2.Y - p1.Y) * (p.X - p1.X);
        }

        /// <summary>
        /// Kiểm tra điểm p có nằm trên đoạn AB không (giả sử A, B, p thẳng hàng).
        /// </summary>
        private static bool OnSegment(Point a, Point b, Point p)
        {
            return Math.Min(a.X, b.X) <= p.X && p.X <= Math.Max(a.X, b.X) &&
                   Math.Min(a.Y, b.Y) <= p.Y && p.Y <= Math.Max(a.Y, b.Y);
        }
    }
}