/*
**  AttackSet.cs: Defines a class, with attacks and stats
*/

using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Attack Set", menuName = "Data/Attack Set")]
public class AttackSet : ScriptableObject
{
    public string setName = "Attack Set";

    public int health = 100;
    public int mana = 100;

    public int healthRegen = 0;
    public int manaRegen = 2;

    public float moveSpeed = 10.0f;

    public string description;

    public Attack[] attacks = new Attack[4];
}
