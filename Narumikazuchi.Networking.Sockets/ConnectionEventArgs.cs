using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the connected or disconnected client.
    /// </summary>
    public sealed class ConnectionEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Contains the connected or disconnected client.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public ConnectionEventArgs([DisallowNull] Socket whichClient, in ConnectionType type)
        {
            if (whichClient is null)
            {
                throw new ArgumentNullException(nameof(whichClient));
            }

            this.Client = whichClient;
            this.EventType = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the client <see cref="Socket"/> that connected/disconnected.
        /// </summary>
        [DisallowNull]
        public Socket Client { get; set; }
        /// <summary>
        /// Gets or sets if the connection was established, closed or lost.
        /// </summary>
        public ConnectionType EventType { get; set; }

        #endregion
    }
}
