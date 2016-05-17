/*
**  ConnectionManager.cs: Extends NetworkManager to provide additional functionality
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ConnectionManager : NetworkManager
{
    public override void OnStartHost()
    {
        base.OnStartHost();

        GameManager.instance.maxPlayers = maxConnections;
        GameManager.instance.connectedPlayers = numPlayers;
        GameManager.instance.ReadyGame();

        NotificationManager.instance.ShowNotice("?", "Host connect called");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
    }

    //When the client is disconnected unexpectedly
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        //Notify the player that the host was disconnected, and the possible cause.
        NotificationManager.instance.ShowNotice("!", "<b>Disconnected from host.</b>\nThe host may have quit the game, or the connection may have been interrupted. Please quit and retry.");
    }
}
