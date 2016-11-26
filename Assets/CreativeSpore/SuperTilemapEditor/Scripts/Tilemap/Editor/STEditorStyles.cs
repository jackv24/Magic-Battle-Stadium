using UnityEngine;
using UnityEditor;
using System.Collections;

public class STEditorStyles
{
    static STEditorStyles s_instance;
    public static STEditorStyles Instance { get { if (s_instance == null) s_instance = new STEditorStyles(); return s_instance; } }

    public GUIStyle visibleToggleStyle = new GUIStyle(EditorStyles.toggle)
    {
        normal = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff") },
        active = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff") },
        focused = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff") },
        hover = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff") },
        onNormal = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleon") },
        onActive = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleon") },
        onFocused = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleon") },
        onHover = { background = EditorGUIUtility.FindTexture("animationvisibilitytoggleon") },
    };
}
