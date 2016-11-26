using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;

namespace CreativeSpore.SuperTilemapEditor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RandomBrush))]
    public class RandomBrushEditor : TilesetBrushEditor
    {
        [MenuItem("Assets/Create/SuperTilemapEditor/Brush/RandomBrush")]
        public static RandomBrush CreateAsset()
        {
            return EditorUtils.CreateAssetInSelectedDirectory<RandomBrush>();
        }

        RandomBrush m_brush;
        ReorderableList m_randTileList;
        bool m_randTileListHasFocus = false;
        Tileset m_prevTileset;

        public override void OnEnable()
        {
            base.OnEnable();
            m_brush = (RandomBrush)target;
            if (m_brush.Tileset != null)
            {
                m_brush.Tileset.OnTileSelected += OnTileSelected;
            }

            m_randTileList = new ReorderableList(serializedObject, serializedObject.FindProperty("RandomTileList"), true, true, true, true);
            m_randTileList.drawHeaderCallback += (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Random Tiles", EditorStyles.boldLabel);
            };
            m_randTileList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect rTile = rect; rTile.width = rTile.height = m_brush.Tileset.VisualTileSize.y;
                uint tileData = m_brush.RandomTileList[index].tileData;
                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                if (tileId != Tileset.k_TileId_Empty)
                {
                    GUI.Box(new Rect(rTile.position - Vector2.one, rTile.size + 2 * Vector2.one), "");
                    TilesetEditor.DoGUIDrawTileFromTileData(rTile, tileData, m_brush.Tileset);
                }

                Rect rTileId = rect;
                rTileId.x += rTile.width + 10; rTileId.width -= rTile.width + 20;
                rTileId.height = rect.height / 2;
                GUI.Label(rTileId, "Id(" + tileId + ")");

                SerializedProperty randomTileDataProperty = m_randTileList.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty probabilityFactorProperty = randomTileDataProperty.FindPropertyRelative("probabilityFactor");
                Rect rProbabilityField = new Rect(rect.x + rTile.width + 10f, rect.y + EditorGUIUtility.singleLineHeight * 2.5f, rect.width - rTile.width - 10f, EditorGUIUtility.singleLineHeight);
                Rect rProbabilityLabel = new Rect(rProbabilityField.x, rProbabilityField.y - EditorGUIUtility.singleLineHeight, rProbabilityField.width, rProbabilityField.height);
                float sumProbabilityFactor = m_brush.GetSumProbabilityFactor();
                float probability = sumProbabilityFactor >= 0 ? probabilityFactorProperty.floatValue * 100f / sumProbabilityFactor : 100f;
                EditorGUI.PrefixLabel(rProbabilityLabel, new GUIContent("Probability (" + Mathf.RoundToInt(probability) + "%)"));
                EditorGUI.PropertyField(rProbabilityField, probabilityFactorProperty, GUIContent.none);
                if (probabilityFactorProperty.floatValue == 0f)
                {
                    serializedObject.ApplyModifiedProperties();
                    sumProbabilityFactor = m_brush.GetSumProbabilityFactor();
                    if (sumProbabilityFactor <= 0f)
                    {
                        probabilityFactorProperty.floatValue = 0.01f;
                    }
                }

                if (GUI.Button(new Rect(rect.x + rect.width - 50f, rect.y, 50f, EditorGUIUtility.singleLineHeight), "Clear"))
                {
                    m_brush.RandomTileList[index].tileData = Tileset.k_TileData_Empty;
                }
            };
            m_randTileList.onSelectCallback += (ReorderableList list) =>
            {
                TileSelectionWindow.Show(m_brush.Tileset);
                TileSelectionWindow.Instance.Ping();
            };
            m_randTileList.onAddCallback += (ReorderableList list) =>
            {
                if (list.index >= 0)
                    list.serializedProperty.InsertArrayElementAtIndex(list.index);
                else
                    list.serializedProperty.InsertArrayElementAtIndex(0);
                list.index = Mathf.Max(0, list.index + 1);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                m_brush.RandomTileList[list.index].probabilityFactor = 1f;
                if(m_brush.Tileset.SelectedTile != null)
                {
                    m_randTileList.GrabKeyboardFocus();
                    OnTileSelected(m_brush.Tileset, -1, m_brush.Tileset.SelectedTileId);
                }
            };
        }

        void OnDisable()
        {
            if (m_brush.Tileset != null)
            {
                m_brush.Tileset.OnTileSelected -= OnTileSelected;
            }
        }

        private void OnTileSelected(Tileset source, int prevTileId, int newTileId)
        {
            if (m_randTileListHasFocus && m_randTileList.index >= 0 && m_randTileList.index < m_brush.RandomTileList.Count)
            {
                if (m_brush.RandomTileList[m_randTileList.index].tileData == Tileset.k_TileData_Empty)
                {
                    m_brush.RandomTileList[m_randTileList.index].tileData = 0u;// reset flags and everything
                }
                m_brush.RandomTileList[m_randTileList.index].tileData &= ~Tileset.k_TileDataMask_TileId;
                m_brush.RandomTileList[m_randTileList.index].tileData |= (uint)(newTileId & Tileset.k_TileDataMask_TileId);
            }
            EditorUtility.SetDirty(target);
        }

        enum eRandomFlags
        {
            Rot90,
            FlipVertical,
            FlipHorizontal,
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_prevTileset != m_brush.Tileset)
            {
                OnDisable();
                OnEnable();
            }
            m_prevTileset = m_brush.Tileset;

            base.OnInspectorGUI();
            if (!m_brush.Tileset) return;

            Vector2 visualTileSize = m_brush.Tileset.VisualTileSize;

            EditorGUILayout.Space();

            GUILayoutUtility.GetRect(1f, 1f, GUILayout.Width(visualTileSize.x), GUILayout.Height(visualTileSize.y));
            Rect rSelectedTile = GUILayoutUtility.GetLastRect();
            uint tileData = m_brush.GetAnimTileData();
            if (tileData != Tileset.k_TileData_Empty)
            {
                rSelectedTile.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rSelectedTile.center.y);
                GUI.Box(new Rect(rSelectedTile.position - Vector2.one, rSelectedTile.size + 2 * Vector2.one), "");
                TilesetEditor.DoGUIDrawTileFromTileData(rSelectedTile, tileData, m_brush.Tileset);
            }

            EditorGUILayout.Space();

            SerializedProperty randomFlagMaskProperty = serializedObject.FindProperty("RandomizeFlagMask");
            System.Enum enumNew = EditorGUILayout.EnumMaskField(new GUIContent("Random Flags", "Applies random flags when painting tiles"), (eRandomFlags)(randomFlagMaskProperty.longValue >> 29));
            randomFlagMaskProperty.longValue = ((long)System.Convert.ChangeType(enumNew, typeof(long)) & 0x7) << 29;

            uint brushTileData = m_randTileList.index >= 0 ? m_brush.RandomTileList[m_randTileList.index].tileData : Tileset.k_TileData_Empty;
            brushTileData = BrushTileGridControl.DoTileDataPropertiesLayout(brushTileData, m_brush.Tileset, false);
            if (m_randTileList.index >= 0)
            {
                m_brush.RandomTileList[m_randTileList.index].tileData = brushTileData;
            }
            EditorGUILayout.Space();

            // Draw List
            m_randTileList.elementHeight = visualTileSize.y + 35f;
            m_randTileList.DoLayoutList();
            if (Event.current.type == EventType.Repaint)
            {
                m_randTileListHasFocus = m_randTileList.HasKeyboardControl();
            }

            TileSelection tileSelection = ((TilesetBrush)target).Tileset.TileSelection;
            if (tileSelection != null)
            {
                if (GUILayout.Button("Add tiles from tile selection"))
                {
                    m_brush.RandomTileList.AddRange(tileSelection.selectionData.Select(x => new RandomBrush.RandomTileData() { tileData = x, probabilityFactor = 1f }));
                }
            }

            EditorGUILayout.HelpBox("Select a tile from list and then select a tile from tile selection window.", MessageType.Info);
            EditorGUILayout.HelpBox("Add and Remove tiles with '+' and '-' buttons.", MessageType.Info);

            Repaint();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                m_brush.InvalidateSortedList();
                EditorUtility.SetDirty(target);
            }
        }
    }
}