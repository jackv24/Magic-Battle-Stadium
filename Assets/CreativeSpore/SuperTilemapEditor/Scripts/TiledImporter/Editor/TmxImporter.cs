using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using CreativeSpore.SuperTilemapEditor;

namespace CreativeSpore.TiledImporter
{    
    public static class TmxImporter
    {

        const string k_keyLastOpenFilePanelPath = "TmxImporter/LastOpenFilePanelPath";

        const uint k_FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint k_FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint k_FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        [MenuItem("Assets/Create/SuperTilemapEditor/Tiled/Tileset (from TMX File)")]
        public static void CreateTilesetFromTmx()
        {
            string path = PlayerPrefs.GetString(k_keyLastOpenFilePanelPath);
            string tmxFilePath = EditorUtility.OpenFilePanel("Create Tileset From TMX File", path, "tmx");
            if (!string.IsNullOrEmpty(tmxFilePath))
            {
                PlayerPrefs.SetString(k_keyLastOpenFilePanelPath, Path.GetDirectoryName(tmxFilePath));
                TmxTilemap tilemap = TmxTilemap.LoadFromFile(tmxFilePath);
                if (tilemap.DicTilesetTex2D.Values.Count == 0)
                {
                    Debug.LogError("Not tileset found!");
                    return;
                }

                CreateTilesetFromTmx(tmxFilePath, EditorUtils.GetAssetSelectedPath());
            }
        }

        public static Tileset CreateTilesetFromTmx(string tmxFilePath, string dstPath)
        {
            string tmxFileName = Path.GetFileNameWithoutExtension(tmxFilePath);
            TmxTilemap tilemap = TmxTilemap.LoadFromFile(tmxFilePath);
            if (tilemap.DicTilesetTex2D.Values.Count == 0)
            {
                return null;
            }

            //NOTE: calling this after PackTextures will make the atlasTexture to be null sometimes
            Tileset tilesetAsset = ScriptableObject.CreateInstance<Tileset>();
            AssetDatabase.CreateAsset(tilesetAsset, Path.Combine(dstPath, tmxFileName + "Tileset.asset"));
            Texture2D atlasTexture;
            Rect[] tilesetRects;
            if (tilemap.DicTilesetTex2D.Values.Count == 1)
            {
                atlasTexture = tilemap.DicTilesetTex2D.Values.ToArray()[0];
                tilesetRects = new Rect[] { new Rect(0, 0, atlasTexture.width, atlasTexture.height) };
            }
            else
            {
                atlasTexture = new Texture2D(8192, 8192, TextureFormat.ARGB32, false, false);
                tilesetRects = atlasTexture.PackTextures(tilemap.DicTilesetTex2D.Values.ToArray(), 0);
            }
            string atlasPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(tilesetAsset)) + "/" + tmxFileName + "Atlas.png";
            AssetDatabase.CreateAsset(atlasTexture, atlasPath);
            File.WriteAllBytes(atlasPath, atlasTexture.EncodeToPNG());            
            ImportTexture(atlasPath);
            AssetDatabase.Refresh();            

            // Link Atlas with asset to be able to save it in the prefab
            tilesetAsset.AtlasTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(Texture2D));

            tilesetAsset.TilePxSize = new Vector2(tilemap.Map.Tilesets[0].TileWidth, tilemap.Map.Tilesets[0].TileHeight);
            int tilesetIdx = 0;
            List<Tile> tileList = new List<Tile>();
            foreach (Texture2D tilesetTexture in tilemap.DicTilesetTex2D.Values)
            {
                TmxTileset tmxTileset = tilemap.Map.Tilesets[tilesetIdx];
                Vector2 tileSize = new Vector2(tilemap.Map.Tilesets[tilesetIdx].TileWidth, tilemap.Map.Tilesets[tilesetIdx].TileHeight);
                Rect[] tileRects = GenerateGridSpriteRectangles(tilesetTexture, new Vector2(tmxTileset.Margin, tmxTileset.Margin), tileSize, new Vector2(tmxTileset.Spacing, tmxTileset.Spacing));
                TileSelection tilesetSelection = new TileSelection(Enumerable.Range(tileList.Count, tileRects.Length).Select(i => (uint)i).ToList(), tmxTileset.Columns);
                foreach (Rect tileRect in tileRects)
                {
                    Rect uv = tileRect;
                    uv.xMin /= tilesetAsset.AtlasTexture.width;
                    uv.xMax /= tilesetAsset.AtlasTexture.width;
                    uv.yMin /= tilesetAsset.AtlasTexture.height;
                    uv.yMax /= tilesetAsset.AtlasTexture.height;
                    uv.position += tilesetRects[tilesetIdx].position;
                    tileList.Add(new Tile() { uv = uv });
                }
                tilesetIdx++;
                tilesetAsset.TileViews.Add(new TileView(tilesetTexture.name, tilesetSelection));
            }
            tilesetAsset.SetTiles(tileList);
            return tilesetAsset;
        }

        public static void ImportTmxIntoTheScene(Tileset tileset)
        {
            string path = PlayerPrefs.GetString(k_keyLastOpenFilePanelPath);
            string tmxFilePath = EditorUtility.OpenFilePanel("Import TMX into the Scene", path, "tmx");
            if (!string.IsNullOrEmpty(tmxFilePath))
            {
                PlayerPrefs.SetString(k_keyLastOpenFilePanelPath, Path.GetDirectoryName(tmxFilePath));
                CreativeSpore.TiledImporter.TmxImporter.ImportTmxIntoTheScene(tmxFilePath, tileset);
            }
        }

        public static void ImportTmxIntoTheScene(string tmxFilePath, Tileset tileset )
        {
            string tmxFileName = Path.GetFileNameWithoutExtension(tmxFilePath);
            TmxTilemap tilemap = TmxTilemap.LoadFromFile(tmxFilePath);
            if (tilemap.DicTilesetTex2D.Values.Count == 0)
            {
                return;
            }

            GameObject tilemapGroupObj = new GameObject(tmxFileName);
            TilemapGroup tilemapGroup = tilemapGroupObj.AddComponent<TilemapGroup>();
            int orderInLayer = 0;
            foreach (TmxLayer layer in tilemap.Map.Layers)
            {
                GameObject tilemapObj = new GameObject(layer.Name);
                tilemapObj.transform.SetParent(tilemapGroupObj.transform);
                Tilemap tilemapBhv = tilemapObj.AddComponent<Tilemap>();
                tilemapBhv.Tileset = tileset;
                tilemapBhv.OrderInLayer = orderInLayer++;
                tilemapBhv.IsVisible =layer.Visible;
                tilemapBhv.TintColor = new Color(1f, 1f, 1f, layer.Opacity);
                for (int gx = 0; gx < layer.Width; gx++)
                    for (int gy = 0; gy < layer.Height; gy++)
                    {
                        int tileIdx = gy * layer.Width + gx;
                        TmxLayerTile tile = layer.Tiles[tileIdx];

                        //skip non valid tiles
                        if (tile.GId == 0) continue;

                        int tileId = tilemap.GetTileAbsoluteId(tile);
                        uint tileData = tileId >= 0 ? (uint)tileId : Tileset.k_TileData_Empty;
                        if (tileData != Tileset.k_TileData_Empty)
                        {
                            // add tile flags
                            if( (tile.GId & k_FLIPPED_HORIZONTALLY_FLAG) != 0 ) tileData |= Tileset.k_TileFlag_FlipH;
                            if ((tile.GId & k_FLIPPED_VERTICALLY_FLAG) != 0) tileData |= Tileset.k_TileFlag_FlipV;
                            if ((tile.GId & k_FLIPPED_DIAGONALLY_FLAG) != 0) tileData |= Tileset.k_TileFlag_Rot90;
                            //convert from tiled flip diagonal to rot90
                            if((tileData & Tileset.k_TileFlag_Rot90) != 0)
                            {
                                if (((tile.GId & k_FLIPPED_HORIZONTALLY_FLAG) != 0) != ((tile.GId & k_FLIPPED_VERTICALLY_FLAG) != 0))
                                {
                                    tileData ^= Tileset.k_TileFlag_FlipH;
                                }
                                else
                                {
                                    tileData ^= Tileset.k_TileFlag_FlipV;
                                }
                            }
                        }
                        tilemapBhv.SetTileData(gx, layer.Height - gy - 1, tileData);
                    }
                tilemapBhv.UpdateMesh();
            }
            tilemapGroup.Refresh();
        }

        public static Rect[] GenerateGridSpriteRectangles(Texture2D texture, Vector2 offset, Vector2 size, Vector2 padding)
        {
            List<Rect> rects = new List<Rect>();
            if (texture != null)
            {
                int uInc = Mathf.RoundToInt(size.x + padding.x);
                int vInc = Mathf.RoundToInt(size.y + padding.y);
                if (uInc > 0 && vInc > 0)
                {
                    for (int v = Mathf.RoundToInt(offset.y); v + size.y <= texture.height; v += vInc)
                    {
                        for (int u = Mathf.RoundToInt(offset.x); u + size.x <= texture.width; u += uInc)
                        {
                            rects.Add(new Rect(new Vector2((float)u, (float)(texture.height - v - size.y)), size));
                        }
                    }              
                }
                else
                {
                    Debug.LogWarning(" Error while slicing. There is something wrong with slicing parameters. uInc = " + uInc + "; vInc = " + vInc);
                }
            }
            return rects.ToArray();
        }

        /// <summary>
        /// Import the texture making sure the texture import settings are properly set
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static bool ImportTexture(Texture2D texture)
        {
			if( texture != null )
			{
				return ImportTexture( AssetDatabase.GetAssetPath(texture) );
			}
            return false;
        }   

        /// <summary>
        /// Import the texture making sure the texture import settings are properly set
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ImportTexture(string path)
        {
			if( path.Length > 0 )
			{
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
				if( textureImporter )
				{
                    textureImporter.alphaIsTransparency = true; // default
                    textureImporter.anisoLevel = 1; // default
                    textureImporter.borderMipmap = false; // default
                    textureImporter.mipmapEnabled = false; // default
                    textureImporter.compressionQuality = 100;
					//textureImporter.isReadable = true;
					textureImporter.spritePixelsPerUnit = 100;                    
					textureImporter.spriteImportMode = SpriteImportMode.Single;
					textureImporter.wrapMode = TextureWrapMode.Clamp;
					textureImporter.filterMode = FilterMode.Point;
					textureImporter.textureFormat = TextureImporterFormat.ARGB32;
                    textureImporter.textureType = TextureImporterType.Sprite;
					textureImporter.maxTextureSize = 8192;                    
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); 
				}
				return true;
			}
            return false;
        }
    }
}