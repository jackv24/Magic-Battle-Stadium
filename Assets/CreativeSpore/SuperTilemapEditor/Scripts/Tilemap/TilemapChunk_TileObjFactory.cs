using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    partial class TilemapChunk
    {
        const string k_OnTilePrefabCreation = "OnTilePrefabCreation";

        public struct OnTilePrefabCreationData
        {
            public Tilemap ParentTilemap;
            public int GridX;
            public int GridY;
        }

        [System.Serializable]
        private class TileObjData
        {
            public int tilePos;
            public TilePrefabData tilePrefabData;
            public GameObject obj = null;
        }        


        [SerializeField, HideInInspector]
        private List<TileObjData> m_tileObjList = new List<TileObjData>();
        private List<GameObject> m_tileObjToBeRemoved = new List<GameObject>();

        /// <summary>
        /// Update all tile objects if tile prefab data was changed and create tile objects for tiles with new prefab data.
        /// Call this method only when a tile prefab data has been changed to update changes. You may need to call UpdateMesh after this.
        /// </summary>
        public void RefreshTileObjects()
        {
            // Destroy tile objects where tile prefab is now null
            for (int i = 0; i < m_tileObjList.Count; ++i)
            {
                TileObjData tileObjData = m_tileObjList[i];
                uint tileData = m_tileDataList[tileObjData.tilePos];
                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                Tile tile = tileId != Tileset.k_TileId_Empty ? Tileset.Tiles[tileId] : null;
                if (tile == null || tile.prefabData.prefab == null)
                {
                    DestroyTileObject(tileObjData.tilePos);
                }
            }

            // Recreate or update all tile objects
            for (int tileIdx = 0; tileIdx < m_tileDataList.Count; ++tileIdx)
            {
                uint tileData = m_tileDataList[tileIdx];
                int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                Tile tile = tileId != Tileset.k_TileId_Empty ? Tileset.Tiles[tileId] : null;
                if (tile != null && tile.prefabData.prefab != null)
                {
                    CreateTileObject(tileIdx, tile.prefabData);
                }
            }
        }

        private GameObject CreateTileObject(int locGridX, int locGridY, TilePrefabData tilePrefabData)
        {
            if (locGridX >= 0 && locGridX < m_width && locGridY >= 0 && locGridY < m_height)
            {
                int tileIdx = locGridY * m_width + locGridX;
                return CreateTileObject(tileIdx, tilePrefabData);
            }
            else
            {
                return null;
            }
        }

        private GameObject CreateTileObject(int tileIdx, TilePrefabData tilePrefabData)
        {
            if (tilePrefabData.prefab != null)
            {
                TileObjData tileObjData = m_tileObjList.Find(x => x.tilePos == tileIdx);
                GameObject tileObj = null;
                int gx = tileIdx % m_width;
                int gy = tileIdx / m_width;
                if (tileObjData == null || tileObjData.tilePrefabData != tilePrefabData || tileObjData.obj == null)
                {
                    Vector3 chunkLocPos = new Vector3((gx + .5f) * CellSize.x, (gy + .5f) * CellSize.y);
                    if (tilePrefabData.offsetMode == TilePrefabData.eOffsetMode.Pixels)
                    {
                        float ppu = Tileset.TilePxSize.x / CellSize.x;
                        chunkLocPos += tilePrefabData.offset / ppu;
                    }
                    else //if (tilePrefabData.offsetMode == TilePrefabData.eOffsetMode.Units)
                    {
                        chunkLocPos += tilePrefabData.offset;
                    }
                    Vector3 worldPos = transform.TransformPoint(chunkLocPos);
#if UNITY_EDITOR
                    tileObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefabData.prefab);
                    tileObj.transform.position = worldPos;
                    tileObj.transform.rotation = transform.rotation;
                    // allow destroy the object with undo operations
                    if (ParentTilemap.IsUndoEnabled)
                    {
                        UnityEditor.Undo.RegisterCreatedObjectUndo(tileObj, Tilemap.k_UndoOpName + ParentTilemap.name);
                    }
#else
                    tileObj = (GameObject)Instantiate(tilePrefabData.prefab, worldPos, transform.rotation);
#endif
                    tileObj.transform.parent = transform.parent;
                    tileObj.transform.localRotation = tilePrefabData.prefab.transform.localRotation;
                    tileObj.transform.localScale = tilePrefabData.prefab.transform.localScale;
                    if (tileObjData != null)
                    {
                        m_tileObjToBeRemoved.Add(tileObjData.obj);
                        tileObjData.obj = tileObj;
                        tileObjData.tilePrefabData = tilePrefabData;
                    }
                    else
                    {
                        m_tileObjList.Add(new TileObjData() { tilePos = tileIdx, obj = tileObj, tilePrefabData = tilePrefabData });
                    }
                    tileObj.SendMessage(k_OnTilePrefabCreation, 
                        new OnTilePrefabCreationData() 
                        { 
                            ParentTilemap = ParentTilemap, 
                            GridX = GridPosX + gx, GridY = GridPosY + gy 
                        }, SendMessageOptions.DontRequireReceiver);
                    return tileObj;
                }
                else if (tileObjData.obj != null)
                {
                    tileObjData.obj.SendMessage(k_OnTilePrefabCreation,
                        new OnTilePrefabCreationData()
                        {
                            ParentTilemap = ParentTilemap,
                            GridX = GridPosX + gx,
                            GridY = GridPosY + gy
                        }, SendMessageOptions.DontRequireReceiver);
                    return tileObjData.obj;
                }
            }
            return null;
        }

        private void DestroyTileObject(int locGridX, int locGridY)
        {
            if (locGridX >= 0 && locGridX < m_width && locGridY >= 0 && locGridY < m_height)
            {
                int tileIdx = locGridY * m_width + locGridX;
                DestroyTileObject(tileIdx);
            }
        }

        private void DestroyTileObject(int tileIdx)
        {
            TileObjData tileObjData = m_tileObjList.Find(x => x.tilePos == tileIdx);
            if (tileObjData != null)
            {
                m_tileObjToBeRemoved.Add(tileObjData.obj);
                m_tileObjList.Remove(tileObjData);
            }
        }

        /// <summary>
        /// Call DestroyTileObject(int tileIdx) to destroy tile objects. This should be called only by UpdateMesh.
        /// NOTE: this delayed destruction is fixing an undo / redo issue
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyTileObject(GameObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (ParentTilemap.IsUndoEnabled)
                {
                    //Note: tested in UNITY 5.2 For some reason, after this is called, all changes made to the chunk overwrite the original state of the data
                    // for that reason, this should be called after all changes are made
                    UnityEditor.Undo.DestroyObjectImmediate(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
#else
                DestroyImmediate(obj);
#endif
            }
        }
    }
}