using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CreativeSpore.SuperTilemapEditor
{

    /// <summary>
    /// Unity Event.current.clickCount is not always working as intended, so this class is used to check mouse double clicks safely
    /// </summary>
    internal class MouseDblClick
    {
        public bool IsDblClick { get { return m_isDblClick; } }
        float m_lastClickTime = 0f;
        bool m_isDblClick = false;

        public void Update()
        {
            Event e = Event.current;
            m_isDblClick = false;
            if (e.isMouse && e.type == EventType.MouseDown)
            {
                m_isDblClick = (Time.realtimeSinceStartup - m_lastClickTime) <= 0.2f;
                m_lastClickTime = Time.realtimeSinceStartup;
            }
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tilemap))]
    public class TilemapEditor : Editor
    {
        [MenuItem("GameObject/SuperTilemapEditor/Tilemap", false, 10)]
        static void CreateTilemap()
        {
            GameObject obj = new GameObject("New Tilemap");
            obj.AddComponent<Tilemap>();
        }

        private class Styles
        {
            static Styles s_instance;
            public static Styles Instance 
            {
                get 
                {
                    if (s_instance == null)
                        s_instance = new Styles();
                    return s_instance;
                }
            }

            public GUIStyle toolbarBoxStyle = new GUIStyle()
            {
                normal = { textColor = Color.white },
                richText = true,
            };
            public GUIStyle collVertexHandleStyle = new GUIStyle("U2D.dragDot")
            {
                normal = { textColor = Color.cyan },
                contentOffset = Vector2.one * 8f,
            };
            public GUIStyle richHelpBoxStyle = new GUIStyle("HelpBox")
            {
                richText = true
            };
        }

        public enum eEditMode
        {
            Paint,
            Renderer,
            Map,
            Collider,
        }
        public static eEditMode EditMode { get { return s_editMode; } }
        static eEditMode s_editMode = eEditMode.Paint;

        public enum eBrushMode
        {
            Paint,
            Erase,
            Fill
        }
        public enum eBrushPaintMode
        {
            Pencil,
            Line,
            Rect,
            FilledRect,
            Circle,
            FilledCircle,
        }
        public static eBrushMode s_brushMode = eBrushMode.Paint;
        //static eBrushPaintMode s_brushPaintMode = eBrushPaintMode.Pencil;
        public static bool s_displayHelpBox = false;

        [SerializeField]
        private bool m_toggleMapBoundsEdit = false;

        private Tilemap m_tilemap;
        private Tileset m_tilemapTileset; //NOTE: needed to Unregister tileset events. When tilemap is destroyed, m_tilemap is nulled during OnDisable
        //private Editor m_matEditor;
        private static TilesetControl s_tilesetCtrl;

        #region Tileset Events

        private void OnTileSelected(Tileset source, int prevTileId, int newTileId)
        {
            Tools.current = Tool.Rect;
            ResetBrushMode();
            BrushBehaviour brush = BrushBehaviour.GetOrCreateBrush(m_tilemap);
            brush.BrushTilemap.ClearMap();
            brush.BrushTilemap.SetTileData(0, 0, (uint)newTileId);
            brush.BrushTilemap.UpdateMesh();
            brush.Offset = Vector2.zero;
        }

        private void OnBrushSelected(Tileset source, int prevBrushId, int newBrushId)
        {
            Tools.current = Tool.Rect;
            ResetBrushMode();
            BrushBehaviour brush = BrushBehaviour.GetOrCreateBrush(m_tilemap);
            brush.BrushTilemap.ClearMap();
            brush.BrushTilemap.SetTileData(0, 0, (uint)(newBrushId << 16) | Tileset.k_TileDataMask_TileId);
            brush.BrushTilemap.UpdateMesh();
            brush.Offset = Vector2.zero;
        }

        private void OnTileSelectionChanged(Tileset source)
        {
            Tools.current = Tool.Rect;
            ResetBrushMode();
            BrushBehaviour brush = BrushBehaviour.GetOrCreateBrush(m_tilemap);
            brush.BrushTilemap.ClearMap();

            if (source.TileSelection != null)
            {
                for (int i = 0; i < source.TileSelection.selectionData.Count; ++i)
                {
                    int gx = i % source.TileSelection.rowLength;
                    int gy = i / source.TileSelection.rowLength;
                    brush.BrushTilemap.SetTileData(gx, gy, (uint)source.TileSelection.selectionData[i]);
                }
            }
            brush.BrushTilemap.UpdateMesh();
            brush.Offset = Vector2.zero;
        }
        #endregion

        #region Message Methods
        void OnEnable()
        {
            m_tilemap = (Tilemap)target;
            m_tilemapTileset = m_tilemap.Tileset;
            RegisterTilesetEvents(m_tilemapTileset);
            //fix missing material on prefabs tilemaps (when pressing play for example)
            if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m_tilemap.gameObject)))
            {
                m_tilemap.Refresh(true, false, false, false);
            }
            if (m_tilemap.ParentTilemapGroup)
            {
                m_tilemap.ParentTilemapGroup.SelectedTilemap = m_tilemap;
            }
        }

        void OnDisable()
        {
            UnregisterTilesetEvents(m_tilemapTileset);
            BrushBehaviour.SetVisible(false);
        }

        void RegisterTilesetEvents(Tileset tileset)
        {
            if (tileset != null)
            {
                UnregisterTilesetEvents(tileset);
                tileset.OnTileSelected += OnTileSelected;
                tileset.OnBrushSelected += OnBrushSelected;
                tileset.OnTileSelectionChanged += OnTileSelectionChanged;
            }
        }

        void UnregisterTilesetEvents(Tileset tileset)
        {
            if (tileset != null)
            {
                tileset.OnTileSelected -= OnTileSelected;
                tileset.OnBrushSelected -= OnBrushSelected;
                tileset.OnTileSelectionChanged -= OnTileSelectionChanged;
            }
        }

        void OnDestroy()
        {
            //        DestroyImmediate(m_matEditor);
        }        

        public override void OnInspectorGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed")
                    m_tilemap.Refresh();
            serializedObject.Update();

            Tileset prevTileset = m_tilemap.Tileset;
            
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_tilemap.Tileset = (Tileset)EditorGUILayout.ObjectField("Tileset", m_tilemap.Tileset, typeof(Tileset), false);
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (prevTileset != m_tilemap.Tileset)
            {
                UnregisterTilesetEvents(prevTileset);
                RegisterTilesetEvents(m_tilemap.Tileset);
                m_tilemapTileset = m_tilemap.Tileset;
            }

            if (m_tilemap.Tileset == null)
            {
                EditorGUILayout.HelpBox("There is no tileset selected", MessageType.Info);
                return;
            }

            string[] editModeNames = System.Enum.GetNames(typeof(eEditMode));
            s_editMode = (eEditMode)GUILayout.Toolbar((int)s_editMode, editModeNames);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight( s_editMode == eEditMode.Paint? Screen.height * 0.8f : 0f ));
            {
                if (s_editMode == eEditMode.Renderer)
                {
                    OnInspectorGUI_Renderer();                    
                }
                else if (s_editMode == eEditMode.Map)
                {
                    OnInspectorGUI_Map();                    
                }
                else if (s_editMode == eEditMode.Collider)
                {
                    OnInspectorGUI_Collider();                    
                }
                else if (s_editMode == eEditMode.Paint)
                {
                    if (m_tilemap.Tileset != null)
                    {
                        if (s_tilesetCtrl == null)
                        {
                            s_tilesetCtrl = new TilesetControl();
                        }
                        s_tilesetCtrl.Tileset = m_tilemap.Tileset;
                        s_tilesetCtrl.Display();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            Repaint();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        MouseDblClick m_dblClick = new MouseDblClick();
        bool m_brushVisible = false;
        List<Vector2> m_fillPreview = new List<Vector2>(500);
        public void OnSceneGUI()
        {
            m_dblClick.Update();
            if (m_tilemap == null || m_tilemap.Tileset == null)
            {
                return;
            }

            m_brushVisible = s_editMode == eEditMode.Paint;
            if (s_editMode == eEditMode.Paint)
            {
                DoPaintSceneGUI();

                if (GetBrushMode() == eBrushMode.Fill && Event.current.type == EventType.Repaint)
                {
                    Color fillPreviewColor = new Color(1f, 1f, 1f, 0.2f);
                    Color emptyColor = new Color(0f, 0f, 0f, 0f);
                    Rect rTile = new Rect(Vector2.zero, m_tilemap.CellSize);
                    foreach (Vector2 tilePos in m_fillPreview)
                    {
                        rTile.position = tilePos;
                        HandlesEx.DrawRectWithOutline(m_tilemap.transform, rTile, fillPreviewColor, emptyColor);
                    }
                }
            }
            else if (s_editMode == eEditMode.Map)
            {
                DoMapSceneGUI();
            }
            else if (s_editMode == eEditMode.Collider)
            {
                DoColliderSceneGUI();
            }
            BrushBehaviour.SetVisible(m_brushVisible);
        }

        #endregion

        #region OnInspectorGUI Methods

        private void OnInspectorGUI_Collider()
        {
            EditorGUI.BeginChangeCheck();
            {
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderType"));
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Collider Type:", EditorStyles.boldLabel);
                EditorGUI.indentLevel += 2;
                SerializedProperty colliderTypeProperty = serializedObject.FindProperty("ColliderType");
                string[] colliderTypeNames = new List<string>(System.Enum.GetNames(typeof(eColliderType)).Select(x => x.Replace('_', ' '))).ToArray();

                colliderTypeProperty.intValue = GUILayout.Toolbar(colliderTypeProperty.intValue, colliderTypeNames);
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                if (m_tilemap.ColliderType == eColliderType._3D)
                {
                    SerializedProperty colliderDepthProperty = serializedObject.FindProperty("ColliderDepth");
                    EditorGUILayout.PropertyField(colliderDepthProperty);
                    colliderDepthProperty.floatValue = Mathf.Clamp(colliderDepthProperty.floatValue, Vector3.kEpsilon, Mathf.Max(colliderDepthProperty.floatValue));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_physicMaterial"));
                }
                else if (m_tilemap.ColliderType == eColliderType._2D)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Collider2DType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowColliderNormals"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_physicMaterial2D"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_isTrigger"));

                if (m_tilemap.IsTrigger)
                {
                    EditorGUILayout.HelpBox("Activating IsTrigger could generate wrong collider lines if not used properly. Use Show Tilechunks in Map section to display the real collider lines.", MessageType.Warning);
                }
                else if (m_tilemap.ColliderType == eColliderType._2D && m_tilemap.Collider2DType == e2DColliderType.PolygonCollider2D)
                {
                    EditorGUILayout.HelpBox("Using Polygon colliders could generate wrong collider lines if not used properly. Use Show Tilechunks in Map section to display the real collider lines.", MessageType.Warning);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_tilemap.Refresh(false, true);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Update Collider Mesh"))
            {
                m_tilemap.Refresh(false, true);
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Global Parameters:", EditorStyles.boldLabel);
                EditorGUI.indentLevel += 2;
                EditorGlobalSettings.TilemapColliderColor = EditorGUILayout.ColorField("Collider Color", EditorGlobalSettings.TilemapColliderColor);
                EditorGUI.indentLevel -= 2;
            }
            EditorGUILayout.EndVertical();
        }

        private void OnInspectorGUI_Map()
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh Map", GUILayout.MaxWidth(125)))
            {
                m_tilemap.Refresh(true, true, true, true);
            }
            if (GUILayout.Button("Clear Map", GUILayout.MaxWidth(125)))
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to clear the map?\nThis action will remove all children objects under the tilemap", "Yes", "No"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(m_tilemap.gameObject, "Clear Map " + m_tilemap.name);
                    m_tilemap.IsUndoEnabled = true;
                    m_tilemap.ClearMap();
                    m_tilemap.IsUndoEnabled = false;
                }
            }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_cellSize"));
                if(GUILayout.Button("R", GUILayout.Width(20)))
                {
                    serializedObject.FindProperty("m_cellSize").vector2Value = m_tilemap.Tileset.TilePxSize / m_tilemap.Tileset.PixelsPerUnit;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowGrid"), new GUIContent("Show Grid", "Show the tilemap grid."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Size (" + m_tilemap.GridWidth + "," + m_tilemap.GridHeight + ")");

            //+++ Display Map Bounds
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Map Bounds (in tiles):", EditorStyles.boldLabel);
                m_toggleMapBoundsEdit = EditorUtils.DoToggleIconButton("Edit Map Bounds", m_toggleMapBoundsEdit, EditorGUIUtility.IconContent("EditCollider"));

                float savedLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                EditorGUI.indentLevel += 2;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_minGridX"), new GUIContent("Left"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_minGridY"), new GUIContent("Bottom"));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maxGridX"), new GUIContent("Right"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maxGridY"), new GUIContent("Top"));
                EditorGUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    m_tilemap.RecalculateMapBounds();
                }

                EditorGUI.indentLevel -= 2;
                EditorGUIUtility.labelWidth = savedLabelWidth;
            }
            EditorGUILayout.EndVertical();
            //---

            EditorGUILayout.Space();

            m_tilemap.AllowPaintingOutOfBounds = EditorGUILayout.ToggleLeft("Allow Painting Out of Bounds", m_tilemap.AllowPaintingOutOfBounds);

            EditorGUILayout.Space();

            if (GUILayout.Button("Shrink to Visible Area", GUILayout.MaxWidth(150)))
            {
                m_tilemap.ShrinkMapBoundsToVisibleArea();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_autoShrink"));

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                bool isChunksVisible = IsTilemapChunksVisible();
                isChunksVisible = EditorGUILayout.Toggle(new GUIContent("Show Tile Chunks", "Show tilemap chunk objects for debugging or other purposes. Hiding will be refreshed after collapsing the tilemap."), isChunksVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    SetTilemapChunkHideFlag(HideFlags.HideInHierarchy, !isChunksVisible);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_enableUndoWhilePainting"), new GUIContent("Enable Undo", "Disable Undo when painting on big maps to improve performance."));
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Global Parameters:", EditorStyles.boldLabel);
                EditorGUI.indentLevel += 2;
                EditorGlobalSettings.TilemapGridColor = EditorGUILayout.ColorField("Grid Color", EditorGlobalSettings.TilemapGridColor);
                EditorGUI.indentLevel -= 2;
            }
            EditorGUILayout.EndVertical();
        }

        private void OnInspectorGUI_Renderer()
        {
            EditorGUI.BeginChangeCheck();
            Material prevMaterial = m_tilemap.Material;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_material"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_tilemap.Refresh();
                if (m_tilemap.Material != prevMaterial && !AssetDatabase.Contains(prevMaterial))
                {
                    //avoid memory leak
                    DestroyImmediate(prevMaterial);
                }
            }
            // Draw Material Control
            /*
            if (m_matEditor == null || EditorGUI.EndChangeCheck())
            {
                if (m_matEditor != null) DestroyImmediate(m_matEditor);
                m_matEditor = MaterialEditor.CreateEditor(m_tilemap.Material);
                m_matEditor.hideFlags = HideFlags.DontSave;
            }
            float savedLabelWidth = EditorGUIUtility.labelWidth;
            m_matEditor.DrawHeader();
            m_matEditor.OnInspectorGUI();
            EditorGUIUtility.labelWidth = savedLabelWidth;
            */
            //---

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_tintColor"));

            //Pixel Snap
            if (m_tilemap.Material.HasProperty("PixelSnap"))
            {
                EditorGUI.BeginChangeCheck();
                bool isPixelSnapOn = EditorGUILayout.Toggle("Pixel Snap", m_tilemap.PixelSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    m_tilemap.PixelSnap = isPixelSnapOn;
                }
            }

            // Sorting Layer and Order in layer            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_sortingLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_orderInLayer"));
            serializedObject.FindProperty("m_orderInLayer").intValue = (serializedObject.FindProperty("m_orderInLayer").intValue << 16) >> 16; // convert from int32 to int16 keeping sign
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_tilemap.RefreshChunksSortingAttributes();
                SceneView.RepaintAll();
            }
            //---            

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InnerPadding"), new GUIContent("Inner Padding", "The size, in pixels, the tile UV will be stretched. Use this to fix pixel precision artifacts when tiles have no padding border in the atlas."));

            m_tilemap.IsVisible = EditorGUILayout.Toggle("Visible", m_tilemap.IsVisible);
        }

        #endregion        

        #region OnSceneGUI Methods

        Vector2 m_startDragging;
        Vector2 m_endDragging;
        bool m_isDragging = false;
        Vector2 m_localPaintPos;
        int m_mouseGridX;
        int m_mouseGridY;
        uint m_floodFillRestoredTileData = Tileset.k_TileData_Empty;
        private void DoPaintSceneGUI()
        {
            Event e = Event.current;            

            if (DoToolBar()
                || DragAndDrop.objectReferences.Length > 0 // hide brush when user is dragging a prefab into the scene
                || EditorWindow.mouseOverWindow != SceneView.currentDrawingSceneView // hide brush when it's not over the scene view
                || (Tools.current != Tool.Rect && Tools.current != Tool.None)
                )
            {
                m_brushVisible = false;
                SceneView.RepaintAll();
                return;
            }

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            EventType currentEventType = Event.current.GetTypeForControl(controlID);
            bool skip = false;
            int saveControl = GUIUtility.hotControl;

            try
            {
                if (currentEventType == EventType.Layout) { skip = true; }
                else if (currentEventType == EventType.ScrollWheel) { skip = true; }

                if (m_tilemap.Tileset == null)
                {
                    return;
                }

                if (!skip)
                {
                    if (e.type == EventType.KeyDown)
                    {
                        if (e.keyCode == ShortcutKeys.k_FlipH)
                        {
                            BrushBehaviour.GetOrCreateBrush(m_tilemap).FlipH(!e.shift);
                            e.Use(); // Use key event
                        }
                        else if (e.keyCode == ShortcutKeys.k_FlipV)
                        {
                            BrushBehaviour.GetOrCreateBrush(m_tilemap).FlipV(!e.shift);
                            e.Use(); // Use key event
                        }
                        else if (e.keyCode == ShortcutKeys.k_Rot90)
                        {
                            BrushBehaviour.GetOrCreateBrush(m_tilemap).Rot90(!e.shift);
                            e.Use(); // Use key event
                        }
                        else if (e.keyCode == ShortcutKeys.k_Rot90Back)
                        {
                            BrushBehaviour.GetOrCreateBrush(m_tilemap).Rot90Back(!e.shift);
                            e.Use(); // Use key event
                        }
                    }

                    EditorGUIUtility.AddCursorRect(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), MouseCursor.Arrow);
                    GUIUtility.hotControl = controlID;
                    {
                        Plane chunkPlane = new Plane(m_tilemap.transform.forward, m_tilemap.transform.position);
                        Vector2 mousePos = Event.current.mousePosition; mousePos.y = Screen.height - mousePos.y;
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        float dist;
                        if (chunkPlane.Raycast(ray, out dist))
                        {
                            Rect rTile = new Rect(0, 0, m_tilemap.CellSize.x, m_tilemap.CellSize.y);
                            rTile.position = m_tilemap.transform.InverseTransformPoint(ray.GetPoint(dist));

                            Vector2 tilePos = rTile.position;
                            if (tilePos.x < 0) tilePos.x -= m_tilemap.CellSize.x;
                            if (tilePos.y < 0) tilePos.y -= m_tilemap.CellSize.y;
                            tilePos.x -= tilePos.x % m_tilemap.CellSize.x;
                            tilePos.y -= tilePos.y % m_tilemap.CellSize.y;
                            rTile.position = tilePos;


                            Vector2 startPos = new Vector2(Mathf.Min(m_startDragging.x, m_endDragging.x), Mathf.Min(m_startDragging.y, m_endDragging.y));
                            Vector2 endPos = new Vector2(Mathf.Max(m_startDragging.x, m_endDragging.x), Mathf.Max(m_startDragging.y, m_endDragging.y));
                            Vector2 selectionSnappedPos = BrushUtil.GetSnappedPosition(startPos, m_tilemap.CellSize);
                            Vector2 selectionSize = BrushUtil.GetSnappedPosition(endPos, m_tilemap.CellSize) - selectionSnappedPos + m_tilemap.CellSize;

                            BrushBehaviour brush = BrushBehaviour.GetOrCreateBrush(m_tilemap);
                            // Update brush transform
                            m_localPaintPos = (Vector2)m_tilemap.transform.InverseTransformPoint(ray.GetPoint(dist));
                            Vector2 brushSnappedPos = BrushUtil.GetSnappedPosition(brush.Offset + m_localPaintPos, m_tilemap.CellSize);
                            brush.transform.rotation = m_tilemap.transform.rotation;
                            brush.transform.localScale = m_tilemap.transform.lossyScale;
                            brush.transform.position = m_tilemap.transform.TransformPoint(new Vector3(brushSnappedPos.x, brushSnappedPos.y, -0.01f));
                            //---

                            int prevMouseGridX = m_mouseGridX;
                            int prevMouseGridY = m_mouseGridY;
                            if (e.isMouse)
                            {
                                m_mouseGridX = BrushUtil.GetGridX(m_localPaintPos, m_tilemap.CellSize);
                                m_mouseGridY = BrushUtil.GetGridY(m_localPaintPos, m_tilemap.CellSize);
                            }
                            bool isMouseGridChanged = prevMouseGridX != m_mouseGridX || prevMouseGridY != m_mouseGridY;
                            //Update Fill Preview
                            if(GetBrushMode() == eBrushMode.Fill && isMouseGridChanged)
                            {
                                m_fillPreview.Clear();
                                TilemapDrawingUtils.FloodFillPreview(m_tilemap, brush.Offset + m_localPaintPos, brush.BrushTilemap.GetTileData(0, 0), m_fillPreview);
                            }

                            if (
                                (EditorWindow.focusedWindow == EditorWindow.mouseOverWindow) && // fix painting tiles when closing another window popup over the SceneView like GameObject Selection window
                                (e.type == EventType.MouseDown || e.type == EventType.MouseDrag && isMouseGridChanged)
                            )
                            {
                                if (e.button == 0)
                                {
                                    if (m_dblClick.IsDblClick && brush.BrushTilemap.GridWidth == 1 && brush.BrushTilemap.GridHeight == 1)
                                    {
                                        // Restore previous tiledata modified by Paint, because before the double click, a single click is done before
                                        m_tilemap.SetTileData(brush.Offset + m_localPaintPos, m_floodFillRestoredTileData);
                                        brush.FloodFill(m_tilemap, brush.Offset + m_localPaintPos, brush.BrushTilemap.GetTileData(0, 0));
                                    }
                                    // Do a brush paint action
                                    else
                                    {
                                        switch (GetBrushMode())
                                        { 
                                            case eBrushMode.Paint:
                                                m_floodFillRestoredTileData = m_tilemap.GetTileData(m_mouseGridX, m_mouseGridY);
                                                brush.Paint(m_tilemap, brush.Offset + m_localPaintPos);
                                                break;
                                            case eBrushMode.Erase:
                                                brush.Erase(m_tilemap, brush.Offset + m_localPaintPos);
                                                break;
                                            case eBrushMode.Fill:
                                                brush.FloodFill(m_tilemap, brush.Offset + m_localPaintPos, brush.BrushTilemap.GetTileData(0, 0));
                                                break;
                                        }
                                    }
                                }
                                else if (e.button == 1)
                                {
                                    if (e.type == EventType.MouseDown)
                                    {
                                        m_isDragging = true;
                                        brush.BrushTilemap.ClearMap();
                                        m_startDragging = m_endDragging = m_localPaintPos;
                                    }
                                    else
                                    {
                                        m_endDragging = m_localPaintPos;
                                    }
                                }
                            }
                            else if (e.type == EventType.MouseUp)
                            {
                                if (e.button == 1) // right mouse button
                                {
                                    m_isDragging = false;
                                    ResetBrushMode();
                                    // Copy one tile
                                    if (selectionSize.x <= m_tilemap.CellSize.x && selectionSize.y <= m_tilemap.CellSize.y)
                                    {
                                        uint tileData = m_tilemap.GetTileData(m_localPaintPos);
                                        //Select the first tile not null if any and select the tilemap
                                        if (e.control && m_tilemap.ParentTilemapGroup)
                                        {
                                            for (int i = m_tilemap.ParentTilemapGroup.Tilemaps.Count - 1; i >= 0; --i)
                                            {
                                                Tilemap tilemap = m_tilemap.ParentTilemapGroup.Tilemaps[i];
                                                tileData = tilemap.GetTileData(m_localPaintPos);
                                                if(tileData != Tileset.k_TileData_Empty)
                                                {
                                                    tilemap.ParentTilemapGroup.SelectedTilemap = tilemap;
                                                    if (Selection.activeGameObject == m_tilemap.gameObject)
                                                        Selection.activeGameObject = tilemap.gameObject;
                                                    break;
                                                }
                                            }
                                        }

                                        if (tileData == Tileset.k_TileData_Empty)
                                        {                                            
                                            m_tilemap.Tileset.SelectedTileId = Tileset.k_TileId_Empty;
                                            brush.BrushTilemap.SetTileData(0, 0, Tileset.k_TileData_Empty);
                                        }
                                        else
                                        {
                                            int brushId = Tileset.GetBrushIdFromTileData(tileData);
                                            int tileId = Tileset.GetTileIdFromTileData(tileData);

                                            // Select the copied tile in the tileset, alternating between the brush and the tile drawn by the brush
                                            if (brushId > 0 /*&& brushId != m_tilemap.Tileset.SelectedBrushId */) //NOTE: alternating between brush and tile disabled because its causing more issues than help
                                            {
                                                m_tilemap.Tileset.SelectedBrushId = brushId;
                                            }
                                            else
                                            {
                                                m_tilemap.Tileset.SelectedTileId = tileId;
                                                brush.BrushTilemap.SetTileData(0, 0, tileData & ~Tileset.k_TileDataMask_BrushId); // keep tile flags
                                            }
                                        }

                                        // Cut tile if key shift is pressed
                                        if (e.shift)
                                        {
                                            int startGridX = BrushUtil.GetGridX(startPos, m_tilemap.CellSize);
                                            int startGridY = BrushUtil.GetGridY(startPos, m_tilemap.CellSize);
                                            brush.CutRect(m_tilemap, startGridX, startGridY, startGridX, startGridY);
                                        }

                                        brush.BrushTilemap.UpdateMesh();
                                        brush.Offset = Vector2.zero;
                                    }
                                    // copy a rect of tiles
                                    else
                                    {
                                        int startGridX = BrushUtil.GetGridX(startPos, m_tilemap.CellSize);
                                        int startGridY = BrushUtil.GetGridY(startPos, m_tilemap.CellSize);
                                        int endGridX = BrushUtil.GetGridX(endPos, m_tilemap.CellSize);
                                        int endGridY = BrushUtil.GetGridY(endPos, m_tilemap.CellSize);

                                        // Cut tile if key shift is pressed
                                        if (e.shift)
                                        {
                                            brush.CutRect(m_tilemap, startGridX, startGridY, endGridX, endGridY);
                                        }
                                        else
                                        {
                                            brush.CopyRect(m_tilemap, startGridX, startGridY, endGridX, endGridY);
                                        }
                                        brush.Offset.x = m_endDragging.x > m_startDragging.x ? -(endGridX - startGridX) * m_tilemap.CellSize.x : 0f;
                                        brush.Offset.y = m_endDragging.y > m_startDragging.y ? -(endGridY - startGridY) * m_tilemap.CellSize.y : 0f;

                                    }
                                }
                            }

                            if (m_isDragging)
                            {
                                Rect rGizmo = new Rect(selectionSnappedPos, selectionSize);
                                HandlesEx.DrawRectWithOutline(m_tilemap.transform, rGizmo, new Color(), Color.white);
                            }
                            else // Draw brush border
                            {
                                Rect rBound = new Rect(brush.BrushTilemap.MapBounds.min, brush.BrushTilemap.MapBounds.size);
                                Color fillColor;
                                switch (GetBrushMode())
                                {
                                    case eBrushMode.Paint:
                                        fillColor = new Color(0, 0, 0, 0);
                                        break;
                                    case eBrushMode.Erase:
                                        fillColor = new Color(1f, 0f, 0f, 0.1f);
                                        break;
                                    case eBrushMode.Fill:
                                        fillColor = new Color(1f, 1f, 0f, 0.2f);
                                        break;
                                    default:
                                        fillColor = new Color(0, 0, 0, 0);
                                        break;
                                }
                                HandlesEx.DrawRectWithOutline(brush.transform, rBound, fillColor, new Color(1, 1, 1, 0.2f));
                            }
                        }
                    }

                    if (currentEventType == EventType.MouseDrag && Event.current.button < 2) // 2 is for central mouse button
                    {
                        // avoid dragging the map
                        Event.current.Use();
                    }
                }
            }
            // Avoid loosing the hotControl because of a triggered exception
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }

            SceneView.RepaintAll();
            GUIUtility.hotControl = saveControl;
        }

        private void DoMapSceneGUI()
        {
            if (m_toggleMapBoundsEdit)
            {
                EditorGUI.BeginChangeCheck();
                Handles.color = Color.green;
                Vector3 vMinX = Handles.FreeMoveHandle(m_tilemap.transform.TransformPoint(new Vector2(m_tilemap.MinGridX * m_tilemap.CellSize.x, m_tilemap.MapBounds.center.y)), Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Vector3.zero, Handles.CubeCap);
                Vector3 vMaxX = Handles.FreeMoveHandle(m_tilemap.transform.TransformPoint(new Vector2((m_tilemap.MaxGridX + 1f) * m_tilemap.CellSize.x, m_tilemap.MapBounds.center.y)), Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Vector3.zero, Handles.CubeCap);
                Vector3 vMinY = Handles.FreeMoveHandle(m_tilemap.transform.TransformPoint(new Vector2(m_tilemap.MapBounds.center.x, m_tilemap.MinGridY * m_tilemap.CellSize.y)), Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Vector3.zero, Handles.CubeCap);
                Vector3 vMaxY = Handles.FreeMoveHandle(m_tilemap.transform.TransformPoint(new Vector2(m_tilemap.MapBounds.center.x, (m_tilemap.MaxGridY + 1f) * m_tilemap.CellSize.y)), Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Vector3.zero, Handles.CubeCap);
                Handles.color = Color.white;
                m_tilemap.MinGridX = Mathf.RoundToInt(m_tilemap.transform.InverseTransformPoint(vMinX).x / m_tilemap.CellSize.x);
                m_tilemap.MaxGridX = Mathf.RoundToInt(m_tilemap.transform.InverseTransformPoint(vMaxX).x / m_tilemap.CellSize.x - 1f);
                m_tilemap.MinGridY = Mathf.RoundToInt(m_tilemap.transform.InverseTransformPoint(vMinY).y / m_tilemap.CellSize.y);
                m_tilemap.MaxGridY = Mathf.RoundToInt(m_tilemap.transform.InverseTransformPoint(vMaxY).y / m_tilemap.CellSize.y - 1f);
                if(EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                    m_tilemap.RecalculateMapBounds();
                }
            }
        }

        private static readonly Vector2[] s_fullCollTileVertices = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        private Vector2 m_colMouseLocPos = Vector2.zero;
        private int m_lastControl = -1;
        private void DoColliderSceneGUI()
        {
            Event e = Event.current;
            Vector2 vLocMousePos;
            //if (!m_editColliders) return;            
            if (EditorWindow.mouseOverWindow == SceneView.currentDrawingSceneView)
            {
                if (EditorGUIUtility.hotControl == 0 && // don't select another tile while dragging a handle
                    GetMousePosOverTilemap(m_tilemap, out vLocMousePos))
                {
                    //NOTE: Threshold should be a value between [0, 1). This is a percent of the size of the tile area to be taken into account to consider the mouse over the tile. 
                    /// Ex: for a value of 0.5, the mouse should be inside the tile at a distance of half of the size
                    float threshold = 0.7f;
                    Vector2 vTileCenter = BrushUtil.GetSnappedPosition(vLocMousePos, m_tilemap.CellSize) + m_tilemap.CellSize / 2f;
                    float distX = Mathf.Abs(vLocMousePos.x - vTileCenter.x);
                    float distY = Mathf.Abs(vLocMousePos.y - vTileCenter.y);
                    if (distX <= m_tilemap.CellSize.x * threshold / 2f && distY <= m_tilemap.CellSize.y * threshold / 2f)
                        m_colMouseLocPos = vLocMousePos;
                }
                // Draw selected tile handles     
                uint tileData = m_tilemap.GetTileData(m_colMouseLocPos);
                Tile tile = m_tilemap.GetTile(m_colMouseLocPos);
                Vector2 tileLocPos = BrushUtil.GetSnappedPosition(m_colMouseLocPos, m_tilemap.CellSize);
                Vector2 tileCenter = tileLocPos + m_tilemap.CellSize / 2f;
                if (tile != null)
                {
                    // Prevent from selecting another gameobject when clicking the scene
                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    HandleUtility.AddDefaultControl(controlID);

                    HandlesEx.DrawRectWithOutline(m_tilemap.transform, new Rect(BrushUtil.GetSnappedPosition(m_colMouseLocPos, m_tilemap.CellSize), m_tilemap.CellSize), new Color(0f, 0f, 0f, 0.1f), new Color(0f, 0f, 0f, 0f));
                    bool updateColliders = false;
                    TileColliderData tileColliderData = tile.collData.Clone();
                    tileColliderData.ApplyFlippingFlags(tileData);
                    List<Vector2> handlesPos = new List<Vector2>(tile.collData.vertices != null && tile.collData.vertices.Length > 0 ? tileColliderData.vertices : s_fullCollTileVertices);
                    for (int i = 0; i < handlesPos.Count; ++i)
                        handlesPos[i] = m_tilemap.transform.TransformPoint(tileLocPos + Vector2.Scale(handlesPos[i], m_tilemap.CellSize));
                    HandlesEx.DrawDottedPolyLine(handlesPos.Select(x => (Vector3)x).ToArray(), 10, Color.white * 0.6f);
                    if (e.isMouse && e.button == 1)
                        m_tilemap.Tileset.SelectedTileId = Tileset.GetTileIdFromTileData(tileData);
                    if (e.alt)
                    {                        
                        bool hasColliders = tile.collData.type != eTileCollider.None;
                        Handles.color = hasColliders ? Color.white : Color.black;
                        Vector2 centerPos = m_tilemap.transform.TransformPoint(tileCenter);
                        float size = 0.1f * m_tilemap.CellSize.x;
                        Handles.DrawSolidDisc(centerPos, -m_tilemap.transform.forward, size);
                        if (Handles.Button(centerPos, Quaternion.identity, size, m_tilemap.CellSize.x, Handles.CircleCap))
                        {
                            Undo.RecordObject(m_tilemap.Tileset, "Update Tile Collider");
                            tile.collData.type = tile.collData.type == eTileCollider.None ? eTileCollider.Polygon : eTileCollider.None;
                            updateColliders = true;
                        }
                        Handles.color = Color.white;
                    }

                    int closestVertIdx = -1;
                    if (e.shift && m_lastControl == 0)
                    {
                        List<Vector3> polyLine = handlesPos.Select(x => (Vector3)x).ToList();
                        polyLine.Add(polyLine[0]); // close poly
                        Vector3 newVertexHandlePos = ClosestPointToPolyLine(polyLine.ToArray(), out closestVertIdx);
                        if (HandleUtility.DistanceToCircle(newVertexHandlePos, 0f) < 10f)
                        {
                            handlesPos.Insert(closestVertIdx + 1, newVertexHandlePos);
                        }
                    }
                    int selectedIdx = -1;
                    for (int i = 0; i < handlesPos.Count; ++i)
                    {
                        int idx = i; // real index having into account flip and rotation flags
                        if ((tileData & Tileset.k_TileFlag_FlipH) != 0 ^ (tileData & Tileset.k_TileFlag_FlipV) != 0)
                            idx = handlesPos.Count - i - 1;
                        Vector2 handlePos = handlesPos[i];
                        Vector2 vLocVertex = (Vector2)m_tilemap.transform.InverseTransformPoint(handlePos) - tileLocPos;
                        Vector2 oldVertexValue = PointToSnappedVertex(vLocVertex, m_tilemap);
                        if (tile.collData.type != eTileCollider.None)
                        {
                            handlePos = Handles.FreeMoveHandle(handlePos, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Vector3.zero, Handles.CubeCap);
                            if (closestVertIdx >= 0 && i == closestVertIdx + 1)
                                HandlesEx.DrawDotOutline(handlePos, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Color.green, Color.green);
                            else if (e.control && HandleUtility.DistanceToCircle(handlePos, 0f) < 10f)
                            {
                                selectedIdx = idx;
                                HandlesEx.DrawDotOutline(handlePos, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Color.red, Color.red);
                            }
                            vLocVertex = (Vector2)m_tilemap.transform.InverseTransformPoint(handlePos) - tileLocPos;
                            Vector2 newVertexValue = PointToSnappedVertex(vLocVertex, m_tilemap);
                            updateColliders |= (oldVertexValue != newVertexValue) || m_lastControl == 0 && EditorGUIUtility.hotControl != 0 && HandleUtility.DistanceToCircle(handlePos, 0f) < 10f;
                            if (updateColliders)
                            {
                                Undo.RecordObject(m_tilemap.Tileset, "Update Tile Collider");
                                handlesPos[i] = handlePos;
                                tile.collData.type = eTileCollider.Polygon;
                                tile.collData.vertices = handlesPos.Select(x => PointToSnappedVertex((Vector2)m_tilemap.transform.InverseTransformPoint(x) - tileLocPos, m_tilemap)).ToArray();
                                tile.collData.RemoveFlippingFlags(tileData);
                            }
                        }
                        else
                        {
                            HandlesEx.DrawDotOutline(handlePos, 0.1f * HandleUtility.GetHandleSize(m_tilemap.transform.position), Color.gray, Color.gray);
                        }
                        Handles.Label(handlePos, idx.ToString());
                    }
                    if (e.control && HandleUtility.nearestControl == EditorGUIUtility.hotControl && tile.collData.type != eTileCollider.None)
                    {
                        EditorGUIUtility.hotControl = 0;
                        if (selectedIdx >= 0 && tile.collData.vertices.Length > 3)
                        {
                            updateColliders = true;
                            Undo.RecordObject(m_tilemap.Tileset, "Update Tile Collider");
                            ArrayUtility.RemoveAt(ref tile.collData.vertices, selectedIdx);
                        }
                    }
                    // optimize polygon collider if the vertices are they same as full collider
                    if (tile.collData.type == eTileCollider.Polygon && Enumerable.SequenceEqual(tile.collData.vertices, s_fullCollTileVertices))
                        tile.collData.type = eTileCollider.Full;
                    if (updateColliders)
                    {
                        m_tilemap.Refresh(false, true); // This is less optimized but updates all the tilemap
                        EditorUtility.SetDirty(m_tilemap.Tileset);
                    }
                    SceneView.RepaintAll();
                    Handles.color = Color.white;
                }
            }
            Rect rHelpInfoArea = new Rect(0, Screen.height - 100f, 350f, 50f);
            GUILayout.BeginArea(rHelpInfoArea);
            string helpInfo =
                "<b>"+
                "  - Click and drag over a vertex to move it" + "\n" +
                "  - Hold Shift + Click for adding a new vertex" + "\n" +
                "  - Hold " + ((Application.platform == RuntimePlatform.OSXEditor) ? "Command" : "Ctrl") + " + Click for removing a vertex. (should be more than 3)" + "\n" +
                "  - Hold " + ((Application.platform == RuntimePlatform.OSXEditor) ? "Option" : "Alt") + " + Click to add/remove tile colliders" + "\n" +
                "</b>";
            EditorGUI.TextArea(new Rect(Vector2.zero, rHelpInfoArea.size), helpInfo, Styles.Instance.richHelpBoxStyle);
            GUILayout.EndArea();
            m_lastControl = EditorGUIUtility.hotControl;
        }

        private static bool GetMousePosOverTilemap(Tilemap tilemap, out Vector2 vLocPos)
        {
            Plane chunkPlane = new Plane(tilemap.transform.forward, tilemap.transform.position);
            Vector2 mousePos = Event.current.mousePosition; mousePos.y = Screen.height - mousePos.y;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float dist;
            if (chunkPlane.Raycast(ray, out dist))
            {
                vLocPos = tilemap.transform.InverseTransformPoint(ray.GetPoint(dist));
                return true;
            }
            vLocPos = Vector2.zero;
            return false;
        }

        private static Vector2 PointToSnappedVertex(Vector2 point, Tilemap tilemap)
        {
            Vector2 vertex = new Vector2(point.x /= tilemap.CellSize.x, point.y /= tilemap.CellSize.y);
            return TileColliderData.SnapVertex(vertex, tilemap.Tileset);
        }

        private static Vector3 ClosestPointToPolyLine(Vector3[] vertices, out int closestSegmentIdx)
        {
            float minDist = float.MaxValue;
            closestSegmentIdx = 0;
            for (int i = 0; i < vertices.Length - 1; ++i)
            {
                float dist = HandleUtility.DistanceToLine(vertices[i], vertices[i + 1]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestSegmentIdx = i;
                }
            }
            return HandleUtility.ClosestPointToPolyLine(vertices);
        }        

        #endregion        

        #region Toolbar Methods

        static Color s_toolbarBoxBgColor = new Color(0f, 0f, .4f, 0.4f);
        static Color s_toolbarBoxOutlineColor = new Color(.25f, .25f, 1f, 0.70f);
        bool DoToolBar()
        {
            bool isMouseInsideToolbar = false;
            GUIContent brushCoords = new GUIContent("<b> Brush Pos: (" + m_mouseGridX + "," + m_mouseGridY + ")</b>");
            GUIContent selectedTileOrBrushId = null;
            if (m_tilemap.Tileset.SelectedTileId != Tileset.k_TileId_Empty)
                selectedTileOrBrushId = new GUIContent("<b> Selected Tile Id: " + m_tilemap.Tileset.SelectedTileId.ToString() + "</b>");
            else if (m_tilemap.Tileset.SelectedBrushId != Tileset.k_BrushId_Default)
                selectedTileOrBrushId = new GUIContent("<b> Selected Brush Id: " + m_tilemap.Tileset.SelectedBrushId.ToString() + "</b>");
            else
                selectedTileOrBrushId = new GUIContent("<b> Empty tile selected</b>");

            Rect rTools = new Rect(4f, 4f, Mathf.Max(Mathf.Max(Styles.Instance.toolbarBoxStyle.CalcSize(brushCoords).x, Styles.Instance.toolbarBoxStyle.CalcSize(selectedTileOrBrushId).x) + 4f, 180f), 54f);
            
            Handles.BeginGUI();            
            GUILayout.BeginArea(rTools);
            HandlesEx.DrawRectWithOutline(new Rect(Vector2.zero, rTools.size), s_toolbarBoxBgColor, s_toolbarBoxOutlineColor);

            GUILayout.Space(2f);
            GUILayout.Label(brushCoords, Styles.Instance.toolbarBoxStyle);
            if (selectedTileOrBrushId != null)
            {
                GUILayout.Label(selectedTileOrBrushId, Styles.Instance.toolbarBoxStyle);
            }
            GUILayout.Label("<b> F1 - Display Help</b>", Styles.Instance.toolbarBoxStyle);
            GUILayout.Label("<b> F5 - Refresh Tilemap</b>", Styles.Instance.toolbarBoxStyle);
            GUILayout.EndArea();

            // Display ToolBar           
            Vector2 toolbarPos = new Vector2(rTools.xMax + 4f, rTools.y);
            Vector2 toolbarButtonSize = new Vector2(32f, 32f);
            STEditorToolbars.Instance.brushToolbar.SetHighlight((int)ToolIcons.eToolIcon.Erase, GetBrushMode() == eBrushMode.Erase);
            STEditorToolbars.Instance.brushToolbar.DoGUI(toolbarPos, toolbarButtonSize, s_toolbarBoxBgColor, s_toolbarBoxOutlineColor);

            if (Tools.current != Tool.None && Tools.current != Tool.Rect)
            {
                Rect rWarningArea = new Rect(toolbarPos.x, toolbarPos.y + toolbarButtonSize.y, 370f, 22f);
                GUILayout.BeginArea(rWarningArea);
                EditorGUI.HelpBox(new Rect(Vector2.zero, rWarningArea.size), "Select the Rect Tool (T) or press any toolbar button to start painting", MessageType.Warning);
                GUILayout.EndArea();
            }
            //---            

            Handles.EndGUI();

            if(s_displayHelpBox)
            {
                DisplayHelpBox();
            }
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.F1)
                    s_displayHelpBox = !s_displayHelpBox;
                else if (Event.current.keyCode == KeyCode.F5)
                    m_tilemap.Refresh(true, true, true, true);
            }

            return isMouseInsideToolbar;
        }

        void DisplayHelpBox()
        {
            string sCtrl = (Application.platform == RuntimePlatform.OSXEditor) ? "Command" : "Ctrl";
            string sHelp =
                "\n" +
                " - <b>Drag:</b>\t Middle mouse button\n" +
                " - <b>Paint:</b>\t Left mouse button\n" +
                " - <b>Erase:</b>\t Shift + Left mouse button\n" +
                " - <b>Fill:</b>\t Double Click\n\n" +
                " - <b>Copy</b> tiles by dragging and holding right mouse button\n\n" +
                " - <b>Cut</b> copy while holding Shift key\n\n" +
                " - <b>Select</b> a tile or brush by right clicking over the tile.\n    If it's a brush, the brush will be selected first and second time the\n    tile painted by the brush will be selected cycling between them\n" +
                " - <b>Select + " + sCtrl + "</b> to select the first tile not empty found\n in a tilemap group from bottom to top in the tilemap list.\n\n" +
                " - <b>Rotating and flipping:</b>\n" +
                "   * <b>Rotate</b> ±90º by using <b>comma ','</b> and <b>period '.'</b>\n" +
                "   * <b>Vertical Flip</b> by pressing X\n" +
                "   * <b>Horizontal Flip</b> by pressing Y\n" +
                "   * <i>Hold shift to only rotate or flip tile positions</i>\n" +
                "\n - <b>Use Ctrl-Z/Ctrl-Y</b> to Undo/Redo changes\n";
            GUIContent helpContent = new GUIContent(sHelp);
            Handles.BeginGUI();
            Rect rHelpBox = new Rect(new Vector2(2f, 64f), Styles.Instance.toolbarBoxStyle.CalcSize(helpContent));
            GUILayout.BeginArea(rHelpBox);
            HandlesEx.DrawRectWithOutline(new Rect(Vector2.zero, rHelpBox.size), s_toolbarBoxBgColor, s_toolbarBoxOutlineColor);
            GUILayout.Label(sHelp, Styles.Instance.toolbarBoxStyle);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        #endregion

        #region Private Methods
        private eBrushMode GetBrushMode()
        {
            if (Event.current.shift) return eBrushMode.Erase;
            return s_brushMode;
        }

        private void ResetBrushMode()
        {
            s_brushMode = eBrushMode.Paint;
        }
        
        private bool IsTilemapChunksVisible()
        {
            TilemapChunk chunk = m_tilemap.GetComponentInChildren<TilemapChunk>();
            return chunk && (chunk.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0;
        }

        private void SetTilemapChunkHideFlag(HideFlags flags, bool value)
        {
            TilemapChunk[] chunks = m_tilemap.GetComponentsInChildren<TilemapChunk>();
            foreach (TilemapChunk chunk in chunks)
            {
                if (value)
                    chunk.gameObject.hideFlags |= flags;
                else
                    chunk.gameObject.hideFlags &= ~flags;

            }
        }
        #endregion
    }
}