using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;

namespace CreativeSpore.SuperTilemapEditor
{
    [CustomEditor(typeof(TilemapGroup))]
    public class TilemapGroupEditor : Editor
    {
        [MenuItem("GameObject/SuperTilemapEditor/TilemapGroup", false, 10)]
        static void CreateTilemap()
        {
            GameObject obj = new GameObject("TilemapGroup");
            obj.AddComponent<TilemapGroup>();
        }

        private ReorderableList m_tilemapReordList;
        private TilemapEditor m_tilemapEditor;
        private TilemapGroup m_target;

        private SerializedProperty m_selectedIndexProp;
        private void OnEnable()
        {
            m_target = target as TilemapGroup;
            m_target.Refresh();
            m_selectedIndexProp = serializedObject.FindProperty("m_selectedIndex");
            m_tilemapReordList = CreateTilemapReorderableList();
            m_tilemapReordList.index = m_selectedIndexProp.intValue;
        }

        private void OnDisable()
        {
            if (m_tilemapEditor)
                TilemapEditor.DestroyImmediate(m_tilemapEditor);
        }

        public override void OnInspectorGUI()
        {
            var targetObj = target as TilemapGroup;
            // NOTE: this happens during undo/redo
            if( targetObj.transform.childCount != targetObj.Tilemaps.Count) 
            {
                targetObj.Refresh();
            }
            serializedObject.Update();

            if (m_tilemapReordList.HasKeyboardControl())
                DoKeyboardChecks();

            // clamp index to valid value
            m_selectedIndexProp.intValue = m_tilemapReordList.index = Mathf.Clamp(m_selectedIndexProp.intValue, -1, targetObj.Tilemaps.Count - 1);

            // Draw Tilemap List
            m_tilemapReordList.DoLayoutList();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_unselectedColorMultiplier"), new GUIContent("Highlight Alpha"));
            EditorGUILayout.Space();

            // Draw Tilemap Inspector
            TilemapEditor tilemapEditor = GetTilemapEditor();
            if (tilemapEditor)
            {
                tilemapEditor.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();            

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                Repaint();
            }
        }

        public void OnSceneGUI()
        {
            TilemapEditor tilemapEditor = GetTilemapEditor();
            if (tilemapEditor)
            {
                (tilemapEditor as TilemapEditor).OnSceneGUI();                
            }

            DoKeyboardChecks();
        }

        private void DoKeyboardChecks()
        {
            Event e = Event.current;
            // Cycle over tilemaps
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == ShortcutKeys.k_NextLayer) m_tilemapReordList.index++;
                else if (e.keyCode == ShortcutKeys.k_PrevLayer) m_tilemapReordList.index += m_tilemapReordList.count - 1;
                m_tilemapReordList.index %= m_tilemapReordList.count;
            }
        }

        //NOTE: m_tilemapEditor.target changes when OnSceneGUI is called, so this method makes sure to create it again if target doesn't match
        private TilemapEditor GetTilemapEditor()
        {
            var targetObj = target as TilemapGroup;
            if (!m_tilemapEditor || !m_tilemapEditor.target || m_tilemapEditor.target != targetObj.SelectedTilemap)
            {
                if (targetObj.SelectedTilemap)
                {
                    if (m_tilemapEditor)
                        TilemapEditor.DestroyImmediate(m_tilemapEditor);
                    m_tilemapEditor = TilemapEditor.CreateEditor(targetObj.SelectedTilemap) as TilemapEditor;
                }
                else
                {
                    if (m_tilemapEditor)
                        TilemapEditor.DestroyImmediate(m_tilemapEditor);
                    m_tilemapEditor = null;
                }
            }
            return m_tilemapEditor;
        }

        private ReorderableList CreateTilemapReorderableList()
        {
            ReorderableList reordList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_tilemaps"), true, true, true, true);
            reordList.displayAdd = reordList.displayRemove = true;
            reordList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Tilemaps", EditorStyles.boldLabel);
                Texture2D btnTexture = reordList.elementHeight == 0f ? EditorGUIUtility.FindTexture("winbtn_win_max_h") : EditorGUIUtility.FindTexture("winbtn_win_min_h");
                if (GUI.Button(new Rect(rect.width - rect.height, rect.y, rect.height, rect.height), btnTexture, EditorStyles.label))
                {
                    reordList.elementHeight = reordList.elementHeight == 0f ? 21f : 0f;
                    reordList.draggable = reordList.elementHeight > 0f;
                }
            };
            reordList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (reordList.elementHeight == 0)
                    return;
                var element = reordList.serializedProperty.GetArrayElementAtIndex(index);               
                rect.y += 2;
                Tilemap tilemap = element.objectReferenceValue as Tilemap;
                SerializedObject tilemapSerialized = new SerializedObject(tilemap);
                SerializedObject tilemapObjSerialized = new SerializedObject(tilemapSerialized.FindProperty("m_GameObject").objectReferenceValue);

                Rect rToggle = new Rect(rect.x, rect.y, 16f, EditorGUIUtility.singleLineHeight);
                Rect rName = new Rect(rect.x + 20f, rect.y, rect.width - 130f - 20f, EditorGUIUtility.singleLineHeight);
                Rect rColliders = new Rect(rect.x + rect.width - 125f, rect.y, 125f, EditorGUIUtility.singleLineHeight);
                Rect rSortingLayer = new Rect(rect.x + rect.width - 125f, rect.y, 80f, EditorGUIUtility.singleLineHeight);
                Rect rSortingOrder = new Rect(rect.x + rect.width - 40f, rect.y, 40f, EditorGUIUtility.singleLineHeight);

                tilemap.IsVisible = EditorGUI.Toggle(rToggle, GUIContent.none, tilemap.IsVisible, STEditorStyles.Instance.visibleToggleStyle);
                EditorGUI.PropertyField(rName, tilemapObjSerialized.FindProperty("m_Name"), GUIContent.none);
                if (TilemapEditor.EditMode == TilemapEditor.eEditMode.Collider)
                {
                    SerializedProperty colliderTypeProperty = tilemapSerialized.FindProperty("ColliderType");
                    string[] colliderTypeNames = new List<string>(System.Enum.GetNames(typeof(eColliderType)).Select(x => x.Replace('_', ' '))).ToArray();
                    EditorGUI.BeginChangeCheck();
                    colliderTypeProperty.intValue = GUI.Toolbar(rColliders, colliderTypeProperty.intValue, colliderTypeNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        tilemapSerialized.ApplyModifiedProperties();
                        tilemap.Refresh(false, true);
                    }
                }
                else
                {
                    // Sorting Layer and Order in layer            
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(rSortingLayer, tilemapSerialized.FindProperty("m_sortingLayer"), GUIContent.none);
                    EditorGUI.PropertyField(rSortingOrder, tilemapSerialized.FindProperty("m_orderInLayer"), GUIContent.none);
                    tilemapSerialized.FindProperty("m_orderInLayer").intValue = (tilemapSerialized.FindProperty("m_orderInLayer").intValue << 16) >> 16; // convert from int32 to int16 keeping sign
                    if (EditorGUI.EndChangeCheck())
                    {
                        tilemapSerialized.ApplyModifiedProperties();
                        tilemap.RefreshChunksSortingAttributes();
                        SceneView.RepaintAll();
                    }
                    //--- 
                }

                if(GUI.changed)
                {
                    tilemapObjSerialized.ApplyModifiedProperties();
                }
            };
            reordList.onReorderCallback = (ReorderableList list) =>
            {
                var targetObj = target as TilemapGroup;
                int sibilingIdx = 0;
                foreach (Tilemap tilemap in targetObj.Tilemaps)
                {
                    tilemap.transform.SetSiblingIndex(sibilingIdx++);
                }
                Repaint();
            };
            reordList.onSelectCallback = (ReorderableList list) =>
            {
                m_selectedIndexProp.intValue = reordList.index;
                serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
                TileSelectionWindow.RefreshIfVisible();
                TilePropertiesWindow.RefreshIfVisible();
            };
            reordList.onAddCallback = (ReorderableList list) =>
            {
                var targetObj = target as TilemapGroup;
                Undo.RegisterCompleteObjectUndo(targetObj, "New Tilemap");
                GameObject obj = new GameObject();
                Undo.RegisterCreatedObjectUndo(obj, "New Tilemap");
                Tilemap newTilemap = obj.AddComponent<Tilemap>();
                obj.transform.parent = targetObj.transform;
                obj.name = GameObjectUtility.GetUniqueNameForSibling(obj.transform.parent, "New Tilemap");

                Tilemap copiedTilemap = targetObj.SelectedTilemap;
                if(copiedTilemap)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(copiedTilemap);
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(newTilemap);
                    obj.name = GameObjectUtility.GetUniqueNameForSibling(obj.transform.parent, copiedTilemap.name);
                }
            };
            reordList.onRemoveCallback = (ReorderableList list) =>
            {
                var targetObj = target as TilemapGroup;
                Undo.DestroyObjectImmediate(targetObj.SelectedTilemap.gameObject);
                //NOTE: Fix argument exception
                if (m_tilemapReordList.index == targetObj.Tilemaps.Count - 1)
                {
                    m_selectedIndexProp.intValue = m_tilemapReordList.index = m_tilemapReordList.index - 1;
                }
            };

            return reordList;
        }
    }
}