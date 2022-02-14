namespace Narumikazuchi.Networking;

/// <summary>
/// Represents the condition that has to be met for a new client to be accepted by the server.
/// </summary>
/// <param name="server">The server that has been connected to.</param>
/// <param name="guidOfNewClient">The <see cref="Guid"/> that the server has associated with the new client.</param>
/// <returns><see langword="true"/> if the client will get accepted by the server; otherwise, <see langword="false"/></returns>
public delegate Boolean ServerAcceptCondition<TData>([DisallowNull] IServer<TData> server,
                                                     in Guid guidOfNewClient);