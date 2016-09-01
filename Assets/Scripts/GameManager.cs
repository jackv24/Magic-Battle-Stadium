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
    [Header("Attack Slots")]
    public AttackSet[] attackSets;
    public int currentAttackSet = 0;

    [Space()]
    public AttackSlots attackSlots;

    [Header("Game Timing")]
    public int gameStartTime = 30;
    public bool hasGameStarted = true;

    [Space()]
    public GameTimer gameStart;

    [Header("Prize Values")]
    public int prizePerKill = 100;
    public int prizePerBadge = 500;
    public int prizePerMatch = 200;

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
