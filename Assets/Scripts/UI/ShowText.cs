/*
**  ShowText.cs: Contains public functions for showing and hiding the text component of this gameobject
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
    public float fadeSeconds = 5f;

    private Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    //Start hidded
    void Start()
    {
        Hide();
    }

    public void Hide()
    {
        text.enabled = false;
    }

    public void Show()
    {
        text.enabled = true;
    }
}
