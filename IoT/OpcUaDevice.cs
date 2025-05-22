namespace IoT
{
    public class OpcUaDevice
    {
        public string Name { get; }
        public string Namespace => $"ns=2;s={Name}";

        public OpcUaDevice(string name)
        {
            Name = name;
        }

        public string Node(string nodeName) => $"{Namespace}/{nodeName}";
    }

}
