using OpcAgent;
using System.Text;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Please, specify devices you want to connect to.");
        Console.WriteLine("Type in format: <DeviceName>, <ConString>.");
        Console.WriteLine("Press ENTER to add another device. Press ESC to finish.");

        var devices = new Dictionary<string, string>();

        Console.WriteLine("\nPress any key to continue or ESC to finish...");
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Escape)
            {
                if (devices.Count == 0)
                {
                    Console.WriteLine("\nNo devices added. Exiting...");
                    return;
                }
                break;
            }
            Console.Write("Device Name, Connection String: ");
            var input = Console.ReadLine()?.Trim();
            var parts = input.Split(new[] { ',' }, 2);

            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                Console.WriteLine("Invalid input. Please enter in the format: <DeviceName>, <ConString>");
                continue;
            }

            devices[parts[0].Trim()] = parts[1].Trim();
            Console.WriteLine("\nPress any key to add another device or ESC to finish...");
        }
        Console.Clear();
        Console.WriteLine("\nDevices added successfully. Starting OPC UA agent...");
        Console.WriteLine("\nTo check for sent data, log in to your IoT Hub and open telemetry listener.");
        Console.WriteLine("\nAny messages regarding Direct Method invocations will be here:");
        var agent = new OpcUaAgent("opc.tcp://localhost:4840/", devices);
        await agent.RunAsync();
    }
}
