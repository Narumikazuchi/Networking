using Narumikazuchi.Serialization.Bytes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the basic functionality for a <see cref="Socket"/>-based client.
    /// </summary>
    [DebuggerDisplay("{Protocol}:{Port}")]
    public sealed partial class Client<TMessage> where TMessage : class, IByteSerializable, IEquatable<TMessage>
    {
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/> with default values.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      [DisallowNull] TMessage shutdownCommand) : 
            this(protocol, 
                 80, 
                 2048, 
                 false, 
                 null, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      in Int32 port, 
                      [DisallowNull] TMessage shutdownCommand) : 
            this(protocol, 
                 port, 
                 2048, 
                 false, 
                 null, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize,
                      [DisallowNull] TMessage shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 null, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions, 
                      [DisallowNull] TMessage shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 forwardExecptions, 
                 null, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize,
                      [AllowNull] ClientDataProcessor<TMessage>? processor, 
                      [DisallowNull] TMessage shutdownCommand) : 
            this(protocol, 
                 port, 
                 bufferSize, 
                 false, 
                 processor, 
                 shutdownCommand) 
        { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, 
                      in Int32 port, 
                      in Int32 bufferSize, 
                      in Boolean forwardExecptions,
                      [AllowNull] ClientDataProcessor<TMessage>? processor, 
                      [DisallowNull] TMessage shutdownCommand)
        {
            if (shutdownCommand is null)
            {
                throw new ArgumentNullException(nameof(shutdownCommand));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            if (protocol is not ProtocolType.Tcp
                         and not ProtocolType.Udp
                         and not ProtocolType.Icmp
                         and not ProtocolType.Igmp)
            {
                throw new ArgumentException(PROTOCOL_INVALID, 
                                            nameof(protocol));
            }

            SocketType type = SocketType.Unknown;
            switch (protocol)
            {
                case ProtocolType.Tcp:
                    type = SocketType.Stream;
                    break;
                case ProtocolType.Udp:
                    type = SocketType.Dgram;
                    break;
                case ProtocolType.Icmp:
                case ProtocolType.Igmp:
                    type = SocketType.Raw;
                    break;
            }
            this._clientSocket = new Socket(AddressFamily.InterNetwork, 
                                            type, 
                                            protocol);
            this._dataBuffer = new Byte[bufferSize];
            this.Protocol = protocol;
            this.Port = port;
            this.ForwardExceptions = forwardExecptions;
            this.CommandProcessor = processor;
            this.ShutdownCommand = shutdownCommand;
        }

        /// <summary>
        /// Tries to establish a connection to a server.<para/>
        /// Use the public IP if you are connecting to a server outside you own LAN.
        /// </summary>
        /// <param name="address">The IP-Address the client should connect to.</param>
        /// <exception cref="ArgumentNullException"/>
        public void Connect([DisallowNull] IPAddress address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            try
            {
                this._currentAttempts = 0;
                this.LoopConnect(address);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void Disconnect() => 
            this.Disconnect(true);
        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        /// <param name="raiseEvent">Defines if the <see cref="ConnectionClosed"/> event should be raised.</param>
        public void Disconnect(in Boolean raiseEvent)
        {
            try
            {
                if (this.Connected)
                {
                    this.Send(this.ShutdownCommand);
                    this._clientSocket.Close();
                    if (raiseEvent)
                    {
                        this.OnConnectionClosed();
                    }
                }
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        /// <summary>
        /// Sends the specified data to the connected server.
        /// </summary>
        /// <param name="data">The data to send over the connection.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NotConnectedException"/>
        public void Send([DisallowNull] TMessage data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                if (!this.Connected)
                {
                    throw new NotConnectedException(this._clientSocket);
                }
                Byte[] bytes = this._serializer.Serialize(data);
                this._clientSocket.BeginSend(bytes, 
                                             0, 
                                             bytes.Length, 
                                             SocketFlags.None, 
                                             this.SendCallback, 
                                             data);
                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        /// <summary>
        /// Occurs when the connection was established.
        /// </summary>
        public event EventHandler<Client<TMessage>>? ConnectionEstablished;
        /// <summary>
        /// Occurs when the connection was closed.
        /// </summary>
        public event EventHandler<Client<TMessage>>? ConnectionClosed;
        /// <summary>
        /// Occurs when an exception occurs inside the server application.
        /// </summary>
        public event EventHandler<Client<TMessage>, ExceptionEventArgs>? ExceptionOccurred;

        /// <summary>
        /// The protocol used for the connection.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ProtocolType Protocol { get; }
        /// <summary>
        /// The port the client should connect to. Default is 80.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int32 Port { get; }
        /// <summary>
        /// Tells the Client to stop any connection attempt.
        /// </summary>
        public Boolean AbortConnecting { get; set; }
        /// <summary>
        /// Returns if the client is connected to a server.
        /// </summary>
        public Boolean Connected => this._clientSocket.Connected;
        /// <summary>
        /// Determines the actions to be taken after the client receives a command from the server.<para/>
        /// Is supposed to be a derived class from <see cref="ClientDataProcessor{T}"/>.
        /// </summary>
        [MaybeNull]
        public ClientDataProcessor<TMessage>? CommandProcessor
        {
            get => this._processor;
            set
            {
                this._processor = value;
                if (this._processor is not null)
                {
                    this._processor.Client = this;
                }
            }
        }
        /// <summary>
        /// Gets or sets if exceptions should be forwarded through the <see cref="ExceptionOccurred"/> event.
        /// </summary>
        public Boolean ForwardExceptions { get; set; }
        /// <summary>
        /// Gets or sets the size of the data buffer.
        /// </summary>
        public Int32 BufferSize
        {
            get => this._dataBuffer.Length;
            set => this._dataBuffer = new Byte[value];
        }
        /// <summary>
        /// Gets the <typeparamref name="TMessage"/> object, which signals a server shutdown or client disconnect.
        /// </summary>
        [NotNull]
        public TMessage ShutdownCommand { get; }
    }

    // Non-Public
    partial class Client<TMessage>
    {
        private void LoopConnect(IPAddress address)
        {
            try
            {
                while (!this.Connected &&
                       this._currentAttempts < MAXATTEMPTS)
                {
                    if (this.AbortConnecting)
                    {
                        this.OnConnectionClosed();
                        return;
                    }
                    try
                    {
                        this._currentAttempts++;
                        this._clientSocket.Connect(address, this.Port);
                    }
                    catch (SocketException) { }
                }
                if (this._currentAttempts == MAXATTEMPTS &&
                    !this.Connected)
                {
                    throw new MaximumAttemptsExceededException();
                }
                if (this.Connected)
                {
                    this.OnConnectionEstablished();
                    this._clientSocket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
                }
            }
            catch (MaximumAttemptsExceededException ex)
            {
                this.OnException(ex);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            Int32 received = 0;
            Byte[]? data = null;
            try
            {
                received = this._clientSocket.EndReceive(result);
                data = new Byte[received];
                Array.Copy(this._dataBuffer, 
                           data, 
                           received);

                this.ProcessData(data);

                this._clientSocket.BeginReceive(this._dataBuffer, 
                                                0, 
                                                this._dataBuffer.Length, 
                                                SocketFlags.None, 
                                                new AsyncCallback(this.ReceiveCallback), 
                                                null);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                this._clientSocket.EndSend(result);
                if (result.AsyncState is TMessage obj &&
                    obj.Equals(this.ShutdownCommand))
                {
                    this._clientSocket.Close();
                    return;
                }
                if (!this.Connected)
                {
                    return;
                }
                this._clientSocket.BeginReceive(this._dataBuffer, 
                                                0, 
                                                this._dataBuffer.Length, 
                                                SocketFlags.None, 
                                                new AsyncCallback(this.ReceiveCallback), 
                                                null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        private void ProcessData(Byte[] bytes)
        {
            TMessage data = this._serializer.Deserialize(bytes, 0);
            if (data.Equals(this.ShutdownCommand))
            {
                this.Disconnect();
                return;
            }
            if (this.CommandProcessor is not null)
            {
                this.CommandProcessor.ProcessReceivedData(data);
            }
        }

        private void OnConnectionEstablished() => 
            this.ConnectionEstablished?.Invoke(this, 
                                               EventArgs.Empty);

        private void OnConnectionClosed() => 
            this.ConnectionClosed?.Invoke(this, 
                                          EventArgs.Empty);

        private void OnException(Exception ex)
        {
            if (this.ForwardExceptions)
            {
                this.ExceptionOccurred?.Invoke(this, 
                                               new(ex));
            }
            else
            {
                throw ex;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private readonly Socket _clientSocket;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ByteSerializer<TMessage> _serializer = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ClientDataProcessor<TMessage>? _processor;
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Byte[] _dataBuffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Int32 _currentAttempts = 0;

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const Int32 MAXATTEMPTS = 20;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String PROTOCOL_INVALID = "The specified protocol is not implemented for this client.";
#pragma warning restore
    }
}
