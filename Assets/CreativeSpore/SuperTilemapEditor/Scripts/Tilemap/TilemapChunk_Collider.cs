using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreativeSpore.SuperTilemapEditor
{
    partial class TilemapChunk
    {
        //+++ MeshCollider
        [SerializeField, HideInInspector]
        private MeshCollider m_meshCollider;
        private List<Vector3> m_meshCollVertices;
        private List<int> m_meshCollTriangles;
        //---

        //+++ 2D Edge colliders
        [SerializeField]
        private bool m_has2DColliders;
        //---

        /// <summary>
        /// Next time UpdateColliderMesh is called, the collider mesh will be rebuild
        /// </summary>
        public void InvalidateMeshCollider()
        {
            m_needsRebuildColliders = true;
        }

        private bool m_needsRebuildColliders = false;
        public bool UpdateColliders()
        {
            if (ParentTilemap == null)
            {
                ParentTilemap = transform.parent.GetComponent<Tilemap>();
            }
            gameObject.layer = ParentTilemap.gameObject.layer;


            //+++ Free unused resources
            if (ParentTilemap.ColliderType != eColliderType._3D)
            {
                if (m_meshCollider != null)
                {
                    if (!s_isOnValidate)
                        DestroyImmediate(m_meshCollider);
                    else
                        m_meshCollider.enabled = false;
                }
            }

            //if (ParentTilemap.ColliderType != eColliderType._2D)
            {
                if (m_has2DColliders)
                {
                    m_has2DColliders = ParentTilemap.ColliderType == eColliderType._2D;
                    System.Type oppositeCollider2DType = ParentTilemap.Collider2DType != e2DColliderType.EdgeCollider2D ? typeof(EdgeCollider2D) : typeof(PolygonCollider2D);
                    System.Type collidersToDestroy = ParentTilemap.ColliderType != eColliderType._2D ? typeof(Collider2D) : oppositeCollider2DType;
                    var aCollider2D = GetComponents(collidersToDestroy);
                    for (int i = 0; i < aCollider2D.Length; ++i)
                    {
                        if (!s_isOnValidate) 
                            DestroyImmediate(aCollider2D[i]); 
                        else 
                            ((Collider2D)aCollider2D[i]).enabled = false;
                    }
                }
            }
            //---

            if (ParentTilemap.ColliderType == eColliderType._3D)
            {
                if (m_meshCollider == null)
                {
                    m_meshCollider = GetComponent<MeshCollider>();
                    if (m_meshCollider == null && ParentTilemap.ColliderType == eColliderType._3D)
                    {
                        m_meshCollider = gameObject.AddComponent<MeshCollider>();
                    }
                }

                if (ParentTilemap.IsTrigger)
                {
                    m_meshCollider.convex = true;
                    m_meshCollider.isTrigger = true;
                }
                else
                {
                    m_meshCollider.isTrigger = false;
                    m_meshCollider.convex = false;
                }
                m_meshCollider.sharedMaterial = ParentTilemap.PhysicMaterial;

                //NOTE: m_meshCollider.sharedMesh is equal to m_meshFilter.sharedMesh when the script is attached or reset
                if (m_meshCollider != null && (m_meshCollider.sharedMesh == null || m_meshCollider.sharedMesh == m_meshFilter.sharedMesh))
                {
                    m_meshCollider.sharedMesh = new Mesh();
                    m_meshCollider.sharedMesh.hideFlags = HideFlags.DontSave;
                    m_meshCollider.sharedMesh.name = ParentTilemap.name + "_collmesh";
                    m_needsRebuildColliders = true;
                }
            } 
            
            if (m_needsRebuildColliders)
            {
                m_needsRebuildColliders = false;
                bool isEmpty = FillColliderMeshData();
                if (ParentTilemap.ColliderType == eColliderType._3D)
                {
                    Mesh mesh = m_meshCollider.sharedMesh;
                    mesh.Clear();
                    mesh.vertices = m_meshCollVertices.ToArray();
                    mesh.triangles = m_meshCollTriangles.ToArray();
                    mesh.RecalculateNormals(); // needed by Gizmos.DrawWireMesh
                    m_meshCollider.sharedMesh = null; // for some reason this fix showing the green lines of the collider mesh
                    m_meshCollider.sharedMesh = mesh;
                }
                return isEmpty;
            }
            return true;
        }

        private void DestroyColliderMeshIfNeeded()
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null && meshCollider.sharedMesh != null
                && (meshCollider.sharedMesh.hideFlags & HideFlags.DontSave) != 0)
            {
                //Debug.Log("Destroy Mesh of " + name);
                DestroyImmediate(meshCollider.sharedMesh);
            }
        }        

        private static Vector2[] s_fullCollTileVertices = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        private bool FillColliderMeshData()
        {
            //Debug.Log( "[" + ParentTilemap.name + "] FillColliderMeshData -> " + name);
            if (Tileset == null || ParentTilemap.ColliderType == eColliderType.None)
            {
                return false;
            }
            List<LinkedList<Vector2>> openEdges = null;
            System.Type collider2DType = ParentTilemap.Collider2DType == e2DColliderType.EdgeCollider2D ? typeof(EdgeCollider2D) : typeof(PolygonCollider2D);
            Component[] aColliders2D = null;
            if (ParentTilemap.ColliderType == eColliderType._3D)
            {
                int totalTiles = m_width * m_height;
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
            }
            else //if (ParentTilemap.ColliderType == eColliderType._2D)
            {
                m_has2DColliders = true;
                openEdges = new List<LinkedList<Vector2>>(10);
                aColliders2D = GetComponents(collider2DType); 
            }

            float halvedCollDepth = ParentTilemap.ColliderDepth / 2f;
            bool isEmpty = true;
            for (int ty = 0, tileIdx = 0; ty < m_height; ++ty)
            {
                for (int tx = 0; tx < m_width; ++tx, ++tileIdx)
                {
                    uint tileData = m_tileDataList[tileIdx];
                    if (tileData != Tileset.k_TileData_Empty)
                    {
                        int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
                        if (tileId != Tileset.k_TileId_Empty)
                        {
                            TileColliderData tileCollData = Tileset.Tiles[tileId].collData;
                            if (tileCollData.type != eTileCollider.None)
                            {
                                if ((tileData & (Tileset.k_TileFlag_FlipH | Tileset.k_TileFlag_FlipV | Tileset.k_TileFlag_Rot90)) != 0)
                                {
                                    tileCollData = tileCollData.Clone();
                                    tileCollData.ApplyFlippingFlags(tileData);                                    
                                }
                                isEmpty = false;
                                int neighborCollFlags = 0; // don't remove, even using neighborTileCollData, neighborTileCollData is not filled if tile is empty
                                bool isSurroundedByFullColliders = true;
                                Vector2[] neighborSegmentMinMax = new Vector2[4];
                                TileColliderData[] neighborTileCollData = new TileColliderData[4];
                                for (int i = 0; i < 4; ++i)
                                {
                                    uint neighborTileData;
                                    bool isTriggerOrPolygon = ParentTilemap.IsTrigger || 
                                        ParentTilemap.ColliderType == eColliderType._2D && 
                                        ParentTilemap.Collider2DType == e2DColliderType.PolygonCollider2D;
                                    switch (i)
                                    {
                                        case 0:  // Up Tile
                                            neighborTileData = (tileIdx + m_width) < m_tileDataList.Count ? 
                                            m_tileDataList[tileIdx + m_width]
                                            :
                                            isTriggerOrPolygon? Tileset.k_TileData_Empty : ParentTilemap.GetTileData( GridPosX + tx, GridPosY + ty + 1); break;
                                        case 1: // Right Tile
                                            neighborTileData = (tileIdx + 1) % m_width != 0 ? //(tileIdx + 1) < m_tileDataList.Count ? 
                                            m_tileDataList[tileIdx + 1]
                                            :
                                            isTriggerOrPolygon? Tileset.k_TileData_Empty : ParentTilemap.GetTileData(GridPosX + tx + 1, GridPosY + ty); break;
                                        case 2: // Down Tile
                                            neighborTileData = tileIdx >= m_width ? 
                                            m_tileDataList[tileIdx - m_width]
                                            :
                                            isTriggerOrPolygon? Tileset.k_TileData_Empty : ParentTilemap.GetTileData(GridPosX + tx, GridPosY + ty - 1); break;  
                                        case 3: // Left Tile
                                            neighborTileData = tileIdx % m_width != 0 ? //neighborTileId = tileIdx >= 1 ? 
                                            m_tileDataList[tileIdx - 1]
                                            :
                                            isTriggerOrPolygon? Tileset.k_TileData_Empty : ParentTilemap.GetTileData(GridPosX + tx - 1, GridPosY + ty); break;
                                        default: neighborTileData = Tileset.k_TileData_Empty; break;
                                    }

                                    int neighborTileId = (int)(neighborTileData & Tileset.k_TileDataMask_TileId);
                                    if (neighborTileId != Tileset.k_TileId_Empty)
                                    {
                                        Vector2 segmentMinMax;
                                        TileColliderData neighborTileCollider;
                                        neighborTileCollider = Tileset.Tiles[neighborTileId].collData;
                                        if ((neighborTileData & (Tileset.k_TileFlag_FlipH | Tileset.k_TileFlag_FlipV | Tileset.k_TileFlag_Rot90)) != 0)
                                        {
                                            neighborTileCollider = neighborTileCollider.Clone();
                                            if ((neighborTileData & Tileset.k_TileFlag_FlipH) != 0) neighborTileCollider.FlipH();
                                            if ((neighborTileData & Tileset.k_TileFlag_FlipV) != 0) neighborTileCollider.FlipV();
                                            if ((neighborTileData & Tileset.k_TileFlag_Rot90) != 0) neighborTileCollider.Rot90();
                                        }
                                        neighborTileCollData[i] = neighborTileCollider;
                                        isSurroundedByFullColliders &= (neighborTileCollider.type == eTileCollider.Full);

                                        if (neighborTileCollider.type == eTileCollider.None)
                                        {
                                            segmentMinMax = new Vector2(float.MaxValue, float.MinValue); //NOTE: x will be min, y will be max
                                        }
                                        else if (neighborTileCollider.type == eTileCollider.Full)
                                        {
                                            segmentMinMax = new Vector2(0f, 1f); //NOTE: x will be min, y will be max
                                            neighborCollFlags |= (1 << i);
                                        }
                                        else
                                        {
                                            segmentMinMax = new Vector2(float.MaxValue, float.MinValue); //NOTE: x will be min, y will be max
                                            neighborCollFlags |= (1 << i);
                                            for (int j = 0; j < neighborTileCollider.vertices.Length; ++j)
                                            {
                                                Vector2 v = neighborTileCollider.vertices[j];
                                                {
                                                    if (i == 0 && v.y == 0 || i == 2 && v.y == 1) //Top || Bottom
                                                    {
                                                        if (v.x < segmentMinMax.x) segmentMinMax.x = v.x;
                                                        if (v.x > segmentMinMax.y) segmentMinMax.y = v.x;
                                                    }
                                                    else if (i == 1 && v.x == 0 || i == 3 && v.x == 1) //Right || Left
                                                    {
                                                        if (v.y < segmentMinMax.x) segmentMinMax.x = v.y;
                                                        if (v.y > segmentMinMax.y) segmentMinMax.y = v.y;
                                                    }
                                                }
                                            }
                                        }
                                        neighborSegmentMinMax[i] = segmentMinMax;
                                    }
                                    else
                                    {
                                        isSurroundedByFullColliders = false;
                                    }
                                }

                                // Create Mesh Colliders
                                if (isSurroundedByFullColliders)
                                {
                                    //Debug.Log(" Surrounded! " + tileIdx);
                                }
                                else
                                {
                                    float px0 = tx * CellSize.x;
                                    float py0 = ty * CellSize.y;
                                    Vector2[] collVertices = tileCollData.type == eTileCollider.Full ? s_fullCollTileVertices : tileCollData.vertices;
                                    for (int i = 0; i < collVertices.Length; ++i)
                                    {
                                        Vector2 s0 = collVertices[i];
                                        Vector2 s1 = collVertices[i == (collVertices.Length - 1) ? 0 : i + 1];

                                        // full collider optimization
                                        if ((tileCollData.type == eTileCollider.Full) &&
                                            (
                                            (i == 0 && neighborTileCollData[3].type == eTileCollider.Full) || // left tile has collider
                                            (i == 1 && neighborTileCollData[0].type == eTileCollider.Full) || // top tile has collider
                                            (i == 2 && neighborTileCollData[1].type == eTileCollider.Full) || // right tile has collider
                                            (i == 3 && neighborTileCollData[2].type == eTileCollider.Full)  // bottom tile has collider
                                            )
                                        )
                                        {
                                            continue;
                                        }
                                        // polygon collider optimization
                                        else // if( tileCollData.type == eTileCollider.Polygon ) Or Full colliders if neighbor is not Full as well
                                        {
                                            Vector2 n, m;
                                            if (s0.y == 1f && s1.y == 1f) // top side
                                            {
                                                if ((neighborCollFlags & 0x1) != 0) // top tile has collider
                                                {
                                                    n = neighborSegmentMinMax[0];
                                                    if (n.x < n.y && n.x <= s0.x && n.y >= s1.x)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.x == 1f && s1.x == 1f) // right side
                                            {
                                                if ((neighborCollFlags & 0x2) != 0) // right tile has collider
                                                {
                                                    n = neighborSegmentMinMax[1];
                                                    if (n.x < n.y && n.x <= s1.y && n.y >= s0.y)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.y == 0f && s1.y == 0f) // bottom side
                                            {
                                                if ((neighborCollFlags & 0x4) != 0) // bottom tile has collider
                                                {
                                                    n = neighborSegmentMinMax[2];
                                                    if (n.x < n.y && n.x <= s1.x && n.y >= s0.x)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.x == 0f && s1.x == 0f) // left side
                                            {
                                                if ((neighborCollFlags & 0x8) != 0) // left tile has collider
                                                {
                                                    n = neighborSegmentMinMax[3];
                                                    if (n.x < n.y && n.x <= s0.y && n.y >= s1.y)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.y == 1f && s1.x == 1f) // top - right diagonal
                                            {
                                                if ((neighborCollFlags & 0x1) != 0 && (neighborCollFlags & 0x2) != 0)
                                                {
                                                    n = neighborSegmentMinMax[0];
                                                    m = neighborSegmentMinMax[1];
                                                    if ((n.x < n.y && n.x <= s0.x && n.y == 1f) && (m.x < m.y && m.x <= s1.y && m.y == 1f))
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.x == 1f && s1.y == 0f) // right - bottom diagonal
                                            {
                                                if ((neighborCollFlags & 0x2) != 0 && (neighborCollFlags & 0x4) != 0)
                                                {
                                                    n = neighborSegmentMinMax[1];
                                                    m = neighborSegmentMinMax[2];
                                                    if ((n.x < n.y && n.x == 0f && n.y >= s0.y) && (m.x < m.y && m.x <= s1.x && m.y == 1f))
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.y == 0f && s1.x == 0f) // bottom - left diagonal
                                            {
                                                if ((neighborCollFlags & 0x4) != 0 && (neighborCollFlags & 0x8) != 0)
                                                {
                                                    n = neighborSegmentMinMax[2];
                                                    m = neighborSegmentMinMax[3];
                                                    if ((n.x < n.y && n.x == 0f && n.y >= s0.x) && (m.x < m.y && m.x == 0f && m.y >= s1.y))
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (s0.x == 0f && s1.y == 1f) // left - top diagonal
                                            {
                                                if ((neighborCollFlags & 0x8) != 0 && (neighborCollFlags & 0x1) != 0)
                                                {
                                                    n = neighborSegmentMinMax[3];
                                                    m = neighborSegmentMinMax[0];
                                                    if ((n.x < n.y && n.x <= s0.y && n.y == 1f) && (m.x < m.y && m.x == 0f && m.y >= s1.x))
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                        }

                                        // Update s0 and s1 to world positions
                                        s0.x = px0 + CellSize.x * s0.x; s0.y = py0 + CellSize.y * s0.y;
                                        s1.x = px0 + CellSize.x * s1.x; s1.y = py0 + CellSize.y * s1.y;
                                        if (ParentTilemap.ColliderType == eColliderType._3D)
                                        {
                                            int collVertexIdx = m_meshCollVertices.Count;
                                            m_meshCollVertices.Add(new Vector3(s0.x, s0.y, -halvedCollDepth));
                                            m_meshCollVertices.Add(new Vector3(s0.x, s0.y, halvedCollDepth));
                                            m_meshCollVertices.Add(new Vector3(s1.x, s1.y, halvedCollDepth));
                                            m_meshCollVertices.Add(new Vector3(s1.x, s1.y, -halvedCollDepth));

                                            m_meshCollTriangles.Add(collVertexIdx + 0);
                                            m_meshCollTriangles.Add(collVertexIdx + 1);
                                            m_meshCollTriangles.Add(collVertexIdx + 2);
                                            m_meshCollTriangles.Add(collVertexIdx + 2);
                                            m_meshCollTriangles.Add(collVertexIdx + 3);
                                            m_meshCollTriangles.Add(collVertexIdx + 0);
                                        }
                                        else //if( ParentTilemap.ColliderType == eColliderType._2D )
                                        {
                                            int linkedSegments = 0;
                                            int segmentIdxToMerge = -1;                                            
                                            for (int edgeIdx = openEdges.Count - 1; edgeIdx >= 0 && linkedSegments < 2; --edgeIdx)
                                            {
                                                LinkedList<Vector2> edgeSegments = openEdges[edgeIdx];
                                                if (edgeSegments.First.Value == edgeSegments.Last.Value) 
                                                    continue; //skip closed edges
                                                if( edgeSegments.Last.Value == s0 )
                                                {
                                                    if (segmentIdxToMerge >= 0)
                                                    {
                                                        LinkedList<Vector2> segmentToMerge = openEdges[segmentIdxToMerge];
                                                        if (s0 == segmentToMerge.First.Value)
                                                            for (LinkedListNode<Vector2> node = segmentToMerge.First.Next; node != null; node = node.Next)
                                                                edgeSegments.AddLast(node.Value);
                                                        else
                                                            for (LinkedListNode<Vector2> node = segmentToMerge.Last.Previous; node != null; node = node.Previous)
                                                                edgeSegments.AddLast(node.Value);
                                                        openEdges.RemoveAt(segmentIdxToMerge);
                                                    }
                                                    else
                                                    {
                                                        segmentIdxToMerge = edgeIdx;
                                                        edgeSegments.AddLast(s1);
                                                    }
                                                    ++linkedSegments;
                                                }
                                                else if( edgeSegments.Last.Value == s1 )
                                                {
                                                    if (segmentIdxToMerge >= 0)
                                                    {
                                                        LinkedList<Vector2> segmentToMerge = openEdges[segmentIdxToMerge];
                                                        if (s1 == segmentToMerge.First.Value)
                                                            for (LinkedListNode<Vector2> node = segmentToMerge.First.Next; node != null; node = node.Next)
                                                                edgeSegments.AddLast(node.Value);
                                                        else
                                                            for (LinkedListNode<Vector2> node = segmentToMerge.Last.Previous; node != null; node = node.Previous)
                                                                edgeSegments.AddLast(node.Value);
                                                        openEdges.RemoveAt(segmentIdxToMerge);
                                                    }
                                                    else
                                                    {
                                                        segmentIdxToMerge = edgeIdx;
                                                        edgeSegments.AddLast(s0);
                                                    }
                                                    ++linkedSegments;
                                                }
                                                else if (edgeSegments.First.Value == s0)
                                                {
                                                    if (segmentIdxToMerge >= 0)
                                                    {
                                                        LinkedList<Vector2> segmentToMerge = openEdges[segmentIdxToMerge];
                                                        if (s0 == segmentToMerge.Last.Value)
                                                            for (LinkedListNode<Vector2> node = edgeSegments.First.Next; node != null; node = node.Next)
                                                                segmentToMerge.AddLast(node.Value);
                                                        else
                                                            for (LinkedListNode<Vector2> node = edgeSegments.Last.Previous; node != null; node = node.Previous)
                                                                segmentToMerge.AddLast(node.Value);
                                                        openEdges.RemoveAt(edgeIdx);
                                                    }
                                                    else
                                                    {
                                                        segmentIdxToMerge = edgeIdx;
                                                        edgeSegments.AddFirst(s1);
                                                    }
                                                    ++linkedSegments;
                                                }
                                                else if (edgeSegments.First.Value == s1)
                                                {
                                                    if (segmentIdxToMerge >= 0)
                                                    {
                                                        LinkedList<Vector2> segmentToMerge = openEdges[segmentIdxToMerge];
                                                        if (s1 == segmentToMerge.Last.Value)
                                                            for (LinkedListNode<Vector2> node = edgeSegments.First.Next; node != null; node = node.Next)
                                                                segmentToMerge.AddLast(node.Value);
                                                        else
                                                            for (LinkedListNode<Vector2> node = edgeSegments.Last.Previous; node != null; node = node.Previous)
                                                                segmentToMerge.AddLast(node.Value);
                                                        openEdges.RemoveAt(edgeIdx);
                                                    }
                                                    else
                                                    {
                                                        segmentIdxToMerge = edgeIdx;
                                                        edgeSegments.AddFirst(s0);
                                                    }
                                                    ++linkedSegments;
                                                }                                                
                                            }                                            

                                            if (linkedSegments == 0)
                                            {
                                                LinkedList<Vector2> newEdge = new LinkedList<Vector2>();
                                                newEdge.AddFirst(s0);
                                                newEdge.AddLast(s1);
                                                openEdges.Add(newEdge);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (ParentTilemap.ColliderType == eColliderType._2D)
            {
                //+++ Process Edges
                //(NOTE: this was added to fix issues related with lighting, otherwise leave this commented)                
                {
                    // Remove vertex inside a line
                    RemoveRedundantVertices(openEdges);

                    // Split segments (NOTE: This is not working with polygon colliders)
                    /*/ commented unless necessary for performance reasons
                    if (ParentTilemap.Collider2DType == e2DColliderType.EdgeCollider2D)
                    {
                        openEdges = SplitSegments(openEdges);
                    }
                    //*/
                }
                //---

                //Create Edges
                for (int i = 0; i < openEdges.Count; ++i)
                {
                    LinkedList<Vector2> edgeSegments = openEdges[i];
                    bool reuseCollider = i < aColliders2D.Length;
                    Collider2D collider2D = reuseCollider ? (Collider2D)aColliders2D[i] : (Collider2D)gameObject.AddComponent(collider2DType);
                    collider2D.enabled = true;
                    collider2D.isTrigger = ParentTilemap.IsTrigger;
                    collider2D.sharedMaterial = ParentTilemap.PhysicMaterial2D;
                    if (ParentTilemap.Collider2DType == e2DColliderType.EdgeCollider2D)
                    {
                        ((EdgeCollider2D)collider2D).points = edgeSegments.ToArray();
                    }
                    else
                    {
                        ((PolygonCollider2D)collider2D).points = edgeSegments.ToArray();
                    }
                }

                //Destroy unused edge colliders
                for (int i = openEdges.Count; i < aColliders2D.Length; ++i)
                {
                    if (!s_isOnValidate)
                        DestroyImmediate(aColliders2D[i]);
                    else
                        ((Collider2D)aColliders2D[i]).enabled = false;
                }
            }

            return !isEmpty;
        }

        void RemoveRedundantVertices(List<LinkedList<Vector2>> edgeList)
        {
            for (int i = 0; i < edgeList.Count; ++i)
            {
                RemoveRedundantVertices(edgeList[i]);
            }
        }
        void RemoveRedundantVertices(LinkedList<Vector2> edgeVertices)
        {
            LinkedListNode<Vector2> iter = edgeVertices.First;
            while (iter != edgeVertices.Last)
            {
                float perpDot;
                //Special case for first node if this is a closed edge
                if (iter == edgeVertices.First)
                {
                    if (iter.Value == edgeVertices.Last.Value)
                    {
                        perpDot = PerpDot(iter.Value, edgeVertices.Last.Previous.Value, iter.Next.Value);
                        if (Mathf.Abs(perpDot) <= Vector2.kEpsilon)
                        {
                            edgeVertices.RemoveFirst();
                            edgeVertices.Last.Value = edgeVertices.First.Value;
                            iter = edgeVertices.First;
                        }
                        else
                        {
                            iter = iter.Next;
                        }
                    }
                    else
                    {
                        iter = iter.Next;
                    }
                }
                else
                {
                    perpDot = PerpDot(iter.Value, iter.Previous.Value, iter.Next.Value);
                    iter = iter.Next;
                    if (Mathf.Abs(perpDot) <= Vector2.kEpsilon)
                    {
                        edgeVertices.Remove(iter.Previous);
                    }
                }
            }
        }

        List<LinkedList<Vector2>> SplitSegments(List<LinkedList<Vector2>> edges)
        {
            List<LinkedList<Vector2>> separatedSegmentList = new List<LinkedList<Vector2>>();
            for (int i = 0; i < edges.Count; ++i)
            {
                LinkedList<Vector2> edgeVertices = edges[i];
                LinkedListNode<Vector2> iter = edgeVertices.First;
                while (iter.Next != null)
                {
                    LinkedList<Vector2> segment = new LinkedList<Vector2>();
                    segment.AddFirst(iter.Value);
                    segment.AddLast(iter.Next.Value);
                    separatedSegmentList.Add(segment);
                    iter = iter.Next;
                }
            }
            return separatedSegmentList;
        }

        float PerpDot(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v0 = a - p;
            Vector2 v1 = b - p;
            return PerpDot(v0, v1);
        }

        float PerpDot(Vector2 v0, Vector2 v1)
        {
            return v0.x * v1.y - v0.y * v1.x;
        }
    }
}