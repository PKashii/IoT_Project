namespace OpcAgent
{
    using IoT;
    using Opc.UaFx;
    using Opc.UaFx.Client;
    using System.Linq;

    public class OpcUaAgent
    {
        private readonly OpcClient _client;
        private readonly List<OpcUaDevice> _devices;

        public OpcUaAgent(string endpoint, List<string> deviceNames)
        {
            _client = new OpcClient(endpoint);
            _devices = deviceNames.Select(name => new OpcUaDevice(name)).ToList();
        }

        public void Run()
        {
            _client.Connect();
            Console.WriteLine("Connected to OPC UA server.\n");

            while (true)
            {
                foreach (var device in _devices)
                {
                    var values = ReadDeviceData(device);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {device.Name}");
                    foreach (var kvp in values)
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }

                Console.WriteLine();
                Thread.Sleep(1000);
            }
        }

        private Dictionary<string, object> ReadDeviceData(OpcUaDevice device)
        {
            var nodes = new OpcReadNode[]
            {
            new(device.Node("ProductionStatus")),
            new(device.Node("WorkorderId")),
            new(device.Node("ProductionRate")),
            new(device.Node("GoodCount")),
            new(device.Node("BadCount")),
            new(device.Node("Temperature")),
            new(device.Node("DeviceError"))
            };

            var results = _client.ReadNodes(nodes).ToArray();

            return new Dictionary<string, object>
            {
                ["ProductionStatus"] = results[0].Value,
                ["WorkorderId"] = results[1].Value,
                ["ProductionRate"] = results[2].Value,
                ["GoodCount"] = results[3].Value,
                ["BadCount"] = results[4].Value,
                ["Temperature"] = results[5].Value,
                ["DeviceError"] = results[6].Value
            };
        }
    }

}
