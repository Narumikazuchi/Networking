namespace Narumikazuchi.Networking;

/// <summary>
/// Represents the base functionality of a socket client.
/// </summary>
public interface IClient<TData> : 
    IDisposable
{
    /// <summary>
    /// Connects the <see cref="IClient{TData}"/> to the specified IP-Address.
    /// </summary>
    /// <param name="address">The address to connect to.</param>
    public void Connect([DisallowNull] IPAddress address);

    /// <summary>
    /// Disconnects the <see cref="IClient{TData}"/> from it's current connection to an <see cref="IServer{TData}"/>.
    /// </summary>
    public void Disconnect();

    /// <summary>
    /// Sends the specified <typeparamref name="TData"/> to the connected <see cref="IServer{TData}"/>.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void Send([DisallowNull] TData data);

    /// <summary>
    /// Occurs when the connection to an <see cref="IServer{TData}"/> has been closed, either client-side or server-side.
    /// </summary>
    public event EventHandler<IClient<TData>>? ConnectionClosed;

    /// <summary>
    /// Occurs when the connection to an <see cref="IServer{TData}"/> has been successfully established.
    /// </summary>
    public event EventHandler<IClient<TData>>? ConnectionEstablished;

    /// <summary>
    /// Occurs when the <see cref="DataProcessor"/> property is set to <see langword="null"/> and the <see cref="IClient{TData}"/> receives <typeparamref name="TData"/> from it's connected <see cref="IServer{TData}"/>.
    /// </summary>
    /// <remarks>
    /// This event will never get raised, if the <see cref="DataProcessor"/> property is set to a not-null object.
    /// </remarks>
    public event EventHandler<IClient<TData>, DataReceivedEventArgs<TData>>? DataReceived;

    /// <summary>
    /// Gets the port through which the <see cref="IClient{TData}"/> is connected.
    /// </summary>
    public Int32 Port { get; }

    /// <summary>
    /// Gets or sets the size of the <see cref="Byte"/> buffer.
    /// </summary>
    public Int32 BufferSize { get; set; }

    /// <summary>
    /// Gets the <see cref="Guid"/> that this <see cref="IClient{TData}"/> is assigned to on it's connected <see cref="IServer{TData}"/>.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets or sets the <see cref="IClientDataProcessor{TData}"/> for this <see cref="IClient{TData}"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IClientDataProcessor{TData}"/> provides the <see cref="IClient{TData}"/> with the functionality to process incoming data.
    /// If this property is not set, the <see cref="IClient{TData}"/> will instead raise the <see cref="DataReceived"/> event every time new data will be received.
    /// </remarks>
    [MaybeNull]
    public IClientDataProcessor<TData>? DataProcessor { get; set; }
}