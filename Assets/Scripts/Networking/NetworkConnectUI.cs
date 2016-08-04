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

    ////Will be deleted when everything is implemented
    //void OnGUI()
    //{
    //    //This code should not run, as it is just for reference
    //    if (showGUI)
    //    {
    //        int xpos = 10 + offsetX;
    //        int ypos = 40 + offsetY;
    //        int spacing = 24;

    //        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
    //        {
    //            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host(H)"))
    //            {
    //                manager.StartHost();
    //            }
    //            ypos += spacing;

    //            if (GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client(C)"))
    //            {
    //                manager.StartClient();
    //            }
    //            manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), manager.networkAddress);
    //            ypos += spacing;

    //            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server Only(S)"))
    //            {
    //                manager.StartServer();
    //            }
    //            ypos += spacing;
    //        }
    //        else
    //        {
    //            if (NetworkServer.active)
    //            {
    //                GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + manager.networkPort);
    //                ypos += spacing;
    //            }
    //            if (NetworkClient.active)
    //            {
    //                GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
    //                ypos += spacing;
    //            }
    //        }

    //        if (NetworkClient.active && !ClientScene.ready)
    //        {
    //            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready"))
    //            {
    //                ClientScene.Ready(manager.client.connection);

    //                if (ClientScene.localPlayers.Count == 0)
    //                {
    //                    ClientScene.AddPlayer(0);
    //                }
    //            }
    //            ypos += spacing;
    //        }

    //        if (NetworkServer.active || NetworkClient.active)
    //        {
    //            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)"))
    //            {
    //                manager.StopHost();
    //            }
    //            ypos += spacing;
    //        }

    //        if (!NetworkServer.active && !NetworkClient.active)
    //        {
    //            ypos += 10;

    //            if (manager.matchMaker == null)
    //            {
    //                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Enable Match Maker (M)"))
    //                {
    //                    manager.StartMatchMaker();
    //                }
    //                ypos += spacing;
    //            }
    //            else
    //            {
    //                if (manager.matchInfo == null)
    //                {
    //                    if (manager.matches == null)
    //                    {
    //                        if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Create Internet Match"))
    //                        {
    //                            manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
    //                        }
    //                        ypos += spacing;

    //                        GUI.Label(new Rect(xpos, ypos, 100, 20), "Room Name:");
    //                        manager.matchName = GUI.TextField(new Rect(xpos + 100, ypos, 100, 20), manager.matchName);
    //                        ypos += spacing;

    //                        ypos += 10;

    //                        if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Find Internet Match"))
    //                        {
    //                            manager.matchMaker.ListMatches(0, 20, "", manager.OnMatchList);
    //                        }
    //                        ypos += spacing;
    //                    }
    //                    else
    //                    {
    //                        foreach (var match in manager.matches)
    //                        {
    //                            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Join Match:" + match.name))
    //                            {
    //                                manager.matchName = match.name;
    //                                manager.matchSize = (uint)match.currentSize;
    //                                manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
    //                            }
    //                            ypos += spacing;
    //                        }
    //                    }
    //                }

    //                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Change MM server"))
    //                {
    //                    showServer = !showServer;
    //                }
    //                if (showServer)
    //                {
    //                    ypos += spacing;
    //                    if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Local"))
    //                    {
    //                        manager.SetMatchHost("localhost", 1337, false);
    //                        showServer = false;
    //                    }
    //                    ypos += spacing;
    //                    if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Internet"))
    //                    {
    //                        manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
    //                        showServer = false;
    //                    }
    //                    ypos += spacing;
    //                    if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Staging"))
    //                    {
    //                        manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
    //                        showServer = false;
    //                    }
    //                }

    //                ypos += spacing;

    //                GUI.Label(new Rect(xpos, ypos, 300, 20), "MM Uri: " + manager.matchMaker.baseUri);
    //                ypos += spacing;

    //                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Disable Match Maker"))
    //                {
    //                    manager.StopMatchMaker();
    //                }
    //                ypos += spacing;
    //            }
    //        }
    //    }
    //}
}
