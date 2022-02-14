namespace Narumikazuchi.Networking.Sockets;

/// <summary>
/// Represents an <see cref="IServer{TData}"/>, which communicates with <see cref="Client{TMessage}"/> objects through an <see cref="ISerializable"/> message class.
/// </summary>
public sealed partial class Server<TMessage>
    where TMessage : IDeserializable<TMessage>, ISerializable
{
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
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
                   acceptCondition: null,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(strategies);
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
                   acceptCondition: null,
                   strategies: strategies);
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="acceptCondition">The condition for a connection <see cref="Client{TMessage}"/> to be accepted.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerAcceptCondition<TMessage> acceptCondition)
    {
        ExceptionHelpers.ThrowIfArgumentNull(acceptCondition);
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
                   acceptCondition: acceptCondition,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="acceptCondition">The condition for a connection <see cref="Client{TMessage}"/> to be accepted.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerAcceptCondition<TMessage> acceptCondition,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(acceptCondition);
        ExceptionHelpers.ThrowIfArgumentNull(strategies);
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
                   acceptCondition: acceptCondition,
                   strategies: strategies);
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerDataProcessor<TMessage> processor)
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
                   acceptCondition: null,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerDataProcessor<TMessage> processor,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(processor);
        ExceptionHelpers.ThrowIfArgumentNull(strategies);
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
                   acceptCondition: null,
                   strategies: strategies);
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <param name="acceptCondition">The condition for a connection <see cref="Client{TMessage}"/> to be accepted.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerDataProcessor<TMessage> processor,
                                                [DisallowNull] ServerAcceptCondition<TMessage> acceptCondition)
    {
        ExceptionHelpers.ThrowIfArgumentNull(processor);
        ExceptionHelpers.ThrowIfArgumentNull(acceptCondition);
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
                   acceptCondition: acceptCondition,
                   strategies: Array.Empty<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>>());
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Server{TMessage}"/> class.
    /// </summary>
    /// <param name="port">The port through which the connections shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
    /// <param name="acceptCondition">The condition for a connection <see cref="Client{TMessage}"/> to be accepted.</param>
    /// <param name="strategies">The strategies to use for unhandled tpyes during serialization.</param>
    /// <returns>A new <see cref="Server{TMessage}"/> instance with the specified parameters</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [return: NotNull]
    public static Server<TMessage> CreateServer(in Int32 port,
                                                in Int32 bufferSize,
                                                [DisallowNull] ServerDataProcessor<TMessage> processor,
                                                [DisallowNull] ServerAcceptCondition<TMessage> acceptCondition,
                                                [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(processor);
        ExceptionHelpers.ThrowIfArgumentNull(acceptCondition);
        ExceptionHelpers.ThrowIfArgumentNull(strategies);
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
                   acceptCondition: acceptCondition,
                   strategies: strategies);
    }
}

// Non-Public
partial class Server<TMessage>
{
    private Server(in Int32 port,
                   in Int32 bufferSize,
                   [AllowNull] IServerDataProcessor<TMessage>? processor,
                   [AllowNull] ServerAcceptCondition<TMessage>? acceptCondition,
                   [DisallowNull] IEnumerable<KeyValuePair<Type, ISerializationDeserializationStrategy<Byte[]>>> strategies) :
        base(port,
             bufferSize,
             processor,
             acceptCondition)
    {
        this._serializer = CreateByteSerializer
                          .ForSerializationAndDeserialization()
                          .ConfigureForOwnedType<TMessage>()
                          .UseDefaultStrategies()
                          .UseStrategies(strategies)
                          .Construct();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IByteSerializerDeserializer<TMessage> _serializer;
}

// ServerBase<TMessage>
partial class Server<TMessage> : ServerBase<TMessage>
{
    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Start() =>
        this.InitiateStart();

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Stop() =>
        this.InitiateStop();

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException"/>
    public override Boolean Disconnect(in Guid guid) =>
        this.InitiateDisconnect(guid);

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Send([DisallowNull] TMessage data,
                              in Guid client)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);

        this.InitiateSend(data: data,
                          client: client);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ObjectDisposedException"/>
    public override void Broadcast([DisallowNull] TMessage data)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);

        this.InitiateBroadcast(data);
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
        return this._serializer
                   .Deserialize(stream: stream)!;
    }
}