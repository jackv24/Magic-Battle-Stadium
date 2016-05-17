/*
**  GameManager.cs: Holds variables and functions that need to be accessed from multiple scripts
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject localPlayer;

    public float gameStartTime = 30f;

    public bool hasGameStarted = false;
    public int connectedPlayers, maxPlayers;

    private Text startText;
    private string startTextString;

    void Awake()
    {
        instance = this;

        startText = GameObject.Find("StartText").GetComponent<Text>();
        startTextString = startText.text;
    }

    public void ReadyGame()
    {
        if (!hasGameStarted)
        {
            startText.enabled = true;

            StartCoroutine("StartGameCountdown", gameStartTime);
        }
    }

    IEnumerator StartGameCountdown(float seconds)
    {
        float timeLeft = seconds;

        while (timeLeft >= 0)
        {
            startText.text = string.Format(startTextString, connectedPlayers, maxPlayers, timeLeft);

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        StartGame();
    }

    void StartGame()
    {
        startText.enabled = false;

        hasGameStarted = true;
    }
}
