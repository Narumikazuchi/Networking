using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the basic functionality for a <see cref="Socket"/>-based server.
    /// </summary>
    [DebuggerDisplay("{Protocol}:{Port}")]
    public sealed partial class Server<T> 
        where T : class, IByteSerializable, IEquatable<T>
    {
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/> with default values.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 80, 
                 2048, 
                 false, 
                 null, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 2048, 
                 false, 
                 null, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 null, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 forwardExecptions, 
                 null, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      [AllowNull] ServerDataProcessor<T>? processor, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 processor, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions, 
                      [AllowNull] ServerDataProcessor<T>? processor, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 forwardExecptions, 
                 processor, 
                 () => true, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions, 
                      [DisallowNull] Func<Boolean> acceptCondition, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 null, 
                 acceptCondition, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      [AllowNull] ServerDataProcessor<T>? processor, 
                      [DisallowNull] Func<Boolean> acceptCondition, 
                      [DisallowNull] T shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 processor, 
                 acceptCondition, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions, 
                      [AllowNull] ServerDataProcessor<T>? processor, 
                      [DisallowNull] Func<Boolean> acceptCondition, 
                      [DisallowNull] T shutdownCommand)
        {
            if (shutdownCommand is null)
            {
                throw new ArgumentNullException(nameof(shutdownCommand));
            }
            if (acceptCondition is null)
            {
                throw new ArgumentNullException(nameof(acceptCondition));
            }
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            if (protocol is not ProtocolType.Tcp
                         and not ProtocolType.Udp
                         and not ProtocolType.Icmp
                         and not ProtocolType.Igmp)
            {
                throw new ArgumentException(PROTOCOL_INVALID,
                                            nameof(protocol));
            }

            SocketType type = SocketType.Unknown;
            switch (protocol)
            {
                case ProtocolType.Tcp:
                    type = SocketType.Stream;
                    break;
                case ProtocolType.Udp:
                    type = SocketType.Dgram;
                    break;
                case ProtocolType.Icmp:
                case ProtocolType.Igmp:
                    type = SocketType.Raw;
                    break;
            }
            this._serverSocket = new Socket(AddressFamily.InterNetwork, 
                                            type, 
                                            protocol);
            this._isRunning = false;
            this._dataBuffer = new Byte[bufferSize];
            this._condition = acceptCondition;
            this.Clients = new ClientCollection();
            this.Protocol = protocol;
            this.Port = port;
            this.ForwardExceptions = forwardExecptions;
            this.CommandProcessor = processor;
            this.ShutdownCommand = shutdownCommand;
        }

        /// <summary>
        /// Starts setting up the server as well as starts to listen for incoming connections.
        /// </summary>
        public void Start()
        {
            try
            {
                this._serverSocket.Bind(new IPEndPoint(IPAddress.Any, 
                                                       this.Port));
                this._serverSocket.Listen(12);
                this.Clients.Clear();
                this._serverSocket.BeginAccept(this.AcceptCallback, 
                                               null);
                this._isRunning = true;
            }
            catch (Exception ex)
            {
                this._isRunning = false;
                this.OnException(ex);
            }
        }

        /// <summary>
        /// Stops the running server.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (this.Clients
                        .Count > 0)
                {
                    Byte[] bytes = this._serializer.Serialize(this.ShutdownCommand);
                    foreach (Socket client in this.Clients)
                    {
                        try
                        {
                            if (client.Connected)
                            {
                                this.SendInternal(bytes, 
                                                  client, 
                                                  true);
                                client.Close();
                                this.OnClientDisconnected(client, 
                                                          ConnectionType.ConnectionClosed);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.OnException(ex);
                        }
                    }
                }
                this.Clients.Clear();
                this._serverSocket.Close();
                this._isRunning = false;
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        /// <summary>
        /// Sends the specified data to the specified client.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public void Send([DisallowNull] T data, 
                         [DisallowNull] Socket client)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            Byte[] bytes = this._serializer.Serialize(data);
            this.SendInternal(bytes, 
                          client, 
                          false);
        }

        /// <summary>
        /// Sends the specified data to all connected clients.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public void SendBroadcast([DisallowNull] T data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Equals(this.ShutdownCommand))
            {
                this.Stop();
                return;
            }
            Byte[] bytes = this._serializer.Serialize(data);
            try
            {
                if (this.Clients.Count > 0)
                {
                    foreach (Socket client in this.Clients)
                    {
                        this.SendInternal(bytes, 
                                      client, 
                                      true);
                    }
                }
                Thread.Sleep(this.BroadcastInterval);
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        /// <summary>
        /// Occurs when a new client is connected.
        /// </summary>
        public event EventHandler<Server<T>, ConnectionEventArgs>? ClientConnected;
        /// <summary>
        /// Occurs when a client gets disconnected.
        /// </summary>
        public event EventHandler<Server<T>, ConnectionEventArgs>? ClientDisconnected;
        /// <summary>
        /// Occurs when an exception occurs inside the server application.
        /// </summary>
        public event EventHandler<Server<T>, ExceptionEventArgs>? ExceptionOccurred;

        /// <summary>
        /// The protocol used for the connection.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ProtocolType Protocol { get; }
        /// <summary>
        /// The port the client should connect to. Default is 80.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int32 Port { get; }
        /// <summary>
        /// Returns if the server is currently running.
        /// </summary>
        public Boolean IsRunning => this._isRunning;
        /// <summary>
        /// List of Clients that are currently connected.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        [NotNull]
        public ClientCollection Clients { get; }
        /// <summary>
        /// Determines the actions to be taken after the server receives data from any client.<para/>
        /// Is supposed to be a derived class from <see cref="ServerDataProcessor{T}"/>.
        /// </summary>
        [MaybeNull]
        public ServerDataProcessor<T>? CommandProcessor
        {
            get => this._processor;
            set
            {
                this._processor = value;
                if (this._processor is not null)
                {
                    this._processor.Server = this;
                }
            }
        }
        /// <summary>
        /// A function in which the conditions to accept new connections are defined.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public Func<Boolean> AcceptCondition
        {
            get => this._condition;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._condition = value;
            }
        }
        /// <summary>
        /// Gets or sets if exceptions should be forwarded through the <see cref="ExceptionOccurred"/> event.
        /// </summary>
        public Boolean ForwardExceptions { get; set; }
        /// <summary>
        /// Gets or sets the size of the data buffer.
        /// </summary>
        public Int32 BufferSize
        {
            get => this._dataBuffer.Length;
            set => this._dataBuffer = new Byte[value];
        }
        /// <summary>
        /// Gets or sets the interval between individual broadcast sends. Default is 10ms.
        /// </summary>
        public Int32 BroadcastInterval { get; set; } = 10;
        /// <summary>
        /// Gets the <typeparamref name="T"/> object, which signals a server shutdown or client disconnect.
        /// </summary>
        [NotNull]
        public T ShutdownCommand { get; }
    }

    // Non-Public
    partial class Server<T>
    {
        private void AcceptCallback(IAsyncResult result)
        {
            Socket? socket = null;
            if (!this.AcceptCondition.Invoke())
            {
                return;
            }
            try
            {
                socket = this._serverSocket.EndAccept(result);
                this.Clients.Add(socket);
                this.OnClientConnected(socket, 
                                       ConnectionType.ConnectionEstablished);
                if (this.IsRunning)
                {
                    socket.BeginReceive(this._dataBuffer,
                                        0, 
                                        this._dataBuffer.Length, 
                                        SocketFlags.None, 
                                        this.ReceiveCallback, 
                                        socket);
                    this._serverSocket.BeginAccept(this.AcceptCallback, 
                                                   null);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
#pragma warning disable
            Int32 received = 0;
            Byte[]? data = null;
            Socket socket = (Socket)result.AsyncState;
            try
            {
                received = socket.EndReceive(result);
                data = new Byte[received];
                Array.Copy(this._dataBuffer, 
                           data, 
                           received);
                Array.Clear(this._dataBuffer, 
                            0, 
                            this.BufferSize);

                this.ProcessReceivedData(data, 
                                         socket);
                if (this.IsRunning)
                {
                    socket.BeginReceive(this._dataBuffer, 
                                        0, 
                                        this._dataBuffer.Length, 
                                        SocketFlags.None, 
                                        this.ReceiveCallback, 
                                        socket);
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
#pragma warning restore
        }

        private void SendCallback(IAsyncResult result)
        {
#pragma warning disable
            Socket? socket = null;
            try
            {
                socket = (Socket)result.AsyncState;
                socket.EndSend(result);
                if (this.IsRunning)
                {
                    socket.BeginReceive(this._dataBuffer, 
                                        0, 
                                        this._dataBuffer.Length, 
                                        SocketFlags.None, 
                                        this.ReceiveCallback, 
                                        socket);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
#pragma warning restore
        }

        private void SendInternal(Byte[] data, 
                                  Socket client, 
                                  Boolean broadcast)
        {
            try
            {
                if (!client.Connected)
                {
                    throw new NotConnectedException(client);
                }
                client.BeginSend(data, 
                                 0, 
                                 data.Length, 
                                 SocketFlags.None, 
                                 this.SendCallback, 
                                 client);
                if (!broadcast)
                {
                    Thread.Sleep(this.BroadcastInterval);
                }
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        private void ProcessReceivedData(Byte[] bytes, 
                                         Socket client)
        {
            if (client is null)
            {
                return;
            }
            T data = this._serializer.Deserialize(bytes, 
                                                  0);
            if (data.Equals(this.ShutdownCommand))
            {
                if (client.Connected)
                {
                    client.Close();
                }
                this.Clients.Remove(client);
                this.OnClientDisconnected(client, 
                                          ConnectionType.ConnectionClosed);
                return;
            }
            if (this.CommandProcessor is not null)
            {
                this.CommandProcessor.ProcessReceivedData(data, 
                                                          client);
            }
        }

        private void OnClientConnected(Socket client, 
                                       ConnectionType connection) => 
            this.ClientConnected?.Invoke(this, 
                                         new(client, 
                                             connection));

        internal void OnClientDisconnected(Socket client, 
                                           ConnectionType connection) => 
            this.ClientDisconnected?.Invoke(this, 
                                            new(client, 
                                                connection));

        private void OnException(Exception exception)
        {
            if (this.ForwardExceptions)
            {
                this.ExceptionOccurred?.Invoke(this, 
                                               new(exception));
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private readonly Socket _serverSocket;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ByteSerializer<T> _serializer = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerDataProcessor<T>? _processor;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<Boolean> _condition;
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Byte[] _dataBuffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Boolean _isRunning;

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String PROTOCOL_INVALID = "The specified protocol is not implemented for this client.";
#pragma warning restore
    }
}
