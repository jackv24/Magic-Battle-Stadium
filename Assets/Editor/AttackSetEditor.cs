/*
**  AttackSetEditor.cs: Custom inspector script for AttackSets
*/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AttackSet))]
public class AttackSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //The item that this script is targeting. That is, the item that is selected in the inspector.
        AttackSet attack = (AttackSet)target;

        attack.setName = EditorGUILayout.TextField("Name", attack.setName);
        EditorGUILayout.Space();

        for (int i = 0; i < attack.attacks.Length; i++)
        {
            attack.attacks[i] = (Attack)EditorGUILayout.ObjectField("Attack " + (i + 1), attack.attacks[i], typeof(Attack), false);
        }

        //Make sure to re-draw
        EditorUtility.SetDirty(target);
    }
}