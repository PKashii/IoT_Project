namespace OpcAgent
{
    using IoT;
    using Opc.UaFx;
    using Opc.UaFx.Client;
    using System.Linq;
    using Newtonsoft.Json;

    public class OpcUaAgent
    {
        private readonly OpcClient _client;
        private readonly List<OpcUaDevice> _devices;
        private readonly Dictionary<string, AzurePublisher> _publishers;

        public OpcUaAgent(string endpoint, Dictionary<string, string> deviceConnectionStrings)
        {
            _client = new OpcClient(endpoint);
            _devices = deviceConnectionStrings.Keys.Select(name => new OpcUaDevice(name)).ToList();

            _publishers = deviceConnectionStrings.ToDictionary(
                kvp => kvp.Key,
                kvp => new AzurePublisher(kvp.Value, kvp.Key)
            );
        }

        public async Task RunAsync()
        {
            _client.Connect();

            while (true)
            {
                foreach (var device in _devices)
                {
                    var values = ReadDeviceData(device);
                    await _publishers[device.Name].SendTelemetryAsync(values);
                    Console.WriteLine($"[{device.Name}] Sent data: {JsonConvert.SerializeObject(values)}");
                }

                await Task.Delay(1000);
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
