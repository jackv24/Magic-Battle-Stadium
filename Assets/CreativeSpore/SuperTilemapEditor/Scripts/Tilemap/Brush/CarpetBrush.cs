using UnityEngine;
using System.Collections;
using System.Linq;

namespace CreativeSpore.SuperTilemapEditor
{
    public class CarpetBrush : RoadBrush
    {
        public uint[] InteriorCornerTileIds = Enumerable.Repeat(Tileset.k_TileData_Empty, 4).ToArray();

        #region IBrush

        public override uint PreviewTileData()
        {
            return TileIds[6];
            //return TileIds[15] != Tileset.k_TileId_Empty ? TileIds[15] : TileIds[6]; //15 center brush (╬) ; 6 top left brush (╔)
        }

        static int s_brushId;
        static int s_neighIdx;
        static uint s_tileData;
        static bool[] s_showDiagonal = new bool[4];
        static bool s_needsSubTiles;

        private void CalculateNeighbourData(Tilemap tilemap, int gridX, int gridY, uint tileData)
        {
            s_needsSubTiles = false;
            s_brushId = (int)((tileData & Tileset.k_TileDataMask_BrushId) >> 16);
            bool autotiling_N = AutotileWith(tilemap, s_brushId, gridX, gridY + 1);
            bool autotiling_E = AutotileWith(tilemap, s_brushId, gridX + 1, gridY);
            bool autotiling_S = AutotileWith(tilemap, s_brushId, gridX, gridY - 1);
            bool autotiling_W = AutotileWith(tilemap, s_brushId, gridX - 1, gridY);
            s_neighIdx = 0;
            if (autotiling_N) s_neighIdx |= 1;
            if (autotiling_E) s_neighIdx |= 2;
            if (autotiling_S) s_neighIdx |= 4;
            if (autotiling_W) s_neighIdx |= 8;

            s_needsSubTiles = (s_neighIdx == 0 || s_neighIdx == 1 || s_neighIdx == 2 || s_neighIdx == 4
            || s_neighIdx == 5 || s_neighIdx == 8 || s_neighIdx == 10) ;
            
            // diagonals
            {
                bool autotiling_NE = AutotileWith(tilemap, s_brushId, gridX + 1, gridY + 1);
                bool autotiling_SE = AutotileWith(tilemap, s_brushId, gridX + 1, gridY - 1);
                bool autotiling_SW = AutotileWith(tilemap, s_brushId, gridX - 1, gridY - 1);
                bool autotiling_NW = AutotileWith(tilemap, s_brushId, gridX - 1, gridY + 1);

                s_showDiagonal[0] = !autotiling_SW && autotiling_S && autotiling_W;
                s_showDiagonal[1] = !autotiling_SE && autotiling_S && autotiling_E;
                s_showDiagonal[2] = !autotiling_NW && autotiling_N && autotiling_W;
                s_showDiagonal[3] = !autotiling_NE && autotiling_N && autotiling_E;

                s_tileData = TileIds[s_neighIdx];
                bool foundTrueDiagonal = false;
                for (int i = 0; !s_needsSubTiles && i < s_showDiagonal.Length; ++i)
                {
                    if (s_showDiagonal[i])
                    {
                        // if only a diagonal is true and it's surrounded by tiles s_neighIdx == 15, we don't need subtiles, instead the right corner tile will be taken
                        s_needsSubTiles = foundTrueDiagonal || s_neighIdx != 15;
                        foundTrueDiagonal = true;
                        if (!s_needsSubTiles)
                        {
                            s_tileData = InteriorCornerTileIds[InteriorCornerTileIds.Length - i - 1];
                        }
                    }
                }                
            }
        }

        public override uint Refresh(Tilemap tilemap, int gridX, int gridY, uint tileData)
        {
            CalculateNeighbourData(tilemap, gridX, gridY, tileData);


            uint brushTileData = RefreshLinkedBrush(tilemap, gridX, gridY, s_tileData);
            // overwrite flags
            brushTileData &= ~Tileset.k_TileDataMask_Flags;
            brushTileData |= s_tileData & Tileset.k_TileDataMask_Flags;
            // overwrite brush id
            brushTileData &= ~Tileset.k_TileDataMask_BrushId;
            brushTileData |= tileData & Tileset.k_TileDataMask_BrushId;   
            return brushTileData;
        }

        // '°', '├', '═', '┤', | 0, 2, 10, 8,
        // '┬', '╔', '╦', '╗', | 4, 6, 14, 12,
        // '║', '╠', '╬', '╣', | 5, 7, 15, 13,
        // '┴', '╚', '╩', '╝', | 1, 3, 11, 9,
        public override uint[] GetSubtiles(Tilemap tilemap, int gridX, int gridY, uint tileData)
        {
            CalculateNeighbourData(tilemap, gridX, gridY, tileData);
            // tiles that need subtile division
            if (s_needsSubTiles)
            {
                uint[] aSubTileData = null;

                if (s_neighIdx == 0) //°
                {
                    aSubTileData = new uint[] { TileIds[3], TileIds[9], TileIds[6], TileIds[12] };
                }
                else if (s_neighIdx == 4)//┬
                {
                    aSubTileData = new uint[] { TileIds[6], TileIds[12], TileIds[6], TileIds[12] };
                }
                else if (s_neighIdx == 5)//║
                {
                    aSubTileData = new uint[] { TileIds[7], TileIds[13], TileIds[7], TileIds[13] };
                }
                else if (s_neighIdx == 1)//┴
                {
                    aSubTileData = new uint[] { TileIds[3], TileIds[9], TileIds[3], TileIds[9] };
                }
                else if (s_neighIdx == 2)//├
                {
                    aSubTileData = new uint[] { TileIds[3], TileIds[3], TileIds[6], TileIds[6] };
                }
                else if (s_neighIdx == 10)//═
                {
                    aSubTileData = new uint[] { TileIds[11], TileIds[11], TileIds[14], TileIds[14] };
                }
                else if (s_neighIdx == 8)//┤
                {
                    aSubTileData = new uint[] { TileIds[9], TileIds[9], TileIds[12], TileIds[12] };
                }
                // NOTE: this case '╬' cut the tiles different (using corner tiles). 
                // If it is commented, and default case is used, instead or corner tiles, it will use the center tile '╬'
                // Depending on the graphics it could be interesting add a check box to choose between using this or not.
                else if (s_neighIdx == 15)// ╬
                {
                    aSubTileData = new uint[] { InteriorCornerTileIds[0], InteriorCornerTileIds[1], InteriorCornerTileIds[2], InteriorCornerTileIds[3] };
                }
                else
                {
                    aSubTileData = new uint[] { TileIds[s_neighIdx], TileIds[s_neighIdx], TileIds[s_neighIdx], TileIds[s_neighIdx] };
                }

                for (int i = 0; i < s_showDiagonal.Length; ++i)
                {
                    if (s_showDiagonal[i])
                    {
                        aSubTileData[i] = InteriorCornerTileIds[3 - i];
                    }
                    // Add animated tiles
                    {
                        TilesetBrush brush = Tileset.FindBrush(Tileset.GetBrushIdFromTileData(aSubTileData[i]));
                        if (brush && brush.IsAnimated())
                        {
                            TilemapChunk.RegisterAnimatedBrush(brush, i);
                        }
                    }
                }

                return aSubTileData;
            }

            // Add animated tiles
            {
                TilesetBrush brush = Tileset.FindBrush(Tileset.GetBrushIdFromTileData(s_tileData));
                if (brush && brush.IsAnimated())
                {
                    TilemapChunk.RegisterAnimatedBrush(brush);
                }
            }
            return null;
        }

        #endregion
    }
}