namespace IoT
{
    public class OpcUaDevice
    {
        public string Name { get; }

        public OpcUaDevice(string name)
        {
            Name = name;
        }

        public string Node(string nodeName) => $"ns=2;s={Name}/{nodeName}";
    }
}
