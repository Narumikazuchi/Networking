namespace UnitTest;

[TestClass]
public partial class ClientTests
{
    [TestMethod]
    public void GenericInstantiationTest()
    {
        using Client<TestMessage> client = Client<TestMessage>.CreateClient(80, 4096);
        Assert.AreEqual(80, client.Port);
        Assert.AreEqual(4096, client.BufferSize);
        Assert.IsNull(client.DataProcessor);
    }

    [TestMethod]
    public void InstantiationTest()
    {
        using Client client = Client.CreateClient(80, 4096);
        Assert.AreEqual(80, client.Port);
        Assert.AreEqual(4096, client.BufferSize);
        Assert.IsNull(client.DataProcessor);
    }

    [TestMethod]
    public void GenericConnectionTest()
    {
        using Client<TestMessage> client = Client<TestMessage>.CreateClient(80, 4096);
        using Server<TestMessage> server = Server<TestMessage>.CreateServer(80, 4096, () => true);
        Assert.AreEqual(client.Port, server.Port);
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        client.Disconnect();
        server.Stop();
    }

    [TestMethod]
    public void ConnectionTest()
    {
        using Client client = Client.CreateClient(80, 4096);
        using Server server = Server.CreateServer(80, 4096, () => true);
        Assert.AreEqual(client.Port, server.Port);
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        client.Disconnect();
        server.Stop();
    }

    [TestMethod]
    public void GenericSendTest()
    {
        using Client<TestMessage> client = Client<TestMessage>.CreateClient(80, 4096);
        using Server<TestMessage> server = Server<TestMessage>.CreateServer(80, 4096, () => true);
        Assert.AreEqual(client.Port, server.Port);
        server.DataProcessor = new ServerProcessor(server);
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        client.Send(new("test"));
        client.Disconnect();
        server.Stop();
    }

    [TestMethod]
    public void SendTest()
    {
        using Client client = Client.CreateClient(80, 4096);
        using Server server = Server.CreateServer(80, 4096, () => true);
        Assert.AreEqual(client.Port, server.Port);
        server.DataReceived += (s, e) => _instance.WriteLine($"Client[{e.FromClient}]: {String.Join(' ', e.Data.Select(b => b.ToString("X")))}");
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        client.Send(new Byte[] { 0xFF, 0xEF, 0xEB, 0x2E, 0x69 });
        client.Disconnect();
        server.Stop();
    }

    [TestMethod]
    public void GenericReceiveTest()
    {
        using Client<TestMessage> client = Client<TestMessage>.CreateClient(80, 4096);
        using Server<TestMessage> server = Server<TestMessage>.CreateServer(80, 4096, () => true);
        client.DataProcessor = new ClientProcessor(client);
        Assert.AreEqual(client.Port, server.Port);
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        server.Broadcast(new("test"));
        server.Stop();
    }

    [TestMethod]
    public void ReceiveTest()
    {
        using Client client = Client.CreateClient(80, 4096);
        using Server server = Server.CreateServer(80, 4096, () => true);
        client.DataReceived += (s, e) => _instance.WriteLine($"[{s.Guid}] From Server: {String.Join(' ', e.Data.Select(b => b.ToString("X")))}");
        Assert.AreEqual(client.Port, server.Port);
        server.Start();
        client.Connect(IPAddress.Loopback);
        while (!client.Connected)
        {
            Thread.Sleep(1);
        }
        server.Broadcast(new Byte[] { 0xFF, 0xEF, 0xEB, 0x2E, 0x69 });
        server.Stop();
    }

    public TestContext TestContext
    {
        get => _instance;
        set => _instance = value;
    }

    public static TestContext _instance;
}

partial class ClientTests
{
    public class ServerProcessor : ServerDataProcessor<TestMessage>
    {
        public ServerProcessor([DisallowNull] Server<TestMessage> server) : base(server)
        { }

        public override void ProcessReceivedData([DisallowNull] TestMessage data, in Guid fromClient)
        {
            _instance.WriteLine($"Client[{fromClient}]: {data.Message}");
        }
    }

    public class ClientProcessor : ClientDataProcessor<TestMessage>
    {
        public ClientProcessor([DisallowNull] Client<TestMessage> client) : base(client)
        { }

        public override void ProcessReceivedData([DisallowNull] TestMessage data)
        {
            _instance.WriteLine($"[{this.Client.Guid}] From Server: {data.Message}");
        }
    }
}