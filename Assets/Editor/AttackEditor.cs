/*
**  AttackEditor.cs: Custom inspector script for Attacks
*/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Attack))]
public class AttackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //The item that this script is targeting. That is, the item that is selected in the inspector.
        Attack attack = (Attack)target;

        DrawAttackOptions(attack);

        //Make sure to re-draw
        EditorUtility.SetDirty(target);
    }

    public static void DrawAttackOptions(Attack attack)
    {
        attack.attackName = EditorGUILayout.TextField("Name", attack.attackName);

        attack.slotIcon = (Sprite)EditorGUILayout.ObjectField("Slot Icon", attack.slotIcon, typeof(Sprite), false);
        EditorGUILayout.Space();

        attack.manaCost = EditorGUILayout.IntField("Mana Cost", attack.manaCost);
        attack.coolDownTime = EditorGUILayout.FloatField("Cool-down Time", attack.coolDownTime);
        EditorGUILayout.Space();

        attack.type = (Attack.Type)EditorGUILayout.EnumPopup("Type", attack.type);
        EditorGUILayout.Space();

        //Show different options depending on attack type
        switch (attack.type)
        {
            case Attack.Type.Projectile:
                attack.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", attack.attackPrefab, typeof(GameObject), false);
                break;
            case Attack.Type.Trap:
                attack.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Trap Prefab", attack.attackPrefab, typeof(GameObject), false);
                break;
            case Attack.Type.Cast:
                //Case type attacks show additional variables related to stats they affect
                attack.statType = (Attack.Stat)EditorGUILayout.EnumPopup("Stat to Affect", attack.statType);
                attack.power = EditorGUILayout.IntField("Power", attack.power);
                EditorGUILayout.Space();

                attack.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Cast Effect Prefab", attack.attackPrefab, typeof(GameObject), false);
                break;
            case Attack.Type.Spawn:
                attack.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Spawn Prefab", attack.attackPrefab, typeof(GameObject), false);
                attack.amountToSpawn = EditorGUILayout.IntField("Amount to Spawn", attack.amountToSpawn);
                break;
        }

        EditorGUILayout.Space();
    }
}