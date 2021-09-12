﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the blueprint for data processing of an <see cref="IServer{TData}"/>.
    /// </summary>
    // Non-Public
    public abstract partial class ServerDataProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataProcessor"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        protected ServerDataProcessor([DisallowNull] Server server)
        {
            if (server is null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            this._server = server;
            if (this._server.DataProcessor != this)
            {
                this._server.DataProcessor = this;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IServer<Byte[]> _server;
    }

    // IServerDataProcessor<Byte[]>
    partial class ServerDataProcessor : IServerDataProcessor<Byte[]>
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public abstract void ProcessReceivedData([DisallowNull] Byte[] data,
                                                 in Guid fromClient);

        /// <summary>
        /// Disconnects the <see cref="Client"/> from the <see cref="Server"/> instance.
        /// </summary>
        /// <param name="client">The client to disconnect.</param>
        /// <exception cref="KeyNotFoundException"/>
        public void DisconnectClient(in Guid client) =>
            this.Server.Disconnect(client);

        /// <summary>
        /// Gets or sets the <see cref="Server{TMessage}"/> associated with this processor.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public IServer<Byte[]> Server
        {
            get => this._server;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._server = value;
                if (this._server.DataProcessor != this)
                {
                    this._server.DataProcessor = this;
                }
            }
        }
    }
}
