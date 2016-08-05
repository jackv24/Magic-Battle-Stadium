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

    public GameObject playerSpawnEffect;

    void Start()
    {
        //If this is the local player
        if (isLocalPlayer)
        {
            //Set a reference to the local player in the gamemanager
            GameManager.instance.LocalPlayer = gameObject;

            //Load name from preferences
            username = PlayerPrefs.GetString("name", System.Environment.UserName);

            //Always check if there is a scoreboard present
            if (Scoreboard.instance)
            {
                //If there is another player with the same name, number each successive player
                int playerNum = 1;
                string newName = username;
                while (Scoreboard.instance.PlayerExists(newName))
                {
                    playerNum++;
                    newName = username + "#" + playerNum;
                }
                username = newName;
            }

            //Send name update command to server
            CmdUpdateName(username);

            nameText.enabled = false;
        }
        else //If this is a remote player, call username syncvar hook (make sure name is updated from the start)
            UpdateNameClient(username);

        //Add player to scoreboard
        if(Scoreboard.instance)
            Scoreboard.instance.AddPlayer(username);

        if (playerSpawnEffect)
        {
            GameObject obj = (GameObject)Instantiate(playerSpawnEffect, transform.position - new Vector3(0, 0.6f, 0), Quaternion.identity);

            Destroy(obj, 2f);
        }
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
        if (Scoreboard.instance)
        {
            //If the player's name has changed, update it on the scoreboard
            //(this happens when names are synced after the player joins)
            if (Scoreboard.instance.PlayerExists(username))
                Scoreboard.instance.ChangeName(username, nameString);
        }

        //set username value passed from syncvar
        username = nameString;
        //Set gameobject name
        name = "Player(" + username + ")";

        //If there is text in which to display the name, display it
        if (nameText)
            nameText.text = username;
    }
}
