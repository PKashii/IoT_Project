using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace OpcAgent
{
    public class DeviceTwinManager
    {
        private readonly DeviceClient _client;
        private readonly string _deviceId;
        private readonly Action<int> _onDesiredProductionRateChanged;

        public DeviceTwinManager(DeviceClient client, string deviceId, Action<int> onDesiredProductionRateChanged)
        {
            _client = client;
            _deviceId = deviceId;
            _onDesiredProductionRateChanged = onDesiredProductionRateChanged;
        }

        public async Task InitializeAsync()
        {
            await _client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null);
        }

        private Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties.Contains("ProductionRate"))
            {
                var value = desiredProperties["ProductionRate"];
                int desired;
                try
                {
                    desired = Convert.ToInt32(value);
                    _onDesiredProductionRateChanged(desired);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Cannot convert desired ProductionRate: {value} ({ex.Message})");
                }
            }
            return Task.CompletedTask;
        }

        public async Task UpdateReportedPropertiesAsync(int productionRate, int deviceError)
        {
            var reported = new TwinCollection
            {
                ["ProductionRate"] = productionRate,
                ["DeviceError"] = deviceError
            };

            await _client.UpdateReportedPropertiesAsync(reported);
        }
    }
}