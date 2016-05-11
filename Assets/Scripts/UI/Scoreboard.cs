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

    private GameObject childPanel;

    //score objects
    private List<string> names = new List<string>();
    private List<int> kills = new List<int>();
    private List<int> deaths = new List<int>();

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
    }

    void Update()
    {
        //Holding info button shows scoreboard
        if (Input.GetButtonDown("Info"))
            childPanel.SetActive(true);
        else if (Input.GetButtonUp("Info"))
            childPanel.SetActive(false);
    }

    //Adds a kill to a name and updates the display
    public void AddKill(string name)
    {
        kills[names.IndexOf(name)] += 1;

        UpdateDisplay();
    }
    //Adds a death to a name and updates the display
    public void AddDeath(string name)
    {
        deaths[names.IndexOf(name)] += 1;

        UpdateDisplay();
    }

    //Adds a player to the scoreboard
    public void AddPlayer(string name)
    {
        //Add name to list
        names.Add(name);
        //Scores should start at 0
        kills.Add(0);
        deaths.Add(0);

        //Update display
        UpdateDisplay();
    }

    //Removes a player from the scoreboard
    public void RemovePlayer(string name)
    {
        //store index of player score info (should be the same in all lists)
        int index = names.IndexOf(name);

        //Remove info from lists
        names.RemoveAt(index);
        kills.RemoveAt(index);
        deaths.RemoveAt(index);

        //Update the display
        UpdateDisplay();
    }

    //Updates the UI with scoreboard values
    void UpdateDisplay()
    {
        //Reset text values
        if(nameText)
            nameText.text = nameTextString;
        if(killsText)
            killsText.text = killsTextString;
        if(deathsText)
            deathsText.text = deathsTextString;

        //Iterate through and add values to scoreboard (names, kills, and deaths list should align)
        foreach (string name in names)
        {
            int index = names.IndexOf(name);

            nameText.text += name + "\n";
            killsText.text += kills[index] + "\n";
            deathsText.text += deaths[index] + "\n";
        }
    }

    //Checks if a player exists in the scoreboard
    public bool PlayerExists(string name)
    {
        return names.Contains(name);
    }

    //Changes the name of a player on the scoreboard
    public void ChangeName(string before, string after)
    {
        names[names.IndexOf(before)] = after;

        UpdateDisplay();
    }
}
