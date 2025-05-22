using Microsoft.Azure.Devices.Client;
using Opc.UaFx;
using Opc.UaFx.Client;

public class DirectMethodHandler
{
    private readonly DeviceClient _client;
    private readonly OpcClient _opcClient;
    private readonly string _deviceId;

    public DirectMethodHandler(DeviceClient client, OpcClient opcClient, string deviceId)
    {
        _client = client;
        _opcClient = opcClient;
        _deviceId = deviceId;
    }

    public async Task InitializeAsync()
    {
        await _client.SetMethodHandlerAsync("EmergencyStop", HandleEmergencyStopAsync, null);
        await _client.SetMethodHandlerAsync("ResetErrorStatus", HandleResetErrorStatusAsync, null);
    }

    private Task<MethodResponse> HandleEmergencyStopAsync(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var methodCall = new OpcCallMethod(
                $"ns=2;s={_deviceId}",
                $"ns=2;s={_deviceId}/EmergencyStop"
            );

            _opcClient.CallMethod(methodCall);
            Console.WriteLine($"[DirectMethod] EmergencyStop executed on {_deviceId}");
            return Task.FromResult(new MethodResponse(200));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DirectMethod] EmergencyStop failed: {ex.Message}");
            return Task.FromResult(new MethodResponse(500));
        }
    }

    private Task<MethodResponse> HandleResetErrorStatusAsync(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var methodCall = new OpcCallMethod(
                $"ns=2;s={_deviceId}",
                $"ns=2;s={_deviceId}/ResetErrorStatus"
            );

            _opcClient.CallMethod(methodCall);
            Console.WriteLine($"[DirectMethod] ResetErrorStatus executed on {_deviceId}");
            return Task.FromResult(new MethodResponse(200));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DirectMethod] ResetErrorStatus failed: {ex.Message}");
            return Task.FromResult(new MethodResponse(500));
        }
    }
}