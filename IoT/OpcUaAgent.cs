using IoT;
using Newtonsoft.Json;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpcAgent
{
    public class OpcUaAgent
    {
        private readonly OpcClient _client;
        private readonly List<OpcUaDevice> _devices;
        private readonly Dictionary<string, AzurePublisher> _publishers;
        private readonly Dictionary<string, int> _desiredProductionRates;

        public OpcUaAgent(string endpoint, Dictionary<string, string> deviceConnectionStrings)
        {
            _client = new OpcClient(endpoint);
            _devices = deviceConnectionStrings.Keys.Select(name => new OpcUaDevice(name)).ToList();
            _desiredProductionRates = new Dictionary<string, int>();

            _publishers = deviceConnectionStrings.ToDictionary(
                kvp => kvp.Key,
                kvp => new AzurePublisher(kvp.Value, kvp.Key, rate => _desiredProductionRates[kvp.Key] = rate)
            );
        }

        public async Task RunAsync()
        {
            _client.Connect();

            while (true)
            {
                foreach (var device in _devices)
                {
                    if (_desiredProductionRates.TryGetValue(device.Name, out int desiredRate))
                    {
                        WriteProductionRate(device, desiredRate);
                    }

                    var values = ReadDeviceData(device);
                    await _publishers[device.Name].SendTelemetryAsync(values);

                    int currentRate = Convert.ToInt32(values["ProductionRate"]);
                    int deviceError = Convert.ToInt32(values["DeviceError"]);
                    await _publishers[device.Name].UpdateReportedAsync(currentRate, deviceError);

                    Console.WriteLine($"[{device.Name}] Sent data: {JsonConvert.SerializeObject(values, Formatting.Indented)}");
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

        private void WriteProductionRate(OpcUaDevice device, int desiredRate)
        {
            var nodeId = device.Node("ProductionRate");

            var status = _client.WriteNode(new OpcWriteNode(nodeId, desiredRate));

            if (status.IsGood)
            {
                Console.WriteLine($"[{device.Name}] Desired ProductionRate set to: {desiredRate}");
            }
            else
            {
                Console.WriteLine($"[{device.Name}] Write failed: {status}");
            }
        }
    }
}