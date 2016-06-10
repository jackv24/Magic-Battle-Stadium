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
    public float coolDownTime = 0.25f;

    //The type of attack (used to differentiate between behaviour types)
    public enum Type
    {
        Projectile,
        Trap,
        Cast,
        Spawn
    }
    public Type type;

    //Prefab to spawn when this attack is used
    public GameObject attackPrefab;

    //How many prefabs to spawn (only shown for spawn-type attacks)
    public int amountToSpawn = 1;

    //The icon to display in the slot for this attack
    public Sprite slotIcon;
}
