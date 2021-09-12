using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents an <see cref="IServer{TData}"/>, which communicates with <see cref="Client"/> objects through arrays of <see cref="Byte"/>s.
    /// </summary>
    public sealed partial class Server
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="port">The port through which the connections shall be established.</param>
        /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
        /// <param name="acceptCondition">The condition for a connection <see cref="Client"/> to be accepted.</param>
        /// <returns>A new <see cref="Server"/> instance with the specified parameters</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static Server CreateServer(in Int32 port,
                                          in Int32 bufferSize,
                                          [DisallowNull] Func<Boolean> acceptCondition)
        {
            if (acceptCondition is null)
            {
                throw new ArgumentNullException(nameof(acceptCondition));
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
                       null,
                       acceptCondition);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="port">The port through which the connections shall be established.</param>
        /// <param name="bufferSize">The size of the data buffer for received data in bytes.</param>
        /// <param name="processor">The processor, who handles the incoming <see cref="Byte"/>[] objects.</param>
        /// <param name="acceptCondition">The condition for a connection <see cref="Client"/> to be accepted.</param>
        /// <returns>A new <see cref="Server"/> instance with the specified parameters</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static Server CreateServer(in Int32 port,
                                          in Int32 bufferSize,
                                          [DisallowNull] ServerDataProcessor processor,
                                          [DisallowNull] Func<Boolean> acceptCondition)
        {
            if (acceptCondition is null)
            {
                throw new ArgumentNullException(nameof(acceptCondition));
            }
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
                       processor,
                       acceptCondition);
        }
    }

    // Non-Public
    partial class Server
    {
        private Server(in Int32 port,
                       in Int32 bufferSize,
                       [AllowNull] IServerDataProcessor<Byte[]>? processor,
                       [DisallowNull] Func<Boolean> acceptCondition) :
            base(port,
                 bufferSize,
                 processor,
                 acceptCondition)
        { }
    }

    // ServerBase<Byte[]>
    partial class Server : ServerBase<Byte[]>
    {
        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Start() =>
            this.InitiateStart();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Stop() =>
            this.InitiateStop();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public override Boolean Disconnect(in Guid guid) =>
            this.InitiateDisconnect(guid);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Send([DisallowNull] Byte[] data,
                                  in Guid client)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            this.InitiateSend(data,
                              client);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void Broadcast([DisallowNull] Byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            this.InitiateBroadcast(data);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        [return: NotNull]
        protected override Byte[] SerializeToBytes([DisallowNull] Byte[] data) =>
            data is null 
                ? throw new ArgumentNullException(nameof(data)) 
                : data;

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        [return: NotNull]
        protected override Byte[] SerializeFromBytes([DisallowNull] Byte[] bytes) => 
            bytes is null 
                ? throw new ArgumentNullException(nameof(bytes)) 
                : bytes;
    }
}
