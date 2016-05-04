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

    void Start()
    {
        text.enabled = false;
    }

    public void Show()
    {
        text.enabled = true;
    }
}
