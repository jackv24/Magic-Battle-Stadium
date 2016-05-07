/*
**  UIFunctions.cs: Contains basic functions to be called from the UI.
*/

using UnityEngine;
using System.Collections;
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
}
