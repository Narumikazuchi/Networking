using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Provides the blueprint for data processing of servers.
    /// </summary>
    public abstract partial class ServerDataProcessor<T> 
        where T : class, IByteSerializable, IEquatable<T>
    {
        /// <summary>
        /// Processes the incoming data and takes action accordingly to the developers needs.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public abstract void ProcessReceivedData([DisallowNull] T data,
                                                 [DisallowNull] Socket fromClient);

        /// <summary>
        /// Disconnects the client from the <see cref="Server{T}"/> instance.
        /// </summary>
        /// <param name="client">The client to disconnect.</param>
        /// <exception cref="ArgumentNullException"/>
        public void DisconnectClient([DisallowNull] Socket client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (client.Connected)
            {
                client.Close();
            }
            this.Server.Clients
                       .Remove(client);
            this.Server.OnClientDisconnected(client, 
                                             ConnectionType.ConnectionClosed);
        }

        /// <summary>
        /// Gets or sets the <see cref="Server{T}"/> associated with this processor.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public Server<T> Server
        {
            get => this._server;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._server = value;
            }
        }
    }

    // Non-Public
    partial class ServerDataProcessor<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataProcessor{T}"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        protected ServerDataProcessor([DisallowNull] Server<T> server)
        {
            if (server is null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            this._server = server;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Server<T> _server;
    }
}
