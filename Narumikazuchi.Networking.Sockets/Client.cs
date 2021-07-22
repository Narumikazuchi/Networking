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
    public sealed class Client<T> where T : IByteConvertable<T>, IEquatable<T>
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/> with default values.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, [DisallowNull] in T shutdownCommand) : this(protocol, 80, 2048, false, null, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, in Int32 port, [DisallowNull] in T shutdownCommand) : this(protocol, port, 2048, false, null, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, null, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, forwardExecptions, null, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, ClientDataProcessor<T>? processor, [DisallowNull] in T shutdownCommand) : this(protocol, port, bufferSize, false, processor, shutdownCommand) { }
        /// <summary>
        /// Instantiates a new object of the <see cref="Client{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SocketException"/>
        public Client(in ProtocolType protocol, in Int32 port, in Int32 bufferSize, in Boolean forwardExecptions, ClientDataProcessor<T>? processor, [DisallowNull] in T shutdownCommand)
        {
            if (shutdownCommand is null)
            {
                throw new ArgumentNullException(nameof(shutdownCommand));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            if (!EnumValidator.IsDefined(protocol))
            {
                throw new ArgumentException("The given protocol type is not a valid value from the enum.", nameof(protocol));
            }

            SocketType type = protocol is ProtocolType.Tcp ? SocketType.Stream : protocol is ProtocolType.Udp ? SocketType.Dgram : protocol is ProtocolType.Icmp or ProtocolType.Igmp ? SocketType.Raw : SocketType.Unknown;
            this._clientSocket = new Socket(AddressFamily.InterNetwork, type, protocol);
            this._dataBuffer = new Byte[bufferSize];
            this.Protocol = protocol;
            this.Port = port;
            this.ForwardExceptions = forwardExecptions;
            this.CommandProcessor = processor;
            this.ShutdownCommand = shutdownCommand;
        }

        #endregion

        #region Establish Connection

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

        #endregion

        #region Close Connection

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void Disconnect() => this.Disconnect(true);
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

        #endregion

        #region Callbacks

        private void ReceiveCallback(IAsyncResult result)
        {
            Int32 received = 0;
            Byte[]? data = null;
            try
            {
                // Trim the received data, so there are no "null"-bytes in the resulting string
                received = this._clientSocket.EndReceive(result);
                data = new Byte[received];
                // Extracting the data from the buffer
                Array.Copy(this._dataBuffer, data, received);

                // Process received data
                this.ProcessData(data);

                // Start receiving anew
                this._clientSocket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
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
                if (result.AsyncState is T obj &&
                    obj.Equals(this.ShutdownCommand))
                {
                    this._clientSocket.Close();
                    return;
                }
                if (!this.Connected)
                {
                    return;
                }
                this._clientSocket.BeginReceive(this._dataBuffer, 0, this._dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        #endregion

        #region Send Data

        /// <summary>
        /// Sends the specified data to the connected server.
        /// </summary>
        /// <param name="data">The data to send over the connection.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NotConnectedException"/>
        public void Send([DisallowNull] in T data)
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
                Byte[] bytes = data.ConvertToBytes();
                this._clientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(this.SendCallback), data);
                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                this.OnException(e);
            }
        }

        #endregion

        #region Process Data

        private void ProcessData(Byte[] bytes)
        {
            T data = IByteConvertable<T>.ConvertFromBytes(bytes);
            // ============================================================================================
            // The server has shutdown
            // ============================================================================================
            if (data.Equals(this.ShutdownCommand))
            {
                this.Disconnect();
                return;
            }
            // ============================================================================================
            // Process the data.
            // ============================================================================================
            if (this.CommandProcessor is not null)
            {
                this.CommandProcessor.ProcessReceivedData(data);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the connection was established.
        /// </summary>
        public event EventHandler<Client<T>>? ConnectionEstablished;
        /// <summary>
        /// Occurs when the connection was closed.
        /// </summary>
        public event EventHandler<Client<T>>? ConnectionClosed;
        /// <summary>
        /// Occurs when an exception occurs inside the server application.
        /// </summary>
        public event Action<Client<T>, Exception>? ExceptionOccurred;

        private void OnConnectionEstablished() => this.ConnectionEstablished?.Invoke(this, EventArgs.Empty);

        private void OnConnectionClosed() => this.ConnectionClosed?.Invoke(this, EventArgs.Empty);

        private void OnException(Exception ex)
        {
            if (this.ForwardExceptions)
            {
                this.ExceptionOccurred?.Invoke(this, ex);
            }
            else
            {
                throw ex;
            }
        }

        #endregion

        #region Properties

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
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Boolean AbortConnecting { get; set; }
        /// <summary>
        /// Returns if the client is connected to a server.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Boolean Connected => this._clientSocket.Connected;
        /// <summary>
        /// Determines the actions to be taken after the client receives a command from the server.<para/>
        /// Is supposed to be a derived class from <see cref="ClientDataProcessor{T}"/>.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public ClientDataProcessor<T>? CommandProcessor
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
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Boolean ForwardExceptions { get; set; }
        /// <summary>
        /// Gets or sets the size of the data buffer.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public Int32 BufferSize
        {
            get => this._dataBuffer.Length;
            set => this._dataBuffer = new Byte[value];
        }
        /// <summary>
        /// Gets the <typeparamref name="T"/> object, which signals a server shutdown or client disconnect.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [DisallowNull]
        public T ShutdownCommand { get; }

        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private readonly Socket _clientSocket;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ClientDataProcessor<T>? _processor;
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Byte[] _dataBuffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Int32 _currentAttempts = 0;

        #endregion

        #region Constants

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const Int32 MAXATTEMPTS = 20;
#pragma warning restore

        #endregion
    }
}
