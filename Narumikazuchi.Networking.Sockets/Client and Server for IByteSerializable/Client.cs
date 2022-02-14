namespace Narumikazuchi.Networking.Sockets;

/// <summary>
/// Represents an <see cref="IClient{TData}"/>, which communicates with <see cref="Server{TMessage}"/> objects through an <see cref="ISerializable"/> message class.
/// </summary>
public sealed partial class Client<TMessage>
    where TMessage : IDeserializable<TMessage>, ISerializable
{
    /// <summary>
    /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client<TMessage> CreateClient(in Int32 port,
                                                in Int32 bufferSize)
    {
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        return new(port: port,
                   bufferSize: bufferSize,
                   processor: null,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client<TMessage> CreateClient(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        return new(port: port,
                   bufferSize: bufferSize,
                   processor: null,
                   strategies: strategies);
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client<TMessage> CreateClient(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ClientDataProcessor<TMessage> processor)
    {
        ExceptionHelpers.ThrowIfArgumentNull(processor);
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        return new(port: port,
                   bufferSize: bufferSize,
                   processor: processor,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client<TMessage> CreateClient(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ClientDataProcessor<TMessage> processor,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(processor);
        if (port < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        return new(port: port,
                   bufferSize: bufferSize,
                   processor: processor,
                   strategies: strategies);
    }

    /// <summary>
    /// Disconnects the <see cref="Client{TMessage}"/> from the <see cref="Server{TMessage}"/>.
    /// </summary>
    /// <param name="raiseEvent">Whether or not to raise the <see cref="IClient{TData}.ConnectionClosed"/> event.</param>
    public void Disconnect(in Boolean raiseEvent) =>
        this.InitiateDisconnect(raiseEvent);
}

// Non-Public
partial class Client<TMessage>
{
    private Client(in Int32 port,
                   in Int32 bufferSize,
                   [AllowNull] IClientDataProcessor<TMessage>? processor,
                   [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies) :
        base(port: port,
             bufferSize: bufferSize,
             processor: processor)
    {
        this._serializer = CreateByteSerializer
                          .ForSerializationAndDeserialization()
                          .ConfigureForOwnedType<TMessage>()
                          .UseDefaultStrategies()
                          .UseStrategies(strategies)
                          .Construct();
    }

    private void LoopConnect(IPAddress address)
    {
        while (!this.Socket
                    .Connected &&
                this._currentAttempts < MAXATTEMPTS)
        {
            try
            {
                this._currentAttempts++;
                this.Socket
                    .Connect(address: address,
                             port: this.Port);
            }
            catch (SocketException) { }
        }
        if (this._currentAttempts == MAXATTEMPTS &&
            !this.Socket
                 .Connected)
        {
            throw new MaximumAttemptsExceededException();
        }
        if (this.Socket
                .Connected)
        {
            this.InitiateConnection();
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IByteSerializerDeserializer<TMessage> _serializer;
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    private Int32 _currentAttempts = 0;

#pragma warning disable IDE1006
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const Int32 MAXATTEMPTS = 20;
#pragma warning restore
}

// ClientBase<TMessage>
partial class Client<TMessage> : ClientBase<TMessage>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Connect([DisallowNull] IPAddress address)
    {
        ExceptionHelpers.ThrowIfArgumentNull(address);

        if (this.IsConnected)
        {
            return;
        }

        this.InitiateSocket();
        this._currentAttempts = 0;
        this.LoopConnect(address);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Disconnect() =>
        this.InitiateDisconnect(true);

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public override void Send([DisallowNull] TMessage data)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);
        if (!this.IsConnected)
        {
            throw new NotConnectedException(this.Socket);
        }

        this.InitiateSend(data);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    [return: NotNull]
    protected override Byte[] SerializeToBytes([DisallowNull] TMessage data)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);

        using MemoryStream stream = new();
        this._serializer
            .Serialize(stream: stream,
                       graph: data);
        return stream.ToArray();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    [return: NotNull]
    protected override TMessage SerializeFromBytes([DisallowNull] Byte[] bytes)
    {
        ExceptionHelpers.ThrowIfArgumentNull(bytes);

        using MemoryStream stream = new(buffer: bytes);
        stream.Position = 0;
        return this._serializer
                   .Deserialize(stream: stream)!;
    }
}