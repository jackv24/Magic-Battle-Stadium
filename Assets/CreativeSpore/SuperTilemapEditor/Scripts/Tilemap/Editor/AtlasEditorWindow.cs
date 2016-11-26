using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CreativeSpore.SuperTilemapEditor
{
    public class AtlasEditorWindow : EditorWindow 
    {
        private Tileset m_tileset;
        private Texture2D m_atlasTexture;
        private Vector2 m_tileSize;
        private Vector2 m_slicePadding;
        private Vector2 m_sliceOffset;
        private int m_extrude;
        private int m_padding;
        private Texture2D m_previewTexture;

        [MenuItem("SuperTilemapEditor/Window/Atlas Editor Window")]
        public static void Display()
        {
            AtlasEditorWindow wnd = (AtlasEditorWindow)EditorWindow.GetWindow(typeof(AtlasEditorWindow), false, "Atlas Editor", true);
            wnd.minSize = new Vector2(337f, 314f);
        }

        private Vector2 m_scrollPos;
        void OnGUI()
        {
            bool sliceTileset = false;
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Atlas Texture:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    EditorGUI.BeginChangeCheck();
                    m_tileset = (Tileset)EditorGUILayout.ObjectField("Tileset (optional)", m_tileset, typeof(Tileset), false);
                    if (EditorGUI.EndChangeCheck() && m_tileset)
                    {
                        m_padding = (int)Mathf.Max(m_slicePadding.x, m_slicePadding.y);
                        m_extrude = 0;
                    }
                    // Read Data From Tileset
                    if (m_tileset)
                    {
                        m_tileSize = m_tileset.TilePxSize;
                        m_slicePadding = m_tileset.SlicePadding;
                        m_sliceOffset = m_tileset.SliceOffset;
                        m_atlasTexture = m_tileset.AtlasTexture;                        
                    }
                    m_atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas texture", m_atlasTexture, typeof(Texture2D), false);
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndHorizontal();

                GUI.enabled = m_atlasTexture != null;
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Slice Settings:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Tile Size (pixels)", GUILayout.MaxWidth(120f));
                    m_tileSize = EditorGUILayout.Vector2Field("", m_tileSize, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(120f));
                    m_sliceOffset = EditorGUILayout.Vector2Field("", m_sliceOffset, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Padding", GUILayout.MaxWidth(120f));
                    m_slicePadding = EditorGUILayout.Vector2Field("", m_slicePadding, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("New Settings:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    m_padding = EditorGUILayout.IntField( new GUIContent("Padding (pixels)", "Separation between tiles in pixels"), m_padding);
                    m_extrude = EditorGUILayout.IntField( new GUIContent("Extrude (pixels)", "How many pixels the color is extruded from tile border"), m_extrude);
                    m_padding = Mathf.Max(0, m_padding, m_extrude * 2);
                    m_extrude = Mathf.Max(0, m_extrude);
                    if (GUILayout.Button("Preview"))
                    {
                        m_previewTexture = BuildAtlas(m_atlasTexture, m_padding, m_extrude, m_tileSize, m_sliceOffset, m_slicePadding);
                        AtlasPreviewWindow atlasPreviewWnd = EditorWindow.GetWindow<AtlasPreviewWindow>(true, "Atlas Preview", true);
                        atlasPreviewWnd.Texture = m_previewTexture;
                        atlasPreviewWnd.TileSize = m_tileSize;
                        atlasPreviewWnd.Padding = m_padding;
                        atlasPreviewWnd.Extrude = m_extrude;
                        atlasPreviewWnd.ShowUtility();
                    }
                    if (GUILayout.Button("Apply Settings"))
                    {
                        Texture2D outputAtlas = BuildAtlas(m_atlasTexture, m_padding, m_extrude, m_tileSize, m_sliceOffset, m_slicePadding);
                        byte[] pngData = outputAtlas.EncodeToPNG();
                        string atlasTexturePath = Application.dataPath + AssetDatabase.GetAssetPath(m_atlasTexture).Substring(6);
                        System.IO.File.WriteAllBytes(atlasTexturePath, pngData);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_atlasTexture));
                        Debug.Log("Saving atlas texture: " + atlasTexturePath);
                        if (m_tileset)
                        {
                            m_sliceOffset = new Vector2(m_padding, m_padding);
                            m_slicePadding = m_sliceOffset;
                            sliceTileset = true;
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndVertical();

                if (m_previewTexture)
                {
                    GUILayout.Box(m_previewTexture, GUILayout.Width(position.width - 24f));
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndScrollView();

            // Save Data into tileset
            if (m_tileset)
            {
                m_tileset.TilePxSize = m_tileSize;
                m_tileset.SlicePadding = m_slicePadding;
                m_tileset.SliceOffset = m_sliceOffset;
                m_tileset.AtlasTexture = m_atlasTexture;
                if (sliceTileset)
                {
                    m_tileset.Slice();
                    foreach (Tilemap tilemap in FindObjectsOfType<Tilemap>())
                    {
                        if (tilemap.Tileset == m_tileset) tilemap.Refresh(true, false);
                    }
                }
            }

            if (GUI.changed)
            {
                if (m_tileset) EditorUtility.SetDirty(m_tileset);
            }
        }

        static Texture2D BuildAtlas(Texture2D atlasTexture, int tilePadding, int tileExtrude, Vector2 tileSize, Vector2 sliceOffset, Vector2 slicePadding)
        {
            int widthInTiles = Mathf.FloorToInt((atlasTexture.width - sliceOffset.x + slicePadding.x) / (tileSize.x + slicePadding.x)); //NOTE: "+ slicePadding.x" makes sure to count the last tile even if no padding pixels are added to the right
            int heightInTiles = Mathf.FloorToInt((atlasTexture.height - sliceOffset.y + +slicePadding.y) / (tileSize.y + slicePadding.y));

            int padTileWidth = Mathf.RoundToInt(tileSize.x + tilePadding);
            int padTileHeight = Mathf.RoundToInt(tileSize.y + tilePadding);
            int width = widthInTiles * padTileWidth + tilePadding;
            int height = heightInTiles * padTileHeight + tilePadding;
            Texture2D output = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            output.filterMode = FilterMode.Point;
            List<Rect> rects = GenerateGridSpriteRectangles(atlasTexture, sliceOffset, tileSize, slicePadding);

            MakeTextureReadable(atlasTexture);
            for (int ty = 0, idx = 0; ty < heightInTiles; ++ty)
            {
                for (int tx = 0; tx < widthInTiles; ++tx, ++idx)
                {
                    Rect rect = rects[idx];
                    Color[] srcTileColors = atlasTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                        
                    int dstX = tx * padTileWidth + tilePadding;
                    int dstY = output.height - (ty + 1) * padTileHeight;//- tilePadding;
                    output.SetPixels(dstX, dstY, (int)rect.width, (int)rect.height, srcTileColors);
                    //Extend border color to fill the padding area
                    Color[] paddingColors;
                    for(int p = 0; p < tileExtrude; ++p)
                    {
                        paddingColors = atlasTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, 1);
                        output.SetPixels(dstX, dstY - p - 1, (int)rect.width, 1, paddingColors); // bottom padding
                        paddingColors = atlasTexture.GetPixels((int)rect.x, (int)rect.y + (int)rect.height - 1, (int)rect.width, 1);
                        output.SetPixels(dstX, dstY + (int)rect.height + p, (int)rect.width, 1, paddingColors); // top padding
                        paddingColors = atlasTexture.GetPixels((int)rect.x, (int)rect.y, 1, (int)rect.height);
                        output.SetPixels(dstX - p - 1, dstY, 1, (int)rect.height, paddingColors); // left padding
                        paddingColors = atlasTexture.GetPixels((int)rect.x + (int)rect.width - 1, (int)rect.y, 1, (int)rect.height);
                        output.SetPixels(dstX + (int)rect.width + p, dstY, 1, (int)rect.height, paddingColors); // right padding
                    }

                    if (tileExtrude > 0)
                    {
                        paddingColors = Enumerable.Repeat(srcTileColors[0], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX - tileExtrude, dstY - tileExtrude, tileExtrude, tileExtrude, paddingColors); // bottom-left corner
                        paddingColors = Enumerable.Repeat(srcTileColors[(int)rect.width - 1], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX + (int)rect.width, dstY - tileExtrude, tileExtrude, tileExtrude, paddingColors); // bottom-right corner
                        paddingColors = Enumerable.Repeat(srcTileColors[srcTileColors.Length - (int)rect.width], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX - tileExtrude, dstY + (int)rect.height, tileExtrude, tileExtrude, paddingColors); // top-left corner
                        paddingColors = Enumerable.Repeat(srcTileColors[srcTileColors.Length - 1], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX + (int)rect.width, dstY + (int)rect.height, tileExtrude, tileExtrude, paddingColors); // top-right corner
                    }
                }
            }
            output.Apply();
            return output;
        }

        //NOTE: Unlike UnityEditorInternal.InternalSpriteUtility.GenerateGridSpriteRectangles, this will take full transparent tiles into account
        public static List<Rect> GenerateGridSpriteRectangles(Texture2D texture, Vector2 offset, Vector2 size, Vector2 padding)
        {
            List<Rect> rects = new List<Rect>();
            if (texture != null)
            {
                int uInc = Mathf.RoundToInt(size.x + padding.x);
                int vInc = Mathf.RoundToInt(size.y + padding.y);
                if (uInc > 0 && vInc > 0)
                {
                    for (int y = Mathf.RoundToInt(offset.y); y + size.y <= texture.height; y += vInc)
                    {
                        for (int x = Mathf.RoundToInt(offset.x); x + size.x <= texture.width; x += uInc)
                        {
                            rects.Add(new Rect(x, texture.height - y - size.y, size.x, size.y));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(" Error while slicing. There is something wrong with slicing parameters. uInc = " + uInc + "; vInc = " + vInc);
                }
            }
            return rects;
        }

        public static void MakeTextureReadable(Texture2D texture2D)
        {
            if (texture2D != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(texture2D);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                    if (textureImporter != null)
                    {
                        if (!textureImporter.isReadable)
                        {
                            textureImporter.isReadable = true;
                            AssetDatabase.ImportAsset(assetPath);
                        }
                    }
                }
            }
        }
    }

    public class AtlasPreviewWindow : EditorWindow
    {
        public Texture2D Texture;
        public Vector2 TileSize;
        public int Padding;
        public int Extrude;
        private int m_zoom = 1;

        void OnLostFocus()
        {
            Close();
        }

        private Vector2 m_scrollPos;
        void OnGUI()
        {
            if (!Texture) Close();

            Event e = Event.current;

            Color emptyColor = new Color();
            Color gridColor = new Color(0f, 1f, 1f, 0.5f);
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            {
                if (e.type == EventType.ScrollWheel)
                {
                    if (e.delta.y < 0f) m_zoom++;
                    else m_zoom--;
                    m_zoom = Mathf.Max(1, m_zoom);
                }
                if (e.type == EventType.MouseDrag)
                {
                    m_scrollPos -= e.delta;
                    Repaint();
                }
                GUILayoutUtility.GetRect(Texture.width * m_zoom, Texture.height * m_zoom);
                GUI.DrawTexture(new Rect(0, 0, Texture.width * m_zoom, Texture.height * m_zoom), Texture);
                Rect tileRect = new Rect(new Vector2(Padding, Padding), TileSize);
                for (; tileRect.yMax <= Texture.height; tileRect.y += TileSize.y + Padding)
                {
                    tileRect.x = Padding;
                    for (; tileRect.xMax <= Texture.width; tileRect.x += TileSize.x + Padding)
                    {
                        Rect scaledRect = tileRect; scaledRect.position *= m_zoom; scaledRect.size *= m_zoom;
                        HandlesEx.DrawRectWithOutline(scaledRect, emptyColor, gridColor);
                    }
                }
            }
            GUILayout.EndScrollView();
        }
    }
}