using OpcAgent;

class Program
{
    static async Task Main()
    {
        var devices = new Dictionary<string, string>
        {
            { "Device 1", "HostName=UL2024.azure-devices.net;DeviceId=Device_1;SharedAccessKey=GlULfp5kNDmp7HeAFVkcWE2v+rs7N4AUGVwxjR2KQaU=" },
            { "Device 2", "HostName=UL2024.azure-devices.net;DeviceId=Device_2;SharedAccessKey=gHSDmJeOm3NzI5/CBgceh2lgiKoy/0bohdY66CHtP9A=" }
        };

        var agent = new OpcUaAgent("opc.tcp://localhost:4840/", devices);
        await agent.RunAsync();
    }
}