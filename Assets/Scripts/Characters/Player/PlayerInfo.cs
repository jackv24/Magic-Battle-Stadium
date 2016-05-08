/*
**  PlayerInfo.cs: Contains identifying information about the player, such as name
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInfo : NetworkBehaviour
{
    //username for individual players should be synced
    [SyncVar(hook ="UpdateNameClient")]
    public string username;

    //The text component in which to display the player's name
    public Text nameText;

    void Start()
    {
        //If this is the local player
        if (isLocalPlayer)
        {
            //Load name from preferences
            username = PlayerPrefs.GetString("name", System.Environment.UserName);

            //Send name update command to server
            CmdUpdateName(username);

            nameText.enabled = false;
        }
        else //If this is a remote player, call username syncvar hook (make sure name is updated from the start)
            UpdateNameClient(username);
    }

    //Updates this player's name on the server
    [Command]
    void CmdUpdateName(string nameString)
    {
        //Same logic as updating player on client, except run on server
        UpdateNameClient(nameString);
    }

    //Update player info on client
    void UpdateNameClient(string nameString)
    {
        //set username value passed from syncvar
        username = nameString;

        //If there is text in which to display the name, display it
        if (nameText)
            nameText.text = username;
    }
}
