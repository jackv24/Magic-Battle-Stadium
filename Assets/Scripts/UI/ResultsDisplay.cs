/*
**  ResultsDisplay.cs: Controls the results screen at the end of a match
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResultsDisplay : MonoBehaviour
{
    //Scoreboard UI
    public Text nameText;
    public Text killsText;
    public Text deathsText;
    public Text ratioText;

    private Scoreboard scoreboard;

    //Player results
    public Text playerNameText;

    public GameObject killsBadge;
    public GameObject ratioBadge;
    public GameObject deathsBadge;

    void OnEnable()
    {
        //Get scoreboard instance reference
        scoreboard = Scoreboard.instance;

        nameText.text = scoreboard.nameText.text;
        killsText.text = scoreboard.killsText.text;
        deathsText.text = scoreboard.deathsText.text;
        ratioText.text = scoreboard.ratioText.text;
    }
}
