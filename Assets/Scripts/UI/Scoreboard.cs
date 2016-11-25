/*
**  Scoreboard.cs: Manages a scoreboard, including UI.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    //There should only be one scoreboard
    public static Scoreboard instance;

    //UI objects
    public Text nameText;
    private string nameTextString;

    public Text killsText;
    private string killsTextString;

    public Text deathsText;
    private string deathsTextString;

    public Text ratioText;
    private string ratioTextString;

    private GameObject childPanel;

    //score objects
    [System.Serializable]
    public class Score
    {
        public string name;
        public int kills;
        public int deaths;
    }

    public List<Score> playerScores = new List<Score>();

    void Awake()
    {
        instance = this;

        //Child panel should hold UI, so this script can still run when UI is disabled
        childPanel = transform.GetChild(0).gameObject;
    }

    void Start()
    {
        //Scoreboard should start hidden
        childPanel.SetActive(false);

        //Set initial string values
        nameTextString = nameText.text;
        killsTextString = killsText.text;
        deathsTextString = deathsText.text;
        ratioTextString = ratioText.text;
    }

    void Update()
    {
        //Holding info button shows scoreboard
        if (Input.GetButtonDown("Info") && GameManager.instance.hasGameStarted)
        {
            childPanel.SetActive(true);
            UpdateDisplay();
        }
        else if (Input.GetButtonUp("Info"))
        {
            childPanel.SetActive(false);
            UpdateDisplay();
        }
    }

    //Adds a kill to a name and updates the display
    public void AddKill(string name)
    {
        int index = GetScoreIndex(name);

        if (index >= 0)
            playerScores[index].kills += 1;

        UpdateDisplay();
    }
    //Adds a death to a name and updates the display
    public void AddDeath(string name)
    {
        int index = GetScoreIndex(name);

        if (index >= 0)
            playerScores[index].deaths += 1;

        UpdateDisplay();
    }

    //Adds a player to the scoreboard
    public void AddPlayer(string name)
    {
        Score playerScore = new Score();
        playerScore.name = name;
        playerScore.kills = 0;
        playerScore.deaths = 0;

        //Add Player to list
        playerScores.Add(playerScore);

        //Update display
        UpdateDisplay();
    }

    public int GetScoreIndex(string playerName)
    {
        for(int i = 0; i < playerScores.Count; i++)
        {
            if (playerScores[i].name == playerName)
                return i;
        }

        return -1;
    }

    //Removes a player from the scoreboard
    public void RemovePlayer(string name)
    {
        //store index of player score info (should be the same in all lists)
        int index = GetScoreIndex(name);

        if(index >= 0)
            playerScores.RemoveAt(index);

        //Update the display
        UpdateDisplay();
    }

    //Updates the UI with scoreboard values
    void UpdateDisplay()
    {
        //Sort by kills
        playerScores.Sort((a, b) => b.kills.CompareTo(a.kills));

        //Reset text values
        if(nameText)
            nameText.text = nameTextString;
        if(killsText)
            killsText.text = killsTextString;
        if(deathsText)
            deathsText.text = deathsTextString;
        if (ratioText)
            ratioText.text = ratioTextString;

        //Iterate through and add values to scoreboard (names, kills, and deaths list should align)
        foreach (Score score in playerScores)
        {
            if(nameText)
                nameText.text += score.name + "\n";
            if(killsText)
                killsText.text += score.kills + "\n";
            if(deathsText)
                deathsText.text += score.deaths + "\n";

            //display ratio as decimal, if NaN display '-'
            if (ratioText)
            {
                if (score.kills <= 0)
                    ratioText.text += "0.00\n";
                else if (score.deaths <= 0)
                    ratioText.text += "<size=20>★</size>" + score.kills + "\n";
                else
                    ratioText.text += ((float)score.kills / score.deaths).ToString("0.00") + "\n";
            }
        }
    }

    //Checks if a player exists in the scoreboard
    public bool PlayerExists(string name)
    {
        if (GetScoreIndex(name) >= 0)
            return true;
        else
            return false;
    }

    //Changes the name of a player on the scoreboard
    public void ChangeName(string before, string after)
    {
        playerScores[GetScoreIndex(before)].name = after;

        UpdateDisplay();
    }

    public enum ScoreType
    {
        Kills, Deaths, Ratio
    }

    //Returns the name of the best player for a particular score type
    public string GetBestPlayer(ScoreType type)
    {
        string name = "";
        float best;
        bool isDraw = false;

        switch (type)
        {
            case ScoreType.Kills:
                best = -1;
                for(int i = 0; i < playerScores.Count; i++)
                {
                    if (playerScores[i].kills > best)
                    {
                        best = playerScores[i].kills;
                        name = playerScores[i].name;
                    }
                    else if (playerScores[i].kills == best)
                        isDraw = true;
                }
                break;
            case ScoreType.Deaths:
                best = int.MaxValue;
                for (int i = 0; i < playerScores.Count; i++)
                {
                    if (playerScores[i].deaths < best)
                    {
                        best = playerScores[i].deaths;
                        name = playerScores[i].name;
                    }
                    else if (playerScores[i].deaths == best)
                        isDraw = true;
                }
                break;
            case ScoreType.Ratio:
                best = -1;
                for (int i = 0; i < playerScores.Count; i++)
                {
                    float ratio;

                    //Prevent ratio being NaN or infinite
                    if (playerScores[i].kills <= 0)
                        ratio = 0;
                    else if (playerScores[i].deaths <= 0)
                        ratio = playerScores[i].deaths;
                    else
                        ratio = (float)playerScores[i].kills / playerScores[i].deaths;

                    if (ratio > best)
                    {
                        best = ratio;
                        name = playerScores[i].name;
                    }
                    else if (ratio == best)
                        isDraw = true;
                }
                break;
        }

        //No one wins if it is a draw
        if (!isDraw)
            return name;
        else
            return "";
    }
}
