using NodeEditor.Executions;
using NodeEditor.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Linq;

namespace NodeEditorDemo.ViewModels
{
    public class MainViewModel : Prism.Mvvm.BindableBase
    {
        private List<ResultType> ResultTypes = new List<ResultType>() { ResultType.OK, ResultType.NG, ResultType.NONE };

        public MainViewModel()
        {
            Nodes = new();
            Containers = new();
            Arrows = new();
            Nodes.Add(new NodeModel(100, 100) { Title = "Chuyển đổi ảnh sang đen trắng", Group = "Image Processings" });
            Containers.Add(new NodeModel(100, 100) { Title = "Chuyển đổi ảnh sang đen trắng", Group = "Image Processings" });
            Containers.Add(new NodeModel(400, 100) { Title = "So khớp hình dạng", Group = "Locatings" });
            Nodes.Add(new NodeModel(400, 100) { Title = "So khớp hình dạng", Group = "Locatings" });
            Nodes.Add(new NodeModel(250, 300) { Title = "Phát hiện cạnh", Group = "Locatings" });
            Containers.Add(new NodeModel(250, 300) { Title = "Phát hiện cạnh", Group = "Locatings" });
            AddDataCommand = new DelegateCommand(OnAddData);
            SelectAllCommand = new DelegateCommand(OnSelectAll);
            RunDemoCommand = new DelegateCommand(OnRun);
        }

        public ICommand AddDataCommand { get; set; }
        public ObservableCollection<ConnectionLineModel> Arrows { get; set; }
        public ObservableCollection<NodeModel> Nodes { get; set; }
        public ObservableCollection<NodeModel> Containers { get; set; }
        public ICommand RunDemoCommand { get; set; }

        public ICommand SelectAllCommand { get; set; }

        public List<NodeModel> GetParentNodes(IList<NodeModel> nodes)
        {
            return nodes.Where(n => n.Fronts.Count <= 0).ToList();
        }

        public Task RunChilds(NodeModel parent, CancellationTokenSource tokenSource)
        {
            if (parent.Childs.Count <= 0) return Task.CompletedTask;
            var tasks = new List<Task>();
            foreach (var child in parent.Childs)
            {
                if (child is not NodeModel node) continue;
                tasks.Add(RunNode(node, tokenSource));
            }
            return Task.WhenAll(tasks);
        }

        public async Task RunNode(NodeModel node, CancellationTokenSource tokenSource)
        {
            if (!node.IsEnabled) return;
            node.IsRunning = true;
            var token = new CancellationTokenSource();
            _ = CalculateCycleTime(node, token.Token);
            await Task.Delay(Random.Shared.Next(10, 1000));
            token.Cancel();
            var index = Random.Shared.Next(0, 2);
            node.Result = ResultTypes[index];
            node.IsRunning = false;
            if (node.Childs.Count > 0)
            {
                await RunChilds(node, tokenSource);
                return;
            }
        }

        public async Task RunParentNodes(IList<NodeModel> nodes, CancellationTokenSource tokenSource)
        {
            foreach (var node in nodes)
            {
                await RunNode(node, tokenSource);
            }
        }

        private async Task CalculateCycleTime(NodeModel node, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                node.Duration += 10;
                await Task.Delay(10);
            }
        }

        private void OnAddData()
        {
            Nodes.Add(new NodeModel(0, 0) { Title = "Chuyển đổi ảnh sang đen trắng" });
        }

        private async void OnRun()
        {
            foreach (var node in Nodes) {
                node.Result = ResultType.NONE;
                node.Duration = 0;
            }
            await NodeGraphExecutor.Execute(Nodes, async (node) =>
            {
                node.IsRunning = true;
                var token = new CancellationTokenSource();
                _ = CalculateCycleTime(node, token.Token);
                await Task.Delay(Random.Shared.Next(10, 1000));
                token.Cancel();
                var index = Random.Shared.Next(0, 2);
                node.Result = ResultTypes[index];
                node.IsRunning = false;
            }, (n) => { n.IsRunning = false; });
        }

        private void OnSelectAll()
        {
            foreach (var item in Nodes)
            {
                item.IsSelected = true;
            }
        }
    }
}