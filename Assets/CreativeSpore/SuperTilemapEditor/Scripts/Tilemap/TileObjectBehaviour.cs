using UnityEngine;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor
{
    /// <summary>
    /// Attached to a gameobject used as tile prefab, it will change the sprite renderer to display the tile that has instantiated the prefab
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteInEditMode] //fix ShouldRunBehaviour warning when using OnTilePrefabCreation
    public class TileObjectBehaviour : MonoBehaviour 
    {
        void OnTilePrefabCreation(TilemapChunk.OnTilePrefabCreationData data)
        {
            Tile tile = data.ParentTilemap.GetTile(data.GridX, data.GridY);
            if (tile != null)
            {
                float pixelsPerUnit = data.ParentTilemap.Tileset.TilePxSize.x / data.ParentTilemap.CellSize.x;
                Vector2 atlasSize = new Vector2(data.ParentTilemap.Tileset.AtlasTexture.width, data.ParentTilemap.Tileset.AtlasTexture.height);
                Rect spriteUV = new Rect( Vector2.Scale(tile.uv.position, atlasSize), Vector2.Scale(tile.uv.size, atlasSize));
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = Sprite.Create(data.ParentTilemap.Tileset.AtlasTexture, spriteUV, new Vector2(.5f, .5f), pixelsPerUnit);
                spriteRenderer.sortingLayerID = data.ParentTilemap.SortingLayerID;
                spriteRenderer.sortingOrder = data.ParentTilemap.OrderInLayer;
            }
        }
    }
}
