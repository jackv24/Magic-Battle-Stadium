/*
**  GameTimer.cs: Controls the start of the game and sending timer to clients
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameTimer : NetworkBehaviour
{
    //For showing the final seconds more boldy
    public Text finalCountdownText;

    public GameObject resultsScreen;

    private Text timerText;

    [Multiline]
    public string startText;
    [Multiline]
    public string timeText;

    //Counters with hooks (timers managed on server, changed propogated to clients)
    [SyncVar(hook ="UpdateStartTime")]
    private int startTimeLeft;
    [SyncVar(hook = "UpdateRunTime")]
    private int runTimeLeft;

#if !UNITY_EDITOR
    private Vector2 initialPos;
#endif

    void Awake()
    {
        timerText = GetComponent<Text>();

        timerText.text = string.Format(startText, "", "");

#if !UNITY_EDITOR
        initialPos = transform.localPosition;
#endif
    }

    void Start()
    {
        //Reset UI position (NetworkIdentity moves it for some reason - this fixes that)
        //Problem only exists outside of unity editor
#if !UNITY_EDITOR
        transform.localPosition = initialPos;
#endif
    }

    void Update()
    {
        //Host can skip countdown
        if (isServer && startTimeLeft > 5)
        {
            if (Input.GetKeyDown(KeyCode.N))
                StartGame(6);

            //For skipping countdown during testing
            if (Input.GetKeyDown(KeyCode.O) && Input.GetKeyDown(KeyCode.P))
                StartGame(0);
        }
    }

    //Starts the pre-game timer
    public void StartGame(int value)
    {
        if (isServer)
        {
            startTimeLeft = value;

            StopCoroutine("GameStartCountdown");
            StartCoroutine("GameStartCountdown");
        }
    }

    //Hook that updates when timeleft changed
    void UpdateStartTime(int value)
    {
        startTimeLeft = value;

        //Only host should be told how to skip countdown
        if(isServer)
            timerText.text = string.Format(startText, startTimeLeft, "Press 'N' to skip countdown");
        else
            timerText.text = string.Format(startText, startTimeLeft, "");

        //Show final seconds more boldly
        if (startTimeLeft <= 5)
        {
            finalCountdownText.gameObject.SetActive(true);
            finalCountdownText.text = startTimeLeft.ToString();
        }

        //Start game when countdown finishes
        if (startTimeLeft <= 0)
        {
            GameManager.instance.hasGameStarted = true;

            finalCountdownText.gameObject.SetActive(false);
        }
        else
            GameManager.instance.hasGameStarted = false;
    }

    //Hook updates when runtimeleft changed
    void UpdateRunTime(int value)
    {
        runTimeLeft = value;

        //If total time is longer than a minute...
        if (runTimeLeft >= 60)
        {
            //Display time as minutes and seconds
            int seconds = runTimeLeft % 60;
            int minutes = runTimeLeft / 60;

            timerText.text = string.Format(timeText, minutes + ":" + seconds.ToString("00"));
        }
        else
            //Else only display seconds
            timerText.text = string.Format(timeText, runTimeLeft);

        if (runTimeLeft <= 0)
        {
            //Stop game
            GameManager.instance.hasGameStarted = false;

            if (resultsScreen)
                //Show results screen
                resultsScreen.SetActive(true);
            else
            {
                finalCountdownText.text = "Game Ended";
                finalCountdownText.gameObject.SetActive(true);
            }
        }
    }

    //Countdown timer (only run on host)
    IEnumerator GameStartCountdown()
    {
        while (startTimeLeft > 0)
        {
            startTimeLeft--;

            yield return new WaitForSeconds(1);
        }

        //After game start countdown has ended, start game run countdown
        //Load time limit from playerprefs
        int minutes = int.Parse(PlayerPrefs.GetString("timeLimit", "5"));

        //Run game timer (making sure to reset it)
        StopCoroutine("GameRunTimer");
        StartCoroutine("GameRunTimer", minutes);
    }

    //Keeps track of game running timer
    IEnumerator GameRunTimer(int minutes)
    {
        //Convert minutes to toal seconds
        runTimeLeft = minutes * 60;

        while (runTimeLeft > 0)
        {
            runTimeLeft--;

            yield return new WaitForSeconds(1);
        }
    }
}
