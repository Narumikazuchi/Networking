namespace Narumikazuchi.Networking.Sockets;

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
                         [AllowNull] ServerAcceptCondition<TData> condition)
    {
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(paramName: nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(paramName: nameof(bufferSize));
        }

        this._socket = new(addressFamily: AddressFamily.InterNetwork,
                           socketType: SocketType.Stream,
                           protocolType: ProtocolType.Tcp);
        this._isRunning = false;
        this._condition = condition;
        this._dataBuffer = new Byte[bufferSize];
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
            throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
        }

        if (this.IsRunning)
        {
            return;
        }
        this._socket = new(addressFamily: AddressFamily.InterNetwork,
                           socketType: SocketType.Stream,
                           protocolType: ProtocolType.Tcp);

        try
        {
            this._socket
                .Bind(localEP: new IPEndPoint(address: IPAddress.Any,
                                              port: this.Port));
            this._socket
                .Listen(backlog: 12);
            this._clients
                .Clear();
            this._socket
                .BeginAccept(callback: this.AcceptCallback,
                             state: null);
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
            throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
        }

        if (this.Clients
                .Count > 0)
        {
            foreach (KeyValuePair<Guid, Socket> kv in this._clients)
            {
                if (kv.Value
                      .Connected)
                {
                    try
                    {
                        this.InitiateSend(bytes: _shutdownSignature,
                                          client: kv.Value);
                    }
                    catch (SocketException) { }
                    kv.Value
                      .Close();
                    this.OnClientDisconnected(client: kv.Key,
                                              connection: ConnectionType.ConnectionClosed);
                }
            }
        }
        this._clients
            .Clear();
        this._socket
            .Close();
        this._socket
            .Dispose();
        this._isRunning = false;
    }

    /// <summary>
    /// Initiates and performs the disconnect from the specified client socket.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    protected Boolean InitiateDisconnect(in Guid guid)
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
        }

        if (!this._clients
                 .ContainsKey(guid))
        {
            return false;
        }

        if (!this._clients[guid]
                 .Connected)
        {
            this._clients
                .Remove(guid);
            this.OnClientDisconnected(client: guid,
                                      connection: ConnectionType.ConnectionLost);
            return true;
        }

        this.InitiateSend(bytes: _shutdownSignature,
                          client: this._clients[guid]);
        this._clients[guid]
            .Close();
        this._clients
            .Remove(guid);
        this.OnClientDisconnected(client: guid,
                                  connection: ConnectionType.ConnectionLost);
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
            throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
        }
        ExceptionHelpers.ThrowIfArgumentNull(data);
        if (!this._clients
                 .ContainsKey(client))
        {
            throw new KeyNotFoundException();
        }

        Socket socket = this._clients[client];
        Byte[] bytes = this.SerializeToBytes(data);
        this.InitiateSend(bytes: bytes,
                          client: socket);
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
            throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
        }
        ExceptionHelpers.ThrowIfArgumentNull(data);

        Byte[] bytes = this.SerializeToBytes(data);
        this.InitiateBroadcast(bytes: bytes);
    }

    /// <summary>
    /// Gets the <see cref="System.Net.Sockets.Socket"/> that this <see cref="IServer{TData}"/> uses to send data to any <see cref="IClient{TData}"/>.
    /// </summary>
    protected Socket Socket => 
        this._socket;

    private void InitiateSend([DisallowNull] Byte[] bytes,
                              Socket client)
    {
        if (!client.Connected)
        {
            throw new NotConnectedException(socket: client);
        }
        client.BeginSend(buffer: bytes,
                         offset: 0,
                         size: bytes.Length,
                         socketFlags: SocketFlags.None,
                         callback: this.SendCallback,
                         state: client);
    }

    private void InitiateBroadcast([DisallowNull] Byte[] bytes)
    {
        foreach (Socket client in this._clients
                                      .Values)
        {
            if (!client.Connected)
            {
                continue;
            }
            client.BeginSend(buffer: bytes,
                             offset: 0,
                             size: bytes.Length,
                             socketFlags: SocketFlags.None,
                             callback: this.SendCallback,
                             state: client);
            Thread.Sleep(millisecondsTimeout: 1);
        }
    }

    private void AcceptCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = this._socket
                                .EndAccept(asyncResult: result);
            Guid guid = Guid.NewGuid();

            if (this._condition is not null &&
                !this._condition
                     .Invoke(server: this,
                             guidOfNewClient: guid))
            {
                socket.Close();
                return;
            }

            this._clients
                .Add(key: guid, 
                     value: socket);
            this.OnClientConnected(client: guid,
                                   connection: ConnectionType.ConnectionEstablished);
            socket.BeginReceive(buffer: this._dataBuffer,
                                offset: 0,
                                size: this._dataBuffer
                                          .Length,
                                socketFlags: SocketFlags.None,
                                callback: this.ReceiveCallback,
                                state: socket);
            this.InitiateSend(bytes: _guidSignature.Concat(guid.ToByteArray())
                                                   .ToArray(),
                              client: socket);
            if (this.IsRunning)
            {
                this._socket
                    .BeginAccept(callback: this.AcceptCallback,
                                 state: null);
            }
        }
        catch (ObjectDisposedException) { }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState!;
            Int32 received = socket.EndReceive(asyncResult: result);
            Byte[] data = new Byte[received];
            Array.Copy(sourceArray: this._dataBuffer,
                       sourceIndex: 0,
                       destinationArray: data,
                       destinationIndex: 0,
                       length: received);
            Array.Clear(array: this._dataBuffer,
                        index: 0,
                        length: received);

            this.ProcessIncomingData(bytes: data,
                                     client: socket);
            if (this.IsRunning)
            {
                socket.BeginReceive(buffer: this._dataBuffer,
                                    offset: 0,
                                    size: this._dataBuffer
                                              .Length,
                                    socketFlags: SocketFlags.None,
                                    callback: this.ReceiveCallback,
                                    state: socket);
            }
        }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }
    }

    private void SendCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState!;
            socket.EndSend(asyncResult: result);
            if (this.IsRunning)
            {
                socket.BeginReceive(buffer: this._dataBuffer,
                                    offset: 0,
                                    size: this._dataBuffer
                                              .Length,
                                    socketFlags: SocketFlags.None,
                                    callback: this.ReceiveCallback,
                                    state: socket);
            }
        }
        catch (ObjectDisposedException) { }
    }

    private void ProcessIncomingData(Byte[] bytes,
                                     Socket client)
    {
        ExceptionHelpers.ThrowIfArgumentNull(bytes);

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
                if (!this._clients
                         .ContainsValue(client))
                {
                    return;
                }
                guid = this._clients
                           .First(kv => kv.Value == client)
                           .Key;
                this._clients
                    .Remove(guid);
                this.OnClientDisconnected(client: guid,
                                          connection: ConnectionType.ConnectionLost);
                return;
            }
        }

        if (!this._clients
                 .ContainsValue(client))
        {
            return;
        }
        guid = this._clients
                   .First(kv => kv.Value == client)
                   .Key;
        TData data = this.SerializeFromBytes(bytes);
        if (this.DataProcessor is null)
        {
            this.DataReceived?.Invoke(sender: this,
                                      eventArgs: new(data: data,
                                                     fromClient: guid));
            return;
        }
        this.DataProcessor
            .ProcessReceivedData(data: data,
                                 fromClient: guid);
    }

    private void OnClientConnected(Guid client,
                                   ConnectionType connection) =>
        this.ClientConnected?
            .Invoke(sender: this,
                    eventArgs: new(whichClient: client,
                                type: connection));

    private void OnClientDisconnected(Guid client,
                                      ConnectionType connection) =>
        this.ClientDisconnected?
            .Invoke(sender: this,
                    eventArgs: new(whichClient: client,
                                    type: connection));

    private Socket _socket;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<Guid, Socket> _clients = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IServerDataProcessor<TData>? _processor;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ServerAcceptCondition<TData>? _condition;
    private Byte[] _dataBuffer;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Boolean _isRunning;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Byte[] _shutdownSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x53, 0x68, 0x75, 0x74, 0x64, 0x6F, 0x77, 0x6E, 0x53, 0x65, 0x72, 0x65, 0x72, 0x43, 0x6C, 0x65, 0x68, 0x2E, 0xBE, 0x67 };
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Byte[] _guidSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x47, 0x75, 0x69, 0x64 };
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
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }
            return this._dataBuffer
                       .Length;
        }
        set
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }
            this._dataBuffer = new Byte[value];
        }
    }

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    [NotNull]
    public IReadOnlyList<Guid> Clients
    {
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }
            return this._clients
                       .Keys
                       .ToList();
        }
    }

    /// <inheritdoc/>
    [MaybeNull]
    public IServerDataProcessor<TData>? DataProcessor
    {
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }
            return this._processor;
        }
        set
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }

            this._processor = value;
            if (this._processor is not null)
            {
                this._processor
                    .Server = this;
            }
        }
    }

    /// <inheritdoc/>
    [MaybeNull]
    public ServerAcceptCondition<TData>? AcceptCondition
    {
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }
            return this._condition;
        }
        set
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ServerBase<TData>));
            }

            if (value is null)
            {
                throw new ArgumentNullException(paramName: nameof(value));
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

        this._disposed = true;
        GC.SuppressFinalize(this);
    }

    private Boolean _disposed = false;
}