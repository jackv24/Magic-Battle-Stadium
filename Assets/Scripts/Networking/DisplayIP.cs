/*
**  DisplayIP.cs:   Must be attached to the same gameobject as a text component,
**                  displays the local player's IP address.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DisplayIP : MonoBehaviour
{
    private Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Start()
    {
        text.text = "NA";

#if !UNITY_WEBGL
        text.text = "IP: " + Network.player.ipAddress;
#endif
    }
}
