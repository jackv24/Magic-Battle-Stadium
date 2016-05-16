using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DisplayKillText : MonoBehaviour
{
    //Static instance to this script (ease of access, as there should only ever be one)
    public static DisplayKillText instance;

    //Opacities of different levels of strings (size of this array also determines to max amount of kill strings)
    public float[] stringOpacity;

    //Current list of kill strings
    private List<string> killStrings = new List<string>();

    //Text to update
    private Text killText;

    void Awake()
    {
        instance = this;

        killText = GetComponent<Text>();
    }

    public void UpdateKills()
    {
        int maxLength = stringOpacity.Length;

        //Remove old strings
        int overflow = killStrings.Count - maxLength;
        for (int i = overflow; i > 0; i--)
        {
            killStrings.RemoveAt(0);
        }

        //The final string to replace old string with
        string finalString = "";

        int opacityIndex = 0;
        //Build string by adding kill strings in reverse order (displays newest in list at top)
        for (int i = killStrings.Count - 1; i >= 0; i--)
        {
            Color textColor = new Color(1f, 1f, 1f, stringOpacity[opacityIndex]);
            opacityIndex++;

            finalString +="<color=\"#" + ColorUtility.ToHtmlStringRGBA(textColor) + "\">" + killStrings[i] + "</color>\n";
        }

        //Set the text to be this new string
        killText.text = finalString;
    }

    //Adds a kill to the display of kill text
    public void AddKill(string playerOneName, string playerTwoName, string weaponName)
    {
        //construct new string
        string newString = playerOneName + " killed " + playerTwoName + " with " + weaponName;
        //add string to kill strings list
        killStrings.Add(newString);

        //Update display
        UpdateKills();
    }
}
