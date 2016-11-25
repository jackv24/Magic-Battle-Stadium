/*
**  NetworkConnectUI.cs: attached to the connection UI, provides functions for connecting to specified hosts, or hosting games
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkConnectUI : MonoBehaviour
{
    public NetworkManager manager;

    //Gameobjects to be enabled when the match has connected
    public GameObject[] enableGameObjects;
    //Gameobjects to be disabled 
    public GameObject[] disableGameObjects;

    public float connectionAttemptTimeout = 1f;

    ////Temporary ongui stuff
    //public int offsetX;
    //public int offsetY;
    //public bool showGUI = false;

    //// Runtime variable
    //private bool showServer = false;

    void Start()
    {
        //Gameobjects to be enabled should start disabled
        ToggleUIObjects(false);

        if (!manager)
            Debug.Log("No NetworkManager assigned to Network Connect UI!");

        LoadPrefs();
    }

    public void LoadPrefs()
    {
        if (PlayerPrefs.HasKey("ip"))
        {
            manager.networkAddress = PlayerPrefs.GetString("ip");
        }

        if (PlayerPrefs.HasKey("port"))
        {
            manager.networkPort = int.Parse(PlayerPrefs.GetString("port"));
        }

        if (PlayerPrefs.HasKey("maxPlayers"))
        {
            manager.maxConnections = int.Parse(PlayerPrefs.GetString("maxPlayers"));
        }
    }

    //Connect functions
    //Creates a new lan game with this machine as the host
    public void HostLANGame()
    {
        //Start the game as a host
        StartCoroutine("StartGame", true);
    }
    //Connects to an existing LAN game as a client
    public void ConnectLANGame()
    {
        //Start the game as a client
        StartCoroutine("StartGame", false);
    }

    //StartGame is a coroutine so code can be paused to wait for connection
    IEnumerator StartGame(bool isHost)
    {
        //Make sure everything is active (and does not need matchmaking)
        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
        {
            //Start the game (either as a host or a client)
            if (isHost)
            {
                manager.StartHost();

                GameManager.instance.StartGame();
            }
            else
                manager.StartClient();

            //Enable all of the gameobjects specified in the inspector
            ToggleUIObjects(true);

            //Wait for a certain amount of time to see if the connection was successful
            yield return new WaitForSeconds(connectionAttemptTimeout);

            //If the connection was successful
            if (!manager.IsClientConnected())
            {
                //Enable all of the gameobjects specified in the inspector
                ToggleUIObjects(false);

                //Display a notice (using custom notice system)
                NotificationManager.instance.ShowNotice("!", string.Format(
                    "<b>Could not connect to <i>{0}</i></b>\nPlease make sure the host has started a game, and you are on the same network.",
                    manager.networkAddress));

                //If the client did not connect, stop it (allows it to attempt connection later)
                manager.StopClient();
            }
        }
    }

    //Matchmaking functions
    //Finds and displays rooms
    public void FindRoom()
    {
        //TODO: Implement matchmaking
        Debug.Log("Cannot Find Room! (Matchmaking not implemented yet)");
    }
    //Creates a room
    public void CreateRoom()
    {
        //TODO: Implement matchmaking
        Debug.Log("Cannot Create Room! (Matchmaking not implemented yet)");
    }

    //Enable all objects in enableObjects (HUD, etc)
    void ToggleUIObjects(bool active)
    {
        foreach (GameObject obj in enableGameObjects)
        {
            obj.SetActive(active);
        }

        foreach (GameObject obj in disableGameObjects)
        {
            obj.SetActive(!active);
        }
    }
}
