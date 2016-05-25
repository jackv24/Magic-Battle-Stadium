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
        attack.fireTime = EditorGUILayout.FloatField("Fire Interval", attack.fireTime);
        EditorGUILayout.Space();

        attack.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Attack Prefab", attack.attackPrefab, typeof(GameObject), false);

        EditorUtility.SetDirty(target);
    }
}