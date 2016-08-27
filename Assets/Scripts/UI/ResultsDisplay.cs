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

        if (playerInfo)
        {
            playerNameText.text = playerInfo.username;

            //Kills badge show/hide
            if (scoreboard.GetBestPlayer(Scoreboard.ScoreType.Kills) == playerInfo.username)
            {
                killsBadge.isOwned = true;
                killsBadge.image.color = enabledTint;
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
            }
            else
            {
                ratioBadge.isOwned = false;
                ratioBadge.image.color = disabledTint;
            }
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