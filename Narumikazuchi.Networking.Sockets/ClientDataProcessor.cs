using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the blueprint for data processing of clients.
    /// </summary>
    public abstract partial class ClientDataProcessor<TMessage> 
        where TMessage : class, IByteSerializable, IEquatable<TMessage>
    {
        /// <summary>
        /// Processes the incoming data and takes action accordingly to the developers needs.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public abstract void ProcessReceivedData([DisallowNull] TMessage data);

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void Disconnect() => 
            this.Client.Disconnect();

        /// <summary>
        /// Gets or sets the <see cref="Client{T}"/> associated with this processor.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [NotNull]
        public Client<TMessage> Client
        {
            get => this._client;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._client = value;
            }
        }
    }

    // Non-Public
    partial class ClientDataProcessor<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDataProcessor{T}"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        protected ClientDataProcessor([DisallowNull] Client<TMessage> client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            this._client = client;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Client<TMessage> _client;
    }
}
