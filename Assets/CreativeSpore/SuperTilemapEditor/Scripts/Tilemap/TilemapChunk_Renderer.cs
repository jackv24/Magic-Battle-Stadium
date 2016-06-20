using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    partial class TilemapChunk
    {
        [SerializeField, HideInInspector]
        private MeshFilter m_meshFilter;
        [SerializeField, HideInInspector]
        private MeshRenderer m_meshRenderer;

        /// <summary>
        /// Next time UpdateMesh is called, the tile mesh will be rebuild
        /// </summary>
        public void InvalidateMesh()
        {
            m_needsRebuildMesh = true;
        }

        /// <summary>
        /// Invalidates brushes, so all tiles with brushes call again the brush refresh method on next UpdateMesh call
        /// </summary>
        public void InvalidateBrushes()
        {
            m_invalidateBrushes = true;
        }

        public void SetSharedMaterial(Material material)
        {
            m_meshRenderer.sharedMaterial = material;
            m_needsRebuildMesh = true;
        }

        private bool m_needsRebuildMesh = false;
        private bool m_invalidateBrushes = false;
        /// <summary>
        /// Update the mesh and return false if all tiles are empty
        /// </summary>
        /// <returns></returns>
        public bool UpdateMesh()
        {
            if (ParentTilemap == null)
            {
                if (transform.parent == null) gameObject.hideFlags = HideFlags.None; //Unhide orphan tilechunks. This shouldn't happen
                ParentTilemap = transform.parent.GetComponent<Tilemap>();
            }
            gameObject.layer = ParentTilemap.gameObject.layer;
            transform.localPosition = new Vector2(GridPosX * CellSize.x, GridPosY * CellSize.y);

            if (m_meshFilter.sharedMesh == null)
            {
                //Debug.Log("Creating new mesh for " + name);
                m_meshFilter.sharedMesh = new Mesh();
                m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
                m_meshFilter.sharedMesh.name = ParentTilemap.name + "_mesh";
                m_needsRebuildMesh = true;
            }

            if (m_meshRenderer.sharedMaterial == null)
            {
                m_meshRenderer.sharedMaterial = ParentTilemap.Material;
                m_meshRenderer.sharedMaterial.mainTexture = Tileset.AtlasTexture;
            }
            m_meshRenderer.enabled = ParentTilemap.IsVisible;

            if (m_needsRebuildMesh)
            {
                m_needsRebuildMesh = false;
                if (FillMeshData())
                {
                    m_invalidateBrushes = false;
                    Mesh mesh = m_meshFilter.sharedMesh;
                    mesh.Clear();
                    mesh.vertices = m_vertices.ToArray();
                    mesh.uv = m_uv.ToArray();
                    mesh.triangles = m_triangles.ToArray();
                    mesh.RecalculateNormals(); //NOTE: allow directional lights to work properly
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void DestroyMeshIfNeeded()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh != null
                && (meshFilter.sharedMesh.hideFlags & HideFlags.DontSave) != 0)
            {
                //Debug.Log("Destroy Mesh of " + name);
                DestroyImmediate(meshFilter.sharedMesh);
            }
        }

        /// <summary>
        /// Fill the mesh data and return false if all tiles are empty
        /// </summary>
        /// <returns></returns>
        private bool FillMeshData()
        {
            //Debug.Log( "[" + ParentTilemap.name + "] FillData -> " + name);
            if (Tileset == null)
            {
                return false;
            }

            m_meshRenderer.sharedMaterial = ParentTilemap.Material;
            m_meshRenderer.sharedMaterial.mainTexture = Tileset.AtlasTexture;

            int totalTiles = m_width * m_height;
            if (m_vertices == null)
            {
                m_vertices = new List<Vector3>(totalTiles * 4);
                m_uv = new List<Vector2>(totalTiles * 4);
                m_triangles = new List<int>(totalTiles * 6);
            }
            else
            {
                m_vertices.Clear();
                m_triangles.Clear();
                m_uv.Clear();
            }

            //+++ MeshCollider
            if (m_meshCollVertices == null)
            {
                m_meshCollVertices = new List<Vector3>(totalTiles * 4);
                m_meshCollTriangles = new List<int>(totalTiles * 6);
            }
            else
            {
                m_meshCollVertices.Clear();
                m_meshCollTriangles.Clear();
            }
            TileColliderData testCollData = new TileColliderData();
            testCollData.vertices = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            //---

            Vector2[] subTileOffset = new Vector2[]
            {
                new Vector2( 0f, 0f ),
                new Vector2( CellSize.x / 2f, 0f ),
                new Vector2( 0f, CellSize.y / 2f ),
                new Vector2( CellSize.x / 2f, CellSize.y / 2f ),
            };
            Vector2 subTileSize = CellSize / 2f;
            m_animatedTiles.Clear();
            bool isEmpty = true;
            for (int ty = 0, tileIdx = 0; ty < m_height; ++ty)
            {
                for (int tx = 0; tx < m_width; ++tx, ++tileIdx)
                {
                    uint tileData = m_tileDataList[tileIdx];
                    if (tileData != Tileset.k_TileData_Empty)
                    {
                        int brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
                        int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                        Tile tile = (tileId != Tileset.k_TileId_Empty) ? Tileset.Tiles[tileId] : null;
                        TilesetBrush tileBrush = null;
                        if (brushId > 0)
                        {
                            tileBrush = Tileset.FindBrush(brushId);
                            if (tileBrush == null)
                            {
                                Debug.LogWarning(ParentTilemap.name + "\\" + name + ": BrushId " + brushId + " not found! GridPos(" + tx + "," + ty + ") tilaData 0x" + tileData.ToString("X"));
                                m_tileDataList[tileIdx] = tileData & ~Tileset.k_TileDataMask_BrushId;
                            }
                            if (tileBrush != null && (m_invalidateBrushes || (tileData & Tileset.k_TileFlag_Updated) == 0))
                            {
                                tileData = tileBrush.Refresh(ParentTilemap, GridPosX + tx, GridPosY + ty, tileData);
                                tileData |= Tileset.k_TileFlag_Updated;// set updated flag
                                m_tileDataList[tileIdx] = tileData; // update tileData                                
                                tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                                tile = (tileId != Tileset.k_TileId_Empty) ? Tileset.Tiles[tileId] : null;
                                // update created objects
                                if (tile != null && tile.prefabData.prefab != null)
                                    CreateTileObject(tileIdx, tile.prefabData);
                                else
                                    DestroyTileObject(tileIdx);
                            }
                        }

                        isEmpty = false;

                        if (tileBrush != null && tileBrush.IsAnimated())
                        {
                            m_animatedTiles.Add(new AnimTileData() { VertexIdx = m_vertices.Count, Brush = tileBrush });
                        }


                        Rect tileUV;
                        uint[] subtileData = tileBrush != null ? tileBrush.GetSubtiles(ParentTilemap, GridPosX + tx, GridPosY + ty, tileData) : null;
                        if (subtileData == null)
                        {
                            if (tile != null)
                            {
                                if (tile.prefabData.prefab == null || tile.prefabData.showTileWithPrefab //hide the tiles with prefabs ( unless showTileWithPrefab is true )
                                    || tileBrush && tileBrush.IsAnimated()) // ( skip if it's an animated brush )
                                {
                                    tileUV = tile.uv;
                                    _AddTileToMesh(tileUV, tx, ty, tileData, Vector2.zero, CellSize);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < subtileData.Length; ++i)
                            {
                                uint subTileData = subtileData[i];
                                int subTileId = (int)(subTileData & Tileset.k_TileDataMask_TileId);
                                tileUV = subTileId != Tileset.k_TileId_Empty ? Tileset.Tiles[subTileId].uv : default(Rect);
                                //if (tileUV != default(Rect)) //NOTE: if this is uncommented, there won't be coherence with geometry ( 16 vertices per tiles with subtiles ). But it means also, the tile shouldn't be null.
                                {
                                    _AddTileToMesh(tileUV, tx, ty, subTileData, subTileOffset[i], subTileSize, i);
                                }
                            }
                        }
                    }
                }
            }

            //NOTE: the destruction of tileobjects needs to be done here to avoid a Undo/Redo bug. Check inside DestroyTileObject for more information.
            for (int i = 0; i < m_tileObjToBeRemoved.Count; ++i)
            {
                DestroyTileObject(m_tileObjToBeRemoved[i]);
            }
            m_tileObjToBeRemoved.Clear();

            return !isEmpty;
        }

        private void _AddTileToMesh(Rect tileUV, int tx, int ty, uint tileData, Vector2 subtileOffset, Vector2 subtileCellSize, int subTileIdx = -1)
        {
            float px0 = tx * CellSize.x + subtileOffset.x;
            float py0 = ty * CellSize.y + subtileOffset.y;
            float px1 = px0 + subtileCellSize.x;
            float py1 = py0 + subtileCellSize.y;

            int vertexIdx = m_vertices.Count;

            m_vertices.Add(new Vector3(px0, py0, 0));
            m_vertices.Add(new Vector3(px1, py0, 0));
            m_vertices.Add(new Vector3(px0, py1, 0));
            m_vertices.Add(new Vector3(px1, py1, 0));

            m_triangles.Add(vertexIdx + 3);
            m_triangles.Add(vertexIdx + 0);
            m_triangles.Add(vertexIdx + 2);
            m_triangles.Add(vertexIdx + 0);
            m_triangles.Add(vertexIdx + 3);
            m_triangles.Add(vertexIdx + 1);

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

            Vector2[] uvs = new Vector2[4];
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

            if (subTileIdx >= 0)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (i == subTileIdx) continue;
                    uvs[i] = (uvs[i] + uvs[subTileIdx]) / 2f;
                }
            }

            for (int i = 0; i < 4; ++i)
            {
                m_uv.Add(uvs[i]);
            }
        }
    }
}
