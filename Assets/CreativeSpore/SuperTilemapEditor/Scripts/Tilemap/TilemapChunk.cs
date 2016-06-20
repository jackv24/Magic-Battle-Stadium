using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreativeSpore.SuperTilemapEditor
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu("")] // Disable attaching it to a gameobject
    [ExecuteInEditMode] //NOTE: this is needed so OnDestroy is called and there is no memory leaks
    public partial class TilemapChunk : MonoBehaviour
    {
        #region Public Properties
        public Tileset Tileset
        {
            get { return ParentTilemap.Tileset; }
        }

        public Tilemap ParentTilemap;
        /// <summary>
        /// The x position inside the parent tilemap
        /// </summary>
        public int GridPosX;
        /// <summary>
        /// The y position inside the parent tilemap
        /// </summary>
        public int GridPosY;

        public int GridWidth { get { return m_width; } }
        public int GridHeight { get { return m_height; } }

        public int SortingLayerID
        {
            get { return m_meshRenderer.sortingLayerID; }
            set { m_meshRenderer.sortingLayerID = value; }
        }

        public string SortingLayerName
        {
            get { return m_meshRenderer.sortingLayerName; }
            set { m_meshRenderer.sortingLayerName = value; }
        }

        public int OrderInLayer
        {
            get { return m_meshRenderer.sortingOrder; }
            set { m_meshRenderer.sortingOrder = value; }
        }

        public MeshFilter MeshFilter { get { return m_meshFilter; } }

        public Vector2 CellSize { get { return ParentTilemap.CellSize; } }
        /// <summary>
        /// Stretch the size of the tile UV the pixels indicated in this value. This trick help to fix pixel artifacts.
        /// Most of the time a value of 0.5 pixels will be fine, but in case of a big zoom-out level, a higher value will be necessary
        /// </summary>
        public float InnerPadding { get { return ParentTilemap.InnerPadding; } }
        #endregion

        #region Private Fields

        [SerializeField, HideInInspector]
        private int m_width = 8;
        [SerializeField, HideInInspector]
        private int m_height = 4;
        [SerializeField, HideInInspector]
        private List<uint> m_tileDataList = new List<uint>();        

        private List<Vector3> m_vertices;
        private List<Vector2> m_uv;
        private List<int> m_triangles;
        // private List<Color32> m_colors; TODO: add color vertex support

        struct AnimTileData
        {
            public int VertexIdx;
            public TilesetBrush Brush;
        }
        private List<AnimTileData> m_animatedTiles = new List<AnimTileData>();
        #endregion

        #region Monobehaviour Methods

        void OnWillRenderObject()
        {
            if (m_animatedTiles.Count > 0) //TODO: add fps attribute to update animated tiles when necessary
            {

                Dictionary<TilesetBrush, Vector2[]> animTileCache = new Dictionary<TilesetBrush, Vector2[]>();
                for (int i = 0; i < m_animatedTiles.Count; ++i)
                {
                    AnimTileData animTileData = m_animatedTiles[i];
                    Vector2[] uvs;
                    if (!animTileCache.TryGetValue(animTileData.Brush, out uvs))
                    {
                        //NOTE: GetAnimTileData will be called only once per brush, because after this call, the brush will be in the cache dictionary
                        uint tileData = animTileData.Brush.GetAnimTileData();
                        Rect tileUV = animTileData.Brush.GetAnimUV();

                        bool flipH = (tileData & Tileset.k_TileFlag_FlipH) != 0;
                        bool flipV = (tileData & Tileset.k_TileFlag_FlipV) != 0;
                        bool rot90 = (tileData & Tileset.k_TileFlag_Rot90) != 0;

                        //NOTE: xMinMax and yMinMax is opposite if width or height is negative
                        float u0 = tileUV.xMin + Tileset.AtlasTexture.texelSize.x * InnerPadding;
                        float v0 = tileUV.yMin + Tileset.AtlasTexture.texelSize.y * InnerPadding;
                        float u1 = tileUV.xMax - Tileset.AtlasTexture.texelSize.x * InnerPadding;
                        float v1 = tileUV.yMax - Tileset.AtlasTexture.texelSize.y * InnerPadding;

                        if (flipH)
                        {
                            float v = v0;
                            v0 = v1;
                            v1 = v;
                        }
                        if (flipV)
                        {
                            float u = u0;
                            u0 = u1;
                            u1 = u;
                        }

                        uvs = new Vector2[4];
                        if (rot90)
                        {
                            uvs[0] = new Vector2(u1, v0);
                            uvs[1] = new Vector2(u1, v1);
                            uvs[2] = new Vector2(u0, v0);
                            uvs[3] = new Vector2(u0, v1);
                        }
                        else
                        {
                            uvs[0] = new Vector2(u0, v0);
                            uvs[1] = new Vector2(u1, v0);
                            uvs[2] = new Vector2(u0, v1);
                            uvs[3] = new Vector2(u1, v1);
                        }

                        animTileCache[animTileData.Brush] = uvs;
                    }

                    m_uv[animTileData.VertexIdx + 0] = uvs[0];
                    m_uv[animTileData.VertexIdx + 1] = uvs[1];
                    m_uv[animTileData.VertexIdx + 2] = uvs[2];
                    m_uv[animTileData.VertexIdx + 3] = uvs[3];
                }
                m_meshFilter.sharedMesh.uv = m_uv.ToArray();

            }
        }

        // NOTE: OnDestroy is not called in editor without [ExecuteInEditMode]
        void OnDestroy()
        {
            //avoid memory leak
            DestroyMeshIfNeeded();
            DestroyColliderMeshIfNeeded();
        }

        // This is needed to refresh tilechunks after undo / redo actions
        static bool s_isOnValidate = false; // fix issue when destroying unused resources from the invalidate call
        void OnValidate()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.ExecuteCommand && (e.commandName == "Duplicate" || e.commandName == "Paste"))
            {
                _DoDuplicate();
            }
            // Refresh only if Mesh is null (this happens if hideFlags == DontSave)
            m_needsRebuildMesh = m_meshFilter.sharedMesh == null;
            m_needsRebuildColliders = ParentTilemap.ColliderType == eColliderType._3D && (m_meshCollider == null || m_meshCollider.sharedMesh == null);
            s_isOnValidate = true;
            UpdateMesh();
            UpdateColliders();
            s_isOnValidate = false;
        }

        private void _DoDuplicate()
        {
            // When copying a tilemap, the sharedMesh will be the same as the copied tilemap, so it has to be created a new one
            m_meshFilter.sharedMesh = null; // NOTE: if not nulled before the new Mesh, the previous mesh will be destroyed
            m_meshFilter.sharedMesh = new Mesh();
            m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
            m_meshFilter.sharedMesh.name = ParentTilemap.name + "_Copy_mesh";
            m_needsRebuildMesh = true;
            if (m_meshCollider != null)
            {
                m_meshCollider.sharedMesh = null; // NOTE: if not nulled before the new Mesh, the previous mesh will be destroyed
                m_meshCollider.sharedMesh = new Mesh();
                m_meshCollider.sharedMesh.hideFlags = HideFlags.DontSave;
                m_meshCollider.sharedMesh.name = ParentTilemap.name + "_Copy_collmesh";
            }
            m_needsRebuildColliders = true;
            //---
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            if (m_meshRenderer != null)
            {
                EditorUtility.SetSelectedWireframeHidden(m_meshRenderer, true);
            }
#endif
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshCollider = GetComponent<MeshCollider>();

            if (m_tileDataList == null || m_tileDataList.Count != m_width * m_height)
            {
                SetDimensions(m_width, m_height);
            }

            // if not playing, this will be done later by OnValidate
            if (Application.isPlaying)
            {
                // Refresh only if Mesh is null (this happens if hideFlags == DontSave)
                m_needsRebuildMesh = m_meshFilter.sharedMesh == null;
                m_needsRebuildColliders = ParentTilemap.ColliderType == eColliderType._3D && (m_meshCollider == null || m_meshCollider.sharedMesh == null);
                UpdateMesh();
                UpdateColliders();
            }
        }

        public void Reset()
        {
            SetDimensions(m_width, m_height);           

#if UNITY_EDITOR
            if (m_meshRenderer != null)
            {
                EditorUtility.SetSelectedWireframeHidden(m_meshRenderer, true);
            }
#endif

            m_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_meshRenderer.receiveShadows = false;

            m_needsRebuildMesh = true;
            m_needsRebuildColliders = true;
        }
        #endregion

        #region Public Methods

        public void DrawColliders()
        {
            if (ParentTilemap.ColliderType == eColliderType._3D)
            {
                if (m_meshCollider != null && m_meshCollider.sharedMesh != null && m_meshCollider.sharedMesh.normals.Length > 0f)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
                    Gizmos.DrawWireMesh(m_meshCollider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.color = Color.white;
                }
            }
            else if(ParentTilemap.ColliderType == eColliderType._2D)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Collider2D[] edgeColliders = GetComponents<Collider2D>();
                for(int i = 0; i < edgeColliders.Length; ++i)
                {
                    Collider2D collider2D = edgeColliders[i];
                    if (collider2D.enabled)
                    {
                        Vector2[] points = collider2D is EdgeCollider2D? ((EdgeCollider2D)collider2D).points : ((PolygonCollider2D)collider2D).points;
                        for (int j = 0; j < (points.Length - 1); ++j)
                        {
                            Gizmos.DrawLine(points[j], points[j + 1]);
                        }
                    }
                }
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.white;
            }
        }

        public void SetDimensions(int width, int height)
        {
            int size = width * height;
            if (size > 0 && size * 4 < 65000) //NOTE: 65000 is the current maximum vertex allowed per mesh and each tile has 4 vertex
            {
                m_width = width;
                m_height = height;
                m_tileDataList = Enumerable.Repeat(Tileset.k_TileData_Empty, size).ToList();
            }
            else
            {
                Debug.LogWarning("Invalid parameters!");
            }
        }

        public void SetTileData(Vector2 vLocalPos, uint tileData)
        {
            SetTileData((int)(vLocalPos.x / CellSize.x), (int)(vLocalPos.y / CellSize.y), tileData);
        }

        public void SetTileData(int locGridX, int locGridY, uint tileData)
        {
            if (locGridX >= 0 && locGridX < m_width && locGridY >= 0 && locGridY < m_height)
            {
                int tileIdx = locGridY * m_width + locGridX;

                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                Tile tile = tileId != Tileset.k_TileId_Empty? Tileset.Tiles[tileId] : null;

                int prevTileId = (int)(m_tileDataList[tileIdx] & Tileset.k_TileDataMask_TileId);
                Tile prevTile = prevTileId != Tileset.k_TileId_Empty? Tileset.Tiles[prevTileId] : null;                             

                int brushId = Tileset.GetBrushIdFromTileData(tileData);
                int prevBrushId = Tileset.GetBrushIdFromTileData(m_tileDataList[tileIdx]);

                if (brushId != prevBrushId)
                {
                    TilesetBrush brush = ParentTilemap.Tileset.FindBrush(brushId);
                    TilesetBrush prevBrush = ParentTilemap.Tileset.FindBrush(prevBrushId);
                    if (prevBrush != null)
                    {
                        prevBrush.OnErase(this, locGridX, locGridY, tileData);
                    }
                    if (brush != null)
                    {
                        tileData = brush.OnPaint(this, locGridX, locGridY, tileData);
                    }

                    // Refresh Neighbors ( and itself if needed )
                    for (int yf = -1; yf <= 1; ++yf)
                    {
                        for (int xf = -1; xf <= 1; ++xf)
                        {
                            if ((xf | yf) == 0 )
                            {
                                if (brushId > 0)
                                {
                                    // Refresh itself
                                    tileData = (tileData & ~Tileset.k_TileFlag_Updated);
                                }
                            }
                            else
                            {
                                int gx = (locGridX + xf);
                                int gy = (locGridY + yf);
                                int idx = gy * m_width + gx;
                                bool isInsideChunk = (gx >= 0 && gx < m_width && gy >= 0 && gy < m_height);
                                uint neighborTileData = isInsideChunk ? m_tileDataList[idx] : ParentTilemap.GetTileData(GridPosX + locGridX + xf, GridPosY + locGridY + yf);
                                int neighborBrushId = (int)((neighborTileData & Tileset.k_TileDataMask_BrushId) >> 16);
                                TilesetBrush neighborBrush = ParentTilemap.Tileset.FindBrush(neighborBrushId);
                                //if (brush != null && brush.AutotileWith(brushId, neighborBrushId) || prevBrush != null && prevBrush.AutotileWith(prevBrushId, neighborBrushId))
                                if (neighborBrush != null && 
                                    (neighborBrush.AutotileWith(neighborBrushId, brushId) || neighborBrush.AutotileWith(neighborBrushId, prevBrushId)))
                                {
                                    neighborTileData = (neighborTileData & ~Tileset.k_TileFlag_Updated); // force a refresh
                                    if (isInsideChunk)
                                    {
                                        m_tileDataList[idx] = neighborTileData;
                                    }
                                    else
                                    {
                                        ParentTilemap.SetTileData(GridPosX + gx, GridPosY + gy, neighborTileData);
                                    }
                                }
                            }
                        }
                    }
                }
                else if(brushId > 0)
                {
                    // Refresh itself
                    tileData = (tileData & ~Tileset.k_TileFlag_Updated);
                }

                m_needsRebuildMesh |= (m_tileDataList[tileIdx] != tileData) || (tileData & Tileset.k_TileDataMask_TileId) == Tileset.k_TileId_Empty;
                m_needsRebuildColliders |= m_needsRebuildMesh &&
                (
                    (prevBrushId > 0) || (brushId > 0) // there is a brush (a brush could change the collider data later)
                    || (tile != null && tile.collData.type != eTileCollider.None) || (prevTile != null && prevTile.collData.type != eTileCollider.None) // prev. or new tile has colliders
                );

                if (ParentTilemap.ColliderType != eColliderType.None && m_needsRebuildColliders)
                {
                    // Refresh Neighbors tilechunk colliders, to make the collider autotiling
                    // Only if neighbor is outside this tilechunk
                    for (int yf = -1; yf <= 1; ++yf)
                    {
                        for (int xf = -1; xf <= 1; ++xf)
                        {
                            if ((xf | yf) != 0) // skip this tile position xf = yf = 0
                            {
                                int gx = (locGridX + xf);
                                int gy = (locGridY + yf);
                                bool isInsideChunk = (gx >= 0 && gx < m_width && gy >= 0 && gy < m_height);
                                if (!isInsideChunk)
                                {
                                    ParentTilemap.InvalidateChunkAt(GridPosX + gx, GridPosY + gy, false, true);
                                }
                            }
                        }
                    }
                }

                // Update tile data
                m_tileDataList[tileIdx] = tileData;

                // Create tile Objects
                if (tile != null && tile.prefabData.prefab != null)
                    CreateTileObject(tileIdx, tile.prefabData);
                else
                    DestroyTileObject(tileIdx);    
            }
        }

        public uint GetTileData(Vector2 vLocalPos)
        {
            return GetTileData((int)(vLocalPos.x / CellSize.x), (int)(vLocalPos.y / CellSize.y));
        }

        public uint GetTileData(int locGridX, int locGridY)
        {
            if (locGridX >= 0 && locGridX < m_width && locGridY >= 0 && locGridY < m_height)
            {
                int tileIdx = locGridY * m_width + locGridX;
                return m_tileDataList[tileIdx];
            }
            else
            {
                return Tileset.k_TileData_Empty;
            }
        }
        #endregion
    }
}