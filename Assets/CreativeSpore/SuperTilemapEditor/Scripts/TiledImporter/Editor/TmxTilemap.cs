using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace CreativeSpore.TiledImporter
{
    public class TmxTilemap
    {

        public string FilePathDirectory { get; protected set; }
        public Dictionary<int, Texture2D> DicTilesetTex2D { get { return m_dicTilesetTex2d; } }
        public TmxMap Map { get { return m_map; } }

        TmxMap m_map;

        Dictionary<int, Texture2D> m_dicTilesetTex2d = new Dictionary<int,Texture2D>();

      
        private TmxTilemap( TmxMap map )
        {
            m_map = map;
        }

        private void LoadImageData()
        {
            foreach( TmxTileset tileset in m_map.Tilesets )
            {
                if (tileset.Image == null)
                {
                    Debug.LogWarning("No texture found for tileset " + tileset.Name);
                }
                else
                {
                    Texture2D texture = new Texture2D(tileset.Image.Width, tileset.Image.Height, TextureFormat.ARGB32, false, false);
                    texture.filterMode = FilterMode.Point;
                    texture.name = tileset.Name;
                    texture.LoadImage(File.ReadAllBytes(Path.Combine(FilePathDirectory, tileset.Image.Source)));
                    texture.hideFlags = HideFlags.DontSave;
                    m_dicTilesetTex2d.Add(
                        tileset.FirstGId,
                        texture
                    );
                }
            }
        }        

        public TmxTileset FindTileset( TmxLayerTile tile )
        {
            // Value 0 means no tileset associated
            if (tile.GId == 0) return null;

            int tilesetIdx;
            for (tilesetIdx = 0; tilesetIdx < m_map.Tilesets.Count; tilesetIdx++ )
            {
                TmxTileset tileset = m_map.Tilesets[tilesetIdx];
                if (tileset.FirstGId > tile.GId)
                {
                    break;
                }
            }
            tilesetIdx--;

            return m_map.Tilesets[tilesetIdx];
        }

        public int GetTileAbsoluteId(TmxLayerTile tile)
        {
            // Value 0 means no tileset associated
            if (tile.GId == 0) return -1;

            int tileGlobalId = (int)(tile.GId & 0x1FFFFFFF); // remove flip flags

            int tileAbsoluteId = 0;
            for (int tilesetIdx = 0; tilesetIdx < m_map.Tilesets.Count; tilesetIdx++)
            {
                TmxTileset tileset = m_map.Tilesets[tilesetIdx];
                if (tileGlobalId >= (tileset.FirstGId + tileset.TileCount))
                {
                    tileAbsoluteId += tileset.TileCount;
                }
                else
                {
                    tileAbsoluteId += (tileGlobalId - tileset.FirstGId);
                    break;
                }
            }

            return tileAbsoluteId;
        }

        public static TmxTilemap LoadFromFile(string sFilePath)
        {
            XMLSerializer objSerializer = new XMLSerializer();
            TmxMap map = objSerializer.LoadFromXMLFile<TmxMap>(sFilePath);
            map.FixExportedTilesets(Path.GetDirectoryName(sFilePath));

            TmxTilemap tilemap_ret = new TmxTilemap(map);

            tilemap_ret.FilePathDirectory = Path.GetDirectoryName(sFilePath);

            tilemap_ret.LoadImageData();

            return tilemap_ret;
        }

        static int s_sortingOrder = 0;
        public void ImportIntoScene()
        {
            foreach (TmxLayer layer in m_map.Layers)
            {
                ImportIntoScene(layer);
                s_sortingOrder++;
            }
        }

        void ImportIntoScene(TmxLayer layer)
        {
            GameObject layerObj = new GameObject("L:" + layer.Name);            
            for (int tile_x = 0; tile_x < layer.Width; tile_x++)
                for (int tile_y = 0; tile_y < layer.Height; tile_y++)
                {
                    int tileIdx = tile_y * layer.Width + tile_x;
                    TmxLayerTile tile = layer.Tiles[tileIdx];

                    //skip non valid tiles
                    if (tile.GId == 0) continue;
                    int tileGlobalId = (int)(tile.GId & 0x1FFFFFFF); // remove flip flags

                    TmxTileset objTileset = FindTileset(tile);

                    // Draw Tile
                    Texture2D texTileset = m_dicTilesetTex2d[objTileset.FirstGId];
                    Rect dstRect = new Rect(tile_x * objTileset.TileWidth, (layer.Height - 1 - tile_y) * objTileset.TileHeight, objTileset.TileWidth, objTileset.TileHeight);

                    int tileBaseIdx = tileGlobalId - objTileset.FirstGId;
                    int scanLine = objTileset.Image.Width / objTileset.TileWidth;
                    int tileset_x = tileBaseIdx % scanLine;
                    int tileset_y = tileBaseIdx / scanLine;
                    Rect srcRect = new Rect(tileset_x * objTileset.TileWidth, texTileset.height - (tileset_y + 1) * objTileset.TileHeight, objTileset.TileWidth, objTileset.TileHeight);

                    GameObject tileObj = new GameObject("tile" + tile_x + "_" + tile_y);
                    tileObj.transform.SetParent(layerObj.transform);
                    SpriteRenderer tileRenderer = tileObj.AddComponent<SpriteRenderer>();
                    tileRenderer.sortingOrder = s_sortingOrder;
                    tileRenderer.sprite = Sprite.Create(texTileset, srcRect, Vector2.zero);
                    tileObj.transform.localPosition = dstRect.position / 100f;
                }

        }
    }
}
