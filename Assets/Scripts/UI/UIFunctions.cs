/*
**  UIFunctions.cs: Contains basic functions to be called from the UI.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class UIFunctions : MonoBehaviour
{
    //Load scene (with overloads)
    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    //Quits the game
    public void QuitGame()
    {
        Application.Quit();
    }

    //Stops the current connection
    public void StopGame()
    {
        //Calling stophost will stop a host, but will also just stop a client
        NetworkManager.singleton.StopHost();
    }
}
