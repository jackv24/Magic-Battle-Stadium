/*
**  GameManager.cs: Holds variables and functions that need to be accessed from multiple scripts
*/

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject localPlayer;

    void Awake()
    {
        instance = this;
    }
}
