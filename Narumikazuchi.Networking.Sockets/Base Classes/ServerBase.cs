using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the shared base functionality for sending data to multiple <see cref="IClient{TData}"/> objects.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract partial class ServerBase<TData>
    {
        /// <summary>
        /// Gets whether the <see cref="IServer{TData}"/> allows connections from <see cref="IClient{TData}"/> objects.
        /// </summary>
        public Boolean IsRunning =>
            this._isRunning;
    }

    // Non-Public
    partial class ServerBase<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBase{TData}"/> class.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        protected ServerBase(in Int32 port,
                             in Int32 bufferSize,
                             [AllowNull] IServerDataProcessor<TData>? processor,
                             [DisallowNull] Func<Boolean> acceptCondition)
        {
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

            this._serverSocket = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);
            this._isRunning = false;
            this._dataBuffer = new Byte[bufferSize];
            this._condition = acceptCondition;
            this.Port = port;
            this.DataProcessor = processor;
        }

        /// <summary>
        /// Serializes the specified <typeparamref name="TData"/> into a <see cref="Byte"/>[] representation, which in turn can be send over the socket connection.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <returns>The <see cref="Byte"/>[] representation of the specified <typeparamref name="TData"/></returns>
        [return: NotNull]
        protected abstract Byte[] SerializeToBytes([DisallowNull] TData data);

        /// <summary>
        /// Serializes the specified <see cref="Byte"/>[] representation back into it's <typeparamref name="TData"/> object.
        /// </summary>
        /// <param name="bytes">The <see cref="Byte"/>[] representation to serialize.</param>
        /// <returns>The <typeparamref name="TData"/> object serialized from the specified <see cref="Byte"/>[] representation</returns>
        [return: NotNull]
        protected abstract TData SerializeFromBytes([DisallowNull] Byte[] bytes);

        /// <summary>
        /// Initiates the connections accept and data receive pipeline.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        protected void InitiateStart()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(ServerBase<TData>));
            }

            try
            {
                this._serverSocket.Bind(new IPEndPoint(IPAddress.Any,
                                                       this.Port));
                this._serverSocket.Listen(12);
                this._clients.Clear();
                this._serverSocket.BeginAccept(this.AcceptCallback,
                                               null);
                this._isRunning = true;
            }
            catch
            {
                this._isRunning = false;
            }
        }

        /// <summary>
        /// Initiates and performs the disconnect from all sockets.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        protected void InitiateStop()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(ServerBase<TData>));
            }

            if (this.Clients
                    .Count > 0)
            {
                foreach (KeyValuePair<Guid, Socket> kv in this._clients)
                {
                    if (kv.Value.Connected)
                    {
                        try
                        {
                            this.InitiateSend(_shutdownSignature,
                                              kv.Value);
                        }
                        catch (SocketException) { }
                        kv.Value.Close();
                        this.OnClientDisconnected(kv.Key,
                                                  ConnectionType.ConnectionClosed);
                    }
                }
            }
            this._clients.Clear();
            this._serverSocket.Close();
            this._isRunning = false;
            this.Dispose();
        }

        /// <summary>
        /// Initiates and performs the disconnect from the specified client socket.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        protected Boolean InitiateDisconnect(in Guid guid)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(ServerBase<TData>));
            }

            if (!this._clients.ContainsKey(guid))
            {
                return false;
            }

            if (!this._clients[guid].Connected)
            {
                this._clients.Remove(guid);
                this.OnClientDisconnected(guid,
                                          ConnectionType.ConnectionLost);
                return true;
            }

            this.InitiateSend(_shutdownSignature,
                              this._clients[guid]);
            this._clients[guid].Close();
            this._clients.Remove(guid);
            this.OnClientDisconnected(guid,
                                      ConnectionType.ConnectionLost);
            return true;
        }

        /// <summary>
        /// Initiates the process for sending data over the socket connection to the specified client.
        /// </summary>
        /// <param name="data">The data to send to the client.</param>
        /// <param name="client">The guid associated with an <see cref="IClient{TData}"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="ObjectDisposedException"/>
        protected void InitiateSend([DisallowNull] TData data,
                                    Guid client)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(ServerBase<TData>));
            }

            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (!this._clients.ContainsKey(client))
            {
                throw new KeyNotFoundException();
            }
            Socket socket = this._clients[client];
            Byte[] bytes = this.SerializeToBytes(data);
            this.InitiateSend(bytes,
                              socket);
        }

        /// <summary>
        /// Initiates the process for sending data over the socket connection to the all connected clients.
        /// </summary>
        /// <param name="data">The data to send to the clients.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        protected void InitiateBroadcast([DisallowNull] TData data)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(ServerBase<TData>));
            }

            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            Byte[] bytes = this.SerializeToBytes(data);
            this.InitiateBroadcast(bytes);
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.Socket"/> that this <see cref="IServer{TData}"/> uses to send data to any <see cref="IClient{TData}"/>.
        /// </summary>
        protected Socket Socket => this._serverSocket;

        private void InitiateSend([DisallowNull] Byte[] bytes,
                                  Socket client)
        {
            if (!client.Connected)
            {
                throw new NotConnectedException(client);
            }
            client.BeginSend(bytes,
                             0,
                             bytes.Length,
                             SocketFlags.None,
                             this.SendCallback,
                             client);
        }

        private void InitiateBroadcast([DisallowNull] Byte[] bytes)
        {
            foreach (Socket client in this._clients.Values)
            {
                if (!client.Connected)
                {
                    continue;
                }
                client.BeginSend(bytes,
                                 0,
                                 bytes.Length,
                                 SocketFlags.None,
                                 this.SendCallback,
                                 client);
                Thread.Sleep(5);
            }
        }

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
                Guid guid = Guid.NewGuid();
                this._clients.Add(guid, socket);
                this.OnClientConnected(guid,
                                       ConnectionType.ConnectionEstablished);
                socket.BeginReceive(this._dataBuffer,
                                    0,
                                    this._dataBuffer.Length,
                                    SocketFlags.None,
                                    this.ReceiveCallback,
                                    socket);
                this.InitiateSend(_guidSignature.Concat(guid.ToByteArray())
                                                .ToArray(),
                                  socket);
                if (this.IsRunning)
                {
                    this._serverSocket.BeginAccept(this.AcceptCallback,
                                                   null);
                }
            }
            catch (ObjectDisposedException) { }
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

                this.ProcessIncomingData(data,
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
#pragma warning restore
        }

        private void ProcessIncomingData(Byte[] bytes,
                                         Socket client)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (client is null)
            {
                return;
            }

            Guid guid;
            if (bytes.Length == 64)
            {
                if (bytes.SequenceEqual(_shutdownSignature))
                {
                    if (client.Connected)
                    {
                        client.Close();
                    }
                    if (!this._clients.ContainsValue(client))
                    {
                        return;
                    }
                    guid = this._clients.First(kv => kv.Value == client).Key;
                    this._clients.Remove(guid);
                    this.OnClientDisconnected(guid,
                                              ConnectionType.ConnectionLost);
                    return;
                }
            }

            if (!this._clients.ContainsValue(client))
            {
                return;
            }
            guid = this._clients.First(kv => kv.Value == client).Key;
            TData data = this.SerializeFromBytes(bytes);
            if (this.DataProcessor is null)
            {
                this.DataReceived?.Invoke(this,
                                          new(data,
                                              guid));
                return;
            }
            this.DataProcessor.ProcessReceivedData(data,
                                                      guid);
        }

        private void OnClientConnected(Guid client,
                                       ConnectionType connection) =>
            this.ClientConnected?.Invoke(this,
                                         new(client,
                                             connection));

        private void OnClientDisconnected(Guid client,
                                          ConnectionType connection) =>
            this.ClientDisconnected?.Invoke(this,
                                            new(client,
                                                connection));

        private readonly Socket _serverSocket;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Dictionary<Guid, Socket> _clients = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IServerDataProcessor<TData>? _processor;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<Boolean> _condition;
        private Byte[] _dataBuffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Boolean _isRunning;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Byte[] _shutdownSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x53, 0x68, 0x75, 0x74, 0x64, 0x6F, 0x77, 0x6E, 0x53, 0x65, 0x72, 0x65, 0x72, 0x43, 0x6C, 0x65, 0x68, 0x2E, 0xBE, 0x67 };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Byte[] _guidSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x47, 0x75, 0x69, 0x64 };
#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String PROTOCOL_INVALID = "The specified protocol is not implemented for this client.";
#pragma warning restore
    }

    // IServer<TData>
    partial class ServerBase<TData> : IServer<TData>
    {
        /// <inheritdoc/>
        public abstract void Start();

        /// <inheritdoc/>
        public abstract void Stop();

        /// <inheritdoc/>
        public abstract Boolean Disconnect(in Guid guid);

        /// <inheritdoc/>
        public abstract void Send([DisallowNull] TData data,
                                  in Guid client);

        /// <inheritdoc/>
        public abstract void Broadcast([DisallowNull] TData data);

        /// <inheritdoc/>
        public event EventHandler<IServer<TData>, ConnectionEventArgs>? ClientConnected;
        /// <inheritdoc/>
        public event EventHandler<IServer<TData>, ConnectionEventArgs>? ClientDisconnected;
        /// <inheritdoc/>
        public event EventHandler<IServer<TData>, DataReceivedEventArgs<TData>>? DataReceived;

        /// <inheritdoc/>
        public Int32 Port { get; }
        /// <inheritdoc/>
        public Int32 BufferSize
        {
            get => this._disposed
                    ? throw new ObjectDisposedException(nameof(ServerBase<TData>))
                    : this._dataBuffer.Length;
            set => this._dataBuffer = this._disposed
                                        ? throw new ObjectDisposedException(nameof(ServerBase<TData>))
                                        : new Byte[value];
        }
        /// <inheritdoc/>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        [NotNull]
        public IReadOnlyList<Guid> Clients =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(ServerBase<TData>))
                : this._clients.Keys.ToList();
        /// <inheritdoc/>
        [MaybeNull]
        public IServerDataProcessor<TData>? DataProcessor
        {
            get => this._disposed
                    ? throw new ObjectDisposedException(nameof(ServerBase<TData>))
                    : this._processor;
            set
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(nameof(ServerBase<TData>));
                }

                this._processor = value;
                if (this._processor is not null)
                {
                    this._processor.Server = this;
                }
            }
        }
        /// <inheritdoc/>
        [NotNull]
        public Func<Boolean> AcceptCondition
        {
            get => this._disposed
                    ? throw new ObjectDisposedException(nameof(ServerBase<TData>))
                    : this._condition;
            set
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(nameof(ServerBase<TData>));
                }

                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._condition = value;
            }
        }
    }

    // IDisposable
    partial class ServerBase<TData> : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            if (this._isRunning)
            {
                this.Stop();
            }

            this._serverSocket.Dispose();
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        private Boolean _disposed = false;
    }
}
