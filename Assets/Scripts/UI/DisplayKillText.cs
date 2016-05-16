using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DisplayKillText : MonoBehaviour
{
    public float[] opacityLevels;

    private List<string> killStrings = new List<string>();

    private Text killText;

    void Awake()
    {
        killText = GetComponent<Text>();
    }

    public void UpdateKills()
    {
        int length = opacityLevels.Length;

        int overflow = killStrings.Count - length;
        for (int i = overflow; i > 0; i--)
        {
            killStrings.RemoveAt(0);
        }

        string finalString = "";

        for (int i = 0; i < killStrings.Count; i++)
        {
            //TODO: Implement opacity
        }
    }

    public void AddKill(string playerOneName, string playerTwoName, string weaponName)
    {
        string newString = playerOneName + " killed " + playerTwoName + " with " + weaponName;

        killStrings.Add(newString);
    }
}
