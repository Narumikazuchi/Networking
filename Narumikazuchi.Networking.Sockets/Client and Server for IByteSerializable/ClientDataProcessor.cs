﻿using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the blueprint for data processing of an <see cref="IClient{TData}"/>.
    /// </summary>
    // Non-Public
    public abstract partial class ClientDataProcessor<TMessage> 
        where TMessage : class, IByteSerializable, IEquatable<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDataProcessor{TMessage}"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        protected ClientDataProcessor([DisallowNull] Client<TMessage> client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            this._client = client;
            if (this._client.DataProcessor != this)
            {
                this._client.DataProcessor = this;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IClient<TMessage> _client;
    }

    // IClientDataProcessor<TMessage>
    partial class ClientDataProcessor<TMessage> : IClientDataProcessor<TMessage>
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public abstract void ProcessReceivedData([DisallowNull] TMessage data);

        /// <summary>
        /// Disconnects the <see cref="Client{TMessage}"/> from the <see cref="Server{TMessage}"/>.
        /// </summary>
        public void Disconnect() =>
            this.Client.Disconnect();

        /// <summary>
        /// Gets or sets the <see cref="Client{TMessage}"/> associated with this processor.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public IClient<TMessage> Client
        {
            get => this._client;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._client = value;
                if (this._client.DataProcessor != this)
                {
                    this._client.DataProcessor = this;
                }
            }
        }
    }
}