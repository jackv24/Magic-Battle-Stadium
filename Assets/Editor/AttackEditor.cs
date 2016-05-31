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
                EditorGUILayout.HelpBox("Cast-type attacks do not spawn a prefab.", MessageType.Info);
                break;
        }

        //Make sure to re-draw
        EditorUtility.SetDirty(target);
    }
}