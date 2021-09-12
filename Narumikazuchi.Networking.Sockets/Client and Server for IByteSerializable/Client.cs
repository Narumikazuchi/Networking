using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents an <see cref="IClient{TData}"/>, which communicates with <see cref="Server{TMessage}"/> objects through an <see cref="IByteSerializable"/> message class.
    /// </summary>
    public sealed partial class Client<TMessage>
        where TMessage : class, IByteSerializable, IEquatable<TMessage>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
        /// </summary>
        /// <param name="port">The port through which the connection shall be established.</param>
        /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static Client<TMessage> CreateClient(in Int32 port,
                                                    in Int32 bufferSize)
        {
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            return new(port,
                       bufferSize,
                       null);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Client{TMessage}"/> class.
        /// </summary>
        /// <param name="port">The port through which the connection shall be established.</param>
        /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
        /// <param name="processor">The processor, who handles the incoming <typeparamref name="TMessage"/> objects.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static Client<TMessage> CreateClient(in Int32 port,
                                                    in Int32 bufferSize,
                                                    [DisallowNull] ClientDataProcessor<TMessage> processor)
        {
            if (processor is null)
            {
                throw new ArgumentNullException(nameof(processor));
            }
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            return new(port,
                       bufferSize,
                       processor);
        }

        /// <summary>
        /// Disconnects the <see cref="Client{TMessage}"/> from the <see cref="Server{TMessage}"/>.
        /// </summary>
        /// <param name="raiseEvent">Whether or not to raise the <see cref="IClient{TData}.ConnectionClosed"/> event.</param>
        public void Disconnect(in Boolean raiseEvent) =>
            this.InitiateDisconnect(raiseEvent);
    }

    // Non-Public
    partial class Client<TMessage>
    {
        private Client(in Int32 port,
                       in Int32 bufferSize,
                       [AllowNull] IClientDataProcessor<TMessage>? processor) : 
            base(port,
                 bufferSize,
                 processor)
        { }

        private void LoopConnect(IPAddress address)
        {
            while (!this.Socket.Connected &&
                    this._currentAttempts < MAXATTEMPTS)
            {
                try
                {
                    this._currentAttempts++;
                    this.Socket.Connect(address, 
                                        this.Port);
                }
                catch (SocketException) { }
            }
            if (this._currentAttempts == MAXATTEMPTS &&
                !this.Socket.Connected)
            {
                throw new MaximumAttemptsExceededException();
            }
            if (this.Socket.Connected)
            {
                this.InitiateConnection();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ByteSerializer<TMessage> _serializer = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Int32 _currentAttempts = 0;

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const Int32 MAXATTEMPTS = 20;
#pragma warning restore
    }

    // ClientBase<TMessage>
    partial class Client<TMessage> : ClientBase<TMessage>
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Connect([DisallowNull] IPAddress address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            this._currentAttempts = 0;
            this.LoopConnect(address);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Disconnect() =>
            this.InitiateDisconnect(true);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public override void Send([DisallowNull] TMessage data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!this.Connected)
            {
                throw new NotConnectedException(this.Socket);
            }
            this.InitiateSend(data);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        [return: NotNull]
        protected override Byte[] SerializeToBytes([DisallowNull] TMessage data) => 
            data is null 
                ? throw new ArgumentNullException(nameof(data)) 
                : this._serializer.Serialize(data);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        [return: NotNull]
        protected override TMessage SerializeFromBytes([DisallowNull] Byte[] bytes) =>
            bytes is null
                ? throw new ArgumentNullException(nameof(bytes))
                : this._serializer.Deserialize(bytes);
    }
}
