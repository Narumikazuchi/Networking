using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the connected or disconnected client.
    /// </summary>
    public sealed partial class ConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public ConnectionEventArgs([DisallowNull] Socket whichClient, 
                                   in ConnectionType type)
        {
            if (whichClient is null)
            {
                throw new ArgumentNullException(nameof(whichClient));
            }

            this._client = whichClient;
            this.EventType = type;
        }

        /// <summary>
        /// Gets or sets the client <see cref="Socket"/> that connected/disconnected.
        /// </summary>
        [NotNull]
        public Socket Client
        {
            get => _client;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this._client = value;
            }
        }
        /// <summary>
        /// Gets or sets if the connection was established, closed or lost.
        /// </summary>
        public ConnectionType EventType { get; set; }
    }

    // Non-Public
    partial class ConnectionEventArgs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Socket _client;
    }
}
