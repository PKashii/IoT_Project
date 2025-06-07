using IoT;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Opc.UaFx;
using Opc.UaFx.Client;
using OpcAgent;

public class OpcUaAgent
{
    private readonly OpcClient _client;
    private readonly List<OpcUaDevice> _devices;
    private readonly Dictionary<string, AzurePublisher> _publishers;
    private readonly Dictionary<string, int> _desiredProductionRates;
    private readonly Dictionary<string, DirectMethodHandler> _directMethodHandlers;
    private readonly Dictionary<string, int> _lastDeviceErrors;
    private readonly Dictionary<string, int> _lastWrittenProductionRates = new();


    public OpcUaAgent(string endpoint, Dictionary<string, string> deviceConnectionStrings)
    {
        _client = new OpcClient(endpoint);
        _devices = deviceConnectionStrings.Keys.Select(name => new OpcUaDevice(name)).ToList();
        _desiredProductionRates = new Dictionary<string, int>();
        _publishers = new Dictionary<string, AzurePublisher>();
        _directMethodHandlers = new Dictionary<string, DirectMethodHandler>();
        _lastDeviceErrors = new Dictionary<string, int>();

        foreach (var kvp in deviceConnectionStrings)
        {
            string deviceId = kvp.Key;
            string connectionString = kvp.Value;

            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            var publisher = new AzurePublisher(connectionString, deviceId, rate => _desiredProductionRates[deviceId] = (int)rate);
            var handler = new DirectMethodHandler(deviceClient, _client, deviceId);

            _publishers[deviceId] = publisher;
            _directMethodHandlers[deviceId] = handler;
            _lastDeviceErrors[deviceId] = -1;
        }
    }

    public async Task RunAsync()
    {
        _client.Connect();

        foreach (var handler in _directMethodHandlers.Values)
        {
            await handler.InitializeAsync();
        }

        while (true)
        {
            foreach (var device in _devices)
            {
                if (_desiredProductionRates.TryGetValue(device.Name, out int desiredRate))
                {
                    if (!_lastWrittenProductionRates.TryGetValue(device.Name, out int lastWritten) || lastWritten != desiredRate)
                    {
                        WriteProductionRate(device, desiredRate);
                        _lastWrittenProductionRates[device.Name] = desiredRate;
                    }
                }


                var values = ReadDeviceData(device);
                await _publishers[device.Name].SendTelemetryAsync(values);

                int currentRate = Convert.ToInt32(values["ProductionRate"]);
                int deviceError = Convert.ToInt32(values["DeviceError"]);
                await _publishers[device.Name].UpdateReportedAsync(currentRate, deviceError);

                if (_lastDeviceErrors[device.Name] != deviceError)
                {
                    var errorEvent = new Dictionary<string, object>
                    {
                        {"deviceId", device.Name},
                        {"timestamp", DateTime.Now},
                        {"eventType", "DeviceErrorChanged"},
                        {"newValue", deviceError}
                    };
                    await _publishers[device.Name].SendTelemetryAsync(errorEvent);
                    _lastDeviceErrors[device.Name] = deviceError;
                }
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
        var node = new OpcWriteNode(nodeId, desiredRate);
        var status = _client.WriteNode(node);

        if (status.IsGood)
        {
            Console.WriteLine($"[INFO] {device.Name}: Desired ProductionRate set to: {desiredRate}");
        }
        else
        {
            Console.WriteLine($"[ERROR] {device.Name}: Write failed: {status}");
        }
    }
}
