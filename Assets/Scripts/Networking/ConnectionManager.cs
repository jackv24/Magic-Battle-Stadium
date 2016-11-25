/*
**  ConnectionManager.cs: Extends NetworkManager to provide additional functionality
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ConnectionManager : NetworkManager
{
    public GameObject playerAIPrefab;

    private List<GameObject> playerAISpawned = new List<GameObject>();

    //When the client is disconnected unexpectedly
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        //Notify the player that the host was disconnected, and the possible cause.
        NotificationManager.instance.ShowNotice("!", "<b>Disconnected from host.</b>\nThe host may have quit the game, or the connection may have been interrupted. Please quit and retry.");
    }

    public override void OnStartServer()
    {
        List<Transform> newStartPositions = GenerateSpawnCircle.instance.Generate(maxConnections);

        if (newStartPositions.Count > 1)
        {
            startPositions.Clear();

            foreach (Transform pos in newStartPositions)
                RegisterStartPosition(pos);
        }

        base.OnStartServer();

        //Spawn AI players using coroutine, so that the server is ready
        StartCoroutine("SpawnAIPlayers");
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //If AI players have been spawned
        if (playerAISpawned.Count >= 1)
        {
            //Get last AI character
            GameObject ai = playerAISpawned[playerAISpawned.Count - 1];
            //Remove ai gameobject from spawned list, as it will be removed
            playerAISpawned.Remove(ai);

            //Add actual player at position of AI character
            GameObject player = (GameObject)Instantiate(playerPrefab, ai.transform.position, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            //Remove AI character after being replaced by actual player
            NetworkServer.Destroy(ai);
        }
        else
        {
            //If there were no AI just spawn normally
            base.OnServerAddPlayer(conn, playerControllerId);
        }
    }

    IEnumerator SpawnAIPlayers()
    {
        yield return new WaitForEndOfFrame();

        //If a player AI prefab is present, then spawn them
        if (playerAIPrefab)
        {
            //Spawn player ai (starting at 1 since the host player is spawned)
            for (int i = 1; i < maxConnections; i++)
            {
                //Instantiate AI player
                GameObject player = (GameObject)Instantiate(playerAIPrefab, GetStartPosition().position, Quaternion.identity);

                //Set player name
                PlayerInfo info = player.GetComponent<PlayerInfo>();
                if (info)
                    info.username = "AI #" + i;

                //Spawn on server
                NetworkServer.Spawn(player);

                //Add to list of spawned AI players
                playerAISpawned.Add(player);
            }
        }
    }
}
