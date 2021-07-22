using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents the error which occurs when two endpoints are not connected.
    /// </summary>
    public sealed class NotConnectedException : Exception
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public NotConnectedException([DisallowNull] Socket socket) : base(MESSAGE)
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
        public NotConnectedException([DisallowNull] Socket socket, String? auxMessage) : base(String.Format("{0} - {1}", MESSAGE, auxMessage))
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
        public NotConnectedException([DisallowNull] Socket socket, String? auxMessage, Exception? inner) : base(String.Format("{0} - {1}", MESSAGE, auxMessage), inner)
        {
            if (socket is null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            this.Endpoint = socket;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The socket which caused the exception.
        /// </summary>
        [DisallowNull]
        public Socket Endpoint { get; }

        #endregion

        #region Constants

        private const String MESSAGE = "The endpoint is not connected.";

        #endregion
    }
}
