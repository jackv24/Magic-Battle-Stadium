/*
**  ResultsDisplay.cs: Controls the results screen at the end of a match
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResultsDisplay : MonoBehaviour
{
    //UI objects
    public Text nameText;
    public Text killsText;
    public Text deathsText;
    public Text ratioText;

    private Scoreboard scoreboard;

    void OnEnable()
    {
        scoreboard = Scoreboard.instance;

        nameText.text = scoreboard.nameText.text;
        killsText.text = scoreboard.killsText.text;
        deathsText.text = scoreboard.deathsText.text;
        ratioText.text = scoreboard.ratioText.text;
    }
}
