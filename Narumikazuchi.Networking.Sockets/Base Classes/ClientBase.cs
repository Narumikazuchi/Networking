namespace Narumikazuchi.Networking.Sockets;

/// <summary>
/// Provides the shared base functionality for sending data to an <see cref="IServer{TData}"/>.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract partial class ClientBase<TData>
{
    /// <summary>
    /// Gets whether the <see cref="IClient{TData}"/> is connected to an <see cref="IServer{TData}"/> at the moment.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    public Boolean IsConnected
    {
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
            }
            return !this._guid
                        .Equals(default) &&
                   this._socket
                       .Connected;
        }
    }
}

// Non-Public
partial class ClientBase<TData>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBase{TData}"/> class.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    protected ClientBase(in Int32 port,
                         in Int32 bufferSize,
                         [AllowNull] IClientDataProcessor<TData>? processor)
    {
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        this._socket = new Socket(addressFamily: AddressFamily.InterNetwork,
                                  socketType: SocketType.Stream,
                                  protocolType: ProtocolType.Tcp);
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
    /// Initiates the data receive pipeline.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    protected void InitiateSocket()
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
        }

        this._socket = new Socket(addressFamily: AddressFamily.InterNetwork,
                                  socketType: SocketType.Stream,
                                  protocolType: ProtocolType.Tcp);
    }

    /// <summary>
    /// Initiates the data receive pipeline.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    protected void InitiateConnection()
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
        }

        this._socket
            .BeginReceive(buffer: this._dataBuffer,
                          offset: 0,
                          size: this._dataBuffer
                                    .Length,
                          socketFlags: SocketFlags.None,
                          callback: this.ReceiveCallback,
                          state: null);
    }

    /// <summary>
    /// Initiates and performs the disconnect from the server socket.
    /// </summary>
    /// <param name="raiseEvent">Whether or not the <see cref="IClient{TData}.ConnectionClosed"/> event shall be raised.</param>
    /// <exception cref="ObjectDisposedException"/>
    protected void InitiateDisconnect(in Boolean raiseEvent)
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
        }

        if (this.IsConnected)
        {
            this._socket
                .Close();
            if (raiseEvent)
            {
                this.OnConnectionClosed();
            }
        }
    }

    /// <summary>
    /// Initiates the process for sending data over the socket connection to the server.
    /// </summary>
    /// <param name="data">The data to send to the server.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ObjectDisposedException"/>
    protected void InitiateSend([DisallowNull] TData data)
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
        }
        ExceptionHelpers.ThrowIfArgumentNull(data);

        Byte[] bytes = this.SerializeToBytes(data);
        this._socket
            .BeginSend(buffer: bytes,
                       offset: 0,
                       size: bytes.Length,
                       socketFlags: SocketFlags.None,
                       callback: this.SendCallback,
                       state: null);
        Thread.Sleep(1);
    }

    /// <summary>
    /// Gets the <see cref="System.Net.Sockets.Socket"/> that this <see cref="IClient{TData}"/> uses to send data to an <see cref="IServer{TData}"/>.
    /// </summary>
    protected Socket Socket => 
        this._socket;

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            Int32 received = this._socket
                                 .EndReceive(asyncResult: result);
            Byte[]  data = new Byte[received];
            Array.Copy(sourceArray: this._dataBuffer,
                       sourceIndex: 0,
                       destinationArray: data,
                       destinationIndex: 0,
                       length: received);
            Array.Clear(array: this._dataBuffer,
                        index: 0,
                        length: received);

            this.ProcessIncomingData(data);

            this._socket
                .BeginReceive(buffer: this._dataBuffer,
                              offset: 0,
                              size: this._dataBuffer
                                        .Length,
                              socketFlags: SocketFlags.None,
                              callback: this.ReceiveCallback,
                              state: null);
        }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }
    }

    private void SendCallback(IAsyncResult result)
    {
        try
        {
            this._socket
                .EndSend(asyncResult: result);
            if (!this.IsConnected)
            {
                return;
            }
            this._socket
                .BeginReceive(buffer: this._dataBuffer,
                              offset: 0,
                              size: this._dataBuffer
                                        .Length,
                              socketFlags: SocketFlags.None,
                              callback: this.ReceiveCallback,
                              state: null);
        }
        catch (ObjectDisposedException) { }
    }

    private void ProcessIncomingData(Byte[] bytes)
    {
        ExceptionHelpers.ThrowIfArgumentNull(bytes);

        if (bytes.Length == 64)
        {
            if (bytes.SequenceEqual(_shutdownSignature))
            {
                this.InitiateDisconnect(true);
                return;
            }
            if (bytes.Take(48)
                     .SequenceEqual(_guidSignature))
            {
                Byte[] guidBytes = bytes.Skip(48)
                                        .Take(16)
                                        .ToArray();
                this._guid = new(b: guidBytes);
                this.OnConnectionEstablished();
                return;
            }
        }

        TData data = this.SerializeFromBytes(bytes);
        if (this.DataProcessor is null)
        {
            this.DataReceived?
                .Invoke(sender: this,
                        eventArgs: new(data: data));
            return;
        }
        this.DataProcessor
            .ProcessReceivedData(data: data);
    }

    private void OnConnectionEstablished() =>
        this.ConnectionEstablished?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);

    private void OnConnectionClosed() =>
        this.ConnectionClosed?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    private Socket _socket;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IClientDataProcessor<TData>? _processor;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Guid _guid = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    private Byte[] _dataBuffer;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Byte[] _shutdownSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x53, 0x68, 0x75, 0x74, 0x64, 0x6F, 0x77, 0x6E, 0x53, 0x65, 0x72, 0x65, 0x72, 0x43, 0x6C, 0x65, 0x68, 0x2E, 0xBE, 0x67 };
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Byte[] _guidSignature = new Byte[] { 0x72, 0x63, 0x53, 0x2E, 0x6D, 0x6E, 0x75, 0x74, 0x9C, 0x63, 0x5A, 0x78, 0x68, 0x2E, 0xBE, 0x67, 0xE4, 0x75, 0x69, 0x69, 0x6B, 0x65, 0x77, 0x74, 0x6F, 0x6B, 0xEE, 0x2E, 0x61, 0x4E, 0x77, 0x5, 0x61, 0xC2, 0x6B, 0x4E, 0x65, 0x73, 0xD1, 0x6F, 0x53, 0xF7, 0x7A, 0x86, 0x47, 0x75, 0x69, 0x64 };
}

// IClient<TData>
partial class ClientBase<TData> : IClient<TData>
{
    /// <inheritdoc/>
    public abstract void Connect([DisallowNull] IPAddress address);

    /// <inheritdoc/>
    public abstract void Disconnect();

    /// <inheritdoc/>
    public abstract void Send([DisallowNull] TData data);

    /// <inheritdoc/>
    public event EventHandler<IClient<TData>>? ConnectionEstablished;

    /// <inheritdoc/>
    public event EventHandler<IClient<TData>>? ConnectionClosed;

    /// <inheritdoc/>
    public event EventHandler<IClient<TData>, DataReceivedEventArgs<TData>>? DataReceived;

    /// <inheritdoc/>
    public Int32 Port { get; }

    /// <inheritdoc/>
    public Guid Guid => 
        this._guid;

    /// <inheritdoc/>
    public Int32 BufferSize
    {
        get

        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
            }
            return this._dataBuffer
                       .Length;
        }
        set
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
            }
            this._dataBuffer = new Byte[value];
        }
    }

    /// <inheritdoc/>
    [MaybeNull]
    public IClientDataProcessor<TData>? DataProcessor
    {
        get

        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
            }
            return this._processor;
        }
        set
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(objectName: nameof(ClientBase<TData>));
            }

            this._processor = value;
            if (this._processor is not null)
            {
                this._processor
                    .Client = this;
            }
        }
    }
}

// IDisposable
partial class ClientBase<TData> : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        if (this.IsConnected)
        {
            this.InitiateDisconnect(false);
        }

        this._disposed = true;
        GC.SuppressFinalize(this);
    }

    private Boolean _disposed = false;
}