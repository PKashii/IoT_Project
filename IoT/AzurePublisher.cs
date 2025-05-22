using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace OpcAgent
{
    public class AzurePublisher
    {
        private readonly DeviceClient _client;
        private readonly string _deviceId;
        private readonly DeviceTwinManager _twinManager;

        public AzurePublisher(string connectionString, string deviceId, Action<int> onDesiredProductionRateChanged)
        {
            _deviceId = deviceId;
            _client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            _twinManager = new DeviceTwinManager(_client, deviceId, onDesiredProductionRateChanged);
            _twinManager.InitializeAsync().Wait();
        }

        public async Task SendTelemetryAsync(Dictionary<string, object> data)
        {
            data["deviceId"] = _deviceId;
            data["timestamp"] = DateTime.UtcNow;

            string json = JsonConvert.SerializeObject(data);
            var message = new Message(Encoding.UTF8.GetBytes(json))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8"
            };

            await _client.SendEventAsync(message);
        }

        public Task UpdateReportedAsync(int productionRate, int deviceError)
        {
            return _twinManager.UpdateReportedPropertiesAsync(productionRate, deviceError);
        }
    }
}