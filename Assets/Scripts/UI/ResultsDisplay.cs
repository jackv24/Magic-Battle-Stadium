/*
**  ResultsDisplay.cs: Controls the results screen at the end of a match
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResultsDisplay : MonoBehaviour
{
    [System.Serializable]
    public class Badge
    {
        public Image image;

        public bool isOwned = false;
    }

    //Scoreboard UI
    [Header("Text Objects")]
    public Text nameText;
    public Text killsText;
    public Text deathsText;
    public Text ratioText;

    [Space()]
    public Text prizeText;

    private Scoreboard scoreboard;

    //Player results
    [System.Serializable]
    public class PlayerGraphics
    {
        public Image hat;
        public Image clothes;
        public Image skin;
    }
    [Header("Player Info")]
    public PlayerGraphics playerGraphics;

    [Space()]
    public Text playerNameText;

    [Header("Badges")]
    public Badge killsBadge;
    public Badge ratioBadge;
    public Badge deathsBadge;

    [Header("Display Preferences")]
    public Color enabledTint = Color.white;
    public Color disabledTint = new Color(1, 1, 1, 0.125f);

    void OnEnable()
    {
        //Get scoreboard instance reference
        scoreboard = Scoreboard.instance;

        if (scoreboard)
        {
            nameText.text = scoreboard.nameText.text;
            killsText.text = scoreboard.killsText.text;
            deathsText.text = scoreboard.deathsText.text;
            ratioText.text = scoreboard.ratioText.text;
        }

        //Display player info
        PlayerInfo playerInfo = null;
        PlayerColour playerColour = null;

        if (GameManager.instance.LocalPlayer)
        {
            playerInfo = GameManager.instance.LocalPlayer.GetComponent<PlayerInfo>();
            playerColour = GameManager.instance.LocalPlayer.GetComponent<PlayerColour>();
        }


        int killCount = 0;
        int badgeCount = 0;

        if (playerInfo)
        {
            playerNameText.text = playerInfo.username;

            //Get player kill count
            killCount = scoreboard.playerScores[scoreboard.GetScoreIndex(playerInfo.username)].kills;

            //Kills badge show/hide
            if (scoreboard.GetBestPlayer(Scoreboard.ScoreType.Kills) == playerInfo.username)
            {
                killsBadge.isOwned = true;
                killsBadge.image.color = enabledTint;
                badgeCount++;
            }
            else
            {
                killsBadge.isOwned = false;
                killsBadge.image.color = disabledTint;
            }

            //Deaths badge show/hide
            if (scoreboard.GetBestPlayer(Scoreboard.ScoreType.Deaths) == playerInfo.username)
            {
                deathsBadge.isOwned = true;
                deathsBadge.image.color = enabledTint;
                badgeCount++;
            }
            else
            {
                deathsBadge.isOwned = false;
                deathsBadge.image.color = disabledTint;
            }

            //Ratio badge show/hide
            if (scoreboard.GetBestPlayer(Scoreboard.ScoreType.Ratio) == playerInfo.username)
            {
                ratioBadge.isOwned = true;
                ratioBadge.image.color = enabledTint;
                badgeCount++;
            }
            else
            {
                ratioBadge.isOwned = false;
                ratioBadge.image.color = disabledTint;
            }

            //Calculate prize
            int prize = (killCount * GameManager.instance.prizePerKill) + (badgeCount * GameManager.instance.prizePerBadge) + GameManager.instance.prizePerMatch;

            //Get, update, and set balance
            int balance = PlayerPrefs.GetInt("cashBalance", 0);
            balance += prize;
            PlayerPrefs.SetInt("cashBalance", balance);

            //Display prize and balance
            prizeText.text = string.Format(prizeText.text, prize, balance);
        }

        //Set player colour display
        if (playerColour)
        {
            playerGraphics.hat.color = playerColour.hatColor;
            playerGraphics.skin.color = playerColour.skinColor;
            playerGraphics.clothes.color = playerColour.clothesColor;
        }
    }
}