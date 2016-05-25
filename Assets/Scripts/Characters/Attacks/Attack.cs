/*
**  Attack.cs: Scriptableobject which holds information pertaining to a specific attack
*/

using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Attack", menuName = "Data/Attack")]
public class Attack : ScriptableObject
{
    //Name to display for this attack
    public string attackName = "Attack";

    //How much mana this attack costs to use
    public int manaCost = 0;

    //Interval at which the attack can be performed
    public float fireTime = 0.25f;

    //Prefab to spawn when this attack is used
    public GameObject attackPrefab;

    //The icon to display in the slot for this attack
    public Sprite slotIcon;
}
