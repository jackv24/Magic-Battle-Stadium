/*
**  GameManager.cs: Holds variables and functions that need to be accessed from multiple scripts
*/

using UnityEngine;
using System.Collections;

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

    public int gameStartTime = 30;

    public bool hasGameStarted = true;

    public GameTimer gameStart;

    void Awake()
    {
        instance = this;
    }

    void Initialise()
    {
        //Initialise attack slots
        attackSlots.InitialiseSlots();
    }

    public void StartGame()
    {
        gameStart.StartGame(gameStartTime);
    }
}
