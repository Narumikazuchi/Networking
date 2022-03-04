namespace Narumikazuchi.Networking;

/// <summary>
/// Contains the connected or disconnected client.
/// </summary>
public sealed partial class ConnectionEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public ConnectionEventArgs(Guid whichClient,
                               in ConnectionType type)
    {
        m_Client = whichClient;
        this.EventType = type;
    }

    /// <summary>
    /// Gets or sets the client <see cref="System.Net.Sockets.Socket"/> that connected/disconnected.
    /// </summary>
    public Guid Client
    {
        get => m_Client;
        set => m_Client = value;
    }

    /// <summary>
    /// Gets or sets if the connection was established, closed or lost.
    /// </summary>
    public ConnectionType EventType { get; set; }
}

// Non-Public
partial class ConnectionEventArgs : EventArgs
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Guid m_Client;
}