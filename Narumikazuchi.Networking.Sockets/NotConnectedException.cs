using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents the error which occurs when two endpoints are not connected.
    /// </summary>
    public sealed partial class NotConnectedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public NotConnectedException([DisallowNull] Socket socket) : 
            base(MESSAGE)
        {
            if (socket is null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            this.Endpoint = socket;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public NotConnectedException([DisallowNull] Socket socket, 
                                     [AllowNull] String? auxMessage) : 
            base(String.Format("{0} - {1}", 
                               MESSAGE, 
                               auxMessage))
        {
            if (socket is null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            this.Endpoint = socket;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public NotConnectedException([DisallowNull] Socket socket,
                                     [AllowNull] String? auxMessage,
                                     [AllowNull] Exception? inner) : 
            base(String.Format("{0} - {1}", 
                               MESSAGE, 
                               auxMessage), 
                inner)
        {
            if (socket is null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            this.Endpoint = socket;
        }

        /// <summary>
        /// The socket which caused the exception.
        /// </summary>
        [NotNull]
        public Socket Endpoint { get; }
    }

    // Non-Public
    partial class NotConnectedException
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String MESSAGE = "The endpoint is not connected.";
    }
}
