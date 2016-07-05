/*
**  AttackSetEditor.cs: Custom inspector script for AttackSets
*/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AttackSet))]
public class AttackSetEditor : Editor
{
    bool[] attackFoldout = { false, false, false, false };

    public override void OnInspectorGUI()
    {
        //The item that this script is targeting. That is, the item that is selected in the inspector.
        AttackSet attack = (AttackSet)target;

        attack.setName = EditorGUILayout.TextField("Name", attack.setName);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Attacks can be edited here, and changes will be made to the attack itself (saves time finding attack in project).", MessageType.Info);
        EditorGUILayout.Space();

        for (int i = 0; i < 4; i++)
        {
            //Display field to place attack scriptableobject in
            attack.attacks[i] = (Attack)EditorGUILayout.ObjectField("Attack " + (i + 1), attack.attacks[i], typeof(Attack), false);

            //Display editor for each attack, under a drop-down menu (saves time finding attack in project)
            if (attack.attacks[i] != null)
            {
                EditorGUI.indentLevel++;

                attackFoldout[i] = EditorGUILayout.Foldout(attackFoldout[i], "Attack: " + attack.attacks[i].attackName);

                if (attackFoldout[i])
                {
                    EditorGUI.indentLevel++;
                    AttackEditor.DrawAttackOptions(attack.attacks[i]);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
            }
        }

        //Make sure to re-draw
        EditorUtility.SetDirty(target);
    }
}