using Narumikazuchi.Networking.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System;
using System.Threading;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public partial class ServerTests
    {
        [TestMethod]
        public void GenericInstantiationTest()
        {
            using Server<TestMessage> server = Server<TestMessage>.CreateServer(8080, 8192, () => true);
            Assert.IsNotNull(server);
            Assert.AreEqual(8080, server.Port);
            Assert.AreEqual(8192, server.BufferSize);
            Assert.IsNull(server.DataProcessor);
        }

        [TestMethod]
        public void InstantiationTest()
        {
            using Server server = Server.CreateServer(8080, 8192, () => true);
            Assert.IsNotNull(server);
            Assert.AreEqual(8080, server.Port);
            Assert.AreEqual(8192, server.BufferSize);
            Assert.IsNull(server.DataProcessor);
        }

        [TestMethod]
        public void GenericConnectionTest()
        {
            using Server<TestMessage> server = Server<TestMessage>.CreateServer(8080, 4096, () => true);
            using Client<TestMessage> client1 = Client<TestMessage>.CreateClient(8080, 4096);
            using Client<TestMessage> client2 = Client<TestMessage>.CreateClient(8080, 4096);
            Assert.AreEqual(client1.Port, server.Port);
            Assert.AreEqual(client2.Port, server.Port);
            server.Start();
            client1.Connect(IPAddress.Loopback);
            client2.Connect(IPAddress.Loopback);
            while (!client1.Connected ||
                   !client2.Connected)
            {
                Thread.Sleep(1);
            }
            server.Stop();
        }

        [TestMethod]
        public void ConnectionTest()
        {
            using Server server = Server.CreateServer(8080, 4096, () => true);
            using Client client1 = Client.CreateClient(8080, 4096);
            using Client client2 = Client.CreateClient(8080, 4096);
            Assert.AreEqual(client1.Port, server.Port);
            Assert.AreEqual(client2.Port, server.Port);
            server.Start();
            client1.Connect(IPAddress.Loopback);
            client2.Connect(IPAddress.Loopback);
            while (!client1.Connected ||
                   !client2.Connected)
            {
                Thread.Sleep(1);
            }
            server.Stop();
        }

        [TestMethod]
        public void GenericSendTest()
        {
            using Server<TestMessage> server = Server<TestMessage>.CreateServer(800, 4096, () => true);
            using Client<TestMessage> client1 = Client<TestMessage>.CreateClient(800, 4096);
            using Client<TestMessage> client2 = Client<TestMessage>.CreateClient(800, 4096);
            client1.DataProcessor = new ClientProcessor(client1);
            client2.DataProcessor = new ClientProcessor(client2);
            Assert.AreEqual(client1.Port, server.Port);
            Assert.AreEqual(client2.Port, server.Port);
            server.Start();
            client1.Connect(IPAddress.Loopback);
            client2.Connect(IPAddress.Loopback);
            while (!client1.Connected ||
                   !client2.Connected)
            {
                Thread.Sleep(1);
            }
            server.Broadcast(new("test"));
            server.Send(new("test2"), client1.Guid);
        }

        [TestMethod]
        public void SendTest()
        {
            using Server server = Server.CreateServer(800, 4096, () => true);
            using Client client1 = Client.CreateClient(800, 4096);
            using Client client2 = Client.CreateClient(800, 4096);
            client1.DataReceived += (s, e) => _instance.WriteLine($"[{s.Guid}] From Server: {String.Join(' ', e.Data.Select(b => b.ToString("X")))}");
            client2.DataReceived += (s, e) => _instance.WriteLine($"[{s.Guid}] From Server: {String.Join(' ', e.Data.Select(b => b.ToString("X")))}");
            Assert.AreEqual(client1.Port, server.Port);
            Assert.AreEqual(client2.Port, server.Port);
            server.Start();
            client1.Connect(IPAddress.Loopback);
            client2.Connect(IPAddress.Loopback);
            while (!client1.Connected ||
                   !client2.Connected)
            {
                Thread.Sleep(1);
            }
            server.Broadcast(new Byte[] { 0xFF, 0xEF, 0xEB, 0x2E, 0x69 });
            server.Send(new Byte[] { 0xEF, 0xEB, 0xEA, 0x29 }, client1.Guid);
        }

        [TestMethod]
        public void GenericReceiveTest()
        {
            using Server<TestMessage> server = Server<TestMessage>.CreateServer(8080, 4096, () => true);
            using Client<TestMessage> client = Client<TestMessage>.CreateClient(8080, 4096);
            server.DataProcessor = new ServerProcessor(server);
            Assert.AreEqual(client.Port, server.Port);
            server.Start();
            client.Connect(IPAddress.Loopback);
            while (!client.Connected)
            {
                Thread.Sleep(1);
            }
            client.Send(new("test"));
            server.Stop();
        }

        [TestMethod]
        public void ReceiveTest()
        {
            using Server server = Server.CreateServer(8080, 4096, () => true);
            using Client client = Client.CreateClient(8080, 4096);
            server.DataReceived += (s, e) => _instance.WriteLine($"Client[{e.FromClient}]: {String.Join(' ', e.Data.Select(b => b.ToString("X")))}");
            Assert.AreEqual(client.Port, server.Port);
            server.Start();
            client.Connect(IPAddress.Loopback);
            while (!client.Connected)
            {
                Thread.Sleep(1);
            }
            client.Send(new Byte[] { 0xFF, 0xEF, 0xEB, 0x2E, 0x69 });
            server.Stop();
        }

        public TestContext TestContext
        {
            get => _instance;
            set => _instance = value;
        }

        public static TestContext _instance;
    }

    partial class ServerTests
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
}
