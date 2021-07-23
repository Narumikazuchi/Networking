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
    public sealed class Server<T> where T : class, IByteSerializable, IEquatable<T>
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/> with default values.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, [DisallowNull] in T shutdownCommand) : this(protocol, 80, 2048, false, null, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, [DisallowNull] in T shutdownCommand) : this(protocol, port, 2048, false, null, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, null, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, forwardExecptions, null, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, ServerDataProcessor<T>? processor, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, processor, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, ServerDataProcessor<T> processor, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, forwardExecptions, processor, () => true, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, [DisallowNull] Func<Boolean> acceptCondition, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, null, acceptCondition, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, ServerDataProcessor<T>? processor, [DisallowNull] Func<Boolean> acceptCondition, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, processor, acceptCondition, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Server{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Server(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, ServerDataProcessor<T>? processor, [DisallowNull] Func<Boolean> acceptCondition, [DisallowNull] in T shutdownCommand)
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
            if (!EnumValidator.IsDefined(protocol))
            {
                throw new ArgumentException("The given protocol type is not a valid value from the enum.", nameof(protocol));
            }

            SocketType type = protocol is ProtocolType.Tcp ? SocketType.Stream : protocol is ProtocolType.Udp ? SocketType.Dgram : protocol is ProtocolType.Icmp or ProtocolType.Igmp ? SocketType.Raw : SocketType.Unknown;
            this._serverSocket = new Socket(AddressFamily.InterNetwork, type, protocol);
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

        #endregion

        #region Start

        /// <summary>
        /// Starts setting up the server as well as starts to listen for incoming connections.
        /// </summary>
        public void Start()
        {
            try
            {
                this._serverSocket.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                this._serverSocket.Listen(12);
                this.Clients.Clear();
                this._serverSocket.BeginAccept(new AsyncCallback(this.AcceptCallback), null);
                this._isRunning = true;
            }
            catch (Exception ex)
            {
                this._isRunning = false;
                this.OnException(ex);
            }
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops the running server.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (this.Clients.Count > 0)
                {
                    Byte[] bytes = this._serializer.Serialize(this.ShutdownCommand);
                    foreach (Socket client in this.Clients)
                    {
                        try
                        {
                            if (client.Connected)
                            {
                                this.SendData(bytes, client, true);
                                client.Close();
                                this.OnClientDisconnected(client, ConnectionType.ConnectionClosed);
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

        #endregion

        #region Callbacks

        private void AcceptCallback(IAsyncResult result)
        {
            Socket? socket = null;
            if (!this.AcceptCondition.Invoke())
            {
                return;
            }
            try
            {
                // Add new connection to client list.
                socket = this._serverSocket.EndAccept(result);
                this.Clients.Add(socket);
                this.OnClientConnected(socket, ConnectionType.ConnectionEstablished);
                if (this.IsRunning)
                {
                    // Received data goes to the DataBuffer
                    socket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), socket);
                    // Begin listening again.
                    this._serverSocket.BeginAccept(new AsyncCallback(this.AcceptCallback), null);
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
            Int32 received = 0;
            Byte[]? data = null;
#pragma warning disable
            Socket socket = (Socket)result.AsyncState;
            // Trim the received data, so there are no "null"-bytes in the resulting string
            try
            {
                received = socket.EndReceive(result);
#pragma warning restore
                data = new Byte[received];
                // Extracting the data from the buffer
                Array.Copy(this._dataBuffer, data, received);
                // Clearing buffer
                Array.Clear(this._dataBuffer, 0, this.BufferSize);

                // Process the received data
                this.ProcessReceivedData(data, socket);
                // Begin receiving anew
                if (this.IsRunning)
                {
                    socket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), socket);
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            Socket? socket = null;
            try
            {
#pragma warning disable
                socket = (Socket)result.AsyncState;
                socket.EndSend(result);
#pragma warning restore
                if (this.IsRunning)
                {
                    socket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), socket);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        #endregion

        #region Sending

        /// <summary>
        /// Sends the specified data to the specified client.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public void Send([DisallowNull] in T data, [DisallowNull] Socket client)
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
            this.SendData(bytes, client, false);
        }

        /// <summary>
        /// Sends the specified data to all connected clients.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public void SendBroadcast([DisallowNull] in T data)
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
                        this.SendData(bytes, client, true);
                    }
                }
                Thread.Sleep(this.BroadcastInterval);
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        private void SendData(Byte[] data, Socket client, Boolean broadcast)
        {
            try
            {
                if (!client.Connected)
                {
                    throw new NotConnectedException(client);
                }
                client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(this.SendCallback), client);
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

        #endregion

        #region Command processing

        private void ProcessReceivedData(Byte[] bytes, Socket client)
        {
            // ============================================================================================
            // No corresponding sender? Ignore.
            // ============================================================================================
            if (client is null)
            {
                return;
            }
            T data = this._serializer.Deserialize(bytes, 0);
            if (data.Equals(this.ShutdownCommand))
            {
                if (client.Connected)
                {
                    client.Close();
                }
                this.Clients.Remove(client);
                this.OnClientDisconnected(client, ConnectionType.ConnectionClosed);
                return;
            }
            // ============================================================================================
            // Process incoming data.
            // ============================================================================================
            if (this.CommandProcessor is not null)
            {
                this.CommandProcessor.ProcessReceivedData(data, client);
            }
        }

        #endregion

        #region Events

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
        public event Action<Server<T>, Exception>? ExceptionOccurred;

        private void OnClientConnected(Socket client, ConnectionType connection) => this.ClientConnected?.Invoke(this, new ConnectionEventArgs(client, connection));

        internal void OnClientDisconnected(Socket client, ConnectionType connection) => this.ClientDisconnected?.Invoke(this, new ConnectionEventArgs(client, connection));

        private void OnException(Exception e)
        {
            if (this.ForwardExceptions)
            {
                this.ExceptionOccurred?.Invoke(this, e);
            }
        }

        #endregion

        #region Properties

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
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Boolean IsRunning => this._isRunning;
        /// <summary>
        /// List of Clients that are currently connected.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        [DisallowNull]
        public ClientCollection Clients { get; }
        /// <summary>
        /// Determines the actions to be taken after the server receives data from any client.<para/>
        /// Is supposed to be a derived class from <see cref="ServerDataProcessor{T}"/>.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
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
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [DisallowNull]
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
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Boolean ForwardExceptions { get; set; }
        /// <summary>
        /// Gets or sets the size of the data buffer.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Int32 BufferSize
        {
            get => this._dataBuffer.Length;
            set => this._dataBuffer = new Byte[value];
        }
        /// <summary>
        /// Gets or sets the interval between individual broadcast sends. Default is 10ms.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Int32 BroadcastInterval { get; set; } = 10;
        /// <summary>
        /// Gets the <typeparamref name="T"/> object, which signals a server shutdown or client disconnect.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [DisallowNull]
        public T ShutdownCommand { get; }

        #endregion

        #region Fields

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

        #endregion
    }
}
