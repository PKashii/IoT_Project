using OpcAgent;

class Program
{
    static void Main()
    {
        var endpoint = "opc.tcp://localhost:4840/";

        // Lista urządzeń, które będą odczytywane
        var devices = new List<string> { "Device 1", "Device 2" }; // Dodaj więcej jeśli trzeba

        var agent = new OpcUaAgent(endpoint, devices);
        agent.Run();
    }
}
