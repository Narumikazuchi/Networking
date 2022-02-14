namespace Narumikazuchi.Networking.Sockets;

/// <summary>
/// Represents an <see cref="IClient{TData}"/>, which communicates with <see cref="Server"/> objects through arrays of <see cref="Byte"/>.
/// </summary>
public sealed partial class Client
{
    /// <summary>
    /// Creates a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client CreateClient(in Int32 port,
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
                   processor: null);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="port">The port through which the connection shall be established.</param>
    /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
    /// <param name="processor">The processor, who handles the incoming <see cref="Byte"/>[] objects.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Client CreateClient(in Int32 port,
                                      in Int32 bufferSize,
                                      [DisallowNull] ClientDataProcessor processor)
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
                   processor: processor);
    }

    /// <summary>
    /// Disconnects the <see cref="Client"/> from the <see cref="Server"/>.
    /// </summary>
    /// <param name="raiseEvent">Whether or not to raise the <see cref="IClient{TData}.ConnectionClosed"/> event.</param>
    public void Disconnect(in Boolean raiseEvent) =>
        this.InitiateDisconnect(raiseEvent);
}

// Non-Public
partial class Client
{
    private Client(in Int32 port,
                   in Int32 bufferSize,
                   [AllowNull] IClientDataProcessor<Byte[]>? processor) :
        base(port: port,
             bufferSize: bufferSize,
             processor: processor)
    { }

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

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    private Int32 _currentAttempts = 0;

#pragma warning disable IDE1006
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const Int32 MAXATTEMPTS = 20;
#pragma warning restore
}

// ClientBase<Byte[]>
partial class Client : ClientBase<Byte[]>
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
    /// <exception cref="ObjectDisposedException"/>
    public override void Send([DisallowNull] Byte[] data)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);
        if (!this.IsConnected)
        {
            throw new NotConnectedException(socket: this.Socket);
        }

        this.InitiateSend(data);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    [return: NotNull]
    protected override Byte[] SerializeToBytes([DisallowNull] Byte[] data)
    {
        ExceptionHelpers.ThrowIfArgumentNull(data);
        return data;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    [return: NotNull]
    protected override Byte[] SerializeFromBytes([DisallowNull] Byte[] bytes)
    {
        ExceptionHelpers.ThrowIfArgumentNull(bytes);
        return bytes;
    }
}