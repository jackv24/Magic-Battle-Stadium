/*
**  GameManager.cs: Holds variables and functions that need to be accessed from multiple scripts
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private GameObject localPlayer;
    public GameObject LocalPlayer
    {
        get { return localPlayer; }
        set
        {
            //Setting the local player calls initialise function to set up player-related things
            localPlayer = value;
            Initialise();
        }
    }

    //UI attack slots
    public AttackSlots attackSlots;

    public AttackSet[] attackSets;
    public int currentAttackSet = 0;

    private Text startText;
    private string startTextString;

    void Awake()
    {
        instance = this;

        if (GameObject.Find("StartText"))
        {
            startText = GameObject.Find("StartText").GetComponent<Text>();
            startTextString = startText.text;
        }
    }

    void Initialise()
    {
        //Initialise attack slots
        attackSlots.InitialiseSlots();
    }
}
