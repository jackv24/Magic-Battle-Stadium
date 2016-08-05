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
        AttackSet attackSet = (AttackSet)target;

        attackSet.setName = EditorGUILayout.TextField("Name", attackSet.setName);
        EditorGUILayout.Space();

        attackSet.health = EditorGUILayout.IntField("Health", attackSet.health);
        attackSet.mana = EditorGUILayout.IntField("Mana", attackSet.mana);
        EditorGUILayout.Space();

        attackSet.healthRegen = EditorGUILayout.IntField("Health Regen", attackSet.healthRegen);
        attackSet.manaRegen = EditorGUILayout.IntField("Mana Regen", attackSet.manaRegen);
        EditorGUILayout.Space();

        attackSet.moveSpeed = EditorGUILayout.FloatField("Move Speed", attackSet.moveSpeed);
        EditorGUILayout.Space();

        EditorGUILayout.PrefixLabel("Description");
        attackSet.description = EditorGUILayout.TextArea(attackSet.description, GUILayout.MinHeight(100f));
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Attacks can be edited here, and changes will be made to the attack itself (saves time finding attack in project).", MessageType.Info);
        EditorGUILayout.Space();

        for (int i = 0; i < 4; i++)
        {
            //Display field to place attack scriptableobject in
            attackSet.attacks[i] = (Attack)EditorGUILayout.ObjectField("Attack " + (i + 1), attackSet.attacks[i], typeof(Attack), false);

            //Display editor for each attack, under a drop-down menu (saves time finding attack in project)
            if (attackSet.attacks[i] != null)
            {
                EditorGUI.indentLevel++;

                attackFoldout[i] = EditorGUILayout.Foldout(attackFoldout[i], "Attack: " + attackSet.attacks[i].attackName);

                if (attackFoldout[i])
                {
                    EditorGUI.indentLevel++;
                    AttackEditor.DrawAttackOptions(attackSet.attacks[i]);
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