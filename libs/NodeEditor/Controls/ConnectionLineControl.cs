using NodeEditor.Contants;
using NodeEditor.Helpers;
using NodeEditor.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls
{
    public class ConnectionLineControl : Control
    {
        public static readonly DependencyProperty ConnectionProperty =
            DependencyProperty.Register(nameof(Connection), typeof(ConnectionInfo), typeof(ConnectionLineControl), new PropertyMetadata(null));

        public static readonly DependencyProperty EditorViewProperty =
            DependencyProperty.Register(nameof(EditorView), typeof(NodeEditorViewControl), typeof(ConnectionLineControl), new PropertyMetadata(null));

        public static readonly DependencyProperty EndXProperty =
                            DependencyProperty.Register(nameof(EndX), typeof(double), typeof(ConnectionLineControl), new PropertyMetadata(0.0, OnPositionChanged));

        public static readonly DependencyProperty EndYProperty =
            DependencyProperty.Register(nameof(EndY), typeof(double), typeof(ConnectionLineControl), new PropertyMetadata(0.0, OnPositionChanged));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ConnectionLineControl), new PropertyMetadata(false, OnSelectedChanged));

        public static readonly DependencyProperty StartXProperty =
                                            DependencyProperty.Register(nameof(StartX), typeof(double), typeof(ConnectionLineControl), new PropertyMetadata(0.0, OnPositionChanged));

        public static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register(nameof(StartY), typeof(double), typeof(ConnectionLineControl), new PropertyMetadata(0.0, OnPositionChanged));

        public static RoutedCommand DeleteLineCommand = new();

        private Point _arrowP1, _arrowP2;

        private PathGeometry _geometry;

        private Pen _pen;

        private bool isLoaded = false;

        // --- manual reshaping (draw.io style) ---
        private List<Point> _chain = new();   // [startAnchor, ...bends, endAnchor] as currently drawn
        private bool _isReshaping;
        private int _dragIndex;
        private bool _dragMoveY;
        private Point _dragStartMouse;
        private List<Point> _dragBase;
        private bool _dragExempt;
        private Rect _boxA, _boxB;            // margin boxes of the start / end node (obstacles)

        public ConnectionLineControl()
        {
            CommandBindings.Add(new CommandBinding(DeleteLineCommand, OnDelete));
            DeleteLineCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            Loaded += ConnectionLineControl_Loaded;
        }

        public ConnectionInfo Connection
        {
            get { return (ConnectionInfo)GetValue(ConnectionProperty); }
            set { SetValue(ConnectionProperty, value); }
        }

        public NodeEditorViewControl EditorView
        {
            get { return (NodeEditorViewControl)GetValue(EditorViewProperty); }
            set { SetValue(EditorViewProperty, value); }
        }

        public double EndX
        {
            get { return (double)GetValue(EndXProperty); }
            set { SetValue(EndXProperty, value); }
        }

        public double EndY
        {
            get { return (double)GetValue(EndYProperty); }
            set { SetValue(EndYProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public ConnectionPointPosition Position { get; set; }

        public double StartX
        {
            get { return (double)GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }

        public double StartY
        {
            get { return (double)GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }

        private Point EndPoint => new(EndX, EndY);

        private Point StartPoint => new(StartX, StartY);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);

            // Double-click: drop the manual shape and go back to auto routing.
            if (e.ClickCount == 2)
            {
                Connection.ManualPath = null;
                ReBuildGeometry();
                IsSelected = true;
                e.Handled = true;
                return;
            }

            // When selected, a handle in the middle of a segment starts a reshape drag.
            if (IsSelected)
            {
                foreach (var h in GetHandles())
                {
                    if (!h.rect.Contains(pos)) continue;
                    _isReshaping = true;
                    _dragIndex = h.index;
                    _dragMoveY = h.moveY;
                    _dragStartMouse = pos;
                    _dragBase = new List<Point>(_chain);
                    _dragExempt = !IsChainClear(_dragBase, false);
                    CaptureMouse();
                    e.Handled = true;
                    return;
                }
            }

            IsSelected = true;
            InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);

            if (_isReshaping)
            {
                double d = Math.Round(_dragMoveY ? pos.Y - _dragStartMouse.Y : pos.X - _dragStartMouse.X);
                var ch = DragSegmentAvoiding(_dragBase, _dragIndex, _dragMoveY, d);
                Connection.ManualPath = ch; // saved on the model: [startAnchor, ...bends, endAnchor]
                ReBuildGeometry();
                return;
            }

            Cursor = null;
            if (IsSelected)
            {
                foreach (var h in GetHandles())
                {
                    if (h.rect.Contains(pos)) { Cursor = h.moveY ? Cursors.SizeNS : Cursors.SizeWE; break; }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isReshaping)
            {
                _isReshaping = false;
                ReleaseMouseCapture();
                e.Handled = true;
                return;
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_geometry == null)
                return;
            if (_pen == null)
                _pen = new Pen(Background, 2)
                {
                };
            _pen.Brush = Background;
            _pen.Thickness = IsSelected ? 3 : 2;
            if (IsSelected)
            {
                dc.PushOpacity(0.18);
                dc.DrawGeometry(null, new Pen(Background, 10) { LineJoin = PenLineJoin.Round }, _geometry);
                dc.Pop();
            }
            dc.DrawGeometry(Brushes.Transparent, _pen, _geometry);
            ToolTip = $"Start: ({StartX} , {StartY}) - End: ({EndX}, {EndY})";
            StreamGeometry arrow = new StreamGeometry();
            using (var ctx = arrow.Open())
            {
                ctx.BeginFigure(EndPoint, true, true);
                ctx.LineTo(_arrowP1, true, true);
                ctx.LineTo(_arrowP2, true, true);
            }
            dc.DrawGeometry(Background, null, arrow);

            if (IsSelected)
            {
                var handlePen = new Pen(Background, 1.5);
                foreach (var h in GetHandles())
                    dc.DrawRectangle(Brushes.White, handlePen, h.rect);
            }
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLineControl control)
            {
                control.ReBuildGeometry();
            }
        }

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ConnectionLineControl)d;
            control?.InvalidateVisual();
            control?.Focus();
        }

        /// <summary>
        /// bend/endPoint define the direction; isHor is only a fallback for the
        /// degenerate case where bend == endPoint (zero-length final segment).
        /// </summary>
        private void BuildArrowHead(Point bend, Point endPoint, bool isHor)
        {
            Vector dir = endPoint - bend;
            if (dir == default(Vector))
            {
                var x = isHor ? 1 : 0;
                var y = isHor ? 0 : 1;
                dir = new Vector(x, y);
            }
            dir.Normalize();

            Vector normal = new Vector(-dir.Y, dir.X);

            double length = 12;
            double width = 6;

            _arrowP1 = endPoint - dir * length + normal * width;
            _arrowP2 = endPoint - dir * length - normal * width;
        }

        private void ConnectionLineControl_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            ReBuildGeometry();
        }

        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            EditorView.DeleteConnectionLine(Connection);
        }

        #region Routing

        private const double HitTestTolerance = 5.0;

        // Dominates any realistic coordinate delta, so "fewest bends" always
        // wins over "shortest path" in the Dijkstra search below.
        private const double TurnPenalty = 1e4;

        public void ReBuildGeometry()
        {
            if (Connection == null) return;
            if (!isLoaded) return;

            var fromPt = Connection.FromPoint;
            var toPt = Connection.ToPoint;
            var fromNode = fromPt.Parent;
            var toNode = toPt.Parent;
            double nodeW = EditorView.NodeWidth;
            double nodeH = EditorView.NodeHeight;
            double dotOffset = CSize.DISTANCE_BETWEEN_NODE_AND_CONNECTION_POINTS;

            // Pair StartPoint/EndPoint with the right node + port by GEOMETRY, not by
            // trusting the From/To labels: for some connections the model's From/To
            // are reversed relative to StartX/EndX (e.g. created in the opposite
            // drag direction). Using the wrong pairing gives the wrong port side and
            // the wrong margin box for each anchor, so the route goes through nodes.
            double straightErr =
                Dist(StartPoint, ExpectedPortLocation(fromNode.X, fromNode.Y, nodeW, nodeH, fromPt.Position, dotOffset)) +
                Dist(EndPoint, ExpectedPortLocation(toNode.X, toNode.Y, nodeW, nodeH, toPt.Position, dotOffset));
            double swappedErr =
                Dist(StartPoint, ExpectedPortLocation(toNode.X, toNode.Y, nodeW, nodeH, toPt.Position, dotOffset)) +
                Dist(EndPoint, ExpectedPortLocation(fromNode.X, fromNode.Y, nodeW, nodeH, fromPt.Position, dotOffset));
            bool swap = swappedErr < straightErr;

            var startNode = swap ? toNode : fromNode;
            var endNode = swap ? fromNode : toNode;
            var startPos = swap ? toPt.Position : fromPt.Position;
            var endPos = swap ? fromPt.Position : toPt.Position;

            var boxA = GetBoundingBoxOfNode(startNode.X, startNode.Y, nodeW, nodeH, CSize.NODE_MARGIN);
            var boxB = GetBoundingBoxOfNode(endNode.X, endNode.Y, nodeW, nodeH, CSize.NODE_MARGIN);

            // Same as the HTML preview: the routing anchor sits right outside its
            // OWN node's margin box (not "connection point + fixed distance").
            _boxA = boxA;
            _boxB = boxB;
            var newStart = CalculateAnchor(StartPoint, startPos, boxA);
            var newEnd = CalculateAnchor(EndPoint, endPos, boxB);

            var routed = RouteGrid(newStart, newEnd, boxA, boxB);

            // Auto route, or the user's waypoints between the two fixed anchors.
            // Saved manual shape: keep it when both anchors moved by the SAME amount (or not at
            // all), translating it along. That is what happens when several selected nodes are
            // dragged together, so the line keeps its shape. If only one end moved (or the
            // anchors changed differently), fall back to auto routing. The saved path is never
            // cleared here, so partial updates while the control loads or while the two ends
            // update one after the other can't destroy it.
            List<Point> chain = null;
            var saved = Connection.ManualPath;
            if (saved != null && saved.Count >= 2)
            {
                var dS = newStart - saved[0];
                var dE = newEnd - saved[^1];
                if (Math.Abs(dS.X - dE.X) < 0.5 && Math.Abs(dS.Y - dE.Y) < 0.5)
                {
                    if (Math.Abs(dS.X) > 0.01 || Math.Abs(dS.Y) > 0.01)
                    {
                        saved = saved.Select(pp => pp + dS).ToList();
                        Connection.ManualPath = saved; // keep the model in sync with the moved shape
                    }
                    chain = new List<Point>(saved);
                    chain[0] = newStart;
                    chain[^1] = newEnd;
                }
            }

            // routed == null should not happen (the grid always has an outer route around
            // both boxes); fall back to a direct anchor-to-anchor segment.
            chain ??= routed ?? new List<Point> { newStart, newEnd };
            _chain = chain;

            var figure = new PathFigure() { StartPoint = StartPoint };
            foreach (var pt in chain)
            {
                figure.Segments.Add(new LineSegment(pt, true));
            }
            figure.Segments.Add(new LineSegment(EndPoint, true));

            _geometry = new PathGeometry();
            _geometry.Figures.Add(figure);

            // The final segment's direction is fixed by the target port and is
            // always known directly - no need to derive it from the computed
            // path, and it is never zero-length because the anchor lies outside the margin box.
            BuildArrowHead(newEnd, EndPoint, endPos is ConnectionPointPosition.LC or ConnectionPointPosition.RC);

            InvalidateVisual();
        }

        // --- Grid-based orthogonal router ---------------------------------
        // Rectilinear shortest-path theory: bends only ever need to sit on
        // coordinates aligned with an obstacle's edges. So instead of a
        // greedy step-by-step direction search, we build the small set of
        // X/Y lines defined by both node boxes (+ margin) plus the stub
        // anchor points, form a sparse visibility grid from them, and search
        // it exactly with Dijkstra (state = grid point + incoming direction,
        // so turning can be penalized). Every candidate grid edge is
        // collision-checked directly against both boxes before being used -
        // there is no "impossible direction" bookkeeping, so overshoot,
        // wrong-axis termination, and cutting through a node cannot occur.
        private const double GridEps = 0.01;

        private static bool RectBlocksH(double x1, double x2, double y, Rect r)
        {
            return (y > r.Y + GridEps && y < r.Y + r.Height - GridEps)
                && (Math.Max(x1, x2) > r.X + GridEps && Math.Min(x1, x2) < r.X + r.Width - GridEps);
        }

        private static bool RectBlocksV(double x, double y1, double y2, Rect r)
        {
            return (x > r.X + GridEps && x < r.X + r.Width - GridEps)
                && (Math.Max(y1, y2) > r.Y + GridEps && Math.Min(y1, y2) < r.Y + r.Height - GridEps);
        }

        /// <summary>
        /// Returns the sequence of points from <paramref name="start"/> to
        /// <paramref name="end"/> (inclusive), with interior points being the
        /// actual bend/turn vertices, or null if genuinely no path exists.
        /// </summary>
        private static List<Point> RouteGrid(Point start, Point end, Rect boxA, Rect boxB)
        {
            var xs = new List<double> { boxA.X, boxA.X + boxA.Width, boxB.X, boxB.X + boxB.Width, start.X, end.X }
                .Distinct().OrderBy(v => v).ToList();
            var ys = new List<double> { boxA.Y, boxA.Y + boxA.Height, boxB.Y, boxB.Y + boxB.Height, start.Y, end.Y }
                .Distinct().OrderBy(v => v).ToList();

            int W = xs.Count, H = ys.Count;
            int si = xs.IndexOf(start.X), sj = ys.IndexOf(start.Y);
            int ei = xs.IndexOf(end.X), ej = ys.IndexOf(end.Y);

            bool BlockedH(int i, int j) => RectBlocksH(xs[i], xs[i + 1], ys[j], boxA) || RectBlocksH(xs[i], xs[i + 1], ys[j], boxB);
            bool BlockedV(int i, int j) => RectBlocksV(xs[i], ys[j], ys[j + 1], boxA) || RectBlocksV(xs[i], ys[j], ys[j + 1], boxB);

            // Never block an edge that touches the start or end anchor
            // directly - the path must always be able to leave `start` and
            // arrive at `end`, even if that point happens to sit inside the
            // OTHER node's margin box because the two nodes are close
            // together or overlapping. Every other edge is still fully
            // collision-checked as normal.
            bool IsAnchor(int i, int j) => (i == si && j == sj) || (i == ei && j == ej);
            bool EdgeBlockedH(int i, int j) => (IsAnchor(i, j) || IsAnchor(i + 1, j)) ? false : BlockedH(i, j);
            bool EdgeBlockedV(int i, int j) => (IsAnchor(i, j) || IsAnchor(i, j + 1)) ? false : BlockedV(i, j);

            static string Key(int i, int j, char dir) => i + "_" + j + "_" + dir;

            var dist = new Dictionary<string, double>();
            var prev = new Dictionary<string, string>();
            var startKey = Key(si, sj, 'N'); // 'N' = no incoming direction yet
            dist[startKey] = 0;

            int seq = 0; // insertion order = tie-breaker, matches JS stable sort
            var frontier = new List<(int i, int j, char dir, double cost, int seq)> { (si, sj, 'N', 0, seq++) };

            IEnumerable<(int ni, int nj, char dir, double w)> Neighbors(int i, int j)
            {
                if (i > 0 && !EdgeBlockedH(i - 1, j)) yield return (i - 1, j, 'H', xs[i] - xs[i - 1]);
                if (i < W - 1 && !EdgeBlockedH(i, j)) yield return (i + 1, j, 'H', xs[i + 1] - xs[i]);
                if (j > 0 && !EdgeBlockedV(i, j - 1)) yield return (i, j - 1, 'V', ys[j] - ys[j - 1]);
                if (j < H - 1 && !EdgeBlockedV(i, j)) yield return (i, j + 1, 'V', ys[j + 1] - ys[j]);
            }

            while (frontier.Count > 0)
            {
                frontier.Sort((a, b) => { int c = a.cost.CompareTo(b.cost); return c != 0 ? c : a.seq.CompareTo(b.seq); });
                var cur = frontier[0];
                frontier.RemoveAt(0);

                var ck = Key(cur.i, cur.j, cur.dir);
                if (dist.TryGetValue(ck, out var curBest) && curBest < cur.cost) continue; // stale entry

                foreach (var nb in Neighbors(cur.i, cur.j))
                {
                    double turn = (cur.dir != 'N' && cur.dir != nb.dir) ? TurnPenalty : 0;
                    double nCost = cur.cost + nb.w + turn;
                    var nk = Key(nb.ni, nb.nj, nb.dir);
                    if (!dist.TryGetValue(nk, out var existing) || existing > nCost)
                    {
                        dist[nk] = nCost;
                        prev[nk] = ck;
                        frontier.Add((nb.ni, nb.nj, nb.dir, nCost, seq++));
                    }
                }
            }

            string bestKey = null;
            double bestCost = double.PositiveInfinity;
            foreach (var dir in new[] { 'H', 'V', 'N' })
            {
                var k = Key(ei, ej, dir);
                if (dist.TryGetValue(k, out var c) && c < bestCost) { bestCost = c; bestKey = k; }
            }
            if (bestKey == null) return null;

            var chain = new List<Point>();
            string curKey = bestKey;
            while (curKey != null)
            {
                var parts = curKey.Split('_');
                int i = int.Parse(parts[0]), j = int.Parse(parts[1]);
                chain.Add(new Point(xs[i], ys[j]));
                prev.TryGetValue(curKey, out curKey);
            }
            chain.Reverse();

            // Collapse collinear runs into actual turn vertices.
            var collapsed = new List<Point> { chain[0] };
            for (int n = 1; n < chain.Count - 1; n++)
            {
                var p0 = collapsed[^1];
                var p1 = chain[n];
                var p2 = chain[n + 1];
                bool sameH = p0.Y == p1.Y && p1.Y == p2.Y;
                bool sameV = p0.X == p1.X && p1.X == p2.X;
                if (!sameH && !sameV) collapsed.Add(p1);
            }
            collapsed.Add(chain[^1]);
            return collapsed;
        }

        /// <summary>
        /// Anchor = the point just outside the node's own margin box, on the
        /// port's axis (cross coordinate taken from the connection point).
        /// </summary>
        private static Point CalculateAnchor(Point port, ConnectionPointPosition pos, Rect ownBox)
        {
            const double clearance = 0.5;
            switch (pos)
            {
                case ConnectionPointPosition.LC:
                    return new(ownBox.X - clearance, port.Y);

                case ConnectionPointPosition.TC:
                    return new(port.X, ownBox.Y - clearance);

                case ConnectionPointPosition.BC:
                    return new(port.X, ownBox.Y + ownBox.Height + clearance);

                case ConnectionPointPosition.RC:
                    return new(ownBox.X + ownBox.Width + clearance, port.Y);

                default:
                    return port;
            }
        }

        private static double Dist(Point a, Point b) => (a - b).Length;

        /// <summary>Where a connection dot is expected to be for a node/side (used only to pair ends).</summary>
        private static Point ExpectedPortLocation(double x, double y, double w, double h, ConnectionPointPosition pos, double offset)
        {
            switch (pos)
            {
                case ConnectionPointPosition.LC: return new(x - offset, y + h / 2);
                case ConnectionPointPosition.RC: return new(x + w + offset, y + h / 2);
                case ConnectionPointPosition.TC: return new(x + w / 2, y - offset);
                case ConnectionPointPosition.BC: return new(x + w / 2, y + h + offset);
                default: return new(x + w / 2, y + h / 2);
            }
        }

        private Rect GetBoundingBoxOfNode(double x, double y, double nodeWidth, double nodeHeight, double margin = 0)
        {
            return new Rect(x - margin, y - margin, nodeWidth + margin * 2, nodeHeight + margin * 2);
        }

        private bool IsPointNearGeometry(Point point, double tolerance)
        {
            if (_geometry == null) return false;

            var widened = _geometry.GetWidenedPathGeometry(
                new Pen(Brushes.Black, tolerance * 2));

            return widened.FillContains(point);
        }

        // --- manual reshaping helpers ---------------------------------------

        private static bool Near(double a, double b) => Math.Abs(a - b) < 0.5;

        /// <summary>One handle per straight, long-enough segment (midpoint). moveY: horizontal segment => drag vertically.</summary>
        private List<(int index, Rect rect, bool moveY)> GetHandles()
        {
            var list = new List<(int index, Rect rect, bool moveY)>();
            for (int i = 0; i < _chain.Count - 1; i++)
            {
                var p = _chain[i];
                var q = _chain[i + 1];
                bool horiz = Near(p.Y, q.Y), vert = Near(p.X, q.X);
                if (horiz == vert) continue;          // zero-length or diagonal
                if ((p - q).Length < 14) continue;    // too short for a handle
                var mid = new Point((p.X + q.X) / 2, (p.Y + q.Y) / 2);
                list.Add((i, new Rect(mid.X - 5, mid.Y - 5, 10, 10), horiz));
            }
            return list;
        }

        private static List<Point> SimplifyChain(List<Point> ch)
        {
            var a = new List<Point> { ch[0] };
            for (int k = 1; k < ch.Count; k++)
            {
                var p = a[^1];
                var q = ch[k];
                if (!(Near(p.X, q.X) && Near(p.Y, q.Y))) a.Add(q);
                else if (k == ch.Count - 1 && a.Count > 1) a[^1] = q; // never drop the end anchor
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int k = 1; k < a.Count - 1; k++)
                {
                    var p = a[k - 1]; var q = a[k]; var r = a[k + 1];
                    if ((Near(p.X, q.X) && Near(q.X, r.X)) || (Near(p.Y, q.Y) && Near(q.Y, r.Y)))
                    {
                        a.RemoveAt(k);
                        changed = true;
                        break;
                    }
                }
            }
            return a;
        }

        /// <summary>
        /// Move segment i (ch[i]..ch[i+1]) perpendicular to itself by d. The two anchors
        /// never move: if the segment touches one, a shifted copy is inserted next to it
        /// so the exit/entry stays put and a new bend appears instead.
        /// </summary>
        private static List<Point> DragSegmentChain(List<Point> ch, int i, bool moveY, double d)
        {
            int n = ch.Count;
            Point Sh(Point pt) => moveY ? new Point(pt.X, pt.Y + d) : new Point(pt.X + d, pt.Y);
            var P = ch[i];
            var Q = ch[i + 1];
            bool pA = i == 0, qA = i == n - 2;
            var r = new List<Point>(ch);
            if (!pA && !qA) { r[i] = Sh(P); r[i + 1] = Sh(Q); }
            else if (pA && !qA) { r[i + 1] = Sh(Q); r.Insert(i + 1, Sh(P)); }
            else if (!pA && qA) { r[i] = Sh(P); r.Insert(i + 1, Sh(Q)); }
            else { r.Insert(1, Sh(Q)); r.Insert(1, Sh(P)); }
            return SimplifyChain(r);
        }

        /// <summary>True if no segment cuts through the interior of either node's margin box.</summary>
        private bool IsChainClear(List<Point> ch, bool exemptAnchorSegments)
        {
            for (int i = 0; i < ch.Count - 1; i++)
            {
                // Same rule as the grid router: edges touching an anchor may start inside
                // the other node's margin box (nodes very close together).
                if (exemptAnchorSegments && (i == 0 || i == ch.Count - 2)) continue;
                var p = ch[i];
                var q = ch[i + 1];
                if (Near(p.Y, q.Y))
                {
                    if (RectBlocksH(p.X, q.X, p.Y, _boxA) || RectBlocksH(p.X, q.X, p.Y, _boxB)) return false;
                }
                else if (Near(p.X, q.X))
                {
                    if (RectBlocksV(p.X, p.Y, q.Y, _boxA) || RectBlocksV(p.X, p.Y, q.Y, _boxB)) return false;
                }
                else return false; // diagonal: never valid
            }
            return true;
        }

        /// <summary>
        /// Like DragSegmentChain, but the segment can only end up where the whole path still
        /// avoids both nodes: picks the valid offset closest to the requested one (so it stops
        /// at a node's margin, or hops over it to the nearest free lane).
        /// </summary>
        private List<Point> DragSegmentAvoiding(List<Point> baseChain, int i, bool moveY, double d)
        {
            const int maxSearch = 1500;
            for (int k = 0; k <= maxSearch; k++)
            {
                var first = d - k;
                var second = d + k;
                if (Math.Abs(second) < Math.Abs(first)) (first, second) = (second, first);
                var ch = DragSegmentChain(baseChain, i, moveY, first);
                if (IsChainClear(ch, _dragExempt)) return ch;
                if (k == 0) continue;
                ch = DragSegmentChain(baseChain, i, moveY, second);
                if (IsChainClear(ch, _dragExempt)) return ch;
            }
            return baseChain; // nothing valid: leave the path as it was
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (_geometry == null) return null;

            var point = hitTestParameters.HitPoint;

            if (IsSelected)
            {
                foreach (var h in GetHandles())
                {
                    var r = h.rect;
                    r.Inflate(2, 2);
                    if (r.Contains(point)) return new PointHitTestResult(this, point);
                }
            }

            if (IsPointNearGeometry(point, HitTestTolerance))
                return new PointHitTestResult(this, point);

            return null;
        }

        #endregion Routing
    }
}