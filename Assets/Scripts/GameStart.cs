/*
**  GameStart.cs: Controls the start of the game and sending timer to clients
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameStart : NetworkBehaviour
{
    //For showing the final seconds more boldy
    public Text finalCountdownText;

    private Text startText;
    private string startTextString;

    [SyncVar(hook ="UpdateTime")]
    private int timeLeft;

    void Awake()
    {
        startText = GetComponent<Text>();
        startTextString = startText.text;

        startText.text = string.Format(startTextString, "", "");
    }

    void Update()
    {
        //Host can skip countdown
        if (isServer)
        {
            if (timeLeft > 5 && Input.GetKeyDown(KeyCode.N))
                StartGame(6);

            //For skipping countdown during testing
            if (Input.GetKeyDown(KeyCode.O) && Input.GetKeyDown(KeyCode.P))
                StartGame(0);
        }
    }

    public void StartGame(int value)
    {
        if (isServer)
        {
            timeLeft = value;

            StopCoroutine("GameStartCountdown");
            StartCoroutine("GameStartCountdown");
        }
    }

    //Hook that updates when timeleft changed
    void UpdateTime(int value)
    {
        timeLeft = value;

        //Only host should be told how to skip countdown
        if(isServer)
            startText.text = string.Format(startTextString, timeLeft, "<size=24>Press 'N' to skip countdown</size>");
        else
            startText.text = string.Format(startTextString, timeLeft, "");

        //Show final seconds more boldly
        if (timeLeft <= 5)
        {
            finalCountdownText.gameObject.SetActive(true);
            finalCountdownText.text = timeLeft.ToString();
        }

        //Start game when countdown finishes
        if (timeLeft < 0)
        {
            GameManager.instance.hasGameStarted = true;

            finalCountdownText.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        else
            GameManager.instance.hasGameStarted = false;
    }

    //Countdown timer (only run on host)
    IEnumerator GameStartCountdown()
    {
        while (timeLeft >= 0)
        {
            timeLeft--;

            yield return new WaitForSeconds(1);
        }
    }
}
