using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditor.Models;

namespace NodeEditor.Executions
{
    /// <summary>
    /// Thực thi flow theo thứ tự topological sort (Kahn's Algorithm),
    /// dựa trên adjacency thực tế trong NodeModel.Childs.
    /// </summary>
    public static class NodeGraphExecutor
    {
        /// <summary>
        /// Sắp xếp topo danh sách node. Ném exception nếu phát hiện chu trình.
        /// </summary>
        public static List<NodeModel> TopologicalSort(IEnumerable<NodeModel> allNodes)
        {
            var nodes = allNodes.Distinct().ToList();

            // Khởi tạo in-degree = 0 cho tất cả node
            var inDegree = new Dictionary<NodeModel, int>();
            foreach (var node in nodes)
                inDegree[node] = 0;

            // Tính in-degree dựa trên Childs (cạnh trực tiếp, KHÔNG dùng Fronts vì nó đã flatten)
            foreach (var node in nodes)
            {
                foreach (var child in node.Childs)
                {
                    if (!inDegree.ContainsKey(child))
                        inDegree[child] = 0; // phòng trường hợp child không nằm trong allNodes truyền vào
                    inDegree[child]++;
                }
            }

            var queue = new Queue<NodeModel>(nodes.Where(n => inDegree[n] == 0));
            var result = new List<NodeModel>();

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);

                foreach (var child in node.Childs)
                {
                    inDegree[child]--;
                    if (inDegree[child] == 0)
                        queue.Enqueue(child);
                }
            }

            if (result.Count != nodes.Count)
            {
                var remaining = nodes.Except(result).Select(n => n.Title);
                throw new InvalidOperationException(
                    $"Phát hiện chu trình (cycle) trong flow, liên quan tới node: {string.Join(", ", remaining)}");
            }

            return result;
        }

        /// <summary>
        /// Chạy toàn bộ flow theo thứ tự topo. Node bị disable (IsEnabled = false) sẽ bị bỏ qua.
        /// </summary>
        public static async Task Execute(IEnumerable<NodeModel> allNodes, Func<NodeModel,Task> processNode, Action<NodeModel> endProcess)
        {
            var order = TopologicalSort(allNodes);

            foreach (var node in order)
            {
                if (!node.IsEnabled)
                    continue;

                await processNode(node);
                endProcess(node);
            }
        }

        /// <summary>
        /// Phân tầng (layer) các node có thể chạy song song — mỗi layer là nhóm node
        /// không phụ thuộc lẫn nhau, dùng để Parallel.ForEach hoặc Task.WhenAll.
        /// </summary>
        public static List<List<NodeModel>> GetExecutionLayers(IEnumerable<NodeModel> allNodes)
        {
            var nodes = allNodes.Distinct().ToList();
            var inDegree = new Dictionary<NodeModel, int>();
            foreach (var node in nodes) inDegree[node] = 0;

            foreach (var node in nodes)
                foreach (var child in node.Childs)
                {
                    if (!inDegree.ContainsKey(child)) inDegree[child] = 0;
                    inDegree[child]++;
                }

            var layers = new List<List<NodeModel>>();
            var currentLayer = nodes.Where(n => inDegree[n] == 0).ToList();
            var processedCount = 0;

            while (currentLayer.Count > 0)
            {
                layers.Add(currentLayer);
                processedCount += currentLayer.Count;

                var nextLayer = new List<NodeModel>();
                foreach (var node in currentLayer)
                {
                    foreach (var child in node.Childs)
                    {
                        inDegree[child]--;
                        if (inDegree[child] == 0)
                            nextLayer.Add(child);
                    }
                }
                currentLayer = nextLayer;
            }

            if (processedCount != nodes.Count)
            {
                var remaining = nodes.Except(layers.SelectMany(l => l)).Select(n => n.Title);
                throw new InvalidOperationException(
                    $"Phát hiện chu trình (cycle) trong flow, liên quan tới node: {string.Join(", ", remaining)}");
            }

            return layers;
        }
    }
}