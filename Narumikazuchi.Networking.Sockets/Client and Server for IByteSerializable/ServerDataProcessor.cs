﻿namespace Narumikazuchi.Networking.Sockets;

/// <summary>
/// Provides the blueprint for data processing of an <see cref="IServer{TData}"/>.
/// </summary>
// Non-Public
public abstract partial class ServerDataProcessor<TMessage>
    where TMessage : IDeserializable<TMessage>, ISerializable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerDataProcessor{TMessage}"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    protected ServerDataProcessor([DisallowNull] Server<TMessage> server)
    {
        ExceptionHelpers.ThrowIfArgumentNull(server);

        this._server = server;
        if (this._server
                .DataProcessor != this)
        {
            this._server
                .DataProcessor = this;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IServer<TMessage> _server;
}

// IServerDataProcessor<TMessage>
partial class ServerDataProcessor<TMessage> : IServerDataProcessor<TMessage>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public abstract void ProcessReceivedData([DisallowNull] TMessage data,
                                             in Guid fromClient);

    /// <summary>
    /// Disconnects the <see cref="Client{TMessage}"/> from the <see cref="Server{TMessage}"/> instance.
    /// </summary>
    /// <param name="client">The client to disconnect.</param>
    /// <exception cref="KeyNotFoundException"/>
    public void DisconnectClient(in Guid client) =>
        this.Server
            .Disconnect(client);

    /// <summary>
    /// Gets or sets the <see cref="Server{TMessage}"/> associated with this processor.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    [NotNull]
    public IServer<TMessage> Server
    {
        get => this._server;
        set
        {
            ExceptionHelpers.ThrowIfArgumentNull(value);

            this._server = value;
            if (this._server
                    .DataProcessor != this)
            {
                this._server
                    .DataProcessor = this;
            }
        }
    }
}