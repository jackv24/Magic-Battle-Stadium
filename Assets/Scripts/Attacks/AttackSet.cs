/*
**  AttackSet.cs: A set of attacks. Basically a container for attacks, with a name.
*/

using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Attack Set", menuName = "Data/Attack Set")]
public class AttackSet : ScriptableObject
{
    public string setName = "Attack Set";

    public Attack[] attacks = new Attack[4];
}
