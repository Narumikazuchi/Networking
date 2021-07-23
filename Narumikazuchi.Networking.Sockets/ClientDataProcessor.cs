using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the blueprint for data processing of clients.
    /// </summary>
    public abstract class ClientDataProcessor<T> where T : class, IByteSerializable, IEquatable<T>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDataProcessor{T}"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        protected ClientDataProcessor([DisallowNull] Client<T> client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            this._client = client;
        }

        #endregion

        #region Data Method Contract

        /// <summary>
        /// Processes the incoming data and takes action accordingly to the developers needs.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public abstract void ProcessReceivedData([DisallowNull] in T data);

        #endregion

        #region Internal Stuff

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void Disconnect() => this.Client.Disconnect();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Client{T}"/> associated with this processor.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [DisallowNull]
        public Client<T> Client
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

        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Client<T> _client;

        #endregion
    }
}
