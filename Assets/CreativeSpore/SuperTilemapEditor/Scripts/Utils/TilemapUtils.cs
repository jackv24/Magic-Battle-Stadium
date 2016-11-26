using UnityEngine;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor
{

    public class TileData
    {
        public bool flipVertical;
        public bool flipHorizontal;
        public bool rot90;
        public int brushId;
        public int tileId;

        public uint Value { get { return BuildData(); } } // for debugging

        /// <summary>
        /// This is true when tileData has the special value 0xFFFFFFFF, meaning the tile will not be drawn
        /// </summary>
        public bool IsEmpty { get { return brushId == Tileset.k_BrushId_Default && tileId == Tileset.k_TileId_Empty; } }

        public TileData()
        {
            SetData(0x0000FFFF);
        }

        public TileData(uint tileData)
        {
            SetData(tileData);
        }

        /// <summary>
        /// Set data by providing a tileData value ( ex: SetData( Tilemap.GetTileData(12, 35) ) )
        /// </summary>
        /// <param name="tileData"></param>
        public void SetData(uint tileData)
        {
            flipVertical = (tileData & Tileset.k_TileFlag_FlipV) != 0;
            flipHorizontal = (tileData & Tileset.k_TileFlag_FlipH) != 0;
            rot90 = (tileData & Tileset.k_TileFlag_Rot90) != 0;
            brushId = tileData != Tileset.k_TileData_Empty ? (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16) : 0;
            tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
        }

        /// <summary>
        /// Build the tile data using current parameters
        /// </summary>
        /// <returns></returns>
        public uint BuildData()
        {
            if( IsEmpty )
            {
                return Tileset.k_TileData_Empty;
            }
            uint tileData = 0;
            if(flipVertical) tileData |= Tileset.k_TileFlag_FlipV;
            if (flipHorizontal) tileData |= Tileset.k_TileFlag_FlipH;
            if (rot90) tileData |= Tileset.k_TileFlag_Rot90;
            tileData |= ( (uint)brushId << 16 ) & Tileset.k_TileDataMask_BrushId;
            tileData |= (uint)tileId & Tileset.k_TileDataMask_TileId;
            return tileData;
        }
    }

    public static class TilemapUtils
    {
        /// <summary>
        /// Get the world position for the center of a given grid cell position.
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        static public Vector3 GetGridWorldPos( Tilemap tilemap, int gridX, int gridY)
        {
            return tilemap.transform.TransformPoint(new Vector2((gridX + .5f) * tilemap.CellSize.x, (gridY + .5f) * tilemap.CellSize.y));
        }

        static public Vector3 GetGridWorldPos(int gridX, int gridY, Vector2 cellSize)
        {
            return new Vector2((gridX + .5f) * cellSize.x, (gridY + .5f) * cellSize.y);
        }

        /// <summary>
        /// Gets the grid X position for a given tilemap and local position. To convert from world to local position use tilemap.transform.InverseTransformPoint(worldPosition).
        /// Avoid using positions multiple of cellSize like 0.32f if cellSize = 0.16f because due float imprecisions the return value could be wrong.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="locPosition"></param>
        /// <returns></returns>
        static public int GetGridX( Tilemap tilemap, Vector2 locPosition)
        {
            return BrushUtil.GetGridX(locPosition, tilemap.CellSize);
        }

        /// <summary>
        /// Gets the grid Y position for a given tilemap and local position. To convert from world to local position use tilemap.transform.InverseTransformPoint(worldPosition).
        /// Avoid using positions multiple of cellSize like 0.32f if cellSize = 0.16f because due float imprecisions the return value could be wrong.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="locPosition"></param>
        /// <returns></returns>
        static public int GetGridY(Tilemap tilemap, Vector2 locPosition)
        {
            return BrushUtil.GetGridY(locPosition, tilemap.CellSize);
        }

        /// <summary>
        /// Gets the grid X position for a given tilemap and camera where the mouse is over.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        static public int GetMouseGridX(Tilemap tilemap, Camera camera)
        {
            Vector2 locPos = camera.ScreenToWorldPoint(Input.mousePosition);
            return GetGridX(tilemap, tilemap.transform.InverseTransformPoint(locPos));
        }

        /// <summary>
        /// /// Gets the grid X position for a given tilemap and camera where the mouse is over.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        static public int GetMouseGridY(Tilemap tilemap, Camera camera)
        {
            Vector2 locPos = camera.ScreenToWorldPoint(Input.mousePosition);
            return GetGridY(tilemap, tilemap.transform.InverseTransformPoint(locPos));
        }

    }
}
