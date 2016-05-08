/*
**  ShowMenu.cs: Toggles the display of a menu when the specified button is pressed. Can also be called externally (from the UI for example)
*/

using UnityEngine;
using System.Collections;

public class ShowMenu : MonoBehaviour
{
    //Name of input axis button to use
    public string inputName = "Cancel";

    //The first chld should be the gameobject to disable (keeps this script running)
    private GameObject childPanel;

    void Start()
    {
        childPanel = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (Input.GetButtonDown(inputName))
            Toggle();
    }

    //Toggles the menu
    public void Toggle()
    {
        if (childPanel.activeSelf)
            Disable();
        else
            Enable();
    }

    //Enable and disable the menu externally
    public void Enable()
    {
        childPanel.SetActive(true);
    }
    public void Disable()
    {
        childPanel.SetActive(false);
    }
}
